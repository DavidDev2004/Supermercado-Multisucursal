using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SupermercadoMVC.Models;

public sealed class AjusteFormViewModel : IValidatableObject
{
    public int? CodigoAjuste { get; set; }
    [Required(ErrorMessage = "Selecciona una sucursal.")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona una sucursal válida.")]
    public int? CodigoSucursal { get; set; }
    [Required(ErrorMessage = "Selecciona un empleado.")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un empleado válido.")]
    public int? CodigoEmpleado { get; set; }
    [Required(ErrorMessage = "El número de ajuste es obligatorio.")]
    [StringLength(30, ErrorMessage = "El número admite hasta 30 caracteres.")]
    public string NumeroAjuste { get; set; } = "";
    [Required(ErrorMessage = "La fecha es obligatoria.")]
    public DateTime? FechaAjuste { get; set; }
    [Required(ErrorMessage = "El motivo es obligatorio.")]
    [StringLength(200, ErrorMessage = "El motivo admite hasta 200 caracteres.")]
    public string Motivo { get; set; } = "";
    [StringLength(300, ErrorMessage = "Las observaciones admiten hasta 300 caracteres.")]
    public string? Observaciones { get; set; }
    [Required(ErrorMessage = "Revisa los detalles del ajuste.")]
    public List<AjusteLineaViewModel> Lineas { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (string.IsNullOrWhiteSpace(NumeroAjuste))
            yield return new("El número de ajuste es obligatorio.", [nameof(NumeroAjuste)]);
        if (string.IsNullOrWhiteSpace(Motivo))
            yield return new("El motivo es obligatorio.", [nameof(Motivo)]);
        if (FechaAjuste.HasValue && FechaAjuste.Value.TimeOfDay != TimeSpan.Zero)
            yield return new("Selecciona una fecha sin hora.", [nameof(FechaAjuste)]);
        var products = new HashSet<int>();
        if (Lineas is null)
        {
            yield return new("Revisa los detalles del ajuste.", [nameof(Lineas)]);
            yield break;
        }
        for (var i = 0; i < Lineas.Count; i++)
        {
            var line = Lineas[i];
            if (line is null)
                yield return new("Revisa las líneas del ajuste.", [nameof(Lineas)]);
            else if (line.CodigoProducto is int product && !products.Add(product))
                yield return new("No repitas un producto en el ajuste.", [$"Lineas[{i}].CodigoProducto"]);
        }
    }
}

public sealed class AjusteLineaViewModel : IValidatableObject
{
    [Required(ErrorMessage = "Selecciona un producto en cada línea.")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un producto válido.")]
    public int? CodigoProducto { get; set; }
    [Required(ErrorMessage = "Indica la cantidad de cada línea.")]
    public decimal? Cantidad { get; set; }
    [BindNever]
    public decimal? CostoUnitario { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (Cantidad.HasValue && (Cantidad == 0 || Cantidad < -9999999999.99m ||
            Cantidad > 9999999999.99m || decimal.Round(Cantidad.Value, 2) != Cantidad.Value))
            yield return new("La cantidad debe ser distinta de cero, con hasta dos decimales y dentro de DECIMAL(12,2).", [nameof(Cantidad)]);
    }
}

public sealed record AjusteOption(int Id, string Nombre);
public sealed record AjusteEmployee(int Id, string Nombre, int CodigoSucursal);
public sealed record AjusteRow(int CodigoAjuste, string NumeroAjuste, DateTime FechaAjuste,
    string Sucursal, string Empleado, string Motivo, string Estado, int CantidadLineas, decimal? Valor);
public sealed record AjusteDetail(AjusteFormViewModel Input, string Estado);

public sealed class AjustesIndexViewModel
{
    public IReadOnlyList<AjusteRow> Rows { get; set; } = [];
    public IReadOnlyList<AjusteOption> Sucursales { get; set; } = [];
    public IReadOnlyList<AjusteEmployee> Empleados { get; set; } = [];
    public IReadOnlyList<AjusteOption> Productos { get; set; } = [];
    public AjusteFormViewModel Input { get; set; } = new() { FechaAjuste = DateTime.Today };
    public string EstadoAjuste { get; set; } = "Borrador";
    public string Search { get; set; } = "";
    public string State { get; set; } = "";
    public int? Branch { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize => 8;
    public long Total { get; set; }
    public long TotalAjustes { get; set; }
    public long Borradores { get; set; }
    public long Aplicados { get; set; }
    public long Anulados { get; set; }
    public int PageCount => (int)Math.Max(1, Math.Min(int.MaxValue, (Total + PageSize - 1) / PageSize));
    public bool OpenModal { get; set; }
    public bool ReadOnly { get; set; }
    public string? Error { get; set; }
    public string? Notice { get; set; }
}
