using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using SupermercadoMVC.Data;
using SupermercadoMVC.Models;

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
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        options.Cookie.Name = ".SupermercadoGT.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.IsEssential = true;
    });
builder.Services.AddAuthorization();
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
builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<UsuarioInicialProvisioner>();
builder.Services.AddScoped<IPasswordHasher<UsuarioAuth>, PasswordHasher<UsuarioAuth>>();
builder.Services.AddScoped<SupermercadoMVC.Filters.DemoSessionFilter>();

var app = builder.Build();

if (args.Contains("--crear-usuario-inicial", StringComparer.Ordinal))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<UsuarioInicialProvisioner>().CrearAsync();
    return;
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHttpsRedirection();
    app.UseHsts();
}

app.UseRouting();
app.UseAuthentication();
app.UseSession();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute("access", "acceso", new { controller = "Access", action = "Index" });
app.MapControllerRoute("catalog", "catalogos/{slug}/{action=Index}/{id?}", new { controller = "Catalog" });
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
