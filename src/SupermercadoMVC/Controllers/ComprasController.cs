using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SupermercadoMVC.Data;
using SupermercadoMVC.Filters;
using SupermercadoMVC.Models;

namespace SupermercadoMVC.Controllers;

[Route("compras")]
[ServiceFilter(typeof(DemoSessionFilter))]
public sealed class ComprasController(CompraRepository compras, ILogger<ComprasController> logger) : Controller
{
    private static string Search(string? value)
    {
        var text = (value ?? "").Trim();
        return text.Length > 100 ? text[..100] : text;
    }
    private static string State(string? value) => value is "Borrador" or "Recibida" or "Anulada" ? value : "";

    [HttpGet("/modulos/compras")]
    public IActionResult Legacy() => RedirectToAction(nameof(Index));

    [HttpGet("")]
    public async Task<IActionResult> Index(string? buscar, string? estado, int pagina = 1, int? compra = null, string? modo = null, bool nuevo = false, CancellationToken cancellationToken = default)
    {
        var model = await PageAsync(buscar, estado, pagina, cancellationToken);
        if (compra.HasValue)
        {
            try
            {
                var detail = await compras.ObtenerParaEditarAsync(compra.Value, cancellationToken);
                if (detail is null) { model.Error = "La compra ya no existe."; return View(model); }
                model.Input = detail.Input; model.EstadoCompra = detail.Estado;
                model.FechaRecepcion = detail.FechaRecepcion;
                model.OpenModal = true; model.ReadOnly = modo != "editar" || detail.Estado != "Borrador";
            }
            catch (Exception exception) { model.Error = Message(exception); }
        }
        else model.OpenModal = nuevo;
        return View(model);
    }

    [HttpPost("guardar/{id:int?}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save([Bind(Prefix = "Input")] CompraFormViewModel input, int? id, string? buscar, string? estado, int pagina = 1, CancellationToken cancellationToken = default)
    {
        input.CodigoCompra = id;
        input.NumeroCompra = (input.NumeroCompra ?? "").Trim();
        if (ModelState.IsValid)
        {
            try
            {
                await compras.GuardarAsync(input, cancellationToken);
                TempData["ComprasNotice"] = "Borrador guardado correctamente. El inventario no se modificó.";
                return RedirectToAction(nameof(Index), new { buscar, estado, pagina });
            }
            catch (Exception exception) { ModelState.AddModelError("", Message(exception)); }
        }
        var model = await PageAsync(buscar, estado, pagina, cancellationToken);
        model.Input = input; model.OpenModal = true;
        return View("Index", model);
    }

    [HttpPost("recibir/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Receive(int id, DateTime? fechaRecepcion, string? buscar, string? estado, int pagina = 1, CancellationToken cancellationToken = default)
    {
        if (!fechaRecepcion.HasValue || !ModelState.IsValid)
        {
            TempData["ComprasError"] = "Indica una fecha de recepción válida.";
            return RedirectToAction(nameof(Index), new { buscar, estado, pagina });
        }
        try { await compras.RecibirAsync(id, fechaRecepcion.Value, cancellationToken); TempData["ComprasNotice"] = "Compra recibida. Existencias y costo promedio actualizados una sola vez."; }
        catch (Exception exception) { TempData["ComprasError"] = Message(exception); }
        return RedirectToAction(nameof(Index), new { buscar, estado, pagina });
    }

    [HttpPost("eliminar/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, string? buscar, string? estado, int pagina = 1, CancellationToken cancellationToken = default)
    {
        try { await compras.EliminarAsync(id, cancellationToken); TempData["ComprasNotice"] = "Borrador y detalles eliminados. El inventario no se modificó."; }
        catch (Exception exception) { TempData["ComprasError"] = Message(exception); }
        return RedirectToAction(nameof(Index), new { buscar, estado, pagina });
    }

    private async Task<ComprasIndexViewModel> PageAsync(string? search, string? state, int page, CancellationToken ct)
    {
        ComprasIndexViewModel model;
        try { model = await compras.BuscarAsync(Search(search), State(state), Math.Max(1, page), ct); }
        catch (Exception exception) { model = new() { Search = Search(search), State = State(state), Error = Message(exception) }; }
        model.Notice = TempData["ComprasNotice"] as string;
        model.Error ??= TempData["ComprasError"] as string;
        return model;
    }

    private string Message(Exception exception)
    {
        if (exception is CompraRuleException)
        {
            logger.LogWarning("Compras operation rejected: {ExceptionType}.", exception.GetType().Name);
            return exception.Message;
        }
        logger.LogWarning("Compras failed: {ExceptionType}, SQL {Number}.", exception.GetType().Name, (exception as SqlException)?.Number);
        return exception is SqlException sql ? sql.Number switch
        {
            2601 or 2627 => "El número de compra ya existe o se repitió un producto. Revisa los datos.",
            547 => "La operación incumple una relación o restricción. Revisa proveedor, sucursal, empleado y productos; los registros relacionados pueden impedir la eliminación.",
            1205 => "La operación coincidió con otra transacción y fue revertida. Consulta el estado antes de reintentar.",
            229 or 230 => "El usuario de SQL Server no tiene permiso para esta operación.",
            _ => "No se pudo completar la operación con SQL Server. Consulta el estado de la compra antes de reintentar."
        } : "No se pudo completar la operación. Consulta el estado de la compra antes de reintentar.";
    }
}
