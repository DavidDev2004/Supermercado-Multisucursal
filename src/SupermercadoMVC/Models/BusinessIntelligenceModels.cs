namespace SupermercadoMVC.Models;

public sealed class BiFilter
{
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public int? Sucursal { get; set; }
    public int? Categoria { get; set; }
    public int? Producto { get; set; }
    public int? Proveedor { get; set; }
    public string? Departamento { get; set; }
    public string? Municipio { get; set; }
    public string? Metodo { get; set; }
    public string? Estado { get; set; }
    public string? CondicionStock { get; set; }
    public int Top { get; set; } = 10;
    public int ActualizarCada { get; set; }
}

public sealed record BiOption(string Value, string Label);
public sealed record BiMetric(string Code, string Label, decimal Value, string Format, decimal? Variation = null, string? Note = null, string? PendingReason = null);
public sealed record BiPoint(string Label, decimal Value, decimal? Other = null, string? Detail = null, string? FilterValue = null);
public sealed record BiSeries(string Label, IReadOnlyList<BiPoint> Points);
public sealed record BiChart(string Code, string Title, string Kind, string Format, IReadOnlyList<BiPoint> Points,
    string? OtherLabel = null, string? Note = null, string? CrossFilter = null, string? PendingReason = null,
    string? PrimaryLabel = null, IReadOnlyList<BiSeries>? Series = null);

public sealed class BiPage
{
    public required string Module { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required BiFilter Filter { get; init; }
    public string? Error { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public List<BiOption> Sucursales { get; } = [];
    public List<BiOption> Categorias { get; } = [];
    public List<BiOption> Productos { get; } = [];
    public List<BiOption> Proveedores { get; } = [];
    public List<BiOption> Departamentos { get; } = [];
    public List<BiOption> Municipios { get; } = [];
    public List<BiOption> Metodos { get; } = [];
    public List<BiOption> Estados { get; } = [];
    public List<BiMetric> Metrics { get; } = [];
    public List<BiChart> Charts { get; } = [];
}
