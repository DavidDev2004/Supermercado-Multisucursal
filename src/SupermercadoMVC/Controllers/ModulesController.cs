using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SupermercadoMVC.Data;
using SupermercadoMVC.Filters;
using SupermercadoMVC.Models;
using System.Globalization;
using System.Text;

namespace SupermercadoMVC.Controllers;

[ServiceFilter(typeof(DemoSessionFilter))]
public sealed class ModulesController(ReportesDataService reportes, ILogger<ModulesController> logger) : Controller
{
    private static readonly IReadOnlyDictionary<string, string> Modules = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["productos"] = "Productos", ["compras"] = "Compras",
        ["inventario"] = "Inventario", ["reportes"] = "Reportes"
    };

    [HttpGet("modulos/{slug}")]
    public async Task<IActionResult> Index(string slug, string? tipo, DateTime? desde, DateTime? hasta,
        int? sucursal, int? proveedor, int? cliente, string? estado, int pagina = 1,
        CancellationToken cancellationToken = default)
    {
        if (slug.Equals("ventas", StringComparison.OrdinalIgnoreCase)) return RedirectToAction("Index", "Sales");
        if (!Modules.TryGetValue(slug, out var title)) return NotFound();
        if (slug.Equals("reportes", StringComparison.OrdinalIgnoreCase))
        {
            var model = BuildQuery(tipo, desde, hasta, sucursal, proveedor, cliente, estado, pagina);
            var error = Validate(model);
            if (error is null)
            {
                try { await reportes.ConsultarAsync(model, false, cancellationToken); }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    logger.LogWarning("Reports SELECT failed: {Type}, SQL {Number}.",
                        exception.GetType().Name, (exception as SqlException)?.Number);
                    error = "No se pudieron consultar los reportes. Revisa la conexión y los permisos de lectura en SQL Server.";
                }
            }
            model.Error = error;
            return View("Reportes", model);
        }
        ViewData["Title"] = title;
        ViewData["ModuleTitle"] = title;
        return View("ComingSoon");
    }

    [HttpGet("modulos/reportes/csv")]
    public async Task<IActionResult> Csv(string? tipo, DateTime? desde, DateTime? hasta,
        int? sucursal, int? proveedor, int? cliente, string? estado,
        CancellationToken cancellationToken = default)
    {
        var model = BuildQuery(tipo, desde, hasta, sucursal, proveedor, cliente, estado, 1);
        var error = Validate(model);
        if (error is not null) return BadRequest(error);
        try { await reportes.ConsultarAsync(model, true, cancellationToken); }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning("Reports CSV SELECT failed: {Type}, SQL {Number}.",
                exception.GetType().Name, (exception as SqlException)?.Number);
            return StatusCode(503, "No se pudo generar el CSV desde SQL Server.");
        }
        var headers = model.Tipo switch
        {
            "inventario" => "UltimoIngreso,CodigoBarra,Producto,Sucursal,Estado,Existencia,CostoPromedio,Valor,Categoria",
            "saldos" => "Fecha,NumeroVenta,Cliente,Sucursal,Estado,TotalVenta,TotalPagado,Saldo,NIT",
            "compras" => "Fecha,NumeroCompra,Proveedor,Sucursal,Estado,Unidades,Importe,NIT",
            _ => "Fecha,NumeroVenta,Cliente,Sucursal,Estado,Unidades,Importe,NIT"
        };
        var output = new StringBuilder("\uFEFF").Append(headers).Append("\r\n");
        foreach (var row in model.Filas)
        {
            var cells = new List<string> { row.Fecha?.ToString("yyyy-MM-dd") ?? "", row.Referencia,
                row.Relacion, row.Sucursal, row.Estado, row.Cantidad.ToString(CultureInfo.InvariantCulture) };
            if (model.Tipo is "inventario" or "saldos") cells.Add(row.Precio.ToString(CultureInfo.InvariantCulture));
            cells.Add(row.Importe.ToString(CultureInfo.InvariantCulture));
            cells.Add(row.Complemento);
            output.AppendJoin(',', cells.Select(CsvCell)).Append("\r\n");
        }
        return File(Encoding.UTF8.GetBytes(output.ToString()), "text/csv; charset=utf-8",
            $"reporte_{model.Tipo}_{DateTime.Today:yyyyMMdd}.csv");
    }

    private static string CsvCell(string value)
    {
        // Impide que hojas de cálculo interpreten textos de SQL como fórmulas.
        if (value.Length > 0 && "=+-@\t\r".Contains(value[0])) value = "'" + value;
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }

    private static ReportesPagina BuildQuery(string? tipo, DateTime? desde, DateTime? hasta,
        int? sucursal, int? proveedor, int? cliente, string? estado, int pagina)
    {
        var kind = tipo is "compras" or "inventario" or "saldos" ? tipo : "ventas";
        string[] states = kind switch
        {
            "compras" => ["Borrador", "Recibida", "Anulada"],
            "inventario" => ["En nivel", "Bajo mínimo", "Sin existencias"],
            "ventas" => ["Borrador", "Confirmada", "Anulada"],
            _ => []
        };
        return new ReportesPagina
        {
            Tipo = kind, Desde = desde, Hasta = hasta,
            Sucursal = sucursal > 0 ? sucursal : null,
            Proveedor = proveedor > 0 ? proveedor : null,
            Cliente = cliente > 0 ? cliente : null,
            Estado = states.Contains(estado) ? estado! : "",
            Pagina = Math.Max(1, pagina)
        };
    }

    private string? Validate(ReportesPagina model)
    {
        if (!ModelState.IsValid) return "Revisa los filtros: una fecha o un identificador no tiene el formato esperado.";
        if (model.Desde > model.Hasta) return "La fecha inicial no puede ser posterior a la fecha final.";
        return null;
    }
}
