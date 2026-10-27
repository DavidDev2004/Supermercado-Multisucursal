using System.ComponentModel.DataAnnotations;

namespace SupermercadoMVC.Models;

// Adaptado de CompraFormViewModel del ZIP de la integrante. El esquema principal
// usa NumeroCompra, fechas programada/recepción y descuento monetario por línea.
public sealed class CompraFormViewModel : IValidatableObject
{
    public int? CodigoCompra { get; set; }
    [Required(ErrorMessage = "Selecciona un proveedor.")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un proveedor válido.")]
    public int? CodigoProveedor { get; set; }
    [Required(ErrorMessage = "Selecciona una sucursal.")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona una sucursal válida.")]
    public int? CodigoSucursal { get; set; }
    [Required(ErrorMessage = "Selecciona un empleado.")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un empleado válido.")]
    public int? CodigoEmpleado { get; set; }
    [Required(ErrorMessage = "Número de compra es obligatorio.")]
    [StringLength(30, ErrorMessage = "Número de compra permite hasta 30 caracteres.")]
    public string NumeroCompra { get; set; } = "";
    [Required(ErrorMessage = "Fecha de compra es obligatoria.")]
    public DateTime? FechaCompra { get; set; }
    [Required(ErrorMessage = "Fecha de entrega programada es obligatoria.")]
    public DateTime? FechaEntregaProgramada { get; set; }
    [StringLength(300, ErrorMessage = "Observaciones permite hasta 300 caracteres.")]
    public string? Observaciones { get; set; }
    public List<CompraDetalleFormViewModel> Lineas { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (string.IsNullOrWhiteSpace(NumeroCompra))
            yield return new("Número de compra es obligatorio.", [nameof(NumeroCompra)]);
        if (FechaCompra?.TimeOfDay != TimeSpan.Zero && FechaCompra.HasValue)
            yield return new("Selecciona una fecha sin hora.", [nameof(FechaCompra)]);
        if (FechaEntregaProgramada?.TimeOfDay != TimeSpan.Zero && FechaEntregaProgramada.HasValue)
            yield return new("Selecciona una fecha sin hora.", [nameof(FechaEntregaProgramada)]);
        if (FechaEntregaProgramada < FechaCompra)
            yield return new("La entrega programada no puede ser anterior a la compra.", [nameof(FechaEntregaProgramada)]);
        var products = new HashSet<int>();
        for (var i = 0; i < Lineas.Count; i++)
            if (Lineas[i].CodigoProducto is int product && !products.Add(product))
                yield return new("No repitas un producto en la misma compra.", [$"Lineas[{i}].CodigoProducto"]);
        // Un borrador puede guardarse sin detalles; la recepción sí los exige.
    }
}

public sealed class CompraDetalleFormViewModel : IValidatableObject
{
    [Required(ErrorMessage = "Selecciona un producto.")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un producto válido.")]
    public int? CodigoProducto { get; set; }
    [Required(ErrorMessage = "Cantidad es obligatoria.")]
    public decimal? Cantidad { get; set; }
    [Required(ErrorMessage = "Costo unitario es obligatorio.")]
    public decimal? CostoUnitario { get; set; }
    [Required(ErrorMessage = "Descuento es obligatorio.")]
    public decimal? Descuento { get; set; } = 0;
    [StringLength(300, ErrorMessage = "Observaciones de línea permite hasta 300 caracteres.")]
    public string? Observaciones { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        foreach (var (name, value, positive) in new[] {
            (nameof(Cantidad), Cantidad, true), (nameof(CostoUnitario), CostoUnitario, true), (nameof(Descuento), Descuento, false) })
            if (value.HasValue && (value < 0 || (positive && value == 0) || value > 9999999999.99m || decimal.Round(value.Value, 2) != value))
                yield return new($"{name}: {(positive ? "debe ser mayor que cero" : "no puede ser negativo")}, máximo 9999999999.99 y hasta dos decimales.", [name]);
        if (Cantidad is > 0 and <= 9999999999.99m && CostoUnitario is > 0 and <= 9999999999.99m && Descuento > Cantidad * CostoUnitario)
            yield return new("El descuento monetario no puede superar cantidad × costo unitario.", [nameof(Descuento)]);
    }
}

public sealed record CompraOption(int Id, string Nombre);
public sealed record CompraProductoOption(int Id, string Nombre, string Proveedores);
public sealed record CompraEmployee(int Id, string Nombre, int CodigoSucursal);
public sealed record CompraRow(int CodigoCompra, string NumeroCompra, DateTime FechaCompra, string Proveedor,
    string Sucursal, string Empleado, string Estado, decimal Total);
public sealed record CompraDetail(CompraFormViewModel Input, string Estado, DateTime? FechaRecepcion);

public sealed class ComprasIndexViewModel
{
    public IReadOnlyList<CompraRow> Rows { get; set; } = [];
    public IReadOnlyList<CompraOption> Proveedores { get; set; } = [];
    public IReadOnlyList<CompraOption> Sucursales { get; set; } = [];
    public IReadOnlyList<CompraEmployee> Empleados { get; set; } = [];
    public IReadOnlyList<CompraProductoOption> Productos { get; set; } = [];
    public CompraFormViewModel Input { get; set; } = new() { FechaCompra = DateTime.Today, FechaEntregaProgramada = DateTime.Today };
    public string EstadoCompra { get; set; } = "Borrador";
    public DateTime? FechaRecepcion { get; set; }
    public string Search { get; set; } = "";
    public string State { get; set; } = "";
    public int Page { get; set; } = 1;
    public int PageSize => 8;
    public long Total { get; set; }
    public long Borradores { get; set; }
    public long Recibidas { get; set; }
    public decimal ImporteRecibido { get; set; }
    public int PageCount => (int)Math.Max(1, (Total + PageSize - 1) / PageSize);
    public bool OpenModal { get; set; }
    public bool ReadOnly { get; set; }
    public string? Error { get; set; }
    public string? Notice { get; set; }
}
