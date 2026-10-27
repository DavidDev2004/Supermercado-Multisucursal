using Microsoft.AspNetCore.Mvc;
using SupermercadoMVC.Data;
using SupermercadoMVC.Filters;

namespace SupermercadoMVC.Controllers;

[Route("Inicio")]
[ServiceFilter(typeof(DemoSessionFilter))]
public sealed class InicioController(InicioDataService dashboard, ILogger<InicioController> logger) : Controller
{
    [HttpGet("Administrador")]
    public Task<IActionResult> Administrador(CancellationToken cancellationToken) => PageAsync("Administrador", null, cancellationToken);
    [HttpGet("Bodega")]
    public Task<IActionResult> Bodega(CancellationToken cancellationToken) => PageAsync("Bodega", null, cancellationToken);
    [HttpGet("Caja")]
    public Task<IActionResult> Caja(string? buscarProducto, CancellationToken cancellationToken) => PageAsync("Caja", buscarProducto, cancellationToken);

    private async Task<IActionResult> PageAsync(string kind, string? search, CancellationToken ct)
    {
        var term = (search ?? "").Trim(); if (term.Length > 100) term = term[..100];
        try { return View(kind, await dashboard.GetAsync(kind, term, ct)); }
        catch (Exception exception)
        {
            logger.LogWarning("Inicio dashboard read failed: {ExceptionType}, {Page}.", exception.GetType().Name, kind);
            var model = InicioDataService.CreatePage(kind, term);
            model.Error = "No se pudieron consultar los datos de SQL Server. Intenta cargar la página nuevamente.";
            return View(kind, model);
        }
    }
}
