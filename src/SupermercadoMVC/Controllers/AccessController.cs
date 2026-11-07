using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SupermercadoMVC.Data;
using SupermercadoMVC.Models;
using SupermercadoMVC.Security;

namespace SupermercadoMVC.Controllers;

// Adapta AccountController de la integrante a la ruta /acceso existente.
public sealed class AccessController(
    UsuarioRepository usuarios,
    IPasswordHasher<UsuarioAuth> hasher,
    ILogger<AccessController> logger) : Controller
{
    [HttpGet]
    public IActionResult Index(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return DestinoLocal(returnUrl);
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enter(LoginViewModel model, string? returnUrl = null, CancellationToken ct = default)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid) return View("Index", model);
        model.Usuario = model.Usuario.Trim();
        if (model.Usuario.Length == 0)
        {
            ModelState.AddModelError(nameof(model.Usuario), "Escribe tu usuario.");
            return View("Index", model);
        }
        try
        {
            var user = await usuarios.ObtenerActivoPorNombreAsync(model.Usuario, ct);
            var result = user is null ? PasswordVerificationResult.Failed :
                hasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
            if (user is null || result == PasswordVerificationResult.Failed || !RolePermissions.IsKnown(user.Rol))
            {
                ModelState.AddModelError("", "Credenciales incorrectas.");
                return View("Index", model);
            }
            if (result == PasswordVerificationResult.SuccessRehashNeeded &&
                !await usuarios.ActualizarHashSiNoCambioAsync(user, hasher.HashPassword(user, model.Password), ct))
            {
                ModelState.AddModelError("", "Credenciales incorrectas.");
                return View("Index", model);
            }
            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, user.CodigoUsuario.ToString()),
                new Claim(ClaimTypes.Name, user.Usuario),
                new Claim(ClaimTypes.Role, user.Rol)
            };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
                new AuthenticationProperties { IsPersistent = false, AllowRefresh = true, ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30) });
            return DestinoLocal(returnUrl);
        }
        catch (SqlException e) when (e.Number == 208)
        {
            logger.LogWarning("Login table is not installed.");
            ModelState.AddModelError("", "El acceso aún no está habilitado en SQL Server. Consulta al administrador del proyecto.");
            return View("Index", model);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            logger.LogError(e, "No se pudo validar el inicio de sesión.");
            ModelState.AddModelError("", "No fue posible iniciar sesión. Inténtalo de nuevo.");
            return View("Index", model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Exit()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("/acceso/denegado")]
    public IActionResult Denied()
    {
        if (User.Identity?.IsAuthenticated != true) return RedirectToAction(nameof(Index));
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View();
    }

    private IActionResult DestinoLocal(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);
        if (User.IsInRole(RolePermissions.Bodega)) return RedirectToAction("Bodega", "Inicio");
        if (User.IsInRole(RolePermissions.Caja)) return RedirectToAction("Caja", "Inicio");
        return RedirectToAction("Index", "Home");
    }
}
