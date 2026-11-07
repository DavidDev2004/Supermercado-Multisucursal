using System.Data;
using Microsoft.Data.SqlClient;
using SupermercadoMVC.Models;

namespace SupermercadoMVC.Data;

public sealed partial class BusinessIntelligenceDataService(DatabaseConnectionFactory connections)
{
    private sealed record SaleLine(int Sale, DateTime Date, int Branch, string BranchName, int Product,
        string ProductName, int Category, string CategoryName, decimal Units, decimal Amount);
    private sealed record ClientSale(int Sale, DateTime Date, int Client, string ClientName,
        string Department, DateTime FirstPurchase, decimal Amount);
    private sealed record Payment(DateTime Date, int Branch, string BranchName, string Method,
        string State, decimal Amount, decimal Commission);

    public static BiPage CreatePage(string module, BiFilter filter) => new()
    {
        Module = module,
        Filter = filter,
        Title = module switch
        {
            "m01" => "Ventas y Tendencias Comerciales",
            "m02" => "Clientes y Comportamiento de Compra",
            "m03" => "Cobros y Medios de Pago",
            "m04" => "Existencias y Disponibilidad",
            "m05" => "Movimientos y Control de Inventario",
            "m06" => "Rotación y Reposición de Productos",
            "m07" => "Compras y Abastecimiento",
            "m08" => "Proveedores y Costos de Adquisición",
            _ => "Gestión y Desempeño de Sucursales"
        },
        Description = module switch
        {
            "m01" => "Ventas confirmadas · importes netos desde el detalle",
            "m02" => "Clientes compradores · ventas confirmadas",
            "m03" => "Comprobantes de ventas · pagos aplicados para importes",
            "m04" => "Estado actual de inventario por producto y sucursal",
            "m05" => "Historial de movimientos · fuentes incompletas identificadas",
            "m06" => "Ventas confirmadas y existencias actuales, sin duplicar cantidades",
            "m07" => "Órdenes registradas y compras efectivamente recibidas",
            "m08" => "Relaciones activas y costos de compras recibidas",
            _ => "Indicadores consolidados por sucursal"
        }
    };

    public async Task<BiPage> GetAsync(string module, BiFilter filter, bool calculate, CancellationToken ct)
    {
        var page = CreatePage(module, filter);
        await using var connection = connections.Create();
        await connection.OpenAsync(ct);
        if (module != "m04")
        {
            var anchorSql = module switch
            {
                "m03" => "SELECT MAX(FechaPago) FROM dbo.Tbl_Pagos WHERE Estado='Aplicado';",
                "m05" or "m07" or "m08" => "SELECT MAX(FechaRecepcion) FROM dbo.Tbl_Compras WHERE Estado='Recibida';",
                _ => "SELECT MAX(FechaVenta) FROM dbo.Tbl_Ventas WHERE Estado='Confirmada';"
            };
            await using var command = new SqlCommand(anchorSql, connection);
            var anchor = await command.ExecuteScalarAsync(ct);
            var end = anchor is DateTime date ? date.Date : DateTime.Today;
            filter.Hasta ??= filter.Desde.HasValue && filter.Desde.Value > end ? filter.Desde.Value : end;
            filter.Desde ??= new DateTime(filter.Hasta.Value.Year, filter.Hasta.Value.Month, 1).AddMonths(-5);
        }
        await OptionsAsync(connection, page, ct);
        if (!calculate || filter.Desde > filter.Hasta) return page;
        switch (module)
        {
            case "m01": await SalesAsync(connection, page, ct); break;
            case "m02": await ClientsAsync(connection, page, ct); break;
            case "m03": await PaymentsAsync(connection, page, ct); break;
            case "m04": await InventoryAsync(connection, page, ct); break;
            case "m05": MovementLimitations(page); break;
            case "m06": await RotationAsync(connection, page, ct); break;
            case "m07": await PurchasesAsync(connection, page, ct); break;
            case "m08": await SuppliersAsync(connection, page, ct); break;
            case "m09": await BranchesAsync(connection, page, ct); break;
        }
        page.UpdatedAt = DateTime.Now;
        return page;
    }

    private static async Task OptionsAsync(SqlConnection c, BiPage page, CancellationToken ct)
    {
        await Options(c, "SELECT CodigoSucursal,Nombre FROM dbo.Tbl_Sucursales ORDER BY Nombre;", page.Sucursales, ct);
        if (page.Module is "m01" or "m04" or "m06" or "m07" or "m08")
        {
            await Options(c, "SELECT CodigoCategoria,Nombre FROM dbo.Tbl_Categorias ORDER BY Nombre;", page.Categorias, ct);
            await Options(c, "SELECT CodigoProducto,Nombre FROM dbo.Tbl_Productos ORDER BY Nombre;", page.Productos, ct);
        }
        if (page.Module == "m02")
        {
            await Options(c, "SELECT DISTINCT Departamento AS FilterValue,Departamento AS DisplayName FROM dbo.Tbl_Clientes ORDER BY FilterValue;", page.Departamentos, ct);
            await Options(c, "SELECT DISTINCT Municipio AS FilterValue,Municipio AS DisplayName FROM dbo.Tbl_Clientes ORDER BY FilterValue;", page.Municipios, ct);
        }
        if (page.Module == "m03")
        {
            await Options(c, "SELECT DISTINCT MetodoPago AS FilterValue,MetodoPago AS DisplayName FROM dbo.Tbl_Pagos ORDER BY FilterValue;", page.Metodos, ct);
            await Options(c, "SELECT DISTINCT Estado AS FilterValue,Estado AS DisplayName FROM dbo.Tbl_Pagos ORDER BY FilterValue;", page.Estados, ct);
        }
        if (page.Module is "m07" or "m08")
            await Options(c, "SELECT CodigoProveedor,Nombre FROM dbo.Tbl_Proveedores ORDER BY Nombre;", page.Proveedores, ct);
        if (page.Module == "m07")
            await Options(c, "SELECT DISTINCT Estado AS FilterValue,Estado AS DisplayName FROM dbo.Tbl_Compras ORDER BY FilterValue;", page.Estados, ct);
        if (page.Module == "m08")
            await Options(c, "SELECT DISTINCT Departamento AS FilterValue,Departamento AS DisplayName FROM dbo.Tbl_Proveedores ORDER BY FilterValue;", page.Departamentos, ct);
        if (page.Module == "m09")
            await Options(c, "SELECT DISTINCT Departamento AS FilterValue,Departamento AS DisplayName FROM dbo.Tbl_Sucursales ORDER BY FilterValue;", page.Departamentos, ct);
    }

    private static async Task Options(SqlConnection c, string sql, List<BiOption> target, CancellationToken ct)
    {
        await using var command = new SqlCommand(sql, c);
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            target.Add(new BiOption(Convert.ToString(reader.GetValue(0))!, reader.GetString(1)));
    }

    private static void Dates(SqlCommand command, BiFilter f)
    {
        var start = f.Desde!.Value.Date;
        var end = f.Hasta!.Value.Date;
        var previousStart = start.AddDays(-(end - start).Days - 1);
        command.Parameters.Add("@PreviousStart", SqlDbType.Date).Value = previousStart;
        command.Parameters.Add("@Start", SqlDbType.Date).Value = start;
        command.Parameters.Add("@EndExclusive", SqlDbType.Date).Value = end.AddDays(1);
        command.Parameters.Add("@Branch", SqlDbType.Int).Value = (object?)f.Sucursal ?? DBNull.Value;
    }

    private static decimal? Change(decimal current, decimal previous) => previous == 0m
        ? null : Math.Round((current - previous) * 100m / previous, 2);
    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
    private static IEnumerable<DateTime> Months(DateTime start, DateTime end)
    {
        var month = new DateTime(start.Year, start.Month, 1);
        var last = new DateTime(end.Year, end.Month, 1);
        while (month <= last) { yield return month; month = month.AddMonths(1); }
    }
    private static string Month(DateTime date) => date.ToString("MMM yyyy", System.Globalization.CultureInfo.GetCultureInfo("es-GT"));
    private static string Key(DateTime date) => date.ToString("yyyy-MM");
    private static string PeriodNote => "Variación frente al período anterior de igual duración; se omite si su base es cero.";

    private static async Task SalesAsync(SqlConnection c, BiPage page, CancellationToken ct)
    {
        var f = page.Filter;
        var rows = new List<SaleLine>();
        const string sql = """
            SELECT v.CodigoVenta,v.FechaVenta,v.CodigoSucursal,s.Nombre,d.CodigoProducto,p.Nombre,
                   p.CodigoCategoria,cat.Nombre,d.Cantidad,
                   CAST(d.Cantidad*d.PrecioUnitario-d.Descuento AS decimal(28,2))
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
        await using (var command = new SqlCommand(sql, c))
        {
            Dates(command, f);
            command.Parameters.Add("@Category", SqlDbType.Int).Value = (object?)f.Categoria ?? DBNull.Value;
            command.Parameters.Add("@Product", SqlDbType.Int).Value = (object?)f.Producto ?? DBNull.Value;
            await using var reader = await command.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct)) rows.Add(new(
                reader.GetInt32(0), reader.GetDateTime(1), reader.GetInt32(2), reader.GetString(3),
                reader.GetInt32(4), reader.GetString(5), reader.GetInt32(6), reader.GetString(7),
                reader.GetDecimal(8), reader.GetDecimal(9)));
        }
        var current = rows.Where(r => r.Date >= f.Desde).ToList();
        var previous = rows.Where(r => r.Date < f.Desde).ToList();
        var amount = current.Sum(r => r.Amount); var oldAmount = previous.Sum(r => r.Amount);
        var count = current.Select(r => r.Sale).Distinct().Count();
        var oldCount = previous.Select(r => r.Sale).Distinct().Count();
        var units = current.Sum(r => r.Units); var oldUnits = previous.Sum(r => r.Units);
        page.Metrics.AddRange([
            new("E01", "Ventas Netas", amount, "money", Change(amount,oldAmount), PeriodNote),
            new("E02", "Número de Ventas", count, "integer", Change(count,oldCount), PeriodNote),
            new("E03", "Ticket Promedio", count == 0 ? 0 : Round(amount/count), "money",
                oldCount == 0 ? null : Change(amount/count, oldAmount/oldCount), PeriodNote),
            new("E04", "Unidades Vendidas", units, "number", Change(units,oldUnits), PeriodNote)
        ]);
        var months = Months(f.Desde!.Value, f.Hasta!.Value).ToList();
        page.Charts.Add(new("E05", "Ventas por Mes", "line", "money", months.Select(m =>
            new BiPoint(Month(m),current.Where(r => r.Date.Year==m.Year && r.Date.Month==m.Month).Sum(r=>r.Amount))).ToList(),
            Note:"Los meses sin ventas se muestran con cero."));
        var branchPoints = page.Sucursales.Where(x => f.Sucursal is null || x.Value == f.Sucursal.ToString())
            .Select(x => new BiPoint(x.Label,current.Where(r=>r.Branch.ToString()==x.Value).Sum(r=>r.Amount),
                Detail:amount == 0 ? "0,0 % del total" :
                    $"{Round(current.Where(r=>r.Branch.ToString()==x.Value).Sum(r=>r.Amount)*100/amount):N1} % del total",
                FilterValue:x.Value)).OrderByDescending(p=>p.Value).ToList();
        page.Charts.Add(new("E06", "Ventas por Sucursal", "bar", "money", branchPoints,
            Note:"Importe neto y participación sobre las ventas filtradas.",CrossFilter:"Sucursal"));
        var categories = current.GroupBy(r => new {r.Category,r.CategoryName})
            .Select(g=>new BiPoint(g.Key.CategoryName,g.Sum(r=>r.Amount),FilterValue:g.Key.Category.ToString()))
            .OrderByDescending(p=>p.Value).ToList();
        page.Charts.Add(new("E07", "Ventas por Categoría", "donut", "money", categories,
            Note:"Participación del importe neto filtrado.",CrossFilter:"Categoria"));
        var products = current.GroupBy(r => new {r.Product,r.ProductName})
            .Select(g=>new BiPoint(g.Key.ProductName,g.Sum(r=>r.Amount),Detail:$"{g.Sum(r=>r.Units):N2} unidades",FilterValue:g.Key.Product.ToString()))
            .OrderByDescending(p=>p.Value).ThenBy(p=>p.Label).Take(f.Top).ToList();
        page.Charts.Add(new("E08", "Productos Más Vendidos", "bar", "money", products,
            Note:"Top por importe neto; el valor emergente incluye unidades.",CrossFilter:"Producto"));
    }

    private static async Task ClientsAsync(SqlConnection c, BiPage page, CancellationToken ct)
    {
        var f=page.Filter; var rows=new List<ClientSale>();
        const string sql="""
            WITH Primera AS (
              SELECT CodigoCliente,MIN(FechaVenta) AS FechaPrimera
              FROM dbo.Tbl_Ventas WHERE Estado='Confirmada' GROUP BY CodigoCliente
            )
            SELECT v.CodigoVenta,v.FechaVenta,v.CodigoCliente,cl.Nombres+' '+cl.Apellidos,
                   cl.Departamento,pr.FechaPrimera,
                   CAST(COALESCE(det.Total,0) AS decimal(28,2))
            FROM dbo.Tbl_Ventas v
            JOIN dbo.Tbl_Clientes cl ON cl.CodigoCliente=v.CodigoCliente
            JOIN Primera pr ON pr.CodigoCliente=v.CodigoCliente
            OUTER APPLY (SELECT SUM(d.Cantidad*d.PrecioUnitario-d.Descuento) AS Total
                         FROM dbo.Tbl_VentasDetalle d WHERE d.CodigoVenta=v.CodigoVenta) det
            WHERE v.Estado='Confirmada' AND v.FechaVenta>=@PreviousStart AND v.FechaVenta<@EndExclusive
              AND (@Branch IS NULL OR v.CodigoSucursal=@Branch)
              AND (@Department IS NULL OR cl.Departamento=@Department)
              AND (@Municipality IS NULL OR cl.Municipio=@Municipality);
            """;
        await using(var command=new SqlCommand(sql,c))
        {
            Dates(command,f);
            command.Parameters.Add("@Department",SqlDbType.VarChar,100).Value=(object?)f.Departamento ?? DBNull.Value;
            command.Parameters.Add("@Municipality",SqlDbType.VarChar,100).Value=(object?)f.Municipio ?? DBNull.Value;
            await using var reader=await command.ExecuteReaderAsync(ct);
            while(await reader.ReadAsync(ct))rows.Add(new(reader.GetInt32(0),reader.GetDateTime(1),reader.GetInt32(2),
                reader.GetString(3),reader.GetString(4),reader.GetDateTime(5),reader.GetDecimal(6)));
        }
        var now=rows.Where(r=>r.Date>=f.Desde).ToList();
        var before=rows.Where(r=>r.Date<f.Desde).ToList();
        var clients=now.Select(r=>r.Client).Distinct().Count();
        var oldClients=before.Select(r=>r.Client).Distinct().Count();
        var recurring=now.GroupBy(r=>r.Client).Count(g=>g.Count()>=2);
        var oldRecurring=before.GroupBy(r=>r.Client).Count(g=>g.Count()>=2);
        var avgPurchases=clients==0?0:Round((decimal)now.Count/clients);
        var oldAvgPurchases=oldClients==0?0:Round((decimal)before.Count/oldClients);
        var avgSpend=clients==0?0:Round(now.Sum(r=>r.Amount)/clients);
        var oldAvgSpend=oldClients==0?0:Round(before.Sum(r=>r.Amount)/oldClients);
        page.Metrics.AddRange([
            new("E01","Clientes con Compras",clients,"integer",Change(clients,oldClients),PeriodNote),
            new("E02","Clientes Recurrentes",recurring,"integer",Change(recurring,oldRecurring),
                clients==0?"Sin compradores en el período.":$"{Round((decimal)recurring*100/clients):N2} % de compradores."),
            new("E03","Compras por Cliente",avgPurchases,"decimal",Change(avgPurchases,oldAvgPurchases),PeriodNote),
            new("E04","Gasto Promedio por Cliente",avgSpend,"money",Change(avgSpend,oldAvgSpend),PeriodNote)
        ]);
        var months=Months(f.Desde!.Value,f.Hasta!.Value).ToList();
        page.Charts.Add(new("E05","Clientes por Mes","line","integer",months.Select(m=>new BiPoint(Month(m),
            now.Where(r=>r.Date.Year==m.Year&&r.Date.Month==m.Month).Select(r=>r.Client).Distinct().Count())).ToList()));
        page.Charts.Add(new("E06","Clientes con Mayor Gasto","bar","money",now.GroupBy(r=>new{r.Client,r.ClientName})
            .Select(g=>new BiPoint(g.Key.ClientName,g.Sum(r=>r.Amount))).OrderByDescending(p=>p.Value)
            .ThenBy(p=>p.Label).Take(10).ToList()));
        page.Charts.Add(new("E07","Distribución Geográfica de Clientes","donut","integer",
            now.GroupBy(r=>new{r.Client,r.Department}).Select(g=>g.Key)
                .GroupBy(x=>x.Department,StringComparer.OrdinalIgnoreCase)
                .Select(g=>new BiPoint(g.Select(x=>x.Department).OrderBy(x=>x,StringComparer.Ordinal).First(),g.Count()))
                .OrderByDescending(p=>p.Value).ToList(),
            Note:"Cada comprador se cuenta una vez en su departamento registrado."));
        page.Charts.Add(new("E08","Clientes Nuevos y Recurrentes","grouped","integer",
            months.Select(m=>{
                var perMonth=now.Where(r=>r.Date.Year==m.Year&&r.Date.Month==m.Month)
                    .GroupBy(r=>r.Client).Select(g=>g.First()).ToList();
                return new BiPoint(Month(m),perMonth.Count(r=>r.FirstPurchase.Year==m.Year&&r.FirstPurchase.Month==m.Month),
                    perMonth.Count(r=>r.FirstPurchase.Year!=m.Year||r.FirstPurchase.Month!=m.Month));
            }).ToList(),OtherLabel:"Recurrentes",
            Note:"Nuevo = primera compra confirmada histórica en ese mes; recurrente = había comprado antes."));
    }

    private static async Task PaymentsAsync(SqlConnection c,BiPage page,CancellationToken ct)
    {
        var f=page.Filter;var rows=new List<Payment>();
        const string sql="""
            SELECT p.FechaPago,v.CodigoSucursal,s.Nombre,p.MetodoPago,p.Estado,p.Monto,p.Comision
            FROM dbo.Tbl_Pagos p
            JOIN dbo.Tbl_Ventas v ON v.CodigoVenta=p.CodigoVenta
            JOIN dbo.Tbl_Sucursales s ON s.CodigoSucursal=v.CodigoSucursal
            WHERE p.FechaPago>=@PreviousStart AND p.FechaPago<@EndExclusive
              AND (@Branch IS NULL OR v.CodigoSucursal=@Branch)
              AND (@Method IS NULL OR p.MetodoPago=@Method)
              AND (@State IS NULL OR p.Estado=@State);
            """;
        await using(var command=new SqlCommand(sql,c))
        {
            Dates(command,f);
            command.Parameters.Add("@Method",SqlDbType.VarChar,50).Value=(object?)f.Metodo??DBNull.Value;
            command.Parameters.Add("@State",SqlDbType.VarChar,20).Value=(object?)f.Estado??DBNull.Value;
            await using var reader=await command.ExecuteReaderAsync(ct);
            while(await reader.ReadAsync(ct))rows.Add(new(reader.GetDateTime(0),reader.GetInt32(1),reader.GetString(2),
                reader.GetString(3),reader.GetString(4),reader.GetDecimal(5),reader.GetDecimal(6)));
        }
        var all=rows.Where(r=>r.Date>=f.Desde).ToList();
        var current=all.Where(r=>r.State=="Aplicado").ToList();
        var previous=rows.Where(r=>r.Date<f.Desde&&r.State=="Aplicado").ToList();
        var total=current.Sum(r=>r.Amount);var oldTotal=previous.Sum(r=>r.Amount);
        var count=current.Count;var oldCount=previous.Count;
        var commissions=current.Sum(r=>r.Commission);var oldCommissions=previous.Sum(r=>r.Commission);
        page.Metrics.AddRange([
            new("E01","Total Cobrado",total,"money",Change(total,oldTotal),PeriodNote),
            new("E02","Número de Cobros",count,"integer",Change(count,oldCount),"Solo comprobantes Aplicados; los Anulados se muestran en el gráfico de estados."),
            new("E03","Cobro Promedio",count==0?0:Round(total/count),"money",
                oldCount==0?null:Change(total/count,oldTotal/oldCount),PeriodNote),
            new("E04","Comisiones",commissions,"money",Change(commissions,oldCommissions),
                "La comisión no reduce el abono del cliente.")
        ]);
        page.Charts.Add(new("E05","Cobros por Método de Pago","donut","money",current.GroupBy(r=>r.Method)
            .Select(g=>new BiPoint(g.Key,g.Sum(r=>r.Amount),FilterValue:g.Key)).OrderByDescending(p=>p.Value).ToList(),
            CrossFilter:"Metodo"));
        page.Charts.Add(new("E06","Cobros por Mes","column","money",Months(f.Desde!.Value,f.Hasta!.Value)
            .Select(m=>new BiPoint(Month(m),current.Where(r=>r.Date.Year==m.Year&&r.Date.Month==m.Month)
                .Sum(r=>r.Amount))).ToList()));
        page.Charts.Add(new("E07","Estado de Cobros","donut","integer",all.GroupBy(r=>r.State)
            .Select(g=>new BiPoint(g.Key,g.Count(),FilterValue:g.Key)).OrderByDescending(p=>p.Value).ToList(),
            Note:"Cuenta los estados existentes en SQL Server; los Anulados no suman a cobros.",CrossFilter:"Estado"));
        page.Charts.Add(new("E08","Cobros por Sucursal","bar","money",
            page.Sucursales.Where(x=>f.Sucursal is null||x.Value==f.Sucursal.ToString())
                .Select(x=>new BiPoint(x.Label,current.Where(r=>r.Branch.ToString()==x.Value)
                    .Sum(r=>r.Amount),Detail:total==0?"0,0 % del total":
                    $"{Round(current.Where(r=>r.Branch.ToString()==x.Value).Sum(r=>r.Amount)*100/total):N1} % del total",
                    FilterValue:x.Value)).OrderByDescending(p=>p.Value).ToList(),
            Note:"Incluye sucursales sin cobros aplicados.",CrossFilter:"Sucursal"));
    }
}
