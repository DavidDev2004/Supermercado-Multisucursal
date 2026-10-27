using System.Data;
using Microsoft.Data.SqlClient;
using SupermercadoMVC.Models;

namespace SupermercadoMVC.Data;

public sealed class InventarioDataService(DatabaseConnectionFactory connections)
{
    private const string Filtro = """
        (@Sucursal IS NULL OR i.CodigoSucursal=@Sucursal)
        AND (@Categoria IS NULL OR p.CodigoCategoria=@Categoria)
        AND (@Situacion='' OR (@Situacion='BajoMinimo' AND i.Existencia<i.ExistenciaMinima)
            OR (@Situacion='SinExistencias' AND i.Existencia=0))
        AND (p.CodigoBarra LIKE @Buscar OR p.Nombre LIKE @Buscar OR p.Marca LIKE @Buscar
            OR c.Nombre LIKE @Buscar OR s.Nombre LIKE @Buscar)
        """;

    private static void Filtros(SqlCommand command, string buscar, int? sucursal, int? categoria, string situacion)
    {
        command.Parameters.Add("@Buscar", SqlDbType.VarChar, 202).Value = $"%{buscar}%";
        command.Parameters.Add("@Sucursal", SqlDbType.Int).Value = (object?)sucursal ?? DBNull.Value;
        command.Parameters.Add("@Categoria", SqlDbType.Int).Value = (object?)categoria ?? DBNull.Value;
        command.Parameters.Add("@Situacion", SqlDbType.VarChar, 20).Value = situacion;
    }

    public async Task<InventarioPagina> ConsultarAsync(string buscar, int? sucursal, int? categoria,
        string situacion, int pagina, CancellationToken ct)
    {
        var model = new InventarioPagina
        {
            Buscar = buscar, Sucursal = sucursal, Categoria = categoria,
            Situacion = situacion, Pagina = Math.Max(1, pagina)
        };
        await using var connection = connections.Create();
        await connection.OpenAsync(ct);

        await using (var command = new SqlCommand($"""
            SELECT COUNT_BIG(*)
            FROM dbo.Tbl_Inventario i
            JOIN dbo.Tbl_Productos p ON p.CodigoProducto=i.CodigoProducto
            JOIN dbo.Tbl_Categorias c ON c.CodigoCategoria=p.CodigoCategoria
            JOIN dbo.Tbl_Sucursales s ON s.CodigoSucursal=i.CodigoSucursal
            WHERE {Filtro};

            SELECT COUNT_BIG(*),
                   COUNT_BIG(CASE WHEN Existencia<ExistenciaMinima THEN 1 END),
                   COUNT_BIG(CASE WHEN Existencia=0 THEN 1 END),
                   COALESCE(SUM(Existencia), CONVERT(decimal(12,2),0))
            FROM dbo.Tbl_Inventario;
            """, connection))
        {
            Filtros(command, buscar, sucursal, categoria, situacion);
            await using var reader = await command.ExecuteReaderAsync(ct);
            await reader.ReadAsync(ct);
            model.TotalFiltrado = reader.GetInt64(0);
            await reader.NextResultAsync(ct);
            await reader.ReadAsync(ct);
            model.TotalInventarios = reader.GetInt64(0);
            model.BajoMinimo = reader.GetInt64(1);
            model.SinExistencias = reader.GetInt64(2);
            model.UnidadesTotales = reader.GetDecimal(3);
        }

        model.Pagina = Math.Min(model.Pagina, model.TotalPaginas);
        await using (var command = new SqlCommand($"""
            SELECT i.CodigoInventario,i.CodigoSucursal,i.CodigoProducto,
                   s.Nombre,p.CodigoBarra,p.Nombre,c.Nombre,
                   i.Existencia,i.ExistenciaMinima,i.CostoPromedio,
                   i.FechaUltimoIngreso,i.FechaUltimoConteo
            FROM dbo.Tbl_Inventario i
            JOIN dbo.Tbl_Productos p ON p.CodigoProducto=i.CodigoProducto
            JOIN dbo.Tbl_Categorias c ON c.CodigoCategoria=p.CodigoCategoria
            JOIN dbo.Tbl_Sucursales s ON s.CodigoSucursal=i.CodigoSucursal
            WHERE {Filtro}
            ORDER BY s.Nombre,p.Nombre,i.CodigoInventario
            OFFSET @Desplazamiento ROWS FETCH NEXT @Tamano ROWS ONLY;
            """, connection))
        {
            Filtros(command, buscar, sucursal, categoria, situacion);
            command.Parameters.Add("@Desplazamiento", SqlDbType.BigInt).Value = ((long)model.Pagina - 1) * model.TamanoPagina;
            command.Parameters.Add("@Tamano", SqlDbType.Int).Value = model.TamanoPagina;
            await using var reader = await command.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                model.Filas.Add(new InventarioFila(
                    reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2),
                    reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetString(6),
                    reader.GetDecimal(7), reader.GetDecimal(8), reader.GetDecimal(9),
                    reader.IsDBNull(10) ? null : reader.GetDateTime(10),
                    reader.IsDBNull(11) ? null : reader.GetDateTime(11)));
            }
        }

        await using (var command = new SqlCommand("""
            SELECT CodigoSucursal,Nombre FROM dbo.Tbl_Sucursales ORDER BY Nombre,CodigoSucursal;
            SELECT CodigoCategoria,Nombre FROM dbo.Tbl_Categorias ORDER BY Nombre,CodigoCategoria;
            """, connection))
        {
            await using var reader = await command.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct)) model.Sucursales.Add(new(reader.GetInt32(0), reader.GetString(1)));
            await reader.NextResultAsync(ct);
            while (await reader.ReadAsync(ct)) model.Categorias.Add(new(reader.GetInt32(0), reader.GetString(1)));
        }
        return model;
    }
}
