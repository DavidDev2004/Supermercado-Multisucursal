using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SupermercadoMVC.Security;

namespace SupermercadoMVC.Filters;

public sealed class DemoSessionFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // El filtro ya está aplicado a todos los módulos existentes. La cookie
        // autenticada reemplaza la antigua sesión demostrativa.
        if (context.HttpContext.User.Identity?.IsAuthenticated != true)
        {
            var target = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
            context.Result = new RedirectToActionResult("Index", "Access", new { returnUrl = target.ToString() });
            return;
        }

        var controller = context.RouteData.Values["controller"]?.ToString();
        var action = context.RouteData.Values["action"]?.ToString();
        var slug = context.ActionArguments.TryGetValue("slug", out var slugValue)
            ? slugValue?.ToString()?.ToLowerInvariant() : context.RouteData.Values["slug"]?.ToString()?.ToLowerInvariant();
        if (controller == "BusinessIntelligence")
            slug = context.ActionArguments.TryGetValue("module", out var moduleValue)
                ? moduleValue?.ToString()?.ToLowerInvariant() : context.RouteData.Values["module"]?.ToString()?.ToLowerInvariant();
        var type = context.ActionArguments.TryGetValue("tipo", out var typeValue)
            ? typeValue?.ToString() : context.HttpContext.Request.Query["tipo"].ToString();
        if (!RolePermissions.CanAccess(context.HttpContext.User, controller, action, slug, type))
        {
            context.Result = new ForbidResult();
            return;
        }

        await next();
    }
}
