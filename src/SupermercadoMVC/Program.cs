using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;
using System.Security.Claims;
using SupermercadoMVC.Data;
using SupermercadoMVC.Models;
using SupermercadoMVC.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".SupermercadoGT.Demo";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.IdleTimeout = TimeSpan.FromHours(3);
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/acceso";
        options.AccessDeniedPath = "/acceso/denegado";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        options.Cookie.Name = ".SupermercadoGT.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.IsEssential = true;
        options.Events.OnValidatePrincipal = async context =>
        {
            var idText = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = context.Principal?.FindFirstValue(ClaimTypes.Role);
            if (!int.TryParse(idText, out var id) || !RolePermissions.IsKnown(role))
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return;
            }
            try
            {
                var account = await context.HttpContext.RequestServices
                    .GetRequiredService<UsuarioRepository>()
                    .ObtenerActivoPorIdAsync(id, context.HttpContext.RequestAborted);
                if (account is not null && account.Rol == role &&
                    account.Usuario == context.Principal?.Identity?.Name) return;
            }
            catch (Exception error) when (error is not OperationCanceledException)
            {
                context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("CookieValidation")
                    .LogWarning("No se pudo validar la sesión: {Type}.", error.GetType().Name);
            }
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddHttpsRedirection(options => options.HttpsPort = 443);
builder.Services.AddScoped<SupermercadoMVC.Data.DatabaseConnectionFactory>();
builder.Services.AddScoped<SupermercadoMVC.Data.DashboardDataService>();
builder.Services.AddScoped<SupermercadoMVC.Data.InicioDataService>();
builder.Services.AddScoped<SupermercadoMVC.Data.CatalogDataService>();
builder.Services.AddScoped<SupermercadoMVC.Data.SalesDataService>();
builder.Services.AddScoped<SupermercadoMVC.Data.CompraRepository>();
builder.Services.AddScoped<SupermercadoMVC.Data.AjustesDataService>();
builder.Services.AddScoped<SupermercadoMVC.Data.ProductosDataService>();
builder.Services.AddScoped<SupermercadoMVC.Data.InventarioDataService>();
builder.Services.AddScoped<SupermercadoMVC.Data.PaymentsDataService>();
builder.Services.AddScoped<SupermercadoMVC.Data.ReportesDataService>();
builder.Services.AddScoped<SupermercadoMVC.Data.BusinessIntelligenceDataService>();
builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<UsuarioInicialProvisioner>();
builder.Services.AddScoped<DemoUsersProvisioner>();
builder.Services.AddScoped<IPasswordHasher<UsuarioAuth>, PasswordHasher<UsuarioAuth>>();
builder.Services.AddScoped<SupermercadoMVC.Filters.DemoSessionFilter>();

var app = builder.Build();

if (args.Contains("--crear-usuario-inicial", StringComparer.Ordinal))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<UsuarioInicialProvisioner>().CrearAsync();
    return;
}

if (args.Contains("--provisionar-demo", StringComparer.Ordinal))
{
    var db = args.FirstOrDefault(arg => arg.StartsWith("--esperar-base=", StringComparison.Ordinal))?
        ["--esperar-base=".Length..] ?? "";
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<DemoUsersProvisioner>().CrearAsync(db);
    return;
}

if (!app.Environment.IsDevelopment())
{
    // El agente HTTPS local entrega la solicitud a Kestrel por HTTP desde loopback.
    // Confiar solo en ese proxy permite emitir cookies y redirecciones HTTPS correctas.
    var forwardedHeaders = new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
        ForwardLimit = 1
    };
    forwardedHeaders.KnownProxies.Add(IPAddress.Loopback);
    forwardedHeaders.KnownProxies.Add(IPAddress.IPv6Loopback);
    app.UseForwardedHeaders(forwardedHeaders);

    app.UseExceptionHandler("/Home/Error");
    // Los túneles terminan HTTPS fuera del host. Mantener HTTP local para
    // diagnóstico, pero redirigir cualquier hostname público a HTTPS.
    app.UseWhen(context => !IsLoopbackHost(context.Request.Host.Host),
        branch => branch.UseHttpsRedirection());
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseSession();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute("access", "acceso", new { controller = "Access", action = "Index" });
app.MapControllerRoute("access-denied", "acceso/denegado", new { controller = "Access", action = "Denied" });
app.MapControllerRoute("catalog", "catalogos/{slug}/{action=Index}/{id?}", new { controller = "Catalog" });
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

static bool IsLoopbackHost(string host) =>
    string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase) ||
    (IPAddress.TryParse(host, out var address) && IPAddress.IsLoopback(address));
