using System.Data;
using Microsoft.Data.SqlClient;
using SupermercadoMVC.Models;

namespace SupermercadoMVC.Data;

public sealed partial class BusinessIntelligenceDataService
{
    private sealed record PurchaseEntry(int Purchase, DateTime OrderDate, DateTime? ReceivedDate,
        string State, int Branch, string BranchName, int Provider, string ProviderName,
        int? Product, int? Category, decimal Quantity, decimal UnitCost, decimal Discount)
    {
        public decimal Net => Quantity * UnitCost - Discount;
    }

    private sealed record SupplierRow(int Id, string Name, string Department, string State);
    private sealed record SupplyRow(int Provider, int Product, string ProductName, int Category,
        decimal ReferenceCost, int LeadDays);

    private static async Task<List<PurchaseEntry>> PurchaseRowsAsync(SqlConnection c, BiFilter f,
        bool branchDepartment, CancellationToken ct)
    {
        const string sql = """
            SELECT c.CodigoCompra,c.FechaCompra,c.FechaRecepcion,c.Estado,c.CodigoSucursal,s.Nombre,
                   c.CodigoProveedor,pr.Nombre,d.CodigoProducto,p.CodigoCategoria,
                   d.Cantidad,d.CostoUnitario,d.Descuento
            FROM dbo.Tbl_Compras c
            JOIN dbo.Tbl_Sucursales s ON s.CodigoSucursal=c.CodigoSucursal
            JOIN dbo.Tbl_Proveedores pr ON pr.CodigoProveedor=c.CodigoProveedor
            LEFT JOIN dbo.Tbl_ComprasDetalle d ON d.CodigoCompra=c.CodigoCompra
            LEFT JOIN dbo.Tbl_Productos p ON p.CodigoProducto=d.CodigoProducto
            WHERE (c.FechaCompra>=@PreviousStart OR c.FechaRecepcion>=@PreviousStart)
              AND (@Branch IS NULL OR c.CodigoSucursal=@Branch)
              AND (@Provider IS NULL OR c.CodigoProveedor=@Provider)
              AND (@Product IS NULL OR d.CodigoProducto=@Product)
              AND (@Category IS NULL OR p.CodigoCategoria=@Category)
              AND (@Department IS NULL OR
                   (@BranchDepartment=1 AND s.Departamento=@Department) OR
                   (@BranchDepartment=0 AND pr.Departamento=@Department));
            """;
        await using var command = new SqlCommand(sql, c);
        Dates(command, f);
        command.Parameters.Add("@Provider", SqlDbType.Int).Value = (object?)f.Proveedor ?? DBNull.Value;
        command.Parameters.Add("@Product", SqlDbType.Int).Value = (object?)f.Producto ?? DBNull.Value;
        command.Parameters.Add("@Category", SqlDbType.Int).Value = (object?)f.Categoria ?? DBNull.Value;
        command.Parameters.Add("@Department", SqlDbType.VarChar, 100).Value = (object?)f.Departamento ?? DBNull.Value;
        command.Parameters.Add("@BranchDepartment", SqlDbType.Bit).Value = branchDepartment;
        var rows = new List<PurchaseEntry>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            rows.Add(new(reader.GetInt32(0), reader.GetDateTime(1), reader.IsDBNull(2) ? null : reader.GetDateTime(2),
                reader.GetString(3), reader.GetInt32(4), reader.GetString(5), reader.GetInt32(6),
                reader.GetString(7), reader.IsDBNull(8) ? null : reader.GetInt32(8),
                reader.IsDBNull(9) ? null : reader.GetInt32(9),
                reader.IsDBNull(10) ? 0 : reader.GetDecimal(10),
                reader.IsDBNull(11) ? 0 : reader.GetDecimal(11),
                reader.IsDBNull(12) ? 0 : reader.GetDecimal(12)));
        return rows;
    }

    private static async Task PurchasesAsync(SqlConnection c, BiPage page, CancellationToken ct)
    {
        var f = page.Filter;
        var rows = await PurchaseRowsAsync(c, f, false, ct);
        if (f.Estado is not null) rows = rows.Where(r => r.State == f.Estado).ToList();
        var received = rows.Where(r => r.State == "Recibida" && r.ReceivedDate >= f.Desde &&
            r.ReceivedDate <= f.Hasta).ToList();
        var oldReceived = rows.Where(r => r.State == "Recibida" && r.ReceivedDate < f.Desde &&
            r.ReceivedDate >= f.Desde!.Value.AddDays(-(f.Hasta!.Value - f.Desde.Value).Days - 1)).ToList();
        var orders = rows.Where(r => r.OrderDate >= f.Desde && r.OrderDate <= f.Hasta)
            .DistinctBy(r => r.Purchase).ToList();
        var oldOrders = rows.Where(r => r.OrderDate < f.Desde &&
            r.OrderDate >= f.Desde!.Value.AddDays(-(f.Hasta!.Value - f.Desde.Value).Days - 1))
            .DistinctBy(r => r.Purchase).ToList();
        var receivedCount = received.Select(r => r.Purchase).Distinct().Count();
        var oldReceivedCount = oldReceived.Select(r => r.Purchase).Distinct().Count();
        var amount = received.Sum(r => r.Net);
        var oldAmount = oldReceived.Sum(r => r.Net);
        const string pending = "El esquema solo distingue Borrador, Recibida y Anulada; Borrador no demuestra una orden formal pendiente de recepción.";
        page.Metrics.AddRange([
            new("E01", "Total de Compras", amount, "money", Change(amount, oldAmount),
                "Solo Recibidas por FechaRecepcion; importe neto de líneas con descuento."),
            new("E02", "Número de Órdenes", orders.Count, "integer", Change(orders.Count, oldOrders.Count),
                "Documentos únicos por FechaCompra, según los estados filtrados."),
            new("E03", "Compras Pendientes", 0, "integer", PendingReason: pending),
            new("E04", "Compras Recibidas", receivedCount, "integer", Change(receivedCount, oldReceivedCount),
                "Documentos únicos efectivamente recibidos por FechaRecepcion.")
        ]);
        page.Charts.Add(new("E05", "Compras por Mes", "column", "money",
            Months(f.Desde!.Value, f.Hasta!.Value)
                .Select(m => new BiPoint(Month(m), received.Where(r => r.ReceivedDate!.Value.Year == m.Year &&
                    r.ReceivedDate.Value.Month == m.Month).Sum(r => r.Net))).ToList(),
            Note: "Importes netos de compras Recibidas; meses sin recepciones se muestran con cero."));
        page.Charts.Add(new("E06", "Compras por Estado", "donut", "integer",
            orders.GroupBy(r => r.State).Select(g => new BiPoint(g.Key, g.Count(), FilterValue: g.Key))
                .OrderByDescending(p => p.Value).ToList(),
            Note: "Órdenes por FechaCompra; no agrega estados inexistentes.", CrossFilter: "Estado"));
        page.Charts.Add(new("E07", "Compras por Sucursal", "bar", "money",
            page.Sucursales.Where(o => f.Sucursal is null || o.Value == f.Sucursal.ToString())
                .Select(o => new BiPoint(o.Label, received.Where(r => r.Branch.ToString() == o.Value)
                    .Sum(r => r.Net), FilterValue: o.Value)).OrderByDescending(p => p.Value).ToList(),
            Note: "Incluye sucursales sin recepciones en el período.", CrossFilter: "Sucursal"));
        page.Charts.Add(new("E08", "Compras por Proveedor", "bar", "money",
            received.GroupBy(r => new { r.Provider, r.ProviderName })
                .Select(g => new BiPoint(g.Key.ProviderName, g.Sum(r => r.Net),
                    Detail: amount == 0 ? "0 %" : $"{Round(g.Sum(r => r.Net) * 100 / amount):N1} % del total",
                    FilterValue: g.Key.Provider.ToString()))
                .OrderByDescending(p => p.Value).ThenBy(p => p.Label).Take(f.Top).ToList(),
            Note: "Importe neto y participación de compras Recibidas.", CrossFilter: "Proveedor"));
    }

    private static async Task<List<SupplierRow>> SupplierRowsAsync(SqlConnection c, CancellationToken ct)
    {
        var rows = new List<SupplierRow>();
        await using var command = new SqlCommand(
            "SELECT CodigoProveedor,Nombre,Departamento,Estado FROM dbo.Tbl_Proveedores;", c);
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            rows.Add(new(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3)));
        return rows;
    }

    private static async Task<List<SupplyRow>> SupplyRowsAsync(SqlConnection c, BiFilter f, CancellationToken ct)
    {
        const string sql = """
            SELECT pp.CodigoProveedor,pp.CodigoProducto,p.Nombre,p.CodigoCategoria,
                   pp.CostoReferencia,pp.PlazoEntrega
            FROM dbo.Tbl_ProductoProveedor pp
            JOIN dbo.Tbl_Productos p ON p.CodigoProducto=pp.CodigoProducto
            JOIN dbo.Tbl_Proveedores pr ON pr.CodigoProveedor=pp.CodigoProveedor
            WHERE pp.Estado='Activo' AND p.Estado='Activo' AND pr.Estado='Activo'
              AND (@Provider IS NULL OR pp.CodigoProveedor=@Provider)
              AND (@Product IS NULL OR pp.CodigoProducto=@Product)
              AND (@Category IS NULL OR p.CodigoCategoria=@Category)
              AND (@Department IS NULL OR pr.Departamento=@Department);
            """;
        await using var command = new SqlCommand(sql, c);
        command.Parameters.Add("@Provider", SqlDbType.Int).Value = (object?)f.Proveedor ?? DBNull.Value;
        command.Parameters.Add("@Product", SqlDbType.Int).Value = (object?)f.Producto ?? DBNull.Value;
        command.Parameters.Add("@Category", SqlDbType.Int).Value = (object?)f.Categoria ?? DBNull.Value;
        command.Parameters.Add("@Department", SqlDbType.VarChar, 100).Value = (object?)f.Departamento ?? DBNull.Value;
        var rows = new List<SupplyRow>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct)) rows.Add(new(reader.GetInt32(0), reader.GetInt32(1),
            reader.GetString(2), reader.GetInt32(3), reader.GetDecimal(4), reader.GetInt32(5)));
        return rows;
    }

    private static async Task<int> ActiveProductDenominatorAsync(SqlConnection c, BiFilter f, CancellationToken ct)
    {
        const string sql = """
            SELECT COUNT(*) FROM dbo.Tbl_Productos
            WHERE Estado='Activo' AND (@Category IS NULL OR CodigoCategoria=@Category)
              AND (@Product IS NULL OR CodigoProducto=@Product);
            """;
        await using var command = new SqlCommand(sql, c);
        command.Parameters.Add("@Category", SqlDbType.Int).Value = (object?)f.Categoria ?? DBNull.Value;
        command.Parameters.Add("@Product", SqlDbType.Int).Value = (object?)f.Producto ?? DBNull.Value;
        return Convert.ToInt32(await command.ExecuteScalarAsync(ct));
    }

    private static async Task SuppliersAsync(SqlConnection c, BiPage page, CancellationToken ct)
    {
        var f = page.Filter;
        var suppliers = (await SupplierRowsAsync(c, ct)).Where(r => r.State == "Activo" &&
            (f.Proveedor is null || r.Id == f.Proveedor) &&
            (f.Departamento is null || r.Department == f.Departamento)).ToList();
        var supplies = await SupplyRowsAsync(c, f, ct);
        if (f.Producto is not null || f.Categoria is not null)
        {
            var eligible = supplies.Select(r => r.Provider).ToHashSet();
            suppliers = suppliers.Where(r => eligible.Contains(r.Id)).ToList();
        }
        var rows = (await PurchaseRowsAsync(c, f, false, ct))
            .Where(r => r.State == "Recibida" && r.ReceivedDate >= f.Desde && r.ReceivedDate <= f.Hasta && r.Product is not null)
            .ToList();
        var quantity = rows.Sum(r => r.Quantity);
        var denominator = await ActiveProductDenominatorAsync(c, f, ct);
        var covered = supplies.Select(r => r.Product).Distinct().Count();
        var docs = rows.DistinctBy(r => r.Purchase).ToList();
        var days = docs.Select(r => (r.ReceivedDate!.Value - r.OrderDate).Days).ToList();
        page.Metrics.AddRange([
            new("E01", "Proveedores Activos", suppliers.Count, "integer",
                Note: "Estado actual del catálogo; no se infiere un estado histórico."),
            new("E02", "Productos con Proveedor", covered, "integer",
                Note: denominator == 0 ? "Sin productos activos elegibles." :
                    $"{Round((decimal)covered * 100 / denominator):N1} % del catálogo activo con filtros de categoría/producto."),
            new("E03", "Costo Promedio de Adquisición", quantity == 0 ? 0 : Round(rows.Sum(r => r.Net) / quantity),
                "money", Note: "Costo neto ponderado por unidades recibidas; diferentes unidades de medida pueden coexistir."),
            new("E04", "Plazo Promedio de Entrega", days.Count == 0 ? 0 : Math.Round((decimal)days.Sum() / days.Count, 1),
                "days", Note: "Días reales entre FechaCompra y FechaRecepcion; no usa el plazo estimado.")
        ]);
        var total = rows.Sum(r => r.Net);
        page.Charts.Add(new("E05", "Proveedores por Volumen de Compras", "bar", "money",
            rows.GroupBy(r => new { r.Provider, r.ProviderName })
                .Select(g => new BiPoint(g.Key.ProviderName, g.Sum(r => r.Net),
                    Detail: total == 0 ? "0 %" : $"{Round(g.Sum(r => r.Net) * 100 / total):N1} % del total",
                    FilterValue: g.Key.Provider.ToString()))
                .OrderByDescending(p => p.Value).ThenBy(p => p.Label).Take(f.Top).ToList(),
            Note: "Volumen monetario neto de compras Recibidas.", CrossFilter: "Proveedor"));
        var months = Months(f.Desde!.Value, f.Hasta!.Value).ToList();
        var topProducts = rows.GroupBy(r => r.Product!.Value)
            .OrderByDescending(g => g.Sum(r => r.Quantity)).Take(f.Producto is null ? 5 : 1).ToList();
        var series = topProducts.Select(g => new BiSeries(page.Productos.FirstOrDefault(p => p.Value == g.Key.ToString())?.Label
            ?? g.Key.ToString(), months.Select(m => {
                var monthRows = g.Where(r => r.ReceivedDate!.Value.Year == m.Year && r.ReceivedDate.Value.Month == m.Month).ToList();
                return new BiPoint(Key(m), monthRows.Count == 0 ? 0 : Round(monthRows.Sum(r => r.Net) / monthRows.Sum(r => r.Quantity)),
                    Detail: monthRows.Count == 0 ? "Sin compras" : "Costo neto ponderado");
            }).ToList())).ToList();
        page.Charts.Add(new("E06", "Evolución del Costo de Productos", "multi-line", "money",
            months.Select(m => new BiPoint(Key(m), 0, Detail: Month(m))).ToList(),
            Note: "Hasta cinco productos con compras recibidas; los meses sin compra quedan como huecos, no como costo cero.",
            Series: series));
        const string currency = "Tbl_ProductoProveedor no registra moneda ni tipo de cambio. No se pueden comparar costos de referencia entre proveedores sin verificar que estén en la misma moneda.";
        page.Charts.Add(new("E07", "Comparativo de Costos por Proveedor", "bar", "money", [],
            PendingReason: currency));
        var ranges = new[] { ("0–2 días", 0, 2), ("3–5 días", 3, 5), ("6–10 días", 6, 10), ("Más de 10 días", 11, int.MaxValue) };
        page.Charts.Add(new("E08", "Plazos de Entrega", "bar", "integer",
            ranges.Select(x => new BiPoint(x.Item1, supplies.Count(r => r.LeadDays >= x.Item2 && r.LeadDays <= x.Item3),
                Detail: supplies.Count == 0 ? "0 %" : $"{Round((decimal)supplies.Count(r => r.LeadDays >= x.Item2 && r.LeadDays <= x.Item3) * 100 / supplies.Count):N1} %"))
                .ToList(),
            Note: "Plazo estimado configurado en relaciones activas; no equivale al tiempo real de entrega."));
    }
}
