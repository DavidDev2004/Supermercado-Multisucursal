using Microsoft.Data.SqlClient;
using SupermercadoMVC.Models;
using System.Globalization;

namespace SupermercadoMVC.Data;

public sealed class DashboardDataService(DatabaseConnectionFactory connections)
{
    public async Task<DashboardViewModel> GetAsync(CancellationToken cancellationToken)
    {
        var model = new DashboardViewModel();
        await using var connection = connections.Create();
        await connection.OpenAsync(cancellationToken);

        await using (var command = connection.CreateCommand())
        {
            command.CommandText = """
                SELECT
                    (SELECT COUNT_BIG(*) FROM dbo.Tbl_Sucursales),
                    (SELECT COUNT_BIG(*) FROM dbo.Tbl_Sucursales WHERE Estado = 'Activo'),
                    (SELECT COUNT_BIG(*) FROM dbo.Tbl_Categorias),
                    (SELECT COUNT_BIG(*) FROM dbo.Tbl_Productos),
                    (SELECT COUNT_BIG(*) FROM dbo.Tbl_Clientes),
                    (SELECT COUNT_BIG(*) FROM dbo.Tbl_Proveedores);
                """;
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            await reader.ReadAsync(cancellationToken);
            model.TotalSucursales = reader.GetInt64(0);
            model.SucursalesActivas = reader.GetInt64(1);
            model.TotalCategorias = reader.GetInt64(2);
            model.TotalProductos = reader.GetInt64(3);
            model.TotalClientes = reader.GetInt64(4);
            model.TotalProveedores = reader.GetInt64(5);
        }

        await using (var command = connection.CreateCommand())
        {
            command.CommandText = """
                DECLARE @Inicio date = DATEADD(month, -5, DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1));
                SELECT YEAR(v.FechaVenta) AS Anio,
                       MONTH(v.FechaVenta) AS Mes,
                       SUM(vd.Cantidad * vd.PrecioUnitario - vd.Descuento) AS Total
                FROM dbo.Tbl_Ventas AS v
                INNER JOIN dbo.Tbl_VentasDetalle AS vd ON vd.CodigoVenta = v.CodigoVenta
                WHERE v.Estado = 'Confirmada' AND v.FechaVenta >= @Inicio
                GROUP BY YEAR(v.FechaVenta), MONTH(v.FechaVenta)
                ORDER BY Anio, Mes;
                """;
            var sales = new List<MonthlySales>();
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var date = new DateTime(reader.GetInt32(0), reader.GetInt32(1), 1);
                var label = date.ToString("MMM", CultureInfo.GetCultureInfo("es-GT"));
                sales.Add(new MonthlySales(label, reader.GetDecimal(2)));
            }
            model.MonthlySales = sales;
        }

        return model;
    }
}
