using System.Data;
using Microsoft.Data.SqlClient;
using SupermercadoMVC.Models;

namespace SupermercadoMVC.Data;

public sealed class InicioDataService(DatabaseConnectionFactory connections)
{
    // Una fila por venta antes de combinar otras colecciones. ROUND se aplica una vez al total de cada venta.
    private const string Sales = """
        WITH Ventas AS (
          SELECT v.CodigoVenta,v.NumeroVenta,v.FechaVenta,v.CodigoCliente,v.CodigoSucursal,v.Estado,
            CAST(ROUND(COALESCE(d.Total,0),2) AS decimal(37,2)) AS Total
          FROM dbo.Tbl_Ventas v OUTER APPLY
            (SELECT SUM(Cantidad*PrecioUnitario-Descuento) AS Total FROM dbo.Tbl_VentasDetalle
             WHERE CodigoVenta=v.CodigoVenta) d
        )
        """;
    private const string Purchases = """
        WITH Compras AS (
          SELECT c.CodigoCompra,c.NumeroCompra,c.FechaCompra,c.Estado,
            CAST(ROUND(COALESCE(d.Total,0),2) AS decimal(37,2)) AS Total
          FROM dbo.Tbl_Compras c OUTER APPLY
            (SELECT SUM(Cantidad*CostoUnitario-Descuento) AS Total FROM dbo.Tbl_ComprasDetalle
             WHERE CodigoCompra=c.CodigoCompra) d
        )
        """;

    private static async Task ReadAsync(SqlConnection connection, SqlTransaction transaction, string sql,
        DateTime today, Action<SqlDataReader> read, CancellationToken ct, string? search = null)
    {
        await using var command = new SqlCommand(sql, connection, transaction);
        command.Parameters.Add("@Today", SqlDbType.Date).Value = today;
        command.Parameters.Add("@Tomorrow", SqlDbType.Date).Value = today.AddDays(1);
        command.Parameters.Add("@Month", SqlDbType.Date).Value = new DateTime(today.Year, today.Month, 1);
        command.Parameters.Add("@Start", SqlDbType.Date).Value = new DateTime(today.Year, today.Month, 1).AddMonths(-5);
        command.Parameters.Add("@PaymentsStart", SqlDbType.Date).Value = today.AddDays(-29);
        if (search is not null) command.Parameters.Add("@Search", SqlDbType.VarChar, 202).Value = "%" + search + "%";
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct)) read(reader);
    }
    private static async Task<List<InicioPoint>> PointsAsync(SqlConnection connection, SqlTransaction transaction,
        string sql, DateTime today, CancellationToken ct)
    {
        var points = new List<InicioPoint>();
        await ReadAsync(connection, transaction, sql, today,
            r => points.Add(new(r.GetString(0), Convert.ToDecimal(r.GetValue(1)))), ct);
        return points;
    }
    private static async Task<List<InicioPoint>> MonthsAsync(SqlConnection connection, SqlTransaction transaction,
        string sql, DateTime today, CancellationToken ct)
    {
        var values = new Dictionary<DateTime, decimal>();
        await ReadAsync(connection, transaction, sql, today,
            r => values.Add(new DateTime(r.GetInt32(0), r.GetInt32(1), 1), r.GetDecimal(2)), ct);
        var start = new DateTime(today.Year, today.Month, 1).AddMonths(-5);
        return Enumerable.Range(0, 6).Select(i => start.AddMonths(i))
            .Select(month => new InicioPoint(InicioFormat.Month(month), values.GetValueOrDefault(month))).ToList();
    }

    public async Task<InicioPage> GetAsync(string kind, string search, CancellationToken ct)
    {
        var model = CreatePage(kind, search);
        await using var connection = connections.Create(); await connection.OpenAsync(ct);
        // Solo lecturas; los paneles de una página comparten una vista coherente durante esta transacción breve.
        using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
        try
        {
            await using (var clock = new SqlCommand("SELECT CAST(GETDATE() AS date);", connection, transaction))
                model.Today = (DateTime)(await clock.ExecuteScalarAsync(ct))!;
            if (kind == "Administrador") await AdminAsync(connection, transaction, model, ct);
            else if (kind == "Bodega") await WarehouseAsync(connection, transaction, model, ct);
            else await CashAsync(connection, transaction, model, ct);
            await transaction.CommitAsync(ct); return model;
        }
        catch
        {
            try { await transaction.RollbackAsync(CancellationToken.None); }
            catch (InvalidOperationException) { }
            catch (SqlException) { }
            throw;
        }
    }

    public static InicioPage CreatePage(string kind, string search)
    {
        var m = new InicioPage { Kind = kind, ProductSearch = search };
        if (kind == "Administrador")
        {
            m.Links.AddRange([new("Clientes", "/catalogos/clientes"), new("Empleados", "/catalogos/empleados"),
                new("Sucursales", "/catalogos/sucursales"), new("Ventas", "/ventas"), new("Pagos y saldos", "/pagos"),
                new("Compras", "/compras"), new("Productos", "/productos"), new("Inventario", "/inventario")]);
            m.Unavailable.Add("Reportes");
        }
        else if (kind == "Bodega")
        {
            m.Links.AddRange([new("Categorías", "/catalogos/categorias"), new("Proveedores", "/catalogos/proveedores"),
                new("Sucursales", "/catalogos/sucursales"), new("Compras", "/compras"),
                new("Productos", "/productos"), new("Inventario", "/inventario")]);
            m.Notices.Add(new("Movimientos, despachos y transferencias", "El esquema no dispone de un historial de movimientos ni documentos de despacho o transferencia. Las compras y las existencias se consultan por separado; no se reconstruye un historial ficticio."));
        }
        else
        {
            m.Links.AddRange([new("Ventas", "/ventas"), new("Pagos y saldos", "/pagos"),
                new("Clientes", "/catalogos/clientes"), new("Productos", "/productos")]);
            m.Notices.Add(new("Ventas por hora", "FechaVenta almacena una fecha sin hora. No es posible distribuir ventas por hora con los datos actuales."));
            m.Notices.Add(new("Estado de caja", "No existen tablas de apertura, cierre, turnos ni arqueo de caja. Los cobros mostrados son pagos Aplicados, no un saldo de efectivo en caja."));
        }
        return m;
    }

    private static async Task AdminAsync(SqlConnection c, SqlTransaction t, InicioPage m, CancellationToken ct)
    {
        await ReadAsync(c, t, Sales + "SELECT COALESCE(SUM(Total),0) FROM Ventas WHERE Estado='Confirmada' AND FechaVenta>=@Month AND FechaVenta<@Tomorrow;",
            m.Today, r => m.Metrics.Add(new("Ventas confirmadas · mes a la fecha", r.GetDecimal(0), true)), ct);
        await ReadAsync(c, t, Purchases + "SELECT COALESCE(SUM(Total),0) FROM Compras WHERE Estado='Recibida' AND FechaCompra>=@Month AND FechaCompra<@Tomorrow;",
            m.Today, r => m.Metrics.Add(new("Compras recibidas · por fecha de compra, mes", r.GetDecimal(0), true)), ct);
        await ReadAsync(c, t, """
            SELECT (SELECT COUNT_BIG(*) FROM dbo.Tbl_Productos WHERE Estado='Activo'),
              (SELECT COUNT_BIG(*) FROM dbo.Tbl_Clientes WHERE Estado='Activo'),
              (SELECT COUNT_BIG(*) FROM dbo.Tbl_Sucursales WHERE Estado='Activo');
            """, m.Today, r => { m.Metrics.Add(new("Productos activos", r.GetInt64(0))); m.Metrics.Add(new("Clientes activos", r.GetInt64(1))); m.Metrics.Add(new("Sucursales activas", r.GetInt64(2))); }, ct);
        m.Charts.Add(new("Ventas por sucursal", "Confirmadas · mes actual a la fecha", await PointsAsync(c, t, Sales + """
            SELECT s.Nombre,COALESCE(SUM(v.Total),0) FROM dbo.Tbl_Sucursales s
            LEFT JOIN Ventas v ON v.CodigoSucursal=s.CodigoSucursal AND v.Estado='Confirmada' AND v.FechaVenta>=@Month AND v.FechaVenta<@Tomorrow
            GROUP BY s.CodigoSucursal,s.Nombre ORDER BY s.Nombre;
            """, m.Today, ct), true));
        m.Charts.Add(new("Compras mensuales", "Recibidas · agrupadas por FechaCompra · últimos seis meses hasta hoy", await MonthsAsync(c, t, Purchases + """
            SELECT YEAR(FechaCompra),MONTH(FechaCompra),SUM(Total) FROM Compras
            WHERE Estado='Recibida' AND FechaCompra>=@Start AND FechaCompra<@Tomorrow
            GROUP BY YEAR(FechaCompra),MONTH(FechaCompra) ORDER BY YEAR(FechaCompra),MONTH(FechaCompra);
            """, m.Today, ct), true));
        m.Charts.Add(new("Estado del inventario", "Posiciones sucursal/producto · existencias actuales", await PointsAsync(c, t, """
            SELECT CASE WHEN Existencia=0 THEN 'Sin existencias' WHEN Existencia<ExistenciaMinima THEN 'Bajo mínimo'
              ELSE 'En mínimo o superior' END, COUNT_BIG(*) FROM dbo.Tbl_Inventario
            GROUP BY CASE WHEN Existencia=0 THEN 'Sin existencias' WHEN Existencia<ExistenciaMinima THEN 'Bajo mínimo' ELSE 'En mínimo o superior' END;
            """, m.Today, ct)));
        m.Charts.Add(new("Categorías por ventas", "Importes de líneas confirmadas · últimos seis meses hasta hoy", await PointsAsync(c, t, """
            SELECT cat.Nombre,SUM(d.Cantidad*d.PrecioUnitario-d.Descuento) FROM dbo.Tbl_Ventas v
            JOIN dbo.Tbl_VentasDetalle d ON d.CodigoVenta=v.CodigoVenta JOIN dbo.Tbl_Productos p ON p.CodigoProducto=d.CodigoProducto
            JOIN dbo.Tbl_Categorias cat ON cat.CodigoCategoria=p.CodigoCategoria
            WHERE v.Estado='Confirmada' AND v.FechaVenta>=@Start AND v.FechaVenta<@Tomorrow
            GROUP BY cat.CodigoCategoria,cat.Nombre ORDER BY SUM(d.Cantidad*d.PrecioUnitario-d.Descuento) DESC;
            """, m.Today, ct), true, "Distribución por importe neto de líneas; el redondeo a centavos aquí es de presentación."));
        var growth = "Sin base comparable para calcular crecimiento.";
        await ReadAsync(c, t, Sales + """
            SELECT COALESCE(SUM(CASE WHEN FechaVenta>=DATEADD(month,-1,@Month) THEN Total ELSE 0 END),0),
              COALESCE(SUM(CASE WHEN FechaVenta<DATEADD(month,-1,@Month) THEN Total ELSE 0 END),0)
            FROM Ventas WHERE Estado='Confirmada' AND FechaVenta>=DATEADD(month,-2,@Month) AND FechaVenta<@Month;
            """, m.Today, r => {
                var current = r.GetDecimal(0); var previous = r.GetDecimal(1);
                growth = previous == 0 ? "Sin base comparable: el mes anterior al último mes cerrado tiene total cero."
                    : $"Crecimiento: {((current - previous) / previous * 100m).ToString("N2", System.Globalization.CultureInfo.GetCultureInfo("es-GT"))} % · último mes cerrado frente al anterior.";
                growth += $" Totales comparados: {InicioFormat.Money(current)} y {InicioFormat.Money(previous)}.";
            }, ct);
        m.Charts.Add(new("Tendencia y crecimiento de ventas", "Confirmadas · últimos seis meses hasta hoy", await MonthsAsync(c, t, Sales + """
            SELECT YEAR(FechaVenta),MONTH(FechaVenta),SUM(Total) FROM Ventas
            WHERE Estado='Confirmada' AND FechaVenta>=@Start AND FechaVenta<@Tomorrow
            GROUP BY YEAR(FechaVenta),MONTH(FechaVenta) ORDER BY YEAR(FechaVenta),MONTH(FechaVenta);
            """, m.Today, ct), true, growth));
    }

    private static async Task WarehouseAsync(SqlConnection c, SqlTransaction t, InicioPage m, CancellationToken ct)
    {
        await ReadAsync(c, t, """
            SELECT COUNT_BIG(DISTINCT CodigoProducto),COUNT_BIG(CASE WHEN Existencia<ExistenciaMinima THEN 1 END),
              COUNT_BIG(CASE WHEN Existencia=0 THEN 1 END) FROM dbo.Tbl_Inventario;
            """, m.Today, r => {
                m.Metrics.Add(new("Productos registrados en inventario", r.GetInt64(0)));
                m.Metrics.Add(new("Posiciones bajo mínimo", r.GetInt64(1))); m.Metrics.Add(new("Posiciones sin existencias", r.GetInt64(2)));
            }, ct);
        await ReadAsync(c, t, "SELECT COUNT_BIG(*) FROM dbo.Tbl_Compras WHERE Estado='Borrador';", m.Today,
            r => m.Metrics.Add(new("Compras en borrador", r.GetInt64(0))), ct);
        m.Charts.Add(new("Inventario por categoría", "Productos distintos registrados en inventario; no se suman unidades de medida diferentes", await PointsAsync(c, t, """
            SELECT cat.Nombre,COUNT_BIG(DISTINCT i.CodigoProducto) FROM dbo.Tbl_Inventario i
            JOIN dbo.Tbl_Productos p ON p.CodigoProducto=i.CodigoProducto JOIN dbo.Tbl_Categorias cat ON cat.CodigoCategoria=p.CodigoCategoria
            GROUP BY cat.CodigoCategoria,cat.Nombre ORDER BY cat.Nombre;
            """, m.Today, ct)));
        m.Charts.Add(new("Estados de compras", "Documentos existentes · todas las fechas", await PointsAsync(c, t,
            "SELECT Estado,COUNT_BIG(*) FROM dbo.Tbl_Compras GROUP BY Estado ORDER BY Estado;", m.Today, ct)));
        var stock = new List<string[]>();
        await ReadAsync(c, t, """
            SELECT TOP(12) s.Nombre,p.Nombre,p.UnidadMedida,i.Existencia,i.ExistenciaMinima,i.FechaUltimoIngreso
            FROM dbo.Tbl_Inventario i JOIN dbo.Tbl_Sucursales s ON s.CodigoSucursal=i.CodigoSucursal
            JOIN dbo.Tbl_Productos p ON p.CodigoProducto=i.CodigoProducto
            WHERE i.Existencia<i.ExistenciaMinima ORDER BY i.Existencia,i.CodigoInventario;
            """, m.Today, r => stock.Add([r.GetString(0),r.GetString(1),r.GetString(2),InicioFormat.Quantity(r.GetDecimal(3)),
                InicioFormat.Quantity(r.GetDecimal(4)),r.IsDBNull(5) ? "Sin fecha" : InicioFormat.Date(r.GetDateTime(5))]), ct);
        m.Tables.Add(new("Existencias bajo mínimo", "Hasta 12 posiciones · Existencia < ExistenciaMinima", ["Sucursal","Producto","Unidad","Existencia","Mínimo","Último ingreso"], stock));
        var purchases = new List<string[]>();
        await ReadAsync(c, t, """
            SELECT TOP(8) c.NumeroCompra,c.FechaCompra,s.Nombre,p.Nombre,c.FechaEntregaProgramada,c.FechaRecepcion,c.Estado
            FROM dbo.Tbl_Compras c JOIN dbo.Tbl_Sucursales s ON s.CodigoSucursal=c.CodigoSucursal
            JOIN dbo.Tbl_Proveedores p ON p.CodigoProveedor=c.CodigoProveedor ORDER BY c.FechaCompra DESC,c.CodigoCompra DESC;
            """, m.Today, r => purchases.Add([r.GetString(0),InicioFormat.Date(r.GetDateTime(1)),r.GetString(2),r.GetString(3),
                InicioFormat.Date(r.GetDateTime(4)),r.IsDBNull(5) ? "Sin recepción" : InicioFormat.Date(r.GetDateTime(5)),r.GetString(6)]), ct);
        m.Tables.Add(new("Compras recientes y recepción", "Ocho documentos más recientes por FechaCompra; consulta informativa", ["Número","Compra","Sucursal","Proveedor","Entrega programada","Recepción","Estado"], purchases));
    }

    private static async Task CashAsync(SqlConnection c, SqlTransaction t, InicioPage m, CancellationToken ct)
    {
        await ReadAsync(c, t, Sales + "SELECT COUNT_BIG(*),COALESCE(SUM(Total),0) FROM Ventas WHERE Estado='Confirmada' AND FechaVenta=@Today;", m.Today,
            r => { m.Metrics.Add(new("Ventas confirmadas · hoy", r.GetInt64(0))); m.Metrics.Add(new("Importe de ventas · hoy", r.GetDecimal(1), true)); }, ct);
        await ReadAsync(c, t, "SELECT COALESCE(SUM(Monto),0) FROM dbo.Tbl_Pagos WHERE Estado='Aplicado' AND FechaPago=@Today;", m.Today,
            r => m.Metrics.Add(new("Pagos aplicados · hoy", r.GetDecimal(0), true)), ct);
        await ReadAsync(c, t, Sales + """
            SELECT COUNT_BIG(*) FROM Ventas v OUTER APPLY
              (SELECT SUM(Monto) AS Pagado FROM dbo.Tbl_Pagos WHERE CodigoVenta=v.CodigoVenta AND Estado='Aplicado') p
            WHERE v.Estado='Confirmada' AND v.Total>COALESCE(p.Pagado,0);
            """, m.Today, r => m.Metrics.Add(new("Confirmadas con saldo positivo · todas", r.GetInt64(0))), ct);
        m.Charts.Add(new("Formas de pago", "Monto completo de pagos Aplicados · últimos 30 días hasta hoy", await PointsAsync(c, t, """
            SELECT MetodoPago,SUM(Monto) FROM dbo.Tbl_Pagos WHERE Estado='Aplicado' AND FechaPago>=@PaymentsStart AND FechaPago<@Tomorrow
            GROUP BY MetodoPago ORDER BY SUM(Monto) DESC,MetodoPago;
            """, m.Today, ct), true, "Las comisiones no se restan del abono del cliente. Los pagos Anulados no se incluyen."));
        m.Charts.Add(new("Productos más vendidos", "Top 5 por importe neto de líneas confirmadas · últimos seis meses hasta hoy", await PointsAsync(c, t, """
            SELECT TOP(5) p.Nombre,SUM(d.Cantidad*d.PrecioUnitario-d.Descuento) FROM dbo.Tbl_Ventas v
            JOIN dbo.Tbl_VentasDetalle d ON d.CodigoVenta=v.CodigoVenta JOIN dbo.Tbl_Productos p ON p.CodigoProducto=d.CodigoProducto
            WHERE v.Estado='Confirmada' AND v.FechaVenta>=@Start AND v.FechaVenta<@Tomorrow
            GROUP BY p.CodigoProducto,p.Nombre ORDER BY SUM(d.Cantidad*d.PrecioUnitario-d.Descuento) DESC,p.CodigoProducto;
            """, m.Today, ct), true));
        var sales = new List<string[]>();
        await ReadAsync(c, t, Sales + """
            SELECT TOP(8) v.NumeroVenta,v.FechaVenta,c.Nombres+' '+c.Apellidos,s.Nombre,v.Estado,v.Total
            FROM Ventas v JOIN dbo.Tbl_Clientes c ON c.CodigoCliente=v.CodigoCliente
            JOIN dbo.Tbl_Sucursales s ON s.CodigoSucursal=v.CodigoSucursal ORDER BY v.FechaVenta DESC,v.CodigoVenta DESC;
            """, m.Today, r => sales.Add([r.GetString(0),InicioFormat.Date(r.GetDateTime(1)),r.GetString(2),r.GetString(3),r.GetString(4),InicioFormat.Money(r.GetDecimal(5))]), ct);
        m.Tables.Add(new("Ventas recientes", "Ocho documentos más recientes por fecha; incluye el estado de cada venta", ["Número","Fecha","Cliente","Sucursal","Estado","Total"], sales));
        var products = new List<string[]>();
        await ReadAsync(c, t, """
            SELECT TOP(10) p.CodigoBarra,p.Nombre,p.Marca,cat.Nombre,p.UnidadMedida,p.PrecioVenta,p.Estado
            FROM dbo.Tbl_Productos p JOIN dbo.Tbl_Categorias cat ON cat.CodigoCategoria=p.CodigoCategoria
            WHERE p.CodigoBarra LIKE @Search OR p.Nombre LIKE @Search OR p.Marca LIKE @Search
            ORDER BY p.Nombre,p.CodigoProducto;
            """, m.Today, r => products.Add([r.GetString(0),r.GetString(1),r.GetString(2),r.GetString(3),r.GetString(4),InicioFormat.Money(r.GetDecimal(5)),r.GetString(6)]), ct, m.ProductSearch);
        m.Products = new("Productos", "Hasta 10 coincidencias por código de barras, nombre o marca · consulta de precios", ["Código de barras","Producto","Marca","Categoría","Unidad","Precio","Estado"], products);
    }
}
