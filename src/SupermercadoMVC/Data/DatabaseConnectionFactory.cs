using Microsoft.Data.SqlClient;

namespace SupermercadoMVC.Data;

public sealed class DatabaseConnectionFactory(IConfiguration configuration)
{
    public SqlConnection Create()
    {
        var connectionString = configuration.GetConnectionString("Supermercado");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Falta configurar la conexión SQL Server en User Secrets.");
        }

        return new SqlConnection(connectionString);
    }
}
