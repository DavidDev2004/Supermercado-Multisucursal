namespace SupermercadoMVC.Models;

public sealed record InventarioOpcion(int Id, string Nombre);

public sealed record InventarioFila(
    int CodigoInventario, int CodigoSucursal, int CodigoProducto,
    string Sucursal, string CodigoBarra, string Producto, string Categoria,
    decimal Existencia, decimal ExistenciaMinima, decimal CostoPromedio,
    DateTime? FechaUltimoIngreso, DateTime? FechaUltimoConteo)
{
    public bool BajoMinimo => Existencia < ExistenciaMinima;
    public bool SinExistencias => Existencia == 0;
}

public sealed class InventarioPagina
{
    public List<InventarioFila> Filas { get; set; } = [];
    public List<InventarioOpcion> Sucursales { get; set; } = [];
    public List<InventarioOpcion> Categorias { get; set; } = [];
    public string Buscar { get; set; } = "";
    public int? Sucursal { get; set; }
    public int? Categoria { get; set; }
    public string Situacion { get; set; } = "";
    public int Pagina { get; set; } = 1;
    public int TamanoPagina => 8;
    public long TotalFiltrado { get; set; }
    public long TotalInventarios { get; set; }
    public long BajoMinimo { get; set; }
    public long SinExistencias { get; set; }
    public decimal UnidadesTotales { get; set; }
    public int TotalPaginas => (int)Math.Max(1, Math.Min(int.MaxValue,
        (TotalFiltrado + TamanoPagina - 1) / TamanoPagina));
    public string? Error { get; set; }
}
