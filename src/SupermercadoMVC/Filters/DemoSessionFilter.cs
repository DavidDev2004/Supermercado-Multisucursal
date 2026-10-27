using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

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

        await next();
    }
}
