using System.Data;
using Microsoft.Data.SqlClient;
using SupermercadoMVC.Models;

namespace SupermercadoMVC.Data;

public sealed class SaleRuleException(string message) : Exception(message);

public sealed class SalesDataService(DatabaseConnectionFactory connections)
{
    private static SqlCommand Command(SqlConnection connection, string sql, SqlTransaction? transaction = null) =>
        new(sql, connection, transaction);
    private static void Int(SqlCommand command, string name, int value) => command.Parameters.Add(name, SqlDbType.Int).Value = value;
    private static void Money(SqlCommand command, string name, decimal value)
    {
        var parameter = command.Parameters.Add(name, SqlDbType.Decimal);
        parameter.Precision = 12;
        parameter.Scale = 2;
        parameter.Value = value;
    }
    private static async Task RollbackAsync(SqlTransaction transaction)
    {
        // SQL Server puede haber revertido ya un interbloqueo o cerrado la conexión.
        // Conservar la excepción original; al disponer la conexión no queda una transacción abierta.
        try { await transaction.RollbackAsync(CancellationToken.None); }
        catch (InvalidOperationException) { }
        catch (SqlException) { }
    }
    private static void Filters(SqlCommand command, string search, string state)
    {
        command.Parameters.Add("@Search", SqlDbType.VarChar, 300).Value = $"%{search}%";
        command.Parameters.Add("@State", SqlDbType.VarChar, 20).Value = state;
    }
    private const string Filter = "(@State='' OR v.Estado=@State) AND (v.NumeroVenta LIKE @Search OR c.Nombres LIKE @Search OR c.Apellidos LIKE @Search OR c.NIT LIKE @Search)";

    public async Task<SalesPage> GetPageAsync(string search, string state, int page, CancellationToken ct)
    {
        var model = new SalesPage { Search = search, State = state, Page = Math.Max(1, page) };
        await using var connection = connections.Create();
        await connection.OpenAsync(ct);
        await using (var command = Command(connection, $"""
            SELECT COUNT_BIG(*) FROM dbo.Tbl_Ventas v JOIN dbo.Tbl_Clientes c ON c.CodigoCliente=v.CodigoCliente WHERE {Filter};
            SELECT COUNT_BIG(CASE WHEN Estado='Borrador' THEN 1 END), COUNT_BIG(CASE WHEN Estado='Confirmada' THEN 1 END) FROM dbo.Tbl_Ventas;
            SELECT COALESCE(SUM(d.Cantidad*d.PrecioUnitario-d.Descuento),0)
            FROM dbo.Tbl_Ventas v JOIN dbo.Tbl_VentasDetalle d ON d.CodigoVenta=v.CodigoVenta WHERE v.Estado='Confirmada';
            """))
        {
            Filters(command, search, state);
            await using var reader = await command.ExecuteReaderAsync(ct);
            await reader.ReadAsync(ct); model.Total = reader.GetInt64(0);
            await reader.NextResultAsync(ct); await reader.ReadAsync(ct);
            model.Borradores = reader.GetInt64(0); model.Confirmadas = reader.GetInt64(1);
            await reader.NextResultAsync(ct); await reader.ReadAsync(ct); model.ImporteConfirmado = reader.GetDecimal(0);
        }
        model.Page = Math.Min(model.Page, model.PageCount);
        await using (var command = Command(connection, $"""
            SELECT v.CodigoVenta,v.NumeroVenta,v.FechaVenta,c.Nombres+' '+c.Apellidos,s.Nombre,e.Nombres+' '+e.Apellidos,v.Estado,
              COALESCE((SELECT SUM(d.Cantidad*d.PrecioUnitario-d.Descuento) FROM dbo.Tbl_VentasDetalle d WHERE d.CodigoVenta=v.CodigoVenta),0)
            FROM dbo.Tbl_Ventas v JOIN dbo.Tbl_Clientes c ON c.CodigoCliente=v.CodigoCliente
            JOIN dbo.Tbl_Sucursales s ON s.CodigoSucursal=v.CodigoSucursal
            JOIN dbo.Tbl_Empleados e ON e.CodigoEmpleado=v.CodigoEmpleado AND e.CodigoSucursal=v.CodigoSucursal
            WHERE {Filter} ORDER BY v.CodigoVenta DESC OFFSET @Offset ROWS FETCH NEXT @Size ROWS ONLY;
            """))
        {
            Filters(command, search, state); Int(command, "@Offset", (model.Page - 1) * model.PageSize); Int(command, "@Size", model.PageSize);
            var rows = new List<SaleRow>();
            await using var reader = await command.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct)) rows.Add(new(reader.GetInt32(0), reader.GetString(1), reader.GetDateTime(2), reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetString(6), reader.GetDecimal(7)));
            model.Rows = rows;
        }
        await LoadOptionsAsync(connection, model, ct);
        return model;
    }

    private static async Task LoadOptionsAsync(SqlConnection connection, SalesPage model, CancellationToken ct)
    {
        await using var command = Command(connection, """
            SELECT CodigoCliente, NIT+' · '+Nombres+' '+Apellidos FROM dbo.Tbl_Clientes ORDER BY Nombres,Apellidos,CodigoCliente;
            SELECT CodigoSucursal, Nombre FROM dbo.Tbl_Sucursales ORDER BY Nombre;
            SELECT CodigoEmpleado, Nombres+' '+Apellidos, CodigoSucursal FROM dbo.Tbl_Empleados ORDER BY Nombres,Apellidos;
            SELECT CodigoProducto, Nombre, PrecioVenta FROM dbo.Tbl_Productos ORDER BY Nombre;
            SELECT CodigoSucursal,CodigoProducto,Existencia,CostoPromedio FROM dbo.Tbl_Inventario;
            """);
        await using var reader = await command.ExecuteReaderAsync(ct);
        var clients = new List<SaleOption>(); while (await reader.ReadAsync(ct)) clients.Add(new(reader.GetInt32(0), reader.GetString(1))); model.Clientes = clients;
        await reader.NextResultAsync(ct);
        var branches = new List<SaleOption>(); while (await reader.ReadAsync(ct)) branches.Add(new(reader.GetInt32(0), reader.GetString(1))); model.Sucursales = branches;
        await reader.NextResultAsync(ct);
        var employees = new List<SaleEmployee>(); while (await reader.ReadAsync(ct)) employees.Add(new(reader.GetInt32(0), reader.GetString(1), reader.GetInt32(2))); model.Empleados = employees;
        await reader.NextResultAsync(ct);
        var products = new List<SaleProduct>(); while (await reader.ReadAsync(ct)) products.Add(new(reader.GetInt32(0), reader.GetString(1), reader.GetDecimal(2))); model.Productos = products;
        await reader.NextResultAsync(ct);
        var stock = new List<SaleStock>(); while (await reader.ReadAsync(ct)) stock.Add(new(reader.GetInt32(0), reader.GetInt32(1), reader.GetDecimal(2), reader.GetDecimal(3))); model.Inventario = stock;
    }

    public async Task<SaleDetail?> GetAsync(int id, CancellationToken ct)
    {
        await using var connection = connections.Create(); await connection.OpenAsync(ct);
        // Encabezado y líneas se leen bajo la misma transacción para evitar una vista parcial.
        using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
        var detail = await ReadAsync(connection, transaction, id, false, ct);
        await transaction.CommitAsync(ct);
        return detail;
    }

    private static async Task<SaleDetail?> ReadAsync(SqlConnection connection, SqlTransaction transaction, int id, bool lockForWrite, CancellationToken ct)
    {
        var hint = lockForWrite ? "WITH (UPDLOCK,HOLDLOCK)" : "";
        await using var command = Command(connection, $"""
            SELECT CodigoCliente,CodigoSucursal,CodigoEmpleado,NumeroVenta,FechaVenta,Observaciones,Estado
            FROM dbo.Tbl_Ventas {hint} WHERE CodigoVenta=@Id;
            SELECT CodigoProducto,Cantidad,PrecioUnitario,Descuento,CostoUnitarioHistorico
            FROM dbo.Tbl_VentasDetalle WHERE CodigoVenta=@Id ORDER BY CodigoProducto;
            """, transaction);
        Int(command, "@Id", id);
        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return null;
        var input = new SaleInput { CodigoVenta = id, CodigoCliente = reader.GetInt32(0), CodigoSucursal = reader.GetInt32(1), CodigoEmpleado = reader.GetInt32(2), NumeroVenta = reader.GetString(3), FechaVenta = reader.GetDateTime(4), Observaciones = reader.IsDBNull(5) ? null : reader.GetString(5) };
        var state = reader.GetString(6);
        await reader.NextResultAsync(ct);
        while (await reader.ReadAsync(ct)) input.Lineas.Add(new() { CodigoProducto = reader.GetInt32(0), Cantidad = reader.GetDecimal(1), PrecioUnitario = reader.GetDecimal(2), Descuento = reader.GetDecimal(3), CostoUnitarioHistorico = reader.GetDecimal(4) });
        return new(input, state);
    }

    private static async Task RequireReferenceAsync(SqlConnection connection, SqlTransaction transaction, string sql, int id, string error, CancellationToken ct, int? branch = null)
    {
        await using var command = Command(connection, sql, transaction); Int(command, "@Id", id);
        if (branch.HasValue) Int(command, "@Branch", branch.Value);
        if (Convert.ToInt64(await command.ExecuteScalarAsync(ct)) != 1) throw new SaleRuleException(error);
    }

    private static async Task<decimal> CostAsync(SqlConnection connection, SqlTransaction transaction, int branch, int product, CancellationToken ct)
    {
        await using var command = Command(connection, "SELECT CostoPromedio FROM dbo.Tbl_Inventario WITH (UPDLOCK,HOLDLOCK) WHERE CodigoSucursal=@Branch AND CodigoProducto=@Product;", transaction);
        Int(command, "@Branch", branch); Int(command, "@Product", product);
        var raw = await command.ExecuteScalarAsync(ct);
        if (raw is not decimal cost) throw new SaleRuleException($"El producto #{product} no tiene inventario en esta sucursal.");
        if (cost <= 0) throw new SaleRuleException($"El producto #{product} tiene CostoPromedio cero. No se puede guardar un costo histórico válido.");
        return cost;
    }

    public async Task<int> SaveAsync(SaleInput input, CancellationToken ct)
    {
        await using var connection = connections.Create(); await connection.OpenAsync(ct);
        using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
        try
        {
            if (input.CodigoVenta.HasValue)
            {
                var current = await ReadAsync(connection, transaction, input.CodigoVenta.Value, true, ct);
                if (current?.Estado != "Borrador") throw new SaleRuleException("La venta ya no existe o dejó de ser borrador. Solo se permite consultar confirmadas y anuladas.");
            }
            await RequireReferenceAsync(connection, transaction, "SELECT COUNT_BIG(*) FROM dbo.Tbl_Clientes WHERE CodigoCliente=@Id;", input.CodigoCliente!.Value, "El cliente no existe.", ct);
            await RequireReferenceAsync(connection, transaction, "SELECT COUNT_BIG(*) FROM dbo.Tbl_Sucursales WHERE CodigoSucursal=@Id;", input.CodigoSucursal!.Value, "La sucursal no existe.", ct);
            await RequireReferenceAsync(connection, transaction, "SELECT COUNT_BIG(*) FROM dbo.Tbl_Empleados WHERE CodigoEmpleado=@Id AND CodigoSucursal=@Branch;", input.CodigoEmpleado!.Value, "El empleado no pertenece a la sucursal seleccionada.", ct, input.CodigoSucursal);
            var costs = new Dictionary<int, decimal>();
            foreach (var line in input.Lineas.OrderBy(line => line.CodigoProducto))
            {
                await RequireReferenceAsync(connection, transaction, "SELECT COUNT_BIG(*) FROM dbo.Tbl_Productos WHERE CodigoProducto=@Id;", line.CodigoProducto!.Value, "Uno de los productos ya no existe.", ct);
                costs.Add(line.CodigoProducto.Value, await CostAsync(connection, transaction, input.CodigoSucursal.Value, line.CodigoProducto.Value, ct));
            }
            int id;
            await using (var command = Command(connection, input.CodigoVenta.HasValue ? """
                UPDATE dbo.Tbl_Ventas SET CodigoCliente=@Client,CodigoSucursal=@Branch,CodigoEmpleado=@Employee,NumeroVenta=@Number,FechaVenta=@Date,Observaciones=@Notes WHERE CodigoVenta=@Id AND Estado='Borrador';
                SELECT @Id;
                """ : """
                INSERT INTO dbo.Tbl_Ventas(CodigoCliente,CodigoSucursal,CodigoEmpleado,NumeroVenta,FechaVenta,Estado,Observaciones)
                OUTPUT INSERTED.CodigoVenta VALUES(@Client,@Branch,@Employee,@Number,@Date,'Borrador',@Notes);
                """, transaction))
            {
                Int(command, "@Client", input.CodigoCliente.Value); Int(command, "@Branch", input.CodigoSucursal.Value); Int(command, "@Employee", input.CodigoEmpleado.Value);
                command.Parameters.Add("@Number", SqlDbType.VarChar, 30).Value = input.NumeroVenta.Trim();
                command.Parameters.Add("@Date", SqlDbType.Date).Value = input.FechaVenta!.Value.Date;
                command.Parameters.Add("@Notes", SqlDbType.VarChar, 300).Value = string.IsNullOrWhiteSpace(input.Observaciones) ? DBNull.Value : input.Observaciones.Trim();
                if (input.CodigoVenta.HasValue) Int(command, "@Id", input.CodigoVenta.Value);
                id = Convert.ToInt32(await command.ExecuteScalarAsync(ct));
            }
            if (input.CodigoVenta.HasValue)
            {
                await using var remove = Command(connection, "DELETE FROM dbo.Tbl_VentasDetalle WHERE CodigoVenta=@Id;", transaction);
                Int(remove, "@Id", id); await remove.ExecuteNonQueryAsync(ct);
            }
            foreach (var line in input.Lineas)
            {
                await using var command = Command(connection, """
                    INSERT INTO dbo.Tbl_VentasDetalle(CodigoVenta,CodigoProducto,Cantidad,PrecioUnitario,Descuento,CostoUnitarioHistorico)
                    VALUES(@Id,@Product,@Quantity,@Price,@Discount,@Cost);
                    """, transaction);
                Int(command, "@Id", id); Int(command, "@Product", line.CodigoProducto!.Value);
                Money(command, "@Quantity", line.Cantidad!.Value); Money(command, "@Price", line.PrecioUnitario!.Value); Money(command, "@Discount", line.Descuento!.Value); Money(command, "@Cost", costs[line.CodigoProducto.Value]);
                await command.ExecuteNonQueryAsync(ct);
            }
            await transaction.CommitAsync(ct); return id;
        }
        catch { await RollbackAsync(transaction); throw; }
    }

    public async Task ConfirmAsync(int id, CancellationToken ct)
    {
        await using var connection = connections.Create(); await connection.OpenAsync(ct);
        using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
        try
        {
            // Todos los mutadores bloquean primero el encabezado; una segunda confirmación verá el nuevo estado.
            var sale = await ReadAsync(connection, transaction, id, true, ct);
            if (sale?.Estado != "Borrador") throw new SaleRuleException("La venta ya no es borrador o no existe. No se descontó inventario nuevamente.");
            if (sale.Input.Lineas.Count == 0) throw new SaleRuleException("Agrega al menos un detalle antes de confirmar.");
            // Orden común entre ventas para reducir interbloqueos sobre inventario.
            foreach (var line in sale.Input.Lineas.OrderBy(line => line.CodigoProducto))
            {
                var cost = await CostAsync(connection, transaction, sale.Input.CodigoSucursal!.Value, line.CodigoProducto!.Value, ct);
                await using var command = Command(connection, """
                    UPDATE dbo.Tbl_Inventario SET Existencia=Existencia-@Quantity
                    WHERE CodigoSucursal=@Branch AND CodigoProducto=@Product AND Existencia>=@Quantity;
                    """, transaction);
                Int(command, "@Branch", sale.Input.CodigoSucursal.Value); Int(command, "@Product", line.CodigoProducto.Value); Money(command, "@Quantity", line.Cantidad!.Value);
                if (await command.ExecuteNonQueryAsync(ct) != 1) throw new SaleRuleException($"Existencia insuficiente para el producto #{line.CodigoProducto}. No se confirmó la venta.");
                await using var history = Command(connection, "UPDATE dbo.Tbl_VentasDetalle SET CostoUnitarioHistorico=@Cost WHERE CodigoVenta=@Id AND CodigoProducto=@Product;", transaction);
                Int(history, "@Id", id); Int(history, "@Product", line.CodigoProducto.Value); Money(history, "@Cost", cost);
                if (await history.ExecuteNonQueryAsync(ct) != 1) throw new SaleRuleException("El detalle cambió. Vuelve a consultar la venta.");
            }
            await using var state = Command(connection, "UPDATE dbo.Tbl_Ventas SET Estado='Confirmada' WHERE CodigoVenta=@Id AND Estado='Borrador';", transaction);
            Int(state, "@Id", id);
            if (await state.ExecuteNonQueryAsync(ct) != 1) throw new SaleRuleException("El estado de la venta cambió. Vuelve a consultarla.");
            await transaction.CommitAsync(ct);
        }
        catch { await RollbackAsync(transaction); throw; }
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        await using var connection = connections.Create(); await connection.OpenAsync(ct);
        using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
        try
        {
            var sale = await ReadAsync(connection, transaction, id, true, ct);
            if (sale?.Estado != "Borrador") throw new SaleRuleException("Solo pueden eliminarse ventas en borrador.");
            await using var command = Command(connection, """
                DELETE FROM dbo.Tbl_VentasDetalle WHERE CodigoVenta=@Id;
                DELETE FROM dbo.Tbl_Ventas WHERE CodigoVenta=@Id AND Estado='Borrador';
                """, transaction);
            Int(command, "@Id", id); await command.ExecuteNonQueryAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch { await RollbackAsync(transaction); throw; }
    }
}
