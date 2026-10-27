using System.ComponentModel.DataAnnotations;
using System.Data;
using Microsoft.Data.SqlClient;
using SupermercadoMVC.Models;

namespace SupermercadoMVC.Data;

public sealed class AjusteRuleException(string message) : Exception(message);

public sealed class AjustesDataService(DatabaseConnectionFactory connections)
{
    private static SqlCommand Command(SqlConnection c, string sql, SqlTransaction? tx = null) => new(sql, c, tx);
    private static void Int(SqlCommand c, string name, int? value) =>
        c.Parameters.Add(name, SqlDbType.Int).Value = value.HasValue ? value.Value : DBNull.Value;
    private static void Text(SqlCommand c, string name, string? value, int length) =>
        c.Parameters.Add(name, SqlDbType.VarChar, length).Value = value is null ? DBNull.Value : value;
    private static void Number(SqlCommand c, string name, decimal value)
    {
        var p = c.Parameters.Add(name, SqlDbType.Decimal); p.Precision = 12; p.Scale = 2; p.Value = value;
    }
    private static async Task RollbackAsync(SqlTransaction tx)
    {
        try { await tx.RollbackAsync(CancellationToken.None); }
        catch (SqlException) { }
        catch (InvalidOperationException) { }
    }
    private const string Filter = """
        (@Estado='' OR a.Estado=@Estado) AND (@Sucursal IS NULL OR a.CodigoSucursal=@Sucursal)
        AND (a.NumeroAjuste LIKE @Buscar OR a.Motivo LIKE @Buscar OR s.Nombre LIKE @Buscar
             OR e.Nombres+' '+e.Apellidos LIKE @Buscar)
        """;
    private static void Filters(SqlCommand c, string search, string state, int? branch)
    {
        Text(c, "@Buscar", $"%{search}%", 202); Text(c, "@Estado", state, 20); Int(c, "@Sucursal", branch);
    }

    public async Task<AjustesIndexViewModel> BuscarAsync(string search, string state, int? branch, int page, CancellationToken ct)
    {
        var model = new AjustesIndexViewModel { Search = search, State = state, Branch = branch, Page = Math.Max(1, page) };
        await using var c = connections.Create(); await c.OpenAsync(ct);
        await using (var command = Command(c, $"""
            SELECT COUNT_BIG(*) FROM dbo.Tbl_AjustesInventario a
            JOIN dbo.Tbl_Sucursales s ON s.CodigoSucursal=a.CodigoSucursal
            JOIN dbo.Tbl_Empleados e ON e.CodigoEmpleado=a.CodigoEmpleado AND e.CodigoSucursal=a.CodigoSucursal
            WHERE {Filter};
            SELECT COUNT_BIG(*), COUNT_BIG(CASE WHEN Estado='Borrador' THEN 1 END),
                   COUNT_BIG(CASE WHEN Estado='Aplicado' THEN 1 END), COUNT_BIG(CASE WHEN Estado='Anulado' THEN 1 END)
            FROM dbo.Tbl_AjustesInventario;
            """))
        {
            Filters(command, search, state, branch);
            await using var r = await command.ExecuteReaderAsync(ct);
            await r.ReadAsync(ct); model.Total = r.GetInt64(0);
            await r.NextResultAsync(ct); await r.ReadAsync(ct);
            model.TotalAjustes = r.GetInt64(0); model.Borradores = r.GetInt64(1);
            model.Aplicados = r.GetInt64(2); model.Anulados = r.GetInt64(3);
        }
        model.Page = Math.Min(model.Page, model.PageCount);
        await using (var command = Command(c, $"""
            SELECT a.CodigoAjuste,a.NumeroAjuste,a.FechaAjuste,s.Nombre,e.Nombres+' '+e.Apellidos,
                   a.Motivo,a.Estado,d.Lineas,d.Valor
            FROM dbo.Tbl_AjustesInventario a
            JOIN dbo.Tbl_Sucursales s ON s.CodigoSucursal=a.CodigoSucursal
            JOIN dbo.Tbl_Empleados e ON e.CodigoEmpleado=a.CodigoEmpleado AND e.CodigoSucursal=a.CodigoSucursal
            OUTER APPLY (SELECT COUNT(*) AS Lineas,
                CASE WHEN COUNT(*)>0 AND COUNT(CostoUnitario)=COUNT(*) THEN SUM(Cantidad*CostoUnitario) ELSE NULL END AS Valor
                FROM dbo.Tbl_AjustesInventarioDetalle WHERE CodigoAjuste=a.CodigoAjuste) d
            WHERE {Filter} ORDER BY a.FechaAjuste DESC,a.CodigoAjuste DESC
            OFFSET @Offset ROWS FETCH NEXT @Size ROWS ONLY;
            """))
        {
            Filters(command, search, state, branch);
            command.Parameters.Add("@Offset", SqlDbType.BigInt).Value = ((long)model.Page - 1) * model.PageSize;
            Int(command, "@Size", model.PageSize);
            var rows = new List<AjusteRow>(); await using var r = await command.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct)) rows.Add(new(r.GetInt32(0), r.GetString(1), r.GetDateTime(2),
                r.GetString(3), r.GetString(4), r.GetString(5), r.GetString(6), r.GetInt32(7), r.IsDBNull(8) ? null : r.GetDecimal(8)));
            model.Rows = rows;
        }
        await using (var command = Command(c, """
            SELECT CodigoSucursal,Nombre FROM dbo.Tbl_Sucursales ORDER BY Nombre,CodigoSucursal;
            SELECT CodigoEmpleado,Nombres+' '+Apellidos,CodigoSucursal FROM dbo.Tbl_Empleados ORDER BY Nombres,Apellidos;
            SELECT CodigoProducto,CodigoBarra+' · '+Nombre FROM dbo.Tbl_Productos ORDER BY Nombre,CodigoProducto;
            """))
        {
            await using var r = await command.ExecuteReaderAsync(ct);
            var branches = new List<AjusteOption>(); while (await r.ReadAsync(ct)) branches.Add(new(r.GetInt32(0), r.GetString(1))); model.Sucursales = branches;
            await r.NextResultAsync(ct);
            var employees = new List<AjusteEmployee>(); while (await r.ReadAsync(ct)) employees.Add(new(r.GetInt32(0), r.GetString(1), r.GetInt32(2))); model.Empleados = employees;
            await r.NextResultAsync(ct);
            var products = new List<AjusteOption>(); while (await r.ReadAsync(ct)) products.Add(new(r.GetInt32(0), r.GetString(1))); model.Productos = products;
        }
        return model;
    }

    public async Task<AjusteDetail?> ObtenerAsync(int id, CancellationToken ct)
    {
        await using var c = connections.Create(); await c.OpenAsync(ct);
        using var tx = c.BeginTransaction(IsolationLevel.Serializable);
        var result = await LeerAsync(c, tx, id, false, ct); await tx.CommitAsync(ct); return result;
    }
    private static async Task<AjusteDetail?> LeerAsync(SqlConnection c, SqlTransaction tx, int id, bool write, CancellationToken ct)
    {
        var hint = write ? "WITH (UPDLOCK,HOLDLOCK)" : "";
        await using var command = Command(c, $"""
            SELECT CodigoSucursal,CodigoEmpleado,NumeroAjuste,FechaAjuste,Motivo,Estado,Observaciones
            FROM dbo.Tbl_AjustesInventario {hint} WHERE CodigoAjuste=@Id;
            SELECT CodigoProducto,Cantidad,CostoUnitario FROM dbo.Tbl_AjustesInventarioDetalle
            WHERE CodigoAjuste=@Id ORDER BY CodigoProducto;
            """, tx);
        Int(command, "@Id", id); await using var r = await command.ExecuteReaderAsync(ct);
        if (!await r.ReadAsync(ct)) return null;
        var input = new AjusteFormViewModel { CodigoAjuste = id, CodigoSucursal = r.GetInt32(0), CodigoEmpleado = r.GetInt32(1),
            NumeroAjuste = r.GetString(2), FechaAjuste = r.GetDateTime(3), Motivo = r.GetString(4), Observaciones = r.IsDBNull(6) ? null : r.GetString(6) };
        var state = r.GetString(5); await r.NextResultAsync(ct);
        while (await r.ReadAsync(ct)) input.Lineas.Add(new() { CodigoProducto = r.GetInt32(0), Cantidad = r.GetDecimal(1), CostoUnitario = r.IsDBNull(2) ? null : r.GetDecimal(2) });
        return new(input, state);
    }
    private static void Validar(AjusteFormViewModel form)
    {
        var errors = new List<ValidationResult>();
        Validator.TryValidateObject(form, new(form), errors, true);
        if (errors.Count > 0) throw new AjusteRuleException(errors[0].ErrorMessage ?? "Revisa los datos del ajuste.");
        foreach (var line in form.Lineas)
            if (line is not null) Validator.TryValidateObject(line, new(line), errors, true);
        if (errors.Count > 0) throw new AjusteRuleException(errors[0].ErrorMessage ?? "Revisa los datos del ajuste.");
    }
    private static async Task ReferenciasAsync(SqlConnection c, SqlTransaction tx, AjusteFormViewModel form, CancellationToken ct)
    {
        await using var command = Command(c, """
            SELECT COUNT(*) FROM dbo.Tbl_Sucursales WHERE CodigoSucursal=@Sucursal;
            SELECT COUNT(*) FROM dbo.Tbl_Empleados WHERE CodigoEmpleado=@Empleado AND CodigoSucursal=@Sucursal;
            """, tx);
        Int(command, "@Sucursal", form.CodigoSucursal); Int(command, "@Empleado", form.CodigoEmpleado);
        await using (var r = await command.ExecuteReaderAsync(ct))
        {
            await r.ReadAsync(ct); if (r.GetInt32(0) != 1) throw new AjusteRuleException("La sucursal no existe.");
            await r.NextResultAsync(ct); await r.ReadAsync(ct);
            if (r.GetInt32(0) != 1) throw new AjusteRuleException("El empleado no pertenece a la sucursal seleccionada.");
        }
        foreach (var line in form.Lineas.OrderBy(l => l.CodigoProducto))
        {
            await using var product = Command(c, "SELECT COUNT(*) FROM dbo.Tbl_Productos WHERE CodigoProducto=@Producto;", tx);
            Int(product, "@Producto", line.CodigoProducto);
            if (Convert.ToInt32(await product.ExecuteScalarAsync(ct)) != 1) throw new AjusteRuleException("Uno de los productos no existe.");
        }
    }

    public async Task<int> GuardarAsync(AjusteFormViewModel form, CancellationToken ct)
    {
        Validar(form);
        await using var c = connections.Create(); await c.OpenAsync(ct);
        using var tx = c.BeginTransaction(IsolationLevel.Serializable);
        try
        {
            if (form.CodigoAjuste.HasValue && (await LeerAsync(c, tx, form.CodigoAjuste.Value, true, ct))?.Estado != "Borrador")
                throw new AjusteRuleException("Solo pueden editarse ajustes en Borrador. El ajuste ya no existe o cambió de estado.");
            await ReferenciasAsync(c, tx, form, ct);
            int id;
            await using (var command = Command(c, form.CodigoAjuste.HasValue ? """
                UPDATE dbo.Tbl_AjustesInventario SET CodigoSucursal=@Sucursal,CodigoEmpleado=@Empleado,
                    NumeroAjuste=@Numero,FechaAjuste=@Fecha,Motivo=@Motivo,Observaciones=@Notas
                OUTPUT INSERTED.CodigoAjuste WHERE CodigoAjuste=@Id AND Estado='Borrador';
                """ : """
                INSERT dbo.Tbl_AjustesInventario(CodigoSucursal,CodigoEmpleado,NumeroAjuste,FechaAjuste,Motivo,Estado,Observaciones)
                OUTPUT INSERTED.CodigoAjuste VALUES(@Sucursal,@Empleado,@Numero,@Fecha,@Motivo,'Borrador',@Notas);
                """, tx))
            {
                Int(command, "@Sucursal", form.CodigoSucursal); Int(command, "@Empleado", form.CodigoEmpleado);
                Text(command, "@Numero", form.NumeroAjuste.Trim(), 30); Text(command, "@Motivo", form.Motivo.Trim(), 200);
                Text(command, "@Notas", form.Observaciones, 300);
                command.Parameters.Add("@Fecha", SqlDbType.Date).Value = form.FechaAjuste!.Value.Date;
                if (form.CodigoAjuste.HasValue) Int(command, "@Id", form.CodigoAjuste.Value);
                var value = await command.ExecuteScalarAsync(ct);
                if (value is null) throw new AjusteRuleException("El ajuste cambió de estado; no se guardaron cambios.");
                id = Convert.ToInt32(value);
            }
            if (form.CodigoAjuste.HasValue)
            {
                await using var remove = Command(c, "DELETE FROM dbo.Tbl_AjustesInventarioDetalle WHERE CodigoAjuste=@Id;", tx);
                Int(remove, "@Id", id); await remove.ExecuteNonQueryAsync(ct);
            }
            foreach (var line in form.Lineas)
            {
                await using var insert = Command(c, """
                    INSERT dbo.Tbl_AjustesInventarioDetalle(CodigoAjuste,CodigoProducto,Cantidad,CostoUnitario)
                    VALUES(@Id,@Producto,@Cantidad,NULL);
                    """, tx);
                Int(insert, "@Id", id); Int(insert, "@Producto", line.CodigoProducto); Number(insert, "@Cantidad", line.Cantidad!.Value);
                await insert.ExecuteNonQueryAsync(ct);
            }
            await tx.CommitAsync(ct); return id;
        }
        catch { await RollbackAsync(tx); throw; }
    }

    public async Task AplicarAsync(int id, CancellationToken ct)
    {
        await using var c = connections.Create(); await c.OpenAsync(ct);
        using var tx = c.BeginTransaction(IsolationLevel.Serializable);
        try
        {
            // Bloquear siempre primero el encabezado impide aplicar dos veces, incluso en paralelo.
            var adjustment = await LeerAsync(c, tx, id, true, ct);
            if (adjustment?.Estado != "Borrador") throw new AjusteRuleException("El ajuste ya no es Borrador o no existe. El inventario no se volvió a modificar.");
            var form = adjustment.Input; Validar(form);
            if (form.Lineas.Count == 0) throw new AjusteRuleException("Agrega al menos un producto antes de aplicar el ajuste.");
            await ReferenciasAsync(c, tx, form, ct);
            // El mismo orden de productos que Ventas y Compras reduce conflictos de bloqueo.
            foreach (var line in form.Lineas.OrderBy(l => l.CodigoProducto))
            {
                decimal stock, cost;
                await using (var inventory = Command(c, """
                    SELECT Existencia,CostoPromedio FROM dbo.Tbl_Inventario WITH (UPDLOCK,HOLDLOCK)
                    WHERE CodigoSucursal=@Sucursal AND CodigoProducto=@Producto;
                    """, tx))
                {
                    Int(inventory, "@Sucursal", form.CodigoSucursal); Int(inventory, "@Producto", line.CodigoProducto);
                    await using var r = await inventory.ExecuteReaderAsync(ct);
                    if (!await r.ReadAsync(ct)) throw new AjusteRuleException($"El producto #{line.CodigoProducto} no tiene inventario en esta sucursal. No se crea inventario automáticamente.");
                    stock = r.GetDecimal(0); cost = r.GetDecimal(1);
                }
                if (cost <= 0) throw new AjusteRuleException($"El producto #{line.CodigoProducto} tiene costo promedio cero. Se necesita un costo histórico positivo; no se inventaron valores.");
                var newStock = stock + line.Cantidad!.Value;
                if (newStock < 0) throw new AjusteRuleException($"El producto #{line.CodigoProducto} quedaría con existencia negativa. Disponible: {stock:0.00}. Se revierte todo el ajuste.");
                if (newStock > 9999999999.99m) throw new AjusteRuleException($"La existencia del producto #{line.CodigoProducto} excedería DECIMAL(12,2).");
                await using (var update = Command(c, """
                    UPDATE dbo.Tbl_Inventario SET Existencia=@Nueva
                    WHERE CodigoSucursal=@Sucursal AND CodigoProducto=@Producto AND Existencia=@Anterior;
                    """, tx))
                {
                    Number(update, "@Nueva", newStock); Number(update, "@Anterior", stock);
                    Int(update, "@Sucursal", form.CodigoSucursal); Int(update, "@Producto", line.CodigoProducto);
                    if (await update.ExecuteNonQueryAsync(ct) != 1) throw new AjusteRuleException("El inventario cambió. Se revierte todo el ajuste.");
                }
                await using var history = Command(c, """
                    UPDATE dbo.Tbl_AjustesInventarioDetalle SET CostoUnitario=@Costo
                    WHERE CodigoAjuste=@Id AND CodigoProducto=@Producto;
                    """, tx);
                Number(history, "@Costo", cost); Int(history, "@Id", id); Int(history, "@Producto", line.CodigoProducto);
                if (await history.ExecuteNonQueryAsync(ct) != 1) throw new AjusteRuleException("Los detalles cambiaron. Se revierte todo el ajuste.");
            }
            await using var state = Command(c, "UPDATE dbo.Tbl_AjustesInventario SET Estado='Aplicado',FechaAplicacion=@FechaAplicacion WHERE CodigoAjuste=@Id AND Estado='Borrador';", tx);
            Int(state, "@Id", id);
            state.Parameters.Add("@FechaAplicacion", SqlDbType.Date).Value = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(-6)).Date;
            if (await state.ExecuteNonQueryAsync(ct) != 1) throw new AjusteRuleException("El ajuste cambió de estado. Se revierte toda la operación.");
            await tx.CommitAsync(ct);
        }
        catch { await RollbackAsync(tx); throw; }
    }

    public async Task EliminarAsync(int id, CancellationToken ct)
    {
        await using var c = connections.Create(); await c.OpenAsync(ct);
        using var tx = c.BeginTransaction(IsolationLevel.Serializable);
        try
        {
            if ((await LeerAsync(c, tx, id, true, ct))?.Estado != "Borrador")
                throw new AjusteRuleException("Solo pueden eliminarse ajustes en Borrador.");
            await using (var details = Command(c, "DELETE FROM dbo.Tbl_AjustesInventarioDetalle WHERE CodigoAjuste=@Id;", tx))
            { Int(details, "@Id", id); await details.ExecuteNonQueryAsync(ct); }
            await using (var header = Command(c, "DELETE FROM dbo.Tbl_AjustesInventario WHERE CodigoAjuste=@Id AND Estado='Borrador';", tx))
            {
                Int(header, "@Id", id);
                if (await header.ExecuteNonQueryAsync(ct) != 1) throw new AjusteRuleException("El ajuste cambió; no se completó la eliminación.");
            }
            await tx.CommitAsync(ct);
        }
        catch { await RollbackAsync(tx); throw; }
    }
}
