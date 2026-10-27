using System.ComponentModel.DataAnnotations;
using System.Data;
using Microsoft.Data.SqlClient;
using SupermercadoMVC.Models;

namespace SupermercadoMVC.Data;

public sealed class ProductoRuleException(string message) : Exception(message);

public sealed class ProductosDataService(DatabaseConnectionFactory connections)
{
    private static SqlCommand Cmd(SqlConnection c, string sql, SqlTransaction? tx = null) => new(sql, c, tx);
    private static void Id(SqlCommand c, string key, int? value) => c.Parameters.Add(key, SqlDbType.Int).Value = (object?)value ?? DBNull.Value;
    private static void Text(SqlCommand c, string key, string? value, int length) => c.Parameters.Add(key, SqlDbType.VarChar, length).Value = (object?)value ?? DBNull.Value;
    private static void Money(SqlCommand c, string key, decimal value)
    { var p = c.Parameters.Add(key, SqlDbType.Decimal); p.Precision = 12; p.Scale = 2; p.Value = value; }
    private const string Filter = """
        (@Estado='' OR p.Estado=@Estado) AND (@Categoria IS NULL OR p.CodigoCategoria=@Categoria)
        AND (p.CodigoBarra LIKE @Buscar OR p.Nombre LIKE @Buscar OR p.Marca LIKE @Buscar OR c.Nombre LIKE @Buscar)
        """;
    private static void Filters(SqlCommand cmd, string search, string state, int? category)
    { Text(cmd, "@Buscar", $"%{search}%", 202); Text(cmd, "@Estado", state, 20); Id(cmd, "@Categoria", category); }

    public async Task<ProductosPage> BuscarAsync(string search, string state, int? category, int page, CancellationToken ct)
    {
        var model = new ProductosPage { Search = search, State = state, Category = category, Page = Math.Max(1, page) };
        await using var c = connections.Create(); await c.OpenAsync(ct);
        await using (var cmd = Cmd(c, $"""
            SELECT COUNT_BIG(*) FROM dbo.Tbl_Productos p JOIN dbo.Tbl_Categorias c ON c.CodigoCategoria=p.CodigoCategoria WHERE {Filter};
            SELECT COUNT_BIG(*),COUNT_BIG(CASE WHEN Estado='Activo' THEN 1 END),COUNT_BIG(CASE WHEN Estado='Inactivo' THEN 1 END) FROM dbo.Tbl_Productos;
            SELECT COUNT_BIG(*) FROM dbo.Tbl_ProductoProveedor;
            """))
        {
            Filters(cmd, search, state, category); await using var r = await cmd.ExecuteReaderAsync(ct);
            await r.ReadAsync(ct); model.Total = r.GetInt64(0);
            await r.NextResultAsync(ct); await r.ReadAsync(ct); model.TotalProductos = r.GetInt64(0); model.Activos = r.GetInt64(1); model.Inactivos = r.GetInt64(2);
            await r.NextResultAsync(ct); await r.ReadAsync(ct); model.TotalRelaciones = r.GetInt64(0);
        }
        model.Page = Math.Min(model.Page, model.PageCount);
        await using (var cmd = Cmd(c, $"""
            SELECT p.CodigoProducto,p.CodigoBarra,p.Nombre,p.Marca,c.Nombre,p.UnidadMedida,p.PrecioVenta,p.Estado,
                (SELECT COUNT(*) FROM dbo.Tbl_ProductoProveedor pp WHERE pp.CodigoProducto=p.CodigoProducto)
            FROM dbo.Tbl_Productos p JOIN dbo.Tbl_Categorias c ON c.CodigoCategoria=p.CodigoCategoria
            WHERE {Filter} ORDER BY p.Nombre,p.CodigoProducto OFFSET @Offset ROWS FETCH NEXT @Size ROWS ONLY;
            """))
        {
            Filters(cmd, search, state, category); cmd.Parameters.Add("@Offset", SqlDbType.BigInt).Value = ((long)model.Page - 1) * model.PageSize; Id(cmd, "@Size", model.PageSize);
            await using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct)) model.Rows.Add(new(r.GetInt32(0),r.GetString(1),r.GetString(2),r.GetString(3),r.GetString(4),r.GetString(5),r.GetDecimal(6),r.GetString(7),r.GetInt32(8)));
        }
        await using (var cmd = Cmd(c, """
            SELECT CodigoCategoria,Nombre FROM dbo.Tbl_Categorias ORDER BY Nombre,CodigoCategoria;
            SELECT CodigoProveedor,Nombre FROM dbo.Tbl_Proveedores ORDER BY Nombre,CodigoProveedor;
            """))
        {
            await using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct)) model.Categorias.Add(new(r.GetInt32(0),r.GetString(1)));
            await r.NextResultAsync(ct); while (await r.ReadAsync(ct)) model.Proveedores.Add(new(r.GetInt32(0),r.GetString(1)));
        }
        return model;
    }

    public async Task<ProductoForm?> ObtenerAsync(int id, CancellationToken ct)
    {
        await using var c = connections.Create(); await c.OpenAsync(ct);
        await using var cmd = Cmd(c, """
            SELECT CodigoCategoria,CodigoBarra,Nombre,Marca,Descripcion,UnidadMedida,PrecioVenta,FechaRegistro,Estado
            FROM dbo.Tbl_Productos WHERE CodigoProducto=@Id;
            """); Id(cmd, "@Id", id); await using var r = await cmd.ExecuteReaderAsync(ct);
        if (!await r.ReadAsync(ct)) return null;
        return new() { CodigoProducto=id, CodigoCategoria=r.GetInt32(0), CodigoBarra=r.GetString(1), Nombre=r.GetString(2), Marca=r.GetString(3),
            Descripcion=r.IsDBNull(4)?null:r.GetString(4),UnidadMedida=r.GetString(5),PrecioVenta=r.GetDecimal(6),FechaRegistro=r.GetDateTime(7),Estado=r.GetString(8) };
    }

    public async Task<List<ProductoProveedorRow>> RelacionesAsync(int product, CancellationToken ct)
    {
        await using var c = connections.Create(); await c.OpenAsync(ct);
        await using var cmd = Cmd(c, """
            SELECT pp.CodigoProductoProveedor,pp.CodigoProveedor,p.Nombre,pp.CodigoExterno,pp.CostoReferencia,pp.PlazoEntrega,pp.FechaRegistro,pp.Estado
            FROM dbo.Tbl_ProductoProveedor pp JOIN dbo.Tbl_Proveedores p ON p.CodigoProveedor=pp.CodigoProveedor
            WHERE pp.CodigoProducto=@Producto ORDER BY p.Nombre,pp.CodigoProductoProveedor;
            """); Id(cmd, "@Producto", product); var rows = new List<ProductoProveedorRow>();
        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct)) rows.Add(new(r.GetInt32(0),r.GetInt32(1),r.GetString(2),r.GetString(3),r.GetDecimal(4),r.GetInt32(5),r.GetDateTime(6),r.GetString(7)));
        return rows;
    }

    private async Task WriteAsync(Func<SqlConnection,SqlTransaction,Task> action, CancellationToken ct)
    {
        await using var c = connections.Create(); await c.OpenAsync(ct);
        using var tx = c.BeginTransaction(IsolationLevel.Serializable);
        try { await action(c, tx); await tx.CommitAsync(ct); }
        catch { try { await tx.RollbackAsync(CancellationToken.None); } catch (SqlException) { } catch (InvalidOperationException) { } throw; }
    }
    private static void Validate(object model)
    {
        var errors = new List<ValidationResult>();
        if (!Validator.TryValidateObject(model, new ValidationContext(model), errors, true))
            throw new ProductoRuleException(string.Join(" ",errors.Select(e=>e.ErrorMessage)));
    }
    private static async Task MustExist(SqlConnection c, SqlTransaction tx, string sql, int value, string message, CancellationToken ct)
    { await using var cmd = Cmd(c, sql, tx); Id(cmd,"@Id",value); if (await cmd.ExecuteScalarAsync(ct) is null) throw new ProductoRuleException(message); }
    private static Task LockProduct(SqlConnection c, SqlTransaction tx, int id, CancellationToken ct) => MustExist(c,tx,
        "SELECT CodigoProducto FROM dbo.Tbl_Productos WITH(UPDLOCK,HOLDLOCK) WHERE CodigoProducto=@Id;",id,"El producto ya no existe.",ct);

    public async Task GuardarProductoAsync(ProductoForm input, CancellationToken ct)
    {
        Validate(input);
        await WriteAsync(async (c,tx) =>
        {
            if (input.CodigoProducto.HasValue) await LockProduct(c,tx,input.CodigoProducto.Value,ct);
            await MustExist(c,tx,"SELECT CodigoCategoria FROM dbo.Tbl_Categorias WITH(HOLDLOCK) WHERE CodigoCategoria=@Id;",input.CodigoCategoria!.Value,"La categoría seleccionada no existe.",ct);
            await using (var check = Cmd(c,"SELECT CodigoProducto FROM dbo.Tbl_Productos WITH(UPDLOCK,HOLDLOCK) WHERE CodigoBarra=@Barra AND (@Id IS NULL OR CodigoProducto<>@Id);",tx))
            { Text(check,"@Barra",input.CodigoBarra,30); Id(check,"@Id",input.CodigoProducto); if (await check.ExecuteScalarAsync(ct) is not null) throw new ProductoRuleException("El código de barras ya pertenece a otro producto."); }
            var sql = input.CodigoProducto.HasValue ? """
                UPDATE dbo.Tbl_Productos SET CodigoCategoria=@Categoria,CodigoBarra=@Barra,Nombre=@Nombre,Marca=@Marca,
                    Descripcion=@Descripcion,UnidadMedida=@Unidad,PrecioVenta=@Precio,FechaRegistro=@Fecha,Estado=@Estado WHERE CodigoProducto=@Id;
                """ : """
                INSERT INTO dbo.Tbl_Productos(CodigoCategoria,CodigoBarra,Nombre,Marca,Descripcion,UnidadMedida,PrecioVenta,FechaRegistro,Estado)
                VALUES(@Categoria,@Barra,@Nombre,@Marca,@Descripcion,@Unidad,@Precio,@Fecha,@Estado);
                """;
            await using var cmd = Cmd(c,sql,tx); Id(cmd,"@Id",input.CodigoProducto); Id(cmd,"@Categoria",input.CodigoCategoria);
            Text(cmd,"@Barra",input.CodigoBarra,30); Text(cmd,"@Nombre",input.Nombre,150); Text(cmd,"@Marca",input.Marca,100); Text(cmd,"@Descripcion",input.Descripcion,200);
            Text(cmd,"@Unidad",input.UnidadMedida,30); Money(cmd,"@Precio",input.PrecioVenta!.Value); cmd.Parameters.Add("@Fecha",SqlDbType.Date).Value=input.FechaRegistro!.Value; Text(cmd,"@Estado",input.Estado,20);
            if (await cmd.ExecuteNonQueryAsync(ct)!=1) throw new ProductoRuleException("No se guardó el producto. Actualiza el listado.");
        },ct);
    }

    public async Task GuardarRelacionAsync(int product, ProductoProveedorForm input, CancellationToken ct)
    {
        Validate(input);
        await WriteAsync(async (c,tx) =>
        {
            await LockProduct(c,tx,product,ct);
            await MustExist(c,tx,"SELECT CodigoProveedor FROM dbo.Tbl_Proveedores WITH(HOLDLOCK) WHERE CodigoProveedor=@Id;",input.CodigoProveedor!.Value,"El proveedor seleccionado no existe.",ct);
            if (input.CodigoProductoProveedor.HasValue)
            {
                await using var current = Cmd(c,"SELECT CodigoProveedor FROM dbo.Tbl_ProductoProveedor WITH(UPDLOCK,HOLDLOCK) WHERE CodigoProductoProveedor=@Id AND CodigoProducto=@Producto;",tx);
                Id(current,"@Id",input.CodigoProductoProveedor); Id(current,"@Producto",product); var original = await current.ExecuteScalarAsync(ct);
                if (original is null) throw new ProductoRuleException("La relación ya no existe para este producto.");
                if ((int)original != input.CodigoProveedor.Value && await TieneComprasAsync(c,tx,product,(int)original,ct))
                    throw new ProductoRuleException("La relación tiene compras. Conserva el proveedor e inactiva la relación; puedes agregar otro proveedor por separado.");
            }
            await using (var check = Cmd(c,"SELECT CodigoProductoProveedor FROM dbo.Tbl_ProductoProveedor WITH(UPDLOCK,HOLDLOCK) WHERE CodigoProducto=@Producto AND CodigoProveedor=@Proveedor AND (@Id IS NULL OR CodigoProductoProveedor<>@Id);",tx))
            { Id(check,"@Producto",product); Id(check,"@Proveedor",input.CodigoProveedor); Id(check,"@Id",input.CodigoProductoProveedor); if (await check.ExecuteScalarAsync(ct) is not null) throw new ProductoRuleException("Este proveedor ya está relacionado con el producto, aunque esté inactivo. Edita la relación existente."); }
            var sql = input.CodigoProductoProveedor.HasValue ? """
                UPDATE dbo.Tbl_ProductoProveedor SET CodigoProveedor=@Proveedor,CodigoExterno=@Externo,CostoReferencia=@Costo,
                    PlazoEntrega=@Plazo,FechaRegistro=@Fecha,Estado=@Estado WHERE CodigoProductoProveedor=@Id AND CodigoProducto=@Producto;
                """ : """
                INSERT INTO dbo.Tbl_ProductoProveedor(CodigoProducto,CodigoProveedor,CodigoExterno,CostoReferencia,PlazoEntrega,FechaRegistro,Estado)
                VALUES(@Producto,@Proveedor,@Externo,@Costo,@Plazo,@Fecha,@Estado);
                """;
            await using var cmd=Cmd(c,sql,tx); Id(cmd,"@Id",input.CodigoProductoProveedor); Id(cmd,"@Producto",product); Id(cmd,"@Proveedor",input.CodigoProveedor);
            Text(cmd,"@Externo",input.CodigoExterno,50); Money(cmd,"@Costo",input.CostoReferencia!.Value); Id(cmd,"@Plazo",input.PlazoEntrega);
            cmd.Parameters.Add("@Fecha",SqlDbType.Date).Value=input.FechaRegistro!.Value; Text(cmd,"@Estado",input.Estado,20);
            if(await cmd.ExecuteNonQueryAsync(ct)!=1) throw new ProductoRuleException("No se guardó la relación. Actualiza el listado.");
        },ct);
    }

    private static async Task<bool> TieneComprasAsync(SqlConnection c, SqlTransaction tx, int product, int supplier, CancellationToken ct)
    {
        await using var cmd=Cmd(c,"""
            SELECT CASE WHEN EXISTS(SELECT 1 FROM dbo.Tbl_Compras c JOIN dbo.Tbl_ComprasDetalle d ON d.CodigoCompra=c.CodigoCompra
                WHERE c.CodigoProveedor=@Proveedor AND d.CodigoProducto=@Producto) THEN 1 ELSE 0 END;
            """,tx); Id(cmd,"@Producto",product); Id(cmd,"@Proveedor",supplier); return (int)(await cmd.ExecuteScalarAsync(ct))! == 1;
    }
    public async Task EliminarRelacionAsync(int product, int id, bool practice, CancellationToken ct)
    {
        if(!practice) throw new ProductoRuleException("Confirma que la relación es un registro de práctica sin movimientos.");
        await WriteAsync(async (c,tx) =>
        {
            await LockProduct(c,tx,product,ct);
            await using var current=Cmd(c,"SELECT CodigoProveedor FROM dbo.Tbl_ProductoProveedor WITH(UPDLOCK,HOLDLOCK) WHERE CodigoProductoProveedor=@Id AND CodigoProducto=@Producto;",tx);
            Id(current,"@Id",id); Id(current,"@Producto",product); var supplier=await current.ExecuteScalarAsync(ct);
            if(supplier is null) throw new ProductoRuleException("La relación ya no existe para este producto.");
            if(await TieneComprasAsync(c,tx,product,(int)supplier,ct)) throw new ProductoRuleException("La relación tiene compras y debe conservarse. Utiliza el estado Inactivo.");
            await using var cmd=Cmd(c,"DELETE FROM dbo.Tbl_ProductoProveedor WHERE CodigoProductoProveedor=@Id AND CodigoProducto=@Producto;",tx);
            Id(cmd,"@Id",id); Id(cmd,"@Producto",product); if(await cmd.ExecuteNonQueryAsync(ct)!=1) throw new ProductoRuleException("No se eliminó la relación.");
        },ct);
    }
    public async Task EliminarProductoAsync(int id, bool practice, CancellationToken ct)
    {
        if(!practice) throw new ProductoRuleException("Confirma que el producto es un registro de práctica sin dependencias.");
        await WriteAsync(async (c,tx) =>
        {
            await LockProduct(c,tx,id,ct);
            await using (var check=Cmd(c,"""
                SELECT CASE WHEN EXISTS(SELECT 1 FROM dbo.Tbl_ProductoProveedor WHERE CodigoProducto=@Id)
                    OR EXISTS(SELECT 1 FROM dbo.Tbl_Inventario WHERE CodigoProducto=@Id)
                    OR EXISTS(SELECT 1 FROM dbo.Tbl_ComprasDetalle WHERE CodigoProducto=@Id)
                    OR EXISTS(SELECT 1 FROM dbo.Tbl_VentasDetalle WHERE CodigoProducto=@Id)
                    OR EXISTS(SELECT 1 FROM dbo.Tbl_AjustesInventarioDetalle WHERE CodigoProducto=@Id) THEN 1 ELSE 0 END;
                """,tx))
            { Id(check,"@Id",id); if((int)(await check.ExecuteScalarAsync(ct))! == 1) throw new ProductoRuleException("El producto tiene inventario, proveedores o documentos relacionados. Conserva sus relaciones e inactívalo cuando corresponda; no se elimina en cascada."); }
            await using var cmd=Cmd(c,"DELETE FROM dbo.Tbl_Productos WHERE CodigoProducto=@Id;",tx); Id(cmd,"@Id",id);
            if(await cmd.ExecuteNonQueryAsync(ct)!=1) throw new ProductoRuleException("No se eliminó el producto.");
        },ct);
    }
}
