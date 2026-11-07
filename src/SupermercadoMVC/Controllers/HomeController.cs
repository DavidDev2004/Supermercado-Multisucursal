using Microsoft.AspNetCore.Mvc;
using SupermercadoMVC.Data;
using SupermercadoMVC.Filters;
using SupermercadoMVC.Security;

namespace SupermercadoMVC.Controllers;

[ServiceFilter(typeof(DemoSessionFilter))]
public sealed class HomeController(DashboardDataService dashboard, ILogger<HomeController> logger) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        if (User.IsInRole(RolePermissions.Bodega)) return RedirectToAction("Bodega", "Inicio");
        if (User.IsInRole(RolePermissions.Caja)) return RedirectToAction("Caja", "Inicio");
        var model = new Models.DashboardViewModel();
        try
        {
            model = await dashboard.GetAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning("Dashboard database read failed: {ExceptionType}", exception.GetType().Name);
            model.DatabaseError = "No se pudo leer SQL Server. Revisa la configuración local de User Secrets y que el servidor esté disponible.";
        }

        return View("Dashboard", model);
    }
}
