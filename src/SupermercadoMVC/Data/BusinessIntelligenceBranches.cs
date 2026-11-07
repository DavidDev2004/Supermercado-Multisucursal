using System.Data;
using Microsoft.Data.SqlClient;
using SupermercadoMVC.Models;

namespace SupermercadoMVC.Data;

public sealed partial class BusinessIntelligenceDataService
{
    private sealed record BranchRow(int Id, string Name, string Department, string State);
    private sealed record SaleAmount(DateTime Date, int Branch, decimal Net);

    private static async Task<List<BranchRow>> BranchRowsAsync(SqlConnection c, BiFilter f, CancellationToken ct)
    {
        const string sql = """
            SELECT CodigoSucursal,Nombre,Departamento,Estado FROM dbo.Tbl_Sucursales
            WHERE (@Branch IS NULL OR CodigoSucursal=@Branch)
              AND (@Department IS NULL OR Departamento=@Department);
            """;
        await using var command = new SqlCommand(sql, c);
        command.Parameters.Add("@Branch", SqlDbType.Int).Value = (object?)f.Sucursal ?? DBNull.Value;
        command.Parameters.Add("@Department", SqlDbType.VarChar, 100).Value = (object?)f.Departamento ?? DBNull.Value;
        var rows = new List<BranchRow>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            rows.Add(new(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3)));
        return rows;
    }

    private static async Task<List<SaleAmount>> SaleAmountsAsync(SqlConnection c, BiFilter f, CancellationToken ct)
    {
        const string sql = """
            SELECT v.FechaVenta,v.CodigoSucursal,
                   CAST(d.Cantidad*d.PrecioUnitario-d.Descuento AS decimal(28,2))
            FROM dbo.Tbl_Ventas v
            JOIN dbo.Tbl_VentasDetalle d ON d.CodigoVenta=v.CodigoVenta
            JOIN dbo.Tbl_Sucursales s ON s.CodigoSucursal=v.CodigoSucursal
            WHERE v.Estado='Confirmada' AND v.FechaVenta>=@PreviousStart AND v.FechaVenta<@EndExclusive
              AND (@Branch IS NULL OR v.CodigoSucursal=@Branch)
              AND (@Department IS NULL OR s.Departamento=@Department);
            """;
        await using var command = new SqlCommand(sql, c);
        Dates(command, f);
        command.Parameters.Add("@Department", SqlDbType.VarChar, 100).Value = (object?)f.Departamento ?? DBNull.Value;
        var rows = new List<SaleAmount>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            rows.Add(new(reader.GetDateTime(0), reader.GetInt32(1), reader.GetDecimal(2)));
        return rows;
    }

    private static async Task<Dictionary<int, int>> EmployeeCountsAsync(SqlConnection c, CancellationToken ct)
    {
        var result = new Dictionary<int, int>();
        await using var command = new SqlCommand(
            "SELECT CodigoSucursal,COUNT(*) FROM dbo.Tbl_Empleados WHERE Estado='Activo' GROUP BY CodigoSucursal;", c);
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct)) result[reader.GetInt32(0)] = reader.GetInt32(1);
        return result;
    }

    private static async Task BranchesAsync(SqlConnection c, BiPage page, CancellationToken ct)
    {
        var f = page.Filter;
        var branches = await BranchRowsAsync(c, f, ct);
        var ids = branches.Select(r => r.Id).ToHashSet();
        var allSales = await SaleAmountsAsync(c, f, ct);
        var sales = allSales.Where(r => r.Date >= f.Desde).ToList();
        var oldSales = allSales.Where(r => r.Date < f.Desde).ToList();
        var allPurchases = await PurchaseRowsAsync(c, f, true, ct);
        var purchases = allPurchases.Where(r => r.State == "Recibida" && r.ReceivedDate >= f.Desde &&
            r.ReceivedDate <= f.Hasta).ToList();
        var oldPurchases = allPurchases.Where(r => r.State == "Recibida" && r.ReceivedDate < f.Desde &&
            r.ReceivedDate >= f.Desde!.Value.AddDays(-(f.Hasta!.Value - f.Desde.Value).Days - 1)).ToList();
        var employees = await EmployeeCountsAsync(c, ct);
        var stock = (await StockRowsAsync(c, f, ct)).Where(r => ids.Contains(r.Branch)).ToList();
        var saleTotal = sales.Sum(r => r.Net);
        var purchaseTotal = purchases.Sum(r => r.Net);
        page.Metrics.AddRange([
            new("E01", "Sucursales Activas", branches.Count(r => r.State == "Activo"), "integer",
                Note: "Estado actual del catálogo; no hay historial de cambios de estado."),
            new("E02", "Ventas Totales", saleTotal, "money", Change(saleTotal, oldSales.Sum(r => r.Net)),
                "Ventas Confirmadas por FechaVenta; importe neto desde detalles."),
            new("E03", "Compras Totales", purchaseTotal, "money", Change(purchaseTotal, oldPurchases.Sum(r => r.Net)),
                "Compras Recibidas por FechaRecepcion; importe neto desde detalles."),
            new("E04", "Empleados Activos", branches.Sum(r => employees.GetValueOrDefault(r.Id)), "integer",
                Note: "Asignación actual por sucursal; no se infieren cambios históricos.")
        ]);
        page.Charts.Add(new("E05", "Ventas frente a Compras por Sucursal", "grouped", "money",
            branches.Select(r => new BiPoint(r.Name, sales.Where(x => x.Branch == r.Id).Sum(x => x.Net),
                purchases.Where(x => x.Branch == r.Id).Sum(x => x.Net), FilterValue: r.Id.ToString()))
                .OrderByDescending(p => p.Value).ToList(),
            OtherLabel: "compras recibidas", Note: "Comparación de importes, no utilidad contable.",
            CrossFilter: "Sucursal", PrimaryLabel: "ventas confirmadas"));
        var months = Months(f.Desde!.Value, f.Hasta!.Value).ToList();
        var lineSeries = branches.Select(b => new BiSeries(b.Name, months.Select(m =>
            new BiPoint(Key(m), sales.Where(r => r.Branch == b.Id && r.Date.Year == m.Year &&
                r.Date.Month == m.Month).Sum(r => r.Net))).ToList())).ToList();
        page.Charts.Add(new("E06", "Evolución de Ventas por Sucursal", "multi-line", "money",
            months.Select(m => new BiPoint(Key(m), 0, Detail: Month(m))).ToList(),
            Note: "Series por sucursal; los meses sin ventas se muestran con cero.", Series: lineSeries));
        page.Charts.Add(new("E07", "Distribución de Empleados", "donut", "integer",
            branches.Select(r => new BiPoint(r.Name, employees.GetValueOrDefault(r.Id),
                FilterValue: r.Id.ToString())).ToList(),
            Note: "Empleados Activos por sucursal; incluye sucursales con cero.", CrossFilter: "Sucursal"));
        page.Charts.Add(new("E08", "Inventario Valorizado por Sucursal", "bar", "money",
            branches.Select(r => new BiPoint(r.Name, stock.Where(x => x.Branch == r.Id)
                .Sum(x => x.Quantity * x.Cost), FilterValue: r.Id.ToString()))
                .OrderByDescending(p => p.Value).ToList(),
            Note: "Saldo actual de existencia × costo promedio, no valor histórico; el esquema no registra moneda.",
            CrossFilter: "Sucursal"));
    }
}
