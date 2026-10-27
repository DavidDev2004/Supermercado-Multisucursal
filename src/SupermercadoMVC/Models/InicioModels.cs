using System.Globalization;

namespace SupermercadoMVC.Models;

public static class InicioFormat
{
    private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("es-GT");
    public static string Money(decimal value) => "Q " + value.ToString("N2", Culture);
    public static string Number(decimal value) => value.ToString("N0", Culture);
    public static string Quantity(decimal value) => value.ToString("N2", Culture);
    public static string Date(DateTime value) => value.ToString("dd/MM/yyyy", Culture);
    public static string Month(DateTime value) => value.ToString("MMM yyyy", Culture);
}

public sealed record InicioMetric(string Label, decimal Value, bool Money = false);
public sealed record InicioPoint(string Label, decimal Value);
public sealed record InicioChart(string Title, string Description, IReadOnlyList<InicioPoint> Points, bool Money = false, string? Note = null);
public sealed record InicioTable(string Title, string Description, IReadOnlyList<string> Columns, IReadOnlyList<string[]> Rows);
public sealed record InicioLink(string Label, string Url);
public sealed record InicioNotice(string Title, string Text);

public sealed class InicioPage
{
    public string Kind { get; set; } = "Administrador";
    public DateTime Today { get; set; }
    public List<InicioMetric> Metrics { get; } = [];
    public List<InicioChart> Charts { get; } = [];
    public List<InicioTable> Tables { get; } = [];
    public List<InicioLink> Links { get; } = [];
    public List<string> Unavailable { get; } = [];
    public List<InicioNotice> Notices { get; } = [];
    public string ProductSearch { get; set; } = "";
    public InicioTable? Products { get; set; }
    public string? Error { get; set; }
}
