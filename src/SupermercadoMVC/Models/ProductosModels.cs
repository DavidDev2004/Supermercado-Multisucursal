using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SupermercadoMVC.Models;

public sealed class ProductoForm : IValidatableObject
{
    [BindNever] public int? CodigoProducto { get; set; }
    [Required(ErrorMessage = "Selecciona una categoría.")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona una categoría válida.")] public int? CodigoCategoria { get; set; }
    [Required(ErrorMessage = "El código de barras es obligatorio."), StringLength(30, ErrorMessage = "El código de barras admite hasta 30 caracteres.")] public string CodigoBarra { get; set; } = "";
    [Required(ErrorMessage = "El nombre es obligatorio."), StringLength(150, ErrorMessage = "El nombre admite hasta 150 caracteres.")] public string Nombre { get; set; } = "";
    [Required(ErrorMessage = "La marca es obligatoria."), StringLength(100, ErrorMessage = "La marca admite hasta 100 caracteres.")] public string Marca { get; set; } = "";
    [StringLength(200, ErrorMessage = "La descripción admite hasta 200 caracteres.")] public string? Descripcion { get; set; }
    [Required(ErrorMessage = "La unidad de medida es obligatoria."), StringLength(30, ErrorMessage = "La unidad de medida admite hasta 30 caracteres.")] public string UnidadMedida { get; set; } = "";
    [Required(ErrorMessage = "Indica el precio de venta.")]
    public decimal? PrecioVenta { get; set; }
    [Required(ErrorMessage = "Indica la fecha de registro.")]
    public DateTime? FechaRegistro { get; set; }
    [Required(ErrorMessage = "Selecciona un estado."), RegularExpression("^(Activo|Inactivo)$", ErrorMessage = "Selecciona Activo o Inactivo.")]
    public string Estado { get; set; } = "";
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (PrecioVenta.HasValue && !ProductoValidation.PositiveMoney(PrecioVenta.Value))
            yield return new("El precio debe ser positivo, con hasta dos decimales y máximo 9999999999.99.", [nameof(PrecioVenta)]);
        if (FechaRegistro.HasValue && FechaRegistro.Value.TimeOfDay != TimeSpan.Zero)
            yield return new("La fecha de registro no admite hora.", [nameof(FechaRegistro)]);
    }
}

public sealed class ProductoProveedorForm : IValidatableObject
{
    [BindNever] public int? CodigoProductoProveedor { get; set; }
    [Required(ErrorMessage = "Selecciona un proveedor.")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un proveedor válido.")] public int? CodigoProveedor { get; set; }
    [Required(ErrorMessage = "El código externo es obligatorio."), StringLength(50, ErrorMessage = "El código externo admite hasta 50 caracteres.")] public string CodigoExterno { get; set; } = "";
    [Required(ErrorMessage = "Indica el costo de referencia.")]
    public decimal? CostoReferencia { get; set; }
    [Required(ErrorMessage = "Indica el plazo de entrega.")]
    [Range(0, int.MaxValue, ErrorMessage = "El plazo debe ser un entero mayor o igual a cero.")]
    public int? PlazoEntrega { get; set; }
    [Required(ErrorMessage = "Indica la fecha de registro.")]
    public DateTime? FechaRegistro { get; set; }
    [Required(ErrorMessage = "Selecciona un estado."), RegularExpression("^(Activo|Inactivo)$", ErrorMessage = "Selecciona Activo o Inactivo.")]
    public string Estado { get; set; } = "";
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (CostoReferencia.HasValue && !ProductoValidation.PositiveMoney(CostoReferencia.Value))
            yield return new("El costo debe ser positivo, con hasta dos decimales y máximo 9999999999.99.", [nameof(CostoReferencia)]);
        if (FechaRegistro.HasValue && FechaRegistro.Value.TimeOfDay != TimeSpan.Zero)
            yield return new("La fecha de registro no admite hora.", [nameof(FechaRegistro)]);
    }
}

public static class ProductoValidation
{
    public static bool PositiveMoney(decimal value) => value > 0 && value <= 9999999999.99m && decimal.Round(value, 2) == value;
}
public sealed record ProductoOption(int Id, string Nombre);
public sealed record ProductoRow(int Id, string CodigoBarra, string Nombre, string Marca, string Categoria,
    string UnidadMedida, decimal PrecioVenta, string Estado, int Proveedores);
public sealed record ProductoProveedorRow(int Id, int CodigoProveedor, string Proveedor, string CodigoExterno,
    decimal CostoReferencia, int PlazoEntrega, DateTime FechaRegistro, string Estado);

public sealed class ProductosPage
{
    public List<ProductoRow> Rows { get; set; } = [];
    public List<ProductoOption> Categorias { get; set; } = [];
    public List<ProductoOption> Proveedores { get; set; } = [];
    public List<ProductoProveedorRow> Relaciones { get; set; } = [];
    public ProductoForm Input { get; set; } = new() { FechaRegistro = DateTime.Today, Estado = "Activo" };
    public ProductoProveedorForm Relacion { get; set; } = new() { FechaRegistro = DateTime.Today, Estado = "Activo" };
    public string Search { get; set; } = "";
    public string State { get; set; } = "";
    public int? Category { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize => 8;
    public long Total { get; set; }
    public long TotalProductos { get; set; }
    public long Activos { get; set; }
    public long Inactivos { get; set; }
    public long TotalRelaciones { get; set; }
    public int PageCount => (int)Math.Max(1, Math.Min(int.MaxValue, (Total + PageSize - 1) / PageSize));
    public bool OpenProduct { get; set; }
    public bool OpenSuppliers { get; set; }
    public string? Notice { get; set; }
    public string? Error { get; set; }
}
