using System.Data;
using System.ComponentModel.DataAnnotations;
using Microsoft.Data.SqlClient;
using SupermercadoMVC.Models;

namespace SupermercadoMVC.Data;

public sealed class CompraRuleException(string message) : Exception(message);

// Adaptación de CompraRepository (ZIP de la integrante): catálogos, búsqueda,
// encabezado/detalles y parámetros tipados. Totales y recepción se ajustan al
// esquema principal; no se utiliza su Total/Subtotal persistido ni su login.
public sealed class CompraRepository(DatabaseConnectionFactory connections)
{
    private static SqlCommand Command(SqlConnection c, string sql, SqlTransaction? tx = null) => new(sql, c, tx);
    private static void AddInt(SqlCommand c, string name, int value) => c.Parameters.Add(name, SqlDbType.Int).Value = value;
    private static void AddVarChar(SqlCommand c, string name, string? value, int length) =>
        c.Parameters.Add(name, SqlDbType.VarChar, length).Value = value is null ? DBNull.Value : value;
    private static void AddDate(SqlCommand c, string name, DateTime value) => c.Parameters.Add(name, SqlDbType.Date).Value = value.Date;
    private static void AddDecimal(SqlCommand c, string name, decimal value)
    {
        var p = c.Parameters.Add(name, SqlDbType.Decimal); p.Precision = 12; p.Scale = 2; p.Value = value;
    }
    private static async Task RollbackAsync(SqlTransaction tx)
    {
        try { await tx.RollbackAsync(CancellationToken.None); }
        catch (SqlException) { }
        catch (InvalidOperationException) { }
    }
    private const string Filter = "(@Estado='' OR c.Estado=@Estado) AND (c.NumeroCompra LIKE @Buscar OR p.Nombre LIKE @Buscar OR p.NIT LIKE @Buscar)";
    private static void AgregarFiltros(SqlCommand command, string search, string state)
    {
        AddVarChar(command, "@Buscar", $"%{search}%", 300); AddVarChar(command, "@Estado", state, 20);
    }

    public async Task<ComprasIndexViewModel> BuscarAsync(string search, string state, int page, CancellationToken ct)
    {
        var model = new ComprasIndexViewModel { Search = search, State = state, Page = Math.Max(1, page) };
        await using var connection = connections.Create(); await connection.OpenAsync(ct);
        await using (var command = Command(connection, $"""
            SELECT COUNT_BIG(*) FROM dbo.Tbl_Compras c JOIN dbo.Tbl_Proveedores p ON p.CodigoProveedor=c.CodigoProveedor WHERE {Filter};
            SELECT COUNT_BIG(CASE WHEN Estado='Borrador' THEN 1 END),COUNT_BIG(CASE WHEN Estado='Recibida' THEN 1 END) FROM dbo.Tbl_Compras;
            SELECT COALESCE(SUM(d.Cantidad*d.CostoUnitario-d.Descuento),0)
            FROM dbo.Tbl_Compras c JOIN dbo.Tbl_ComprasDetalle d ON d.CodigoCompra=c.CodigoCompra WHERE c.Estado='Recibida';
            """))
        {
            AgregarFiltros(command, search, state);
            await using var r = await command.ExecuteReaderAsync(ct); await r.ReadAsync(ct); model.Total = r.GetInt64(0);
            await r.NextResultAsync(ct); await r.ReadAsync(ct); model.Borradores = r.GetInt64(0); model.Recibidas = r.GetInt64(1);
            await r.NextResultAsync(ct); await r.ReadAsync(ct); model.ImporteRecibido = r.GetDecimal(0);
        }
        model.Page = Math.Min(model.Page, model.PageCount);
        await using (var command = Command(connection, $"""
            SELECT c.CodigoCompra,c.NumeroCompra,c.FechaCompra,p.Nombre,s.Nombre,e.Nombres+' '+e.Apellidos,c.Estado,
                COALESCE((SELECT SUM(d.Cantidad*d.CostoUnitario-d.Descuento) FROM dbo.Tbl_ComprasDetalle d WHERE d.CodigoCompra=c.CodigoCompra),0)
            FROM dbo.Tbl_Compras c JOIN dbo.Tbl_Proveedores p ON p.CodigoProveedor=c.CodigoProveedor
            JOIN dbo.Tbl_Sucursales s ON s.CodigoSucursal=c.CodigoSucursal
            JOIN dbo.Tbl_Empleados e ON e.CodigoEmpleado=c.CodigoEmpleado AND e.CodigoSucursal=c.CodigoSucursal
            WHERE {Filter} ORDER BY c.FechaCompra DESC,c.CodigoCompra DESC OFFSET @Offset ROWS FETCH NEXT @Size ROWS ONLY;
            """))
        {
            AgregarFiltros(command, search, state); AddInt(command, "@Offset", (model.Page - 1) * model.PageSize); AddInt(command, "@Size", model.PageSize);
            var rows = new List<CompraRow>(); await using var r = await command.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct)) rows.Add(new(r.GetInt32(0), r.GetString(1), r.GetDateTime(2), r.GetString(3), r.GetString(4), r.GetString(5), r.GetString(6), r.GetDecimal(7)));
            model.Rows = rows;
        }
        await ObtenerCatalogosAsync(connection, model, ct); return model;
    }

    private static async Task ObtenerCatalogosAsync(SqlConnection connection, ComprasIndexViewModel model, CancellationToken ct)
    {
        await using var command = Command(connection, """
            SELECT CodigoProveedor,NIT+' · '+Nombre FROM dbo.Tbl_Proveedores ORDER BY Nombre,CodigoProveedor;
            SELECT CodigoSucursal,Nombre FROM dbo.Tbl_Sucursales ORDER BY Nombre;
            SELECT CodigoEmpleado,Nombres+' '+Apellidos,CodigoSucursal FROM dbo.Tbl_Empleados ORDER BY Nombres,Apellidos;
            SELECT CodigoProducto,CodigoBarra+' · '+Nombre FROM dbo.Tbl_Productos ORDER BY Nombre,CodigoProducto;
            SELECT CodigoProducto,CodigoProveedor FROM dbo.Tbl_ProductoProveedor ORDER BY CodigoProducto,CodigoProveedor;
            """);
        await using var r = await command.ExecuteReaderAsync(ct);
        var suppliers = new List<CompraOption>(); while (await r.ReadAsync(ct)) suppliers.Add(new(r.GetInt32(0), r.GetString(1))); model.Proveedores = suppliers;
        await r.NextResultAsync(ct);
        var branches = new List<CompraOption>(); while (await r.ReadAsync(ct)) branches.Add(new(r.GetInt32(0), r.GetString(1))); model.Sucursales = branches;
        await r.NextResultAsync(ct);
        var employees = new List<CompraEmployee>(); while (await r.ReadAsync(ct)) employees.Add(new(r.GetInt32(0), r.GetString(1), r.GetInt32(2))); model.Empleados = employees;
        await r.NextResultAsync(ct);
        var products = new List<CompraOption>(); while (await r.ReadAsync(ct)) products.Add(new(r.GetInt32(0), r.GetString(1)));
        await r.NextResultAsync(ct);
        var suppliersByProduct = new Dictionary<int, List<int>>();
        while (await r.ReadAsync(ct))
        {
            var productId = r.GetInt32(0);
            if (!suppliersByProduct.TryGetValue(productId, out var supplierIds))
                suppliersByProduct[productId] = supplierIds = [];
            supplierIds.Add(r.GetInt32(1));
        }
        model.Productos = products.Select(product => new CompraProductoOption(product.Id, product.Nombre,
            suppliersByProduct.TryGetValue(product.Id, out var supplierIds) ? string.Join(',', supplierIds) : "")).ToList();
    }

    public async Task<CompraDetail?> ObtenerParaEditarAsync(int id, CancellationToken ct)
    {
        await using var c = connections.Create(); await c.OpenAsync(ct);
        using var tx = c.BeginTransaction(IsolationLevel.Serializable);
        var detail = await LeerAsync(c, tx, id, false, ct); await tx.CommitAsync(ct); return detail;
    }

    private static async Task<CompraDetail?> LeerAsync(SqlConnection c, SqlTransaction tx, int id, bool write, CancellationToken ct)
    {
        var hint = write ? "WITH (UPDLOCK,HOLDLOCK)" : "";
        // Leer y bloquear primero el encabezado en todos los mutadores.
        await using var command = Command(c, $"""
            SELECT CodigoProveedor,CodigoSucursal,CodigoEmpleado,NumeroCompra,FechaCompra,FechaEntregaProgramada,FechaRecepcion,Estado,Observaciones
            FROM dbo.Tbl_Compras {hint} WHERE CodigoCompra=@Id;
            SELECT CodigoProducto,Cantidad,CostoUnitario,Descuento,Observaciones FROM dbo.Tbl_ComprasDetalle WHERE CodigoCompra=@Id ORDER BY CodigoProducto;
            """, tx);
        AddInt(command, "@Id", id); await using var r = await command.ExecuteReaderAsync(ct);
        if (!await r.ReadAsync(ct)) return null;
        var input = new CompraFormViewModel { CodigoCompra = id, CodigoProveedor = r.GetInt32(0), CodigoSucursal = r.GetInt32(1), CodigoEmpleado = r.GetInt32(2), NumeroCompra = r.GetString(3), FechaCompra = r.GetDateTime(4), FechaEntregaProgramada = r.GetDateTime(5), Observaciones = r.IsDBNull(8) ? null : r.GetString(8) };
        DateTime? received = r.IsDBNull(6) ? null : r.GetDateTime(6); var state = r.GetString(7);
        await r.NextResultAsync(ct);
        while (await r.ReadAsync(ct)) input.Lineas.Add(new() { CodigoProducto = r.GetInt32(0), Cantidad = r.GetDecimal(1), CostoUnitario = r.GetDecimal(2), Descuento = r.GetDecimal(3), Observaciones = r.IsDBNull(4) ? null : r.GetString(4) });
        return new(input, state, received);
    }

    private static void Validar(CompraFormViewModel form)
    {
        var errors = new List<ValidationResult>();
        Validator.TryValidateObject(form, new(form), errors, true);
        foreach (var line in form.Lineas) Validator.TryValidateObject(line, new(line), errors, true);
        if (errors.Count > 0) throw new CompraRuleException(errors[0].ErrorMessage ?? "Revisa los datos de la compra.");
    }
    private static async Task ReferenciaAsync(SqlConnection c, SqlTransaction tx, string sql, int id, string message, CancellationToken ct, int? branch = null)
    {
        await using var command = Command(c, sql, tx); AddInt(command, "@Id", id);
        if (branch.HasValue) AddInt(command, "@Branch", branch.Value);
        if (Convert.ToInt64(await command.ExecuteScalarAsync(ct)) != 1) throw new CompraRuleException(message);
    }
    private static async Task ProductoDelProveedorAsync(SqlConnection c, SqlTransaction tx, int productId, int supplierId, CancellationToken ct)
    {
        await using var command = Command(c, """
            SELECT COUNT_BIG(*) FROM dbo.Tbl_ProductoProveedor WITH (HOLDLOCK)
            WHERE CodigoProducto=@Producto AND CodigoProveedor=@Proveedor;
            """, tx);
        AddInt(command, "@Producto", productId);
        AddInt(command, "@Proveedor", supplierId);
        if (Convert.ToInt64(await command.ExecuteScalarAsync(ct)) != 1)
            throw new CompraRuleException($"El producto #{productId} no está relacionado con el proveedor seleccionado.");
    }

    public async Task<int> GuardarAsync(CompraFormViewModel form, CancellationToken ct)
    {
        Validar(form);
        await using var c = connections.Create(); await c.OpenAsync(ct);
        using var tx = c.BeginTransaction(IsolationLevel.Serializable);
        try
        {
            if (form.CodigoCompra.HasValue)
            {
                var current = await LeerAsync(c, tx, form.CodigoCompra.Value, true, ct);
                if (current?.Estado != "Borrador") throw new CompraRuleException("La compra ya no existe o dejó de ser borrador. Recibidas y Anuladas son de solo consulta.");
            }
            await ReferenciaAsync(c, tx, "SELECT COUNT_BIG(*) FROM dbo.Tbl_Proveedores WHERE CodigoProveedor=@Id;", form.CodigoProveedor!.Value, "El proveedor no existe.", ct);
            await ReferenciaAsync(c, tx, "SELECT COUNT_BIG(*) FROM dbo.Tbl_Sucursales WHERE CodigoSucursal=@Id;", form.CodigoSucursal!.Value, "La sucursal no existe.", ct);
            await ReferenciaAsync(c, tx, "SELECT COUNT_BIG(*) FROM dbo.Tbl_Empleados WHERE CodigoEmpleado=@Id AND CodigoSucursal=@Branch;", form.CodigoEmpleado!.Value, "El empleado no pertenece a la sucursal seleccionada.", ct, form.CodigoSucursal);
            foreach (var line in form.Lineas.OrderBy(l => l.CodigoProducto))
            {
                await ReferenciaAsync(c, tx, "SELECT COUNT_BIG(*) FROM dbo.Tbl_Productos WHERE CodigoProducto=@Id;", line.CodigoProducto!.Value, "Uno de los productos ya no existe.", ct);
                await ProductoDelProveedorAsync(c, tx, line.CodigoProducto.Value, form.CodigoProveedor.Value, ct);
            }

            int id;
            await using (var command = Command(c, form.CodigoCompra.HasValue ? """
                UPDATE dbo.Tbl_Compras SET CodigoProveedor=@Proveedor,CodigoSucursal=@Sucursal,CodigoEmpleado=@Empleado,NumeroCompra=@Numero,
                    FechaCompra=@Fecha,FechaEntregaProgramada=@Entrega,Observaciones=@Notas WHERE CodigoCompra=@Id AND Estado='Borrador';
                SELECT @Id;
                """ : """
                INSERT INTO dbo.Tbl_Compras(CodigoProveedor,CodigoSucursal,CodigoEmpleado,NumeroCompra,FechaCompra,FechaEntregaProgramada,FechaRecepcion,Estado,Observaciones)
                OUTPUT INSERTED.CodigoCompra VALUES(@Proveedor,@Sucursal,@Empleado,@Numero,@Fecha,@Entrega,NULL,'Borrador',@Notas);
                """, tx))
            {
                AddInt(command, "@Proveedor", form.CodigoProveedor.Value); AddInt(command, "@Sucursal", form.CodigoSucursal.Value); AddInt(command, "@Empleado", form.CodigoEmpleado.Value);
                AddVarChar(command, "@Numero", form.NumeroCompra.Trim(), 30); AddDate(command, "@Fecha", form.FechaCompra!.Value); AddDate(command, "@Entrega", form.FechaEntregaProgramada!.Value); AddVarChar(command, "@Notas", form.Observaciones, 300);
                if (form.CodigoCompra.HasValue) AddInt(command, "@Id", form.CodigoCompra.Value);
                id = Convert.ToInt32(await command.ExecuteScalarAsync(ct));
            }
            // Sincronizar dentro de la misma transacción evita conflictos al intercambiar productos.
            if (form.CodigoCompra.HasValue)
            {
                await using var remove = Command(c, "DELETE FROM dbo.Tbl_ComprasDetalle WHERE CodigoCompra=@Id;", tx);
                AddInt(remove, "@Id", id); await remove.ExecuteNonQueryAsync(ct);
            }
            foreach (var line in form.Lineas)
            {
                await using var command = Command(c, """
                    INSERT INTO dbo.Tbl_ComprasDetalle(CodigoCompra,CodigoProducto,Cantidad,CostoUnitario,Descuento,Observaciones)
                    VALUES(@Id,@Producto,@Cantidad,@Costo,@Descuento,@Notas);
                    """, tx);
                AddInt(command, "@Id", id); AddInt(command, "@Producto", line.CodigoProducto!.Value);
                AddDecimal(command, "@Cantidad", line.Cantidad!.Value); AddDecimal(command, "@Costo", line.CostoUnitario!.Value); AddDecimal(command, "@Descuento", line.Descuento!.Value); AddVarChar(command, "@Notas", line.Observaciones, 300);
                await command.ExecuteNonQueryAsync(ct);
            }
            await tx.CommitAsync(ct); return id;
        }
        catch { await RollbackAsync(tx); throw; }
    }

    public async Task RecibirAsync(int id, DateTime fechaRecepcion, CancellationToken ct)
    {
        await using var c = connections.Create(); await c.OpenAsync(ct);
        using var tx = c.BeginTransaction(IsolationLevel.Serializable);
        try
        {
            var purchase = await LeerAsync(c, tx, id, true, ct);
            if (purchase?.Estado != "Borrador") throw new CompraRuleException("La compra ya no es borrador o no existe. No se volvió a ingresar inventario.");
            Validar(purchase.Input);
            if (purchase.Input.Lineas.Count == 0) throw new CompraRuleException("Agrega al menos un detalle válido antes de recibir la compra.");
            if (fechaRecepcion.TimeOfDay != TimeSpan.Zero || fechaRecepcion < purchase.Input.FechaCompra)
                throw new CompraRuleException("La recepción debe ser una fecha sin hora y no anterior a la compra.");
            foreach (var line in purchase.Input.Lineas.OrderBy(l => l.CodigoProducto))
                await ProductoDelProveedorAsync(c, tx, line.CodigoProducto!.Value, purchase.Input.CodigoProveedor!.Value, ct);
            // Orden de producto común con Ventas. El encabezado queda bloqueado hasta COMMIT/ROLLBACK.
            foreach (var line in purchase.Input.Lineas.OrderBy(l => l.CodigoProducto))
            {
                decimal stock, cost;
                await using (var command = Command(c, "SELECT Existencia,CostoPromedio FROM dbo.Tbl_Inventario WITH (UPDLOCK,HOLDLOCK) WHERE CodigoSucursal=@Sucursal AND CodigoProducto=@Producto;", tx))
                {
                    AddInt(command, "@Sucursal", purchase.Input.CodigoSucursal!.Value); AddInt(command, "@Producto", line.CodigoProducto!.Value);
                    await using var r = await command.ExecuteReaderAsync(ct);
                    if (!await r.ReadAsync(ct)) throw new CompraRuleException($"El producto #{line.CodigoProducto} no tiene inventario en esta sucursal. No se recibió la compra ni se creó inventario.");
                    stock = r.GetDecimal(0); cost = r.GetDecimal(1);
                }
                // Decimal conserva el descuento monetario completo. Redondear solo al guardar el costo.
                var quantity = line.Cantidad!.Value;
                var newStock = stock + quantity;
                var newCost = decimal.Round((stock * cost + quantity * line.CostoUnitario!.Value - line.Descuento!.Value) / newStock, 2, MidpointRounding.AwayFromZero);
                if (newStock > 9999999999.99m || newCost < 0 || newCost > 9999999999.99m)
                    throw new CompraRuleException($"La existencia o el costo promedio del producto #{line.CodigoProducto} excede DECIMAL(12,2). La recepción se revierte completa.");
                await using var update = Command(c, """
                    UPDATE dbo.Tbl_Inventario SET Existencia=@Existencia,CostoPromedio=@Costo,FechaUltimoIngreso=@Fecha
                    WHERE CodigoSucursal=@Sucursal AND CodigoProducto=@Producto;
                    """, tx);
                AddDecimal(update, "@Existencia", newStock); AddDecimal(update, "@Costo", newCost); AddDate(update, "@Fecha", fechaRecepcion);
                AddInt(update, "@Sucursal", purchase.Input.CodigoSucursal.Value); AddInt(update, "@Producto", line.CodigoProducto.Value);
                if (await update.ExecuteNonQueryAsync(ct) != 1) throw new CompraRuleException("El inventario cambió. La recepción se revierte completa.");
            }
            await using var state = Command(c, "UPDATE dbo.Tbl_Compras SET Estado='Recibida',FechaRecepcion=@Fecha WHERE CodigoCompra=@Id AND Estado='Borrador';", tx);
            AddInt(state, "@Id", id); AddDate(state, "@Fecha", fechaRecepcion);
            if (await state.ExecuteNonQueryAsync(ct) != 1) throw new CompraRuleException("La compra cambió de estado. La recepción se revierte completa.");
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
            var current = await LeerAsync(c, tx, id, true, ct);
            if (current?.Estado != "Borrador") throw new CompraRuleException("Solo pueden eliminarse compras en Borrador.");
            await using (var details = Command(c, "DELETE FROM dbo.Tbl_ComprasDetalle WHERE CodigoCompra=@Id;", tx))
            { AddInt(details, "@Id", id); await details.ExecuteNonQueryAsync(ct); }
            await using (var header = Command(c, "DELETE FROM dbo.Tbl_Compras WHERE CodigoCompra=@Id AND Estado='Borrador';", tx))
            {
                AddInt(header, "@Id", id);
                if (await header.ExecuteNonQueryAsync(ct) != 1) throw new CompraRuleException("La compra cambió. No se completó la eliminación.");
            }
            await tx.CommitAsync(ct);
        }
        catch { await RollbackAsync(tx); throw; }
    }
}
