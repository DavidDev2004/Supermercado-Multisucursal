using System.Data;
using Microsoft.Data.SqlClient;
using SupermercadoMVC.Models;

namespace SupermercadoMVC.Data;

public sealed partial class BusinessIntelligenceDataService
{
    private sealed record Stock(int Branch, string BranchName, int Product, string ProductName,
        int Category, string CategoryName, string Unit, decimal Quantity, decimal Minimum, decimal Cost)
    {
        public string Condition => Quantity == 0 ? "Sin Stock" : Quantity < Minimum ? "Bajo Mínimo" : "Normal";
    }

    private sealed record Sold(DateTime Date, int Branch, string BranchName, int Product,
        string ProductName, int Category, string CategoryName, string Unit, decimal Quantity);

    private static async Task<List<Stock>> StockRowsAsync(SqlConnection c, BiFilter f, CancellationToken ct)
    {
        const string sql = """
            SELECT i.CodigoSucursal,s.Nombre,i.CodigoProducto,p.Nombre,p.CodigoCategoria,cat.Nombre,
                   p.UnidadMedida,i.Existencia,i.ExistenciaMinima,i.CostoPromedio
            FROM dbo.Tbl_Inventario i
            JOIN dbo.Tbl_Sucursales s ON s.CodigoSucursal=i.CodigoSucursal
            JOIN dbo.Tbl_Productos p ON p.CodigoProducto=i.CodigoProducto
            JOIN dbo.Tbl_Categorias cat ON cat.CodigoCategoria=p.CodigoCategoria
            WHERE (@Branch IS NULL OR i.CodigoSucursal=@Branch)
              AND (@Category IS NULL OR p.CodigoCategoria=@Category)
              AND (@Product IS NULL OR i.CodigoProducto=@Product);
            """;
        await using var command = new SqlCommand(sql, c);
        command.Parameters.Add("@Branch", SqlDbType.Int).Value = (object?)f.Sucursal ?? DBNull.Value;
        command.Parameters.Add("@Category", SqlDbType.Int).Value = (object?)f.Categoria ?? DBNull.Value;
        command.Parameters.Add("@Product", SqlDbType.Int).Value = (object?)f.Producto ?? DBNull.Value;
        var rows = new List<Stock>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            rows.Add(new(reader.GetInt32(0), reader.GetString(1), reader.GetInt32(2), reader.GetString(3),
                reader.GetInt32(4), reader.GetString(5), reader.GetString(6), reader.GetDecimal(7),
                reader.GetDecimal(8), reader.GetDecimal(9)));
        return rows;
    }

    private static async Task InventoryAsync(SqlConnection c, BiPage page, CancellationToken ct)
    {
        var f = page.Filter;
        var stock = (await StockRowsAsync(c, f, ct))
            .Where(r => f.CondicionStock is null || r.Condition == f.CondicionStock).ToList();
        var units = stock.Sum(r => r.Quantity);
        var value = stock.Sum(r => r.Quantity * r.Cost);
        var low = stock.Where(r => r.Quantity < r.Minimum).ToList();
        var empty = stock.Count(r => r.Quantity == 0);
        page.Metrics.AddRange([
            new("E01", "Unidades Disponibles", units, "number", Note: "Suma de unidades de medida heterogéneas; consultar por producto para una unidad homogénea."),
            new("E02", "Valor de Inventario", value, "money", Note: "Existencia × costo promedio actual; el esquema no registra moneda ni tipo de cambio."),
            new("E03", "Productos Bajo Mínimo", low.Count, "integer", Note: $"{low.Select(r => r.Product).Distinct().Count()} productos distintos; el KPI cuenta alertas producto–sucursal, incluyendo stock cero."),
            new("E04", "Sin Existencia", empty, "integer", Note: "Cuenta posiciones de inventario con existencia igual a cero.")
        ]);
        page.Charts.Add(new("E05", "Inventario por Categoría", "donut", "number",
            stock.GroupBy(r => new { r.Category, r.CategoryName })
                .Select(g => new BiPoint(g.Key.CategoryName, g.Sum(r => r.Quantity), FilterValue: g.Key.Category.ToString()))
                .Where(p => p.Value > 0).OrderByDescending(p => p.Value).ToList(),
            Note: "Participación por unidades registradas; categorías pueden mezclar unidades de medida.", CrossFilter: "Categoria"));
        page.Charts.Add(new("E06", "Valor de Inventario por Sucursal", "bar", "money",
            page.Sucursales.Where(o => f.Sucursal is null || o.Value == f.Sucursal.ToString())
                .Select(o => new BiPoint(o.Label, stock.Where(r => r.Branch.ToString() == o.Value)
                    .Sum(r => r.Quantity * r.Cost), FilterValue: o.Value))
                .OrderByDescending(p => p.Value).ToList(),
            Note: "Incluye sucursales sin inventario filtrado; moneda no identificada en el esquema.", CrossFilter: "Sucursal"));
        page.Charts.Add(new("E07", "Productos Bajo Mínimo", "bar", "number",
            low.Select(r => new BiPoint($"{r.ProductName} · {r.BranchName}", r.Minimum - r.Quantity,
                Detail: $"Existencia {r.Quantity:N2}; mínimo {r.Minimum:N2}", FilterValue: r.Product.ToString()))
                .OrderByDescending(p => p.Value).ThenBy(p => p.Label).Take(10).ToList(),
            Note: "Déficit actual por posición producto–sucursal; diez mayores casos.", CrossFilter: "Producto"));
        page.Charts.Add(new("E08", "Estado de Stock", "donut", "integer",
            new[] { "Sin Stock", "Bajo Mínimo", "Normal" }
                .Select(x => new BiPoint(x, stock.Count(r => r.Condition == x), FilterValue: x)).ToList(),
            Note: "Estados excluyentes por posición de inventario.", CrossFilter: "CondicionStock"));
    }

    private static void MovementLimitations(BiPage page)
    {
        const string dates = "No existe un historial único de inventario: la venta confirmada solo guarda FechaVenta del documento, no fecha de confirmación; los ajustes históricos aplicados pueden carecer de FechaAplicacion.";
        const string costs = "Además, ajustes históricos tienen CostoUnitario NULL y el esquema no identifica moneda ni tipo de cambio; no se puede valorizar todo el historial de forma confiable.";
        page.Metrics.AddRange([
            new("E01", "Unidades de Entrada", 0, "number", PendingReason: dates),
            new("E02", "Unidades de Salida", 0, "number", PendingReason: dates),
            new("E03", "Total de Movimientos", 0, "integer", PendingReason: dates),
            new("E04", "Valor de Movimientos", 0, "money", PendingReason: dates + " " + costs)
        ]);
        page.Charts.AddRange([
            new("E05", "Movimientos por Fecha", "grouped", "number", [], PendingReason: dates),
            new("E06", "Entradas frente a Salidas", "grouped", "number", [], PendingReason: dates),
            new("E07", "Movimientos por Tipo", "donut", "integer", [], PendingReason: dates),
            new("E08", "Movimientos por Sucursal", "bar", "integer", [], PendingReason: dates)
        ]);
    }

    private static async Task<List<Sold>> SoldRowsAsync(SqlConnection c, BiFilter f, CancellationToken ct)
    {
        const string sql = """
            SELECT v.FechaVenta,v.CodigoSucursal,s.Nombre,d.CodigoProducto,p.Nombre,p.CodigoCategoria,
                   cat.Nombre,p.UnidadMedida,d.Cantidad
            FROM dbo.Tbl_Ventas v
            JOIN dbo.Tbl_VentasDetalle d ON d.CodigoVenta=v.CodigoVenta
            JOIN dbo.Tbl_Sucursales s ON s.CodigoSucursal=v.CodigoSucursal
            JOIN dbo.Tbl_Productos p ON p.CodigoProducto=d.CodigoProducto
            JOIN dbo.Tbl_Categorias cat ON cat.CodigoCategoria=p.CodigoCategoria
            WHERE v.Estado='Confirmada' AND v.FechaVenta>=@PreviousStart AND v.FechaVenta<@EndExclusive
              AND (@Branch IS NULL OR v.CodigoSucursal=@Branch)
              AND (@Category IS NULL OR p.CodigoCategoria=@Category)
              AND (@Product IS NULL OR d.CodigoProducto=@Product);
            """;
        await using var command = new SqlCommand(sql, c);
        Dates(command, f);
        command.Parameters.Add("@Category", SqlDbType.Int).Value = (object?)f.Categoria ?? DBNull.Value;
        command.Parameters.Add("@Product", SqlDbType.Int).Value = (object?)f.Producto ?? DBNull.Value;
        var rows = new List<Sold>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            rows.Add(new(reader.GetDateTime(0), reader.GetInt32(1), reader.GetString(2), reader.GetInt32(3),
                reader.GetString(4), reader.GetInt32(5), reader.GetString(6), reader.GetString(7), reader.GetDecimal(8)));
        return rows;
    }

    private static async Task<List<(int Product, string Name, int Category)>> ActiveProductsAsync(
        SqlConnection c, BiFilter f, CancellationToken ct)
    {
        const string sql = """
            SELECT p.CodigoProducto,p.Nombre,p.CodigoCategoria
            FROM dbo.Tbl_Productos p
            WHERE p.Estado='Activo'
              AND (@Category IS NULL OR p.CodigoCategoria=@Category)
              AND (@Product IS NULL OR p.CodigoProducto=@Product)
              AND (@Branch IS NULL OR EXISTS
                  (SELECT 1 FROM dbo.Tbl_Inventario i
                   WHERE i.CodigoSucursal=@Branch AND i.CodigoProducto=p.CodigoProducto));
            """;
        await using var command = new SqlCommand(sql, c);
        command.Parameters.Add("@Category", SqlDbType.Int).Value = (object?)f.Categoria ?? DBNull.Value;
        command.Parameters.Add("@Product", SqlDbType.Int).Value = (object?)f.Producto ?? DBNull.Value;
        command.Parameters.Add("@Branch", SqlDbType.Int).Value = (object?)f.Sucursal ?? DBNull.Value;
        var rows = new List<(int, string, int)>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct)) rows.Add((reader.GetInt32(0), reader.GetString(1), reader.GetInt32(2)));
        return rows;
    }

    private static async Task RotationAsync(SqlConnection c, BiPage page, CancellationToken ct)
    {
        var f = page.Filter;
        var allSales = await SoldRowsAsync(c, f, ct);
        var sales = allSales.Where(r => r.Date >= f.Desde).ToList();
        var previous = allSales.Where(r => r.Date < f.Desde).ToList();
        var active = await ActiveProductsAsync(c, f, ct);
        var stock = await StockRowsAsync(c, f, ct);
        var soldProductIds = sales.Select(r => r.Product).ToHashSet();
        var soldUnits = sales.Sum(r => r.Quantity);
        var previousUnits = previous.Sum(r => r.Quantity);
        var alerts = stock.Count(r => r.Quantity < r.Minimum);
        page.Metrics.AddRange([
            new("E01", "Productos con Ventas", soldProductIds.Count, "integer",
                Change(soldProductIds.Count, previous.Select(r => r.Product).Distinct().Count()), PeriodNote),
            new("E02", "Productos Sin Ventas", active.Count(p => !soldProductIds.Contains(p.Product)), "integer",
                Note: "Solo productos activos; con filtro de sucursal se exige una posición de inventario registrada."),
            new("E03", "Unidades Vendidas", soldUnits, "number", Change(soldUnits, previousUnits),
                "Puede sumar distintas unidades de medida; filtra un producto para comparar unidades homogéneas."),
            new("E04", "Requieren Reposición", alerts, "integer",
                Note: "Alertas actuales producto–sucursal, no pronóstico de demanda.")
        ]);
        var byProduct = sales.GroupBy(r => new { r.Product, r.ProductName })
            .Select(g => new BiPoint(g.Key.ProductName, g.Sum(r => r.Quantity),
                FilterValue: g.Key.Product.ToString())).OrderByDescending(p => p.Value).ThenBy(p => p.Label).ToList();
        page.Charts.Add(new("E05", "Productos Más Vendidos", "bar", "number", byProduct.Take(10).ToList(),
            Note: "Unidades de ventas confirmadas; diez productos principales.", CrossFilter: "Producto"));
        var lowSales = active.Select(p => new BiPoint(p.Name,
            sales.Where(r => r.Product == p.Product).Sum(r => r.Quantity), FilterValue: p.Product.ToString()))
            .OrderBy(p => p.Value).ThenBy(p => p.Label).Take(10).ToList();
        page.Charts.Add(new("E06", "Productos de Baja Rotación", "bar", "number", lowSales,
            Note: "Mide ventas bajas o nulas del período; no calcula tasa de rotación de inventario.", CrossFilter: "Producto"));
        var categories = sales.GroupBy(r => new { r.Category, r.CategoryName })
            .Select(g => new BiPoint(g.Key.CategoryName, g.Sum(r => r.Quantity),
                Detail: soldUnits == 0 ? "0 %" : $"{Round(g.Sum(r => r.Quantity) * 100 / soldUnits):N1} % del total",
                FilterValue: g.Key.Category.ToString()))
            .OrderByDescending(p => p.Value).ToList();
        page.Charts.Add(new("E07", "Rotación por Categoría", "bar", "number", categories,
            Note: "Volumen vendido por categoría; las unidades de medida pueden diferir.", CrossFilter: "Categoria"));
        var topPairs = sales.GroupBy(r => new { r.Product, r.ProductName, r.Branch, r.BranchName })
            .Select(g => new BiPoint($"{g.Key.ProductName} · {g.Key.BranchName}", g.Sum(r => r.Quantity),
                stock.Where(i => i.Product == g.Key.Product && i.Branch == g.Key.Branch).Sum(i => i.Quantity),
                FilterValue: g.Key.Product.ToString()))
            .OrderByDescending(p => p.Value).ThenBy(p => p.Label).Take(5).ToList();
        page.Charts.Add(new("E08", "Ventas frente a Existencias", "grouped-column", "number", topPairs,
            OtherLabel: "existencia actual", Note: "Turquesa: unidades vendidas en el período. Coral: saldo de inventario actual; flujo y saldo son medidas diferentes.",
            CrossFilter: "Producto", PrimaryLabel: "ventas del período"));
    }
}
