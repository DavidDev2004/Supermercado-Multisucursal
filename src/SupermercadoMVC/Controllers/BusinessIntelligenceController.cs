using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SupermercadoMVC.Data;
using SupermercadoMVC.Filters;
using SupermercadoMVC.Models;

namespace SupermercadoMVC.Controllers;

[Route("inteligencia-negocios")]
[ServiceFilter(typeof(DemoSessionFilter))]
public sealed class BusinessIntelligenceController(BusinessIntelligenceDataService data,
    ILogger<BusinessIntelligenceController> logger) : Controller
{
    [HttpGet("{module}")]
    public async Task<IActionResult> Index(string module, [FromQuery] BiFilter filter, CancellationToken ct)
    {
        module = module.ToLowerInvariant();
        if (module is not ("m01" or "m02" or "m03" or "m04" or "m05" or "m06" or "m07" or "m08" or "m09")) return NotFound();
        var error = ModelState.IsValid ? Validate(filter) :
            "Uno o más filtros tienen un formato inválido. Revisa fechas e identificadores.";
        BiPage page;
        try
        {
            page = await data.GetAsync(module, filter, error is null, ct);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning("BI SELECT failed in {Module}: {Type}, SQL {Number}.",
                module, exception.GetType().Name, (exception as SqlException)?.Number);
            page = BusinessIntelligenceDataService.CreatePage(module, filter);
            error = "No se pudieron consultar los datos de SQL Server. Intenta actualizar la página.";
        }
        page.Error = error;
        return View(page);
    }

    private static string? Validate(BiFilter f)
    {
        if (f.Desde.HasValue && f.Hasta.HasValue && f.Desde > f.Hasta)
            return "La fecha inicial no puede ser posterior a la final.";
        if (f.Top is not (5 or 10 or 15)) return "El número de posiciones debe ser 5, 10 o 15.";
        if (f.ActualizarCada is not (0 or 30 or 60 or 120)) return "El intervalo de actualización no es válido.";
        if ((f.Departamento?.Length ?? 0) > 100 || (f.Municipio?.Length ?? 0) > 100 ||
            (f.Metodo?.Length ?? 0) > 50 || (f.Estado?.Length ?? 0) > 20)
            return "Un filtro excede la longitud permitida.";
        if (f.CondicionStock is not (null or "Normal" or "Bajo Mínimo" or "Sin Stock"))
            return "La condición de stock no es válida.";
        if (new[] { f.Sucursal, f.Categoria, f.Producto, f.Proveedor }.Any(v => v < 1))
            return "El identificador de filtro no es válido.";
        return null;
    }
}
