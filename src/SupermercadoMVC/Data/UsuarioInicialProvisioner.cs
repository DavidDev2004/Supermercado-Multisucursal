using System.Data;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using SupermercadoMVC.Models;

namespace SupermercadoMVC.Data;

// Adaptado de UsuarioTestProvisioner de la integrante. Solo se ejecuta desde
// la terminal con --crear-usuario-inicial después de aplicar la migración.
public sealed class UsuarioInicialProvisioner(
    DatabaseConnectionFactory connections,
    IPasswordHasher<UsuarioAuth> hasher)
{
    public async Task CrearAsync(CancellationToken ct = default)
    {
        if (Console.IsInputRedirected) throw new InvalidOperationException("Crea el usuario desde una terminal interactiva.");
        Console.Write("Usuario inicial (máximo 50 caracteres): ");
        var nombre = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(nombre) || nombre.Length > 50)
            throw new InvalidOperationException("El nombre de usuario es obligatorio y admite hasta 50 caracteres.");

        Console.Write("Contraseña (mínimo 12 caracteres, entrada oculta): ");
        var password = LeerSecreto();
        Console.Write("Confirma la contraseña (entrada oculta): ");
        var confirmacion = LeerSecreto();
        if (password.Length < 12 || password != confirmacion)
            throw new InvalidOperationException("La contraseña debe tener al menos 12 caracteres y coincidir con la confirmación.");

        await using var connection = connections.Create();
        await connection.OpenAsync(ct);
        await using (var check = new SqlCommand("SELECT COUNT_BIG(*) FROM dbo.Tbl_Usuarios WHERE Usuario=@Usuario;", connection))
        {
            check.Parameters.Add("@Usuario", SqlDbType.NVarChar, 50).Value = nombre;
            if (Convert.ToInt64(await check.ExecuteScalarAsync(ct)) != 0)
                throw new InvalidOperationException("El nombre de usuario ya existe.");
        }

        var user = new UsuarioAuth { Usuario = nombre };
        var hash = hasher.HashPassword(user, password);
        await using var insert = new SqlCommand("INSERT INTO dbo.Tbl_Usuarios(Usuario,PasswordHash) VALUES(@Usuario,@PasswordHash);", connection);
        insert.Parameters.Add("@Usuario", SqlDbType.NVarChar, 50).Value = nombre;
        insert.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, -1).Value = hash;
        try
        {
            await insert.ExecuteNonQueryAsync(ct);
            Console.WriteLine("Usuario creado. La contraseña no se mostró ni se guardó en archivos.");
        }
        catch (SqlException e) when (e.Number is 2601 or 2627)
        {
            throw new InvalidOperationException("El nombre de usuario ya existe.");
        }
        finally
        {
            password = string.Empty;
            confirmacion = string.Empty;
        }
    }

    private static string LeerSecreto()
    {
        var buffer = new StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter) { Console.WriteLine(); return buffer.ToString(); }
            if (key.Key == ConsoleKey.Backspace) { if (buffer.Length > 0) buffer.Length--; continue; }
            if (!char.IsControl(key.KeyChar)) buffer.Append(key.KeyChar);
        }
    }
}
