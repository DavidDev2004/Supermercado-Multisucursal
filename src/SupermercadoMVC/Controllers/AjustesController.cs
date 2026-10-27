using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SupermercadoMVC.Data;
using SupermercadoMVC.Filters;
using SupermercadoMVC.Models;

namespace SupermercadoMVC.Controllers;

[Route("ajustes")]
[ServiceFilter(typeof(DemoSessionFilter))]
public sealed class AjustesController(AjustesDataService ajustes, ILogger<AjustesController> logger) : Controller
{
    private static string Search(string? value)
    {
        var text = (value ?? "").Trim();
        return text.Length > 100 ? text[..100] : text;
    }
    private static string State(string? value) => value is "Borrador" or "Aplicado" or "Anulado" ? value : "";

    [HttpGet("/modulos/ajustes")]
    public IActionResult Legacy() => RedirectToAction(nameof(Index));

    [HttpGet("")]
    public async Task<IActionResult> Index(string? buscar, string? estado, int? sucursal, int pagina = 1,
        int? ajuste = null, string? modo = null, bool nuevo = false, CancellationToken cancellationToken = default)
    {
        var model = await PageAsync(buscar, estado, sucursal, pagina, cancellationToken);
        if (ajuste.HasValue)
        {
            try
            {
                var detail = await ajustes.ObtenerAsync(ajuste.Value, cancellationToken);
                if (detail is null) { model.Error = "El ajuste ya no existe."; return View(model); }
                model.Input = detail.Input; model.EstadoAjuste = detail.Estado;
                model.OpenModal = true; model.ReadOnly = modo != "editar" || detail.Estado != "Borrador";
            }
            catch (Exception e) when (e is not OperationCanceledException) { model.Error = Message(e); }
        }
        else model.OpenModal = nuevo;
        return View(model);
    }

    [HttpPost("guardar/{id:int?}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save([Bind(Prefix = "Input")] AjusteFormViewModel input, int? id,
        string? buscar, string? estado, int? sucursal, int pagina = 1, CancellationToken cancellationToken = default)
    {
        input.CodigoAjuste = id;
        input.NumeroAjuste = (input.NumeroAjuste ?? "").Trim(); input.Motivo = (input.Motivo ?? "").Trim();
        if (ModelState.IsValid)
        {
            try
            {
                await ajustes.GuardarAsync(input, cancellationToken);
                TempData["AjustesNotice"] = "Borrador guardado. El inventario no se modificó; el costo se fijará al aplicar.";
                return RedirectToAction(nameof(Index), new { buscar, estado, sucursal, pagina });
            }
            catch (Exception e) when (e is not OperationCanceledException) { ModelState.AddModelError("", Message(e)); }
        }
        var model = await PageAsync(buscar, estado, sucursal, pagina, cancellationToken);
        model.Input = input; model.OpenModal = true;
        return View("Index", model);
    }

    [HttpPost("aplicar/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(int id, string? buscar, string? estado, int? sucursal,
        int pagina = 1, CancellationToken cancellationToken = default)
    {
        try
        {
            await ajustes.AplicarAsync(id, cancellationToken);
            TempData["AjustesNotice"] = "Ajuste aplicado una sola vez. Existencias y costos históricos guardados; el costo promedio se conserva.";
        }
        catch (Exception e) when (e is not OperationCanceledException) { TempData["AjustesError"] = Message(e); }
        return RedirectToAction(nameof(Index), new { buscar, estado, sucursal, pagina });
    }

    [HttpPost("eliminar/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, string? buscar, string? estado, int? sucursal,
        int pagina = 1, CancellationToken cancellationToken = default)
    {
        try { await ajustes.EliminarAsync(id, cancellationToken); TempData["AjustesNotice"] = "Borrador y detalles eliminados. El inventario no se modificó."; }
        catch (Exception e) when (e is not OperationCanceledException) { TempData["AjustesError"] = Message(e); }
        return RedirectToAction(nameof(Index), new { buscar, estado, sucursal, pagina });
    }

    private async Task<AjustesIndexViewModel> PageAsync(string? search, string? state, int? branch, int page, CancellationToken ct)
    {
        AjustesIndexViewModel model;
        try { model = await ajustes.BuscarAsync(Search(search), State(state), branch > 0 ? branch : null, Math.Max(1, page), ct); }
        catch (Exception e) when (e is not OperationCanceledException)
        { model = new() { Search = Search(search), State = State(state), Branch = branch, Error = Message(e) }; }
        model.Notice = TempData["AjustesNotice"] as string; model.Error ??= TempData["AjustesError"] as string;
        return model;
    }

    private string Message(Exception exception)
    {
        if (exception is AjusteRuleException) return exception.Message;
        logger.LogWarning("Ajustes operation failed: {ExceptionType}, SQL {Number}.", exception.GetType().Name, (exception as SqlException)?.Number);
        return exception is SqlException sql ? sql.Number switch
        {
            2601 or 2627 => "El número de ajuste ya existe o se repitió un producto. Revisa los datos.",
            547 => "La operación incumple una relación o restricción. Revisa sucursal, empleado, productos y cantidades; los registros relacionados pueden impedir la eliminación.",
            207 => "Falta la columna de costo histórico o el esquema no coincide. Verifica que la migración 04 esté aplicada.",
            208 => "No se encuentran las tablas necesarias en la base configurada.",
            1205 => "La operación coincidió con otra transacción y fue revertida. Consulta el estado antes de reintentar.",
            8115 => "Una cantidad o existencia excede los límites numéricos del esquema.",
            229 or 230 => "El usuario de SQL Server no tiene permiso para esta operación.",
            _ => "No se pudo completar la operación con SQL Server. Consulta el estado del ajuste antes de reintentar."
        } : "No se pudo completar la operación. Consulta el estado del ajuste antes de reintentar.";
    }
}
