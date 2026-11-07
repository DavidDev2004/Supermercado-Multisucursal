using System.Security.Claims;

namespace SupermercadoMVC.Security;

public static class RolePermissions
{
    public const string Administrador = "Administrador";
    public const string Bodega = "Bodega";
    public const string Caja = "Caja";

    public static bool IsKnown(string? role) => role is Administrador or Bodega or Caja;

    public static string DefaultReport(ClaimsPrincipal user) => user.IsInRole(Bodega) ? "compras" : "ventas";

    public static string NormalizeReport(ClaimsPrincipal user, string? type) =>
        type is "ventas" or "compras" or "inventario" or "saldos" ? type : DefaultReport(user);

    public static bool CanViewReport(ClaimsPrincipal user, string? type) =>
        user.IsInRole(Administrador) ||
        (user.IsInRole(Bodega) && type is "compras" or "inventario") ||
        (user.IsInRole(Caja) && type is "ventas" or "saldos");

    public static bool CanAccess(ClaimsPrincipal user, string? controller, string? action,
        string? slug = null, string? reportType = null)
    {
        if (user.IsInRole(Administrador)) return true;
        if (user.IsInRole(Bodega))
        {
            return controller switch
            {
                "Home" => action == "Index",
                "Inicio" => action == "Bodega",
                "BusinessIntelligence" => action == "Index" && slug is "m04" or "m05" or "m06" or "m07" or "m08",
                "Catalog" => slug is "categorias" or "proveedores",
                "Productos" or "Compras" or "Inventario" or "Ajustes" => true,
                "Modules" when action == "Csv" => CanViewReport(user, NormalizeReport(user, reportType)),
                "Modules" when action == "Index" => slug switch
                {
                    "productos" or "compras" or "inventario" => true,
                    "reportes" => CanViewReport(user, NormalizeReport(user, reportType)),
                    _ => false
                },
                _ => false
            };
        }
        if (user.IsInRole(Caja))
        {
            return controller switch
            {
                "BusinessIntelligence" => action == "Index" && slug is "m01" or "m02" or "m03",
                "Home" => action == "Index",
                "Inicio" => action == "Caja",
                "Catalog" => slug == "clientes",
                "Sales" or "Payments" => true,
                "Modules" when action == "Csv" => CanViewReport(user, NormalizeReport(user, reportType)),
                "Modules" when action == "Index" => slug switch
                {
                    "ventas" => true,
                    "reportes" => CanViewReport(user, NormalizeReport(user, reportType)),
                    _ => false
                },
                _ => false
            };
        }
        return false;
    }
}
