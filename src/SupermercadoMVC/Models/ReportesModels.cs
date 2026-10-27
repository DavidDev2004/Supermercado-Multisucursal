namespace SupermercadoMVC.Models;

public sealed record ReporteOpcion(int Id, string Nombre);

public sealed class ReportesPagina
{
    public string Tipo { get; set; } = "ventas";
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public int? Sucursal { get; set; }
    public int? Proveedor { get; set; }
    public int? Cliente { get; set; }
    public string Estado { get; set; } = "";
    public int Pagina { get; set; } = 1;
    public int TamanoPagina => 10;
    public long TotalRegistros { get; set; }
    public decimal TotalCantidad { get; set; }
    public decimal TotalImporte { get; set; }
    public int TotalPaginas => (int)Math.Max(1, Math.Min(int.MaxValue,
        (TotalRegistros + TamanoPagina - 1) / TamanoPagina));
    public List<ReporteFila> Filas { get; set; } = [];
    public List<ReporteOpcion> Sucursales { get; set; } = [];
    public List<ReporteOpcion> Proveedores { get; set; } = [];
    public List<ReporteOpcion> Clientes { get; set; } = [];
    public string? Error { get; set; }
}

public sealed record ReporteFila(DateTime? Fecha, string Referencia, string Relacion,
    string Sucursal, string Estado, decimal Cantidad, decimal Precio,
    decimal Importe, string Complemento);
