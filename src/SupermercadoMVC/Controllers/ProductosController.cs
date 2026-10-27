using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SupermercadoMVC.Data;
using SupermercadoMVC.Filters;
using SupermercadoMVC.Models;

namespace SupermercadoMVC.Controllers;

[Route("productos")]
[ServiceFilter(typeof(DemoSessionFilter))]
public sealed class ProductosController(ProductosDataService productos, ILogger<ProductosController> logger) : Controller
{
    private static string Search(string? value) { var s=(value??"").Trim(); return s.Length>100?s[..100]:s; }
    private static string State(string? value) => value is "Activo" or "Inactivo" ? value : "";
    [HttpGet("/modulos/productos")]
    public IActionResult Legacy() => RedirectToAction(nameof(Index));

    [HttpGet("")]
    public async Task<IActionResult> Index(string? buscar, string? estado, int? categoria, int pagina=1,
        int? producto=null, bool nuevo=false, bool proveedores=false, int? relacion=null, CancellationToken cancellationToken=default)
    {
        var model=await PageAsync(buscar,estado,categoria,pagina,cancellationToken);
        if(producto is int id)
        {
            try
            {
                var input=await productos.ObtenerAsync(id,cancellationToken);
                if(input is null) { model.Error="El producto ya no existe."; return View(model); }
                model.Input=input;
                if(proveedores)
                {
                    model.Relaciones=await productos.RelacionesAsync(id,cancellationToken); model.OpenSuppliers=true;
                    if(relacion is int relationId)
                    {
                        var row=model.Relaciones.SingleOrDefault(r=>r.Id==relationId);
                        if(row is null) model.Error="La relación no existe para este producto.";
                        else model.Relacion=new() { CodigoProductoProveedor=row.Id,CodigoProveedor=row.CodigoProveedor,CodigoExterno=row.CodigoExterno,
                            CostoReferencia=row.CostoReferencia,PlazoEntrega=row.PlazoEntrega,FechaRegistro=row.FechaRegistro,Estado=row.Estado };
                    }
                }
                else model.OpenProduct=true;
            }
            catch(Exception e) when(e is not OperationCanceledException) { model.Error=Message(e); }
        }
        else model.OpenProduct=nuevo;
        return View(model);
    }

    [HttpPost("guardar/{id:int?}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save([Bind(Prefix="Input")] ProductoForm input,int? id,string? buscar,string? estado,int? categoria,int pagina=1,CancellationToken cancellationToken=default)
    {
        input.CodigoProducto=id;
        input.CodigoBarra=(input.CodigoBarra??"").Trim(); input.Nombre=(input.Nombre??"").Trim(); input.Marca=(input.Marca??"").Trim();
        input.UnidadMedida=(input.UnidadMedida??"").Trim(); input.Descripcion=string.IsNullOrWhiteSpace(input.Descripcion)?null:input.Descripcion.Trim();
        if(ModelState.IsValid)
        {
            try { await productos.GuardarProductoAsync(input,cancellationToken); TempData["ProductosNotice"]="Producto guardado."; return RedirectToAction(nameof(Index),new{buscar,estado,categoria,pagina}); }
            catch(Exception e) when(e is not OperationCanceledException) { ModelState.AddModelError("",Message(e)); }
        }
        var model=await PageAsync(buscar,estado,categoria,pagina,cancellationToken); model.Input=input; model.OpenProduct=true;
        return View("Index",model);
    }

    [HttpPost("{producto:int}/proveedores/guardar/{id:int?}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveSupplier(int producto,[Bind(Prefix="Relacion")] ProductoProveedorForm input,int? id,
        string? buscar,string? estado,int? categoria,int pagina=1,CancellationToken cancellationToken=default)
    {
        input.CodigoProductoProveedor=id; input.CodigoExterno=(input.CodigoExterno??"").Trim();
        if(ModelState.IsValid)
        {
            try { await productos.GuardarRelacionAsync(producto,input,cancellationToken); TempData["ProductosNotice"]="Relación con proveedor guardada.";
                return RedirectToAction(nameof(Index),new{producto,proveedores=true,buscar,estado,categoria,pagina}); }
            catch(Exception e) when(e is not OperationCanceledException) { ModelState.AddModelError("",Message(e)); }
        }
        var model=await PageAsync(buscar,estado,categoria,pagina,cancellationToken);
        try
        {
            var product=await productos.ObtenerAsync(producto,cancellationToken);
            if(product is null) model.Error="El producto ya no existe.";
            else { model.Input=product; model.Relaciones=await productos.RelacionesAsync(producto,cancellationToken); model.Relacion=input; model.OpenSuppliers=true; }
        }
        catch(Exception e) when(e is not OperationCanceledException) { model.Error=Message(e); }
        return View("Index",model);
    }

    [HttpPost("eliminar/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id,bool practica,string? buscar,string? estado,int? categoria,int pagina=1,CancellationToken cancellationToken=default)
    {
        try { await productos.EliminarProductoAsync(id,practica,cancellationToken); TempData["ProductosNotice"]="Producto de práctica eliminado."; }
        catch(Exception e) when(e is not OperationCanceledException) { TempData["ProductosError"]=Message(e); }
        return RedirectToAction(nameof(Index),new{buscar,estado,categoria,pagina});
    }
    [HttpPost("{producto:int}/proveedores/eliminar/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSupplier(int producto,int id,bool practica,string? buscar,string? estado,int? categoria,int pagina=1,CancellationToken cancellationToken=default)
    {
        try { await productos.EliminarRelacionAsync(producto,id,practica,cancellationToken); TempData["ProductosNotice"]="Relación de práctica eliminada."; }
        catch(Exception e) when(e is not OperationCanceledException) { TempData["ProductosError"]=Message(e); }
        return RedirectToAction(nameof(Index),new{producto,proveedores=true,buscar,estado,categoria,pagina});
    }

    private async Task<ProductosPage> PageAsync(string? search,string? state,int? category,int page,CancellationToken ct)
    {
        ProductosPage model;
        try { model=await productos.BuscarAsync(Search(search),State(state),category>0?category:null,Math.Max(1,page),ct); }
        catch(Exception e) when(e is not OperationCanceledException)
        { model=new() { Search=Search(search),State=State(state),Category=category,Error=Message(e) }; }
        model.Notice=TempData["ProductosNotice"] as string; model.Error??=TempData["ProductosError"] as string; return model;
    }
    private string Message(Exception e)
    {
        if(e is ProductoRuleException) return e.Message;
        logger.LogWarning("Productos operation failed: {Type}, SQL {Number}.",e.GetType().Name,(e as SqlException)?.Number);
        return e is SqlException sql ? sql.Number switch
        {
            2601 or 2627=>"El código de barras o la pareja producto/proveedor ya existe. Revisa los datos.",
            547=>"Una relación o restricción impide la operación. Revisa categoría, proveedor, importes y dependencias; los catálogos con movimientos deben inactivarse.",
            229 or 230=>"El usuario de SQL Server no tiene permiso para esta operación.",
            207 or 208=>"El esquema configurado no contiene las tablas o columnas requeridas de Productos y Producto–Proveedor.",
            1205=>"La operación coincidió con otra transacción y fue revertida. Actualiza el listado antes de reintentar.",
            8115=>"Un importe o plazo excede el límite del esquema.",
            _=>"No se pudo completar la operación con SQL Server. Actualiza el listado antes de reintentar."
        }:"No se pudo completar la operación. Actualiza el listado antes de reintentar.";
    }
}
