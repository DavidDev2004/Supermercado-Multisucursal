using System.Data;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using SupermercadoMVC.Models;
using SupermercadoMVC.Security;

namespace SupermercadoMVC.Data;

// Comando explícito de consola. Recibe las claves por stdin; nunca las imprime.
public sealed class DemoUsersProvisioner(
    DatabaseConnectionFactory connections,
    IPasswordHasher<UsuarioAuth> hasher)
{
    private sealed class Passwords
    {
        public string Bodega { get; set; } = "";
        public string Caja { get; set; } = "";
    }

    public async Task CrearAsync(string expectedDatabase, CancellationToken ct = default)
    {
        if (expectedDatabase != "SupermercadosGuate" &&
            !expectedDatabase.StartsWith("SupermercadosGuate_ROLES_PRUEBAS_", StringComparison.Ordinal))
            throw new InvalidOperationException("Destino SQL no permitido para cuentas de demostración.");

        var input = await Console.In.ReadToEndAsync(ct);
        var passwords = JsonSerializer.Deserialize<Passwords>(input)
            ?? throw new InvalidOperationException("Faltan las claves de demostración.");
        if (passwords.Bodega.Length < 24 || passwords.Caja.Length < 24 || passwords.Bodega == passwords.Caja)
            throw new InvalidOperationException("Las claves deben ser distintas y tener al menos 24 caracteres.");

        await using var connection = connections.Create();
        await connection.OpenAsync(ct);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        try
        {
            await using (var current = new SqlCommand("SELECT DB_NAME();", connection, transaction))
            {
                if (Convert.ToString(await current.ExecuteScalarAsync(ct)) != expectedDatabase)
                    throw new InvalidOperationException("La base conectada no coincide con la base esperada.");
            }
            await using (var check = new SqlCommand("""
                SELECT COUNT(*) FROM dbo.Tbl_Usuarios WITH (UPDLOCK, HOLDLOCK)
                WHERE Usuario IN (N'David', N'BodegaDemo', N'CajaDemo');
                """, connection, transaction))
            {
                if (Convert.ToInt32(await check.ExecuteScalarAsync(ct)) != 1)
                    throw new InvalidOperationException("Se esperaba únicamente la cuenta David; no se sobrescribirán usuarios existentes.");
            }
            await InsertAsync("BodegaDemo", RolePermissions.Bodega, passwords.Bodega, connection, transaction, ct);
            await InsertAsync("CajaDemo", RolePermissions.Caja, passwords.Caja, connection, transaction, ct);
            await transaction.CommitAsync(ct);
            Console.WriteLine("Cuentas de demostración creadas; claves no mostradas.");
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    private async Task InsertAsync(string name, string role, string password,
        SqlConnection connection, SqlTransaction transaction, CancellationToken ct)
    {
        var hash = hasher.HashPassword(new UsuarioAuth { Usuario = name, Rol = role }, password);
        await using var command = new SqlCommand("""
            INSERT INTO dbo.Tbl_Usuarios (Usuario, PasswordHash, Estado, Rol)
            VALUES (@Usuario, @PasswordHash, N'Activo', @Rol);
            """, connection, transaction);
        command.Parameters.Add("@Usuario", SqlDbType.NVarChar, 50).Value = name;
        command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, -1).Value = hash;
        command.Parameters.Add("@Rol", SqlDbType.NVarChar, 20).Value = role;
        if (await command.ExecuteNonQueryAsync(ct) != 1)
            throw new InvalidOperationException("No se pudo crear una cuenta de demostración.");
    }
}
