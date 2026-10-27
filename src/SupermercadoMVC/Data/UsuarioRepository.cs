using System.Data;
using Microsoft.Data.SqlClient;
using SupermercadoMVC.Models;

namespace SupermercadoMVC.Data;

// Adaptado de UsuarioRepository de la integrante; conserva SQL parametrizado.
public sealed class UsuarioRepository(DatabaseConnectionFactory connections)
{
    public async Task<UsuarioAuth?> ObtenerActivoPorNombreAsync(string nombre, CancellationToken ct)
    {
        await using var connection = connections.Create();
        await connection.OpenAsync(ct);
        await using var command = new SqlCommand("""
            SELECT CodigoUsuario, Usuario, PasswordHash
            FROM dbo.Tbl_Usuarios WHERE Usuario=@Usuario AND Estado=N'Activo';
            """, connection);
        command.Parameters.Add("@Usuario", SqlDbType.NVarChar, 50).Value = nombre;
        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, ct);
        if (!await reader.ReadAsync(ct)) return null;
        return new UsuarioAuth { CodigoUsuario = reader.GetInt32(0), Usuario = reader.GetString(1), PasswordHash = reader.GetString(2) };
    }

    public async Task<bool> ActualizarHashSiNoCambioAsync(UsuarioAuth usuario, string hashNuevo, CancellationToken ct)
    {
        await using var connection = connections.Create();
        await connection.OpenAsync(ct);
        await using var command = new SqlCommand("""
            UPDATE dbo.Tbl_Usuarios SET PasswordHash=@Nuevo
            WHERE CodigoUsuario=@Id AND PasswordHash=@Anterior AND Estado=N'Activo';
            """, connection);
        command.Parameters.Add("@Nuevo", SqlDbType.NVarChar, -1).Value = hashNuevo;
        command.Parameters.Add("@Anterior", SqlDbType.NVarChar, -1).Value = usuario.PasswordHash;
        command.Parameters.Add("@Id", SqlDbType.Int).Value = usuario.CodigoUsuario;
        return await command.ExecuteNonQueryAsync(ct) == 1;
    }
}
