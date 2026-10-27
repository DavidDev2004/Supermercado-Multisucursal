using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SupermercadoMVC.Data;
using SupermercadoMVC.Filters;
using SupermercadoMVC.Models;

namespace SupermercadoMVC.Controllers;

[Route("ventas")]
[ServiceFilter(typeof(DemoSessionFilter))]
public sealed class SalesController(SalesDataService sales, ILogger<SalesController> logger) : Controller
{
    private static string Search(string? value)
    {
        var text = (value ?? "").Trim();
        return text.Length > 100 ? text[..100] : text;
    }
    private static string State(string? value) => value is "Borrador" or "Confirmada" or "Anulada" ? value : "";

    [HttpGet("")]
    public async Task<IActionResult> Index(string? buscar, string? estado, int pagina = 1, int? venta = null, string? modo = null, bool nuevo = false, CancellationToken cancellationToken = default)
    {
        var model = await PageAsync(buscar, estado, pagina, cancellationToken);
        if (venta.HasValue)
        {
            try
            {
                var detail = await sales.GetAsync(venta.Value, cancellationToken);
                if (detail is null) { model.Error = "La venta ya no existe."; return View(model); }
                model.Input = detail.Input; model.EstadoVenta = detail.Estado;
                model.OpenModal = true; model.ReadOnly = modo != "editar" || detail.Estado != "Borrador";
            }
            catch (Exception exception) { model.Error = Message(exception); }
        }
        else model.OpenModal = nuevo;
        return View(model);
    }

    [HttpPost("guardar/{id:int?}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save([Bind(Prefix = "Input")] SaleInput input, int? id, string? buscar, string? estado, int pagina = 1, CancellationToken cancellationToken = default)
    {
        input.CodigoVenta = id;
        Validate(input);
        if (ModelState.IsValid)
        {
            try
            {
                await sales.SaveAsync(input, cancellationToken);
                TempData["SalesNotice"] = "Borrador guardado correctamente. El inventario no se modificó.";
                return RedirectToAction(nameof(Index), new { buscar, estado, pagina });
            }
            catch (Exception exception) { ModelState.AddModelError("", Message(exception)); }
        }
        var model = await PageAsync(buscar, estado, pagina, cancellationToken);
        model.Input = input; model.OpenModal = true;
        return View("Index", model);
    }

    [HttpPost("confirmar/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(int id, string? buscar, string? estado, int pagina = 1, CancellationToken cancellationToken = default)
    {
        try { await sales.ConfirmAsync(id, cancellationToken); TempData["SalesNotice"] = "Venta confirmada. Inventario actualizado una sola vez."; }
        catch (Exception exception) { TempData["SalesError"] = Message(exception); }
        return RedirectToAction(nameof(Index), new { buscar, estado, pagina });
    }

    [HttpPost("eliminar/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, string? buscar, string? estado, int pagina = 1, CancellationToken cancellationToken = default)
    {
        try { await sales.DeleteAsync(id, cancellationToken); TempData["SalesNotice"] = "Borrador y detalles eliminados. El inventario no se modificó."; }
        catch (Exception exception) { TempData["SalesError"] = Message(exception); }
        return RedirectToAction(nameof(Index), new { buscar, estado, pagina });
    }

    private async Task<SalesPage> PageAsync(string? search, string? state, int page, CancellationToken ct)
    {
        SalesPage model;
        try { model = await sales.GetPageAsync(Search(search), State(state), Math.Max(1, page), ct); }
        catch (Exception exception) { model = new() { Search = Search(search), State = State(state), Error = Message(exception) }; }
        model.Notice = TempData["SalesNotice"] as string;
        model.Error ??= TempData["SalesError"] as string;
        return model;
    }

    private void Validate(SaleInput input)
    {
        input.NumeroVenta = (input.NumeroVenta ?? "").Trim();
        if (input.NumeroVenta.Length == 0) ModelState.AddModelError("Input.NumeroVenta", "Número de venta es obligatorio.");
        if (input.FechaVenta.HasValue && input.FechaVenta.Value.TimeOfDay != TimeSpan.Zero)
            ModelState.AddModelError("Input.FechaVenta", "Selecciona una fecha sin hora.");
        var products = new HashSet<int>();
        for (var i = 0; i < input.Lineas.Count; i++)
        {
            var line = input.Lineas[i]; line.CostoUnitarioHistorico = null;
            var key = $"Input.Lineas[{i}]";
            if (line.CodigoProducto.HasValue && !products.Add(line.CodigoProducto.Value)) ModelState.AddModelError(key + ".CodigoProducto", "No repitas un producto en la misma venta.");
            Check(line.Cantidad, key + ".Cantidad", "Cantidad", true);
            Check(line.PrecioUnitario, key + ".PrecioUnitario", "Precio", true);
            Check(line.Descuento, key + ".Descuento", "Descuento", false);
            if (line.Cantidad.HasValue && line.PrecioUnitario.HasValue && line.Descuento.HasValue &&
                line.Cantidad.Value is > 0 and <= 9999999999.99m && line.PrecioUnitario.Value is > 0 and <= 9999999999.99m &&
                line.Descuento > line.Cantidad.Value * line.PrecioUnitario.Value)
                ModelState.AddModelError(key + ".Descuento", "El descuento no puede superar cantidad × precio.");
        }
    }

    private void Check(decimal? value, string key, string label, bool positive)
    {
        if (!value.HasValue) return; // Required/model binding ya informa el campo vacío o ilegible.
        if (value > 9999999999.99m || value < 0 || (positive && value == 0) || decimal.Round(value.Value, 2) != value)
            ModelState.AddModelError(key, $"{label}: {(positive ? "debe ser mayor que cero" : "no puede ser negativo")}, máximo 9999999999.99 y hasta dos decimales.");
    }

    private string Message(Exception exception)
    {
        if (exception is SaleRuleException)
        {
            logger.LogWarning("Sales operation rejected: {ExceptionType}.", exception.GetType().Name);
            return exception.Message;
        }
        logger.LogWarning("Sales failed: {ExceptionType}, SQL {Number}.", exception.GetType().Name, (exception as SqlException)?.Number);
        return exception is SqlException sql ? sql.Number switch
        {
            2601 or 2627 => "El número de venta ya existe o se repitió un producto. Revisa los datos.",
            547 => "La operación incumple una relación o restricción. Si hay pagos relacionados, el borrador no puede eliminarse.",
            1205 => "La operación coincidió con otra transacción y fue revertida. Consulta el estado antes de reintentar.",
            229 or 230 => "El usuario de SQL Server no tiene permiso para esta operación.",
            _ => "No se pudo completar la operación con SQL Server. Consulta el estado de la venta antes de reintentar."
        } : "No se pudo completar la operación. Consulta el estado de la venta antes de reintentar.";
    }
}
