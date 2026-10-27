using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SupermercadoMVC.Data;
using SupermercadoMVC.Filters;
using SupermercadoMVC.Models;

namespace SupermercadoMVC.Controllers;

[Route("pagos")]
[ServiceFilter(typeof(DemoSessionFilter))]
public sealed class PaymentsController(PaymentsDataService payments, ILogger<PaymentsController> logger) : Controller
{
    private static string Search(string? value) => (value ?? "").Trim() is var s && s.Length > 100 ? s[..100] : s;
    private static string State(string? value) => value is "Borrador" or "Confirmada" or "Anulada" ? value : "";
    private static string Balance(string? value) => value is "pendiente" or "pagado" or "negativo" ? value : "";

    [HttpGet("")]
    public async Task<IActionResult> Index(string? buscar, string? estado, string? saldo, int pagina = 1,
        int? venta = null, bool registrar = false, CancellationToken cancellationToken = default)
    {
        var model = await PageAsync(buscar, estado, saldo, pagina, cancellationToken);
        if (venta.HasValue)
        {
            try
            {
                await payments.LoadDetailAsync(model, venta.Value, cancellationToken);
                model.Input.CodigoVenta = venta.Value;
                model.Register = registrar && model.Sale!.PuedePagar;
                if (registrar && !model.Register) model.Error = "Solo se permiten pagos para ventas Confirmadas con saldo positivo.";
            }
            catch (Exception exception) { model.Error = Message(exception); }
        }
        return View(model);
    }

    [HttpPost("registrar/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register([Bind(Prefix = "Input")] PaymentInput input, int id,
        string? buscar, string? estado, string? saldo, int pagina = 1, CancellationToken cancellationToken = default)
    {
        input.CodigoVenta = id;
        ModelState.Remove("Input.CodigoVenta"); // La venta se toma de la ruta y se valida nuevamente en SQL.
        if (input.FechaPago.HasValue && input.FechaPago.Value.TimeOfDay != TimeSpan.Zero)
            ModelState.AddModelError("Input.FechaPago", "Selecciona una fecha sin hora.");
        if (input.Monto.HasValue && !PaymentsDataService.ValidMoney(input.Monto, true))
            ModelState.AddModelError("Input.Monto", "Monto debe ser mayor que cero, máximo 9999999999.99 y hasta dos decimales.");
        if (input.Comision.HasValue && (!PaymentsDataService.ValidMoney(input.Comision, false) || input.Comision > input.Monto))
            ModelState.AddModelError("Input.Comision", "Comisión debe estar entre cero y el monto, con hasta dos decimales.");
        if (ModelState.IsValid)
        {
            try
            {
                await payments.RegisterAsync(input, cancellationToken);
                TempData["PaymentsNotice"] = "Pago registrado correctamente. La comisión no reduce el abono del cliente.";
                return RedirectToAction(nameof(Index), new { buscar, estado, saldo, pagina, venta = id });
            }
            catch (Exception exception) { ModelState.AddModelError("", Message(exception)); }
        }
        var model = await PageAsync(buscar, estado, saldo, pagina, cancellationToken);
        model.Input = input;
        try
        {
            await payments.LoadDetailAsync(model, id, cancellationToken);
            model.Register = model.Sale!.PuedePagar;
        }
        catch (Exception exception) { model.Error = Message(exception); }
        return View("Index", model);
    }

    private async Task<PaymentsPage> PageAsync(string? search, string? state, string? balance, int page, CancellationToken ct)
    {
        PaymentsPage model;
        try { model = await payments.GetPageAsync(Search(search), State(state), Balance(balance), Math.Max(1, page), ct); }
        catch (Exception exception) { model = new() { Search = Search(search), State = State(state), Balance = Balance(balance), Error = Message(exception) }; }
        model.Notice = TempData["PaymentsNotice"] as string;
        return model;
    }
    private string Message(Exception exception)
    {
        logger.LogWarning("Payments operation: {ExceptionType}, SQL {Number}.", exception.GetType().Name, (exception as SqlException)?.Number);
        if (exception is PaymentRuleException) return exception.Message;
        return exception is SqlException sql ? sql.Number switch
        {
            2601 or 2627 => "Ese comprobante ya existe. Consulta el historial antes de volver a registrar el pago.",
            547 => "La venta ya no existe o el pago incumple una restricción de SQL Server.",
            1205 => "La operación coincidió con otra transacción y fue revertida. Consulta el historial antes de reintentar.",
            229 or 230 => "El usuario de SQL Server no tiene permiso para esta operación.",
            _ => "No se pudo completar la operación con SQL Server. Consulta el historial y el comprobante antes de reintentar."
        } : "No se pudo completar la operación. Consulta el historial antes de reintentar.";
    }
}
