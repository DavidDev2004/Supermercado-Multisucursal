using Microsoft.Data.SqlClient;
using SupermercadoMVC.Models;
using System.Data;
using System.Globalization;

namespace SupermercadoMVC.Data;

public sealed class CatalogDataService(DatabaseConnectionFactory connections)
{
    public async Task<IReadOnlyList<SucursalOption>> GetSucursalesAsync(CancellationToken cancellationToken)
    {
        await using var connection = connections.Create();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT CodigoSucursal, Nombre FROM dbo.Tbl_Sucursales ORDER BY Nombre, CodigoSucursal;";
        var items = new List<SucursalOption>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken)) items.Add(new(reader.GetInt32(0), reader.GetString(1)));
        return items;
    }

    public async Task<CatalogPageData> GetPageAsync(
        CatalogDefinition definition, string? search, string? state, int page, int pageSize, CancellationToken cancellationToken)
    {
        await using var connection = connections.Create();
        await connection.OpenAsync(cancellationToken);

        long total;
        long active;
        long inactive;
        await using (var counts = connection.CreateCommand())
        {
            var searchPredicate = string.Join(" OR ", definition.SearchColumns.Select(column => $"[{column}] LIKE @Pattern"));
            counts.CommandText = $"""
                SELECT COUNT_BIG(*),
                       COALESCE(SUM(CASE WHEN [Estado] = 'Activo' THEN CONVERT(bigint, 1) ELSE CONVERT(bigint, 0) END), 0),
                       COALESCE(SUM(CASE WHEN [Estado] = 'Inactivo' THEN CONVERT(bigint, 1) ELSE CONVERT(bigint, 0) END), 0)
                FROM {definition.Table};
                SELECT COUNT_BIG(*)
                FROM {definition.Table}
                WHERE (@Pattern IS NULL OR ({searchPredicate}))
                  AND (@State = '' OR [Estado] = @State);
                """;
            AddSearchParameters(counts, search, state);
            await using var reader = await counts.ExecuteReaderAsync(cancellationToken);
            await reader.ReadAsync(cancellationToken);
            var all = reader.GetInt64(0);
            active = reader.GetInt64(1);
            inactive = reader.GetInt64(2);
            await reader.NextResultAsync(cancellationToken);
            await reader.ReadAsync(cancellationToken);
            total = reader.GetInt64(0);
            _ = all;
        }

        var columns = string.Join(", ", definition.SelectColumns.Select(column => $"[{column}]"));
        var filter = $"(@Pattern IS NULL OR ({string.Join(" OR ", definition.SearchColumns.Select(column => $"[{column}] LIKE @Pattern"))})) AND (@State = '' OR [Estado] = @State)";
        var rows = new List<Dictionary<string, object?>>();
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = $"SELECT {columns} FROM {definition.Table} WHERE {filter} ORDER BY [{definition.IdColumn}] DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";
            AddSearchParameters(command, search, state);
            command.Parameters.Add("@Offset", SqlDbType.Int).Value = (page - 1) * pageSize;
            command.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                for (var index = 0; index < reader.FieldCount; index++)
                {
                    row[reader.GetName(index)] = await reader.IsDBNullAsync(index, cancellationToken) ? null : reader.GetValue(index);
                }
                rows.Add(row);
            }
        }

        return new CatalogPageData(rows, total, active, inactive);
    }

    public async Task CreateAsync(CatalogDefinition definition, CatalogInput input, CancellationToken cancellationToken)
    {
        var names = string.Join(", ", definition.Fields.Select(field => $"[{field.Name}]"));
        var parameters = string.Join(", ", definition.Fields.Select(field => $"@{field.Name}"));
        await using var connection = connections.Create();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = $"INSERT INTO {definition.Table} ({names}) VALUES ({parameters});";
        AddFieldParameters(command, definition, input);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<bool> UpdateAsync(CatalogDefinition definition, int id, CatalogInput input, CancellationToken cancellationToken)
    {
        var assignments = string.Join(", ", definition.Fields.Select(field => $"[{field.Name}] = @{field.Name}"));
        await using var connection = connections.Create();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = $"UPDATE {definition.Table} SET {assignments} WHERE [{definition.IdColumn}] = @Id;";
        AddFieldParameters(command, definition, input);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    public async Task<bool> DeleteAsync(CatalogDefinition definition, int id, CancellationToken cancellationToken)
    {
        await using var connection = connections.Create();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = $"DELETE FROM {definition.Table} WHERE [{definition.IdColumn}] = @Id;";
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    private static void AddSearchParameters(SqlCommand command, string? search, string? state)
    {
        var pattern = string.IsNullOrWhiteSpace(search) ? null : $"%{search.Trim()}%";
        command.Parameters.Add("@Pattern", SqlDbType.VarChar, 300).Value = pattern is null ? DBNull.Value : pattern;
        command.Parameters.Add("@State", SqlDbType.VarChar, 20).Value = state ?? string.Empty;
    }

    private static void AddFieldParameters(SqlCommand command, CatalogDefinition definition, CatalogInput input)
    {
        foreach (var field in definition.Fields)
        {
            input.Fields.TryGetValue(field.Name, out var raw);
            var parameter = field.Type == SqlDbType.VarChar
                ? command.Parameters.Add($"@{field.Name}", field.Type, field.Size)
                : command.Parameters.Add($"@{field.Name}", field.Type);
            if (field.Type == SqlDbType.Decimal)
            {
                parameter.Precision = 12;
                parameter.Scale = 2;
            }
            parameter.Value = ToSqlValue(field, raw);
        }
    }

    private static object ToSqlValue(CatalogField field, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return field.Required ? string.Empty : DBNull.Value;
        }

        return field.Type switch
        {
            SqlDbType.VarChar => value.Trim(),
            SqlDbType.Int => int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture),
            SqlDbType.Date => DateTime.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None),
            SqlDbType.Decimal => decimal.Parse(value, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture),
            _ => throw new InvalidOperationException("Tipo de campo no admitido para este catálogo.")
        };
    }
}
