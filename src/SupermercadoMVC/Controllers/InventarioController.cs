using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SupermercadoMVC.Data;
using SupermercadoMVC.Filters;
using SupermercadoMVC.Models;

namespace SupermercadoMVC.Controllers;

[Route("inventario")]
[ServiceFilter(typeof(DemoSessionFilter))]
public sealed class InventarioController(InventarioDataService inventario, ILogger<InventarioController> logger) : Controller
{
    [HttpGet("/modulos/inventario")]
    public IActionResult Legacy() => RedirectToAction(nameof(Index));

    [HttpGet("")]
    public async Task<IActionResult> Index(string? buscar, int? sucursal, int? categoria,
        string? situacion, int pagina = 1, CancellationToken cancellationToken = default)
    {
        var term = (buscar ?? "").Trim();
        if (term.Length > 100) term = term[..100];
        var state = situacion is "BajoMinimo" or "SinExistencias" ? situacion : "";
        try
        {
            return View(await inventario.ConsultarAsync(term, sucursal > 0 ? sucursal : null,
                categoria > 0 ? categoria : null, state, Math.Max(1, pagina), cancellationToken));
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            logger.LogWarning("Inventario SELECT failed: {Type}, SQL {Number}.",
                error.GetType().Name, (error as SqlException)?.Number);
            return View(new InventarioPagina
            {
                Buscar = term, Sucursal = sucursal, Categoria = categoria, Situacion = state,
                Error = error is SqlException sql ? sql.Number switch
                {
                    208 or 207 => "El esquema SQL configurado no contiene las tablas o columnas esperadas de inventario.",
                    229 or 230 => "El usuario de SQL Server no tiene permiso de lectura sobre inventario o sus catálogos.",
                    _ => "No se pudo consultar el inventario. Revisa la conexión con SQL Server."
                } : "No se pudo consultar el inventario. Revisa la conexión con SQL Server."
            });
        }
    }
}
