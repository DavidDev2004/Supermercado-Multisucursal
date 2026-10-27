namespace SupermercadoMVC.Models;

public sealed class DashboardViewModel
{
    public long? TotalSucursales { get; set; }
    public long? SucursalesActivas { get; set; }
    public long? TotalCategorias { get; set; }
    public long? TotalProductos { get; set; }
    public long? TotalClientes { get; set; }
    public long? TotalProveedores { get; set; }
    public IReadOnlyList<MonthlySales> MonthlySales { get; set; } = [];
    public string? DatabaseError { get; set; }
}

public sealed record MonthlySales(string Label, decimal Total);
