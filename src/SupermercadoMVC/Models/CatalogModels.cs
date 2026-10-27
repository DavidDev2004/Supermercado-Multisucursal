using SupermercadoMVC.Data;

namespace SupermercadoMVC.Models;

public sealed class CatalogInput
{
    public int? Id { get; set; }
    public Dictionary<string, string?> Fields { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class CatalogPageViewModel
{
    public IReadOnlyList<SucursalOption> Sucursales { get; init; } = [];
    public required CatalogDefinition Definition { get; init; }
    public required IReadOnlyList<Dictionary<string, object?>> Rows { get; init; }
    public required CatalogInput Input { get; init; }
    public string Search { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 8;
    public long Total { get; init; }
    public long Active { get; init; }
    public long Inactive { get; init; }
    public string? Notice { get; init; }
    public string? Error { get; init; }
    public string? OpenModal { get; init; }
    public int PageCount => Math.Max(1, (int)Math.Ceiling((double)Total / PageSize));
}

public sealed record SucursalOption(int CodigoSucursal, string Nombre);

public sealed record CatalogPageData(
    IReadOnlyList<Dictionary<string, object?>> Rows,
    long Total,
    long Active,
    long Inactive);
