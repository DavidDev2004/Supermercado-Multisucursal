using System.ComponentModel.DataAnnotations;

namespace SupermercadoMVC.Models;

public sealed class SaleInput
{
    public int? CodigoVenta { get; set; }
    [Required(ErrorMessage = "Cliente es obligatorio.")]
    public int? CodigoCliente { get; set; }
    [Required(ErrorMessage = "Sucursal es obligatoria.")]
    public int? CodigoSucursal { get; set; }
    [Required(ErrorMessage = "Empleado es obligatorio.")]
    public int? CodigoEmpleado { get; set; }
    [Required(ErrorMessage = "Número de venta es obligatorio.")]
    [StringLength(30, ErrorMessage = "Número de venta permite hasta 30 caracteres.")]
    public string NumeroVenta { get; set; } = "";
    [Required(ErrorMessage = "Fecha de venta es obligatoria.")]
    public DateTime? FechaVenta { get; set; }
    [StringLength(300, ErrorMessage = "Observaciones permite hasta 300 caracteres.")]
    public string? Observaciones { get; set; }
    public List<SaleLineInput> Lineas { get; set; } = [];
}

public sealed class SaleLineInput
{
    [Required(ErrorMessage = "Selecciona un producto.")]
    public int? CodigoProducto { get; set; }
    [Required(ErrorMessage = "Cantidad es obligatoria.")]
    public decimal? Cantidad { get; set; }
    [Required(ErrorMessage = "Precio es obligatorio.")]
    public decimal? PrecioUnitario { get; set; }
    [Required(ErrorMessage = "Descuento es obligatorio.")]
    public decimal? Descuento { get; set; } = 0;
    // Solo lectura: este valor siempre procede de SQL Server, nunca del POST.
    public decimal? CostoUnitarioHistorico { get; set; }
}

public sealed record SaleOption(int Id, string Nombre);
public sealed record SaleEmployee(int Id, string Nombre, int CodigoSucursal);
public sealed record SaleProduct(int Id, string Nombre, decimal PrecioVenta);
public sealed record SaleStock(int CodigoSucursal, int CodigoProducto, decimal Existencia, decimal CostoPromedio);
public sealed record SaleRow(int CodigoVenta, string NumeroVenta, DateTime FechaVenta, string Cliente,
    string Sucursal, string Empleado, string Estado, decimal Total);
public sealed record SaleDetail(SaleInput Input, string Estado);

public sealed class SalesPage
{
    public IReadOnlyList<SaleRow> Rows { get; set; } = [];
    public IReadOnlyList<SaleOption> Clientes { get; set; } = [];
    public IReadOnlyList<SaleOption> Sucursales { get; set; } = [];
    public IReadOnlyList<SaleEmployee> Empleados { get; set; } = [];
    public IReadOnlyList<SaleProduct> Productos { get; set; } = [];
    public IReadOnlyList<SaleStock> Inventario { get; set; } = [];
    public SaleInput Input { get; set; } = new() { FechaVenta = DateTime.Today };
    public string EstadoVenta { get; set; } = "Borrador";
    public string Search { get; set; } = "";
    public string State { get; set; } = "";
    public int Page { get; set; } = 1;
    public int PageSize => 8;
    public long Total { get; set; }
    public long Borradores { get; set; }
    public long Confirmadas { get; set; }
    public decimal ImporteConfirmado { get; set; }
    public int PageCount => (int)Math.Max(1, (Total + PageSize - 1) / PageSize);
    public bool OpenModal { get; set; }
    public bool ReadOnly { get; set; }
    public string? Error { get; set; }
    public string? Notice { get; set; }
}
