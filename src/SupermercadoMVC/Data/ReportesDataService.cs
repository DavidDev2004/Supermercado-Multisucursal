using System.Data;
using Microsoft.Data.SqlClient;
using SupermercadoMVC.Models;

namespace SupermercadoMVC.Data;

public sealed class ReportesDataService(DatabaseConnectionFactory connections)
{
    // Cada origen produce una fila por documento (o por inventario). Los detalles y
    // pagos se agregan por separado para no multiplicar importes al combinarlos.
    private const string Ventas = """
        WITH Reporte AS (
          SELECT v.FechaVenta AS Fecha,v.NumeroVenta AS Referencia,
            c.Nombres+' '+c.Apellidos AS Relacion,s.Nombre AS Sucursal,v.Estado,
            CAST(COALESCE(d.Cantidad,0) AS decimal(37,2)) AS Cantidad,
            CAST(0 AS decimal(37,2)) AS Precio,
            CAST(ROUND(COALESCE(d.Importe,0),2) AS decimal(37,2)) AS Importe,
            c.NIT AS Complemento,v.CodigoVenta AS Id
          FROM dbo.Tbl_Ventas v
          JOIN dbo.Tbl_Clientes c ON c.CodigoCliente=v.CodigoCliente
          JOIN dbo.Tbl_Sucursales s ON s.CodigoSucursal=v.CodigoSucursal
          OUTER APPLY (SELECT SUM(Cantidad) AS Cantidad,
            SUM(Cantidad*PrecioUnitario-Descuento) AS Importe
            FROM dbo.Tbl_VentasDetalle WHERE CodigoVenta=v.CodigoVenta) d
          WHERE (@Desde IS NULL OR v.FechaVenta>=@Desde)
            AND (@Hasta IS NULL OR v.FechaVenta<=@Hasta)
            AND (@Sucursal IS NULL OR v.CodigoSucursal=@Sucursal)
            AND (@Cliente IS NULL OR v.CodigoCliente=@Cliente)
            AND (@Estado='' OR v.Estado=@Estado)
        )
        """;

    private const string Compras = """
        WITH Reporte AS (
          SELECT c.FechaCompra AS Fecha,c.NumeroCompra AS Referencia,
            p.Nombre AS Relacion,s.Nombre AS Sucursal,c.Estado,
            CAST(COALESCE(d.Cantidad,0) AS decimal(37,2)) AS Cantidad,
            CAST(0 AS decimal(37,2)) AS Precio,
            CAST(ROUND(COALESCE(d.Importe,0),2) AS decimal(37,2)) AS Importe,
            p.NIT AS Complemento,c.CodigoCompra AS Id
          FROM dbo.Tbl_Compras c
          JOIN dbo.Tbl_Proveedores p ON p.CodigoProveedor=c.CodigoProveedor
          JOIN dbo.Tbl_Sucursales s ON s.CodigoSucursal=c.CodigoSucursal
          OUTER APPLY (SELECT SUM(Cantidad) AS Cantidad,
            SUM(Cantidad*CostoUnitario-Descuento) AS Importe
            FROM dbo.Tbl_ComprasDetalle WHERE CodigoCompra=c.CodigoCompra) d
          WHERE (@Desde IS NULL OR c.FechaCompra>=@Desde)
            AND (@Hasta IS NULL OR c.FechaCompra<=@Hasta)
            AND (@Sucursal IS NULL OR c.CodigoSucursal=@Sucursal)
            AND (@Proveedor IS NULL OR c.CodigoProveedor=@Proveedor)
            AND (@Estado='' OR c.Estado=@Estado)
        )
        """;

    private const string Inventario = """
        WITH Reporte AS (
          SELECT i.FechaUltimoIngreso AS Fecha,p.CodigoBarra AS Referencia,
            p.Nombre AS Relacion,s.Nombre AS Sucursal,
            CASE WHEN i.Existencia=0 THEN 'Sin existencias'
                 WHEN i.Existencia<i.ExistenciaMinima THEN 'Bajo mínimo'
                 ELSE 'En nivel' END AS Estado,
            CAST(i.Existencia AS decimal(37,2)) AS Cantidad,
            CAST(i.CostoPromedio AS decimal(37,2)) AS Precio,
            CAST(ROUND(i.Existencia*i.CostoPromedio,2) AS decimal(37,2)) AS Importe,
            cat.Nombre AS Complemento,i.CodigoInventario AS Id
          FROM dbo.Tbl_Inventario i
          JOIN dbo.Tbl_Productos p ON p.CodigoProducto=i.CodigoProducto
          JOIN dbo.Tbl_Categorias cat ON cat.CodigoCategoria=p.CodigoCategoria
          JOIN dbo.Tbl_Sucursales s ON s.CodigoSucursal=i.CodigoSucursal
          WHERE (@Sucursal IS NULL OR i.CodigoSucursal=@Sucursal)
            AND (@Estado='' OR (@Estado='Sin existencias' AND i.Existencia=0)
                 OR (@Estado='Bajo mínimo' AND i.Existencia<i.ExistenciaMinima)
                 OR (@Estado='En nivel' AND i.Existencia>0 AND i.Existencia>=i.ExistenciaMinima))
        )
        """;

    private const string Saldos = """
        WITH Reporte AS (
          SELECT v.FechaVenta AS Fecha,v.NumeroVenta AS Referencia,
            c.Nombres+' '+c.Apellidos AS Relacion,s.Nombre AS Sucursal,v.Estado,
            CAST(ROUND(COALESCE(d.Total,0),2) AS decimal(37,2)) AS Cantidad,
            CAST(COALESCE(p.Pagado,0) AS decimal(37,2)) AS Precio,
            CAST(ROUND(COALESCE(d.Total,0),2) AS decimal(37,2))
              -CAST(COALESCE(p.Pagado,0) AS decimal(37,2)) AS Importe,
            c.NIT AS Complemento,v.CodigoVenta AS Id
          FROM dbo.Tbl_Ventas v
          JOIN dbo.Tbl_Clientes c ON c.CodigoCliente=v.CodigoCliente
          JOIN dbo.Tbl_Sucursales s ON s.CodigoSucursal=v.CodigoSucursal
          OUTER APPLY (SELECT SUM(Cantidad*PrecioUnitario-Descuento) AS Total
            FROM dbo.Tbl_VentasDetalle WHERE CodigoVenta=v.CodigoVenta) d
          OUTER APPLY (SELECT SUM(Monto) AS Pagado FROM dbo.Tbl_Pagos
            WHERE CodigoVenta=v.CodigoVenta AND Estado='Aplicado') p
          WHERE v.Estado='Confirmada'
            AND (@Desde IS NULL OR v.FechaVenta>=@Desde)
            AND (@Hasta IS NULL OR v.FechaVenta<=@Hasta)
            AND (@Sucursal IS NULL OR v.CodigoSucursal=@Sucursal)
            AND (@Cliente IS NULL OR v.CodigoCliente=@Cliente)
        )
        """;

    private static string Source(string tipo) => tipo switch
    {
        "compras" => Compras,
        "inventario" => Inventario,
        "saldos" => Saldos,
        _ => Ventas
    };

    private static string Filter(string tipo) => tipo == "saldos" ? " WHERE Importe>0 " : "";

    private static void AddFilters(SqlCommand command, ReportesPagina model)
    {
        command.Parameters.Add("@Desde", SqlDbType.Date).Value = (object?)model.Desde?.Date ?? DBNull.Value;
        command.Parameters.Add("@Hasta", SqlDbType.Date).Value = (object?)model.Hasta?.Date ?? DBNull.Value;
        command.Parameters.Add("@Sucursal", SqlDbType.Int).Value = (object?)model.Sucursal ?? DBNull.Value;
        command.Parameters.Add("@Proveedor", SqlDbType.Int).Value = (object?)model.Proveedor ?? DBNull.Value;
        command.Parameters.Add("@Cliente", SqlDbType.Int).Value = (object?)model.Cliente ?? DBNull.Value;
        command.Parameters.Add("@Estado", SqlDbType.VarChar, 20).Value = model.Estado;
    }

    public async Task<ReportesPagina> ConsultarAsync(ReportesPagina model, bool exportar, CancellationToken ct)
    {
        await using var connection = connections.Create();
        await connection.OpenAsync(ct);
        var source = Source(model.Tipo);
        var filter = Filter(model.Tipo);
        await using (var command = new SqlCommand(source + $"""
            SELECT COUNT_BIG(*),COALESCE(SUM(Cantidad),0),COALESCE(SUM(Importe),0)
            FROM Reporte {filter};
            """, connection))
        {
            AddFilters(command, model);
            await using var reader = await command.ExecuteReaderAsync(ct);
            await reader.ReadAsync(ct);
            model.TotalRegistros = reader.GetInt64(0);
            model.TotalCantidad = reader.GetDecimal(1);
            model.TotalImporte = reader.GetDecimal(2);
        }

        model.Pagina = Math.Min(Math.Max(1, model.Pagina), model.TotalPaginas);
        var paging = exportar ? "" : " OFFSET @Offset ROWS FETCH NEXT @Size ROWS ONLY";
        await using (var command = new SqlCommand(source + $"""
            SELECT Fecha,Referencia,Relacion,Sucursal,Estado,Cantidad,Precio,Importe,Complemento
            FROM Reporte {filter}
            ORDER BY Id DESC{paging};
            """, connection))
        {
            AddFilters(command, model);
            if (!exportar)
            {
                command.Parameters.Add("@Offset", SqlDbType.BigInt).Value = ((long)model.Pagina - 1) * model.TamanoPagina;
                command.Parameters.Add("@Size", SqlDbType.Int).Value = model.TamanoPagina;
            }
            await using var reader = await command.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
                model.Filas.Add(new(reader.IsDBNull(0) ? null : reader.GetDateTime(0),
                    reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4),
                    reader.GetDecimal(5), reader.GetDecimal(6), reader.GetDecimal(7), reader.GetString(8)));
        }

        if (!exportar)
        {
            await using var command = new SqlCommand("""
                SELECT CodigoSucursal,Nombre FROM dbo.Tbl_Sucursales ORDER BY Nombre,CodigoSucursal;
                SELECT CodigoProveedor,Nombre FROM dbo.Tbl_Proveedores ORDER BY Nombre,CodigoProveedor;
                SELECT CodigoCliente,Nombres+' '+Apellidos FROM dbo.Tbl_Clientes ORDER BY Nombres,Apellidos,CodigoCliente;
                """, connection);
            await using var reader = await command.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct)) model.Sucursales.Add(new(reader.GetInt32(0), reader.GetString(1)));
            await reader.NextResultAsync(ct);
            while (await reader.ReadAsync(ct)) model.Proveedores.Add(new(reader.GetInt32(0), reader.GetString(1)));
            await reader.NextResultAsync(ct);
            while (await reader.ReadAsync(ct)) model.Clientes.Add(new(reader.GetInt32(0), reader.GetString(1)));
        }
        return model;
    }
}
