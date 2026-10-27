using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SupermercadoMVC.Data;
using SupermercadoMVC.Filters;
using SupermercadoMVC.Models;
using System.Globalization;

namespace SupermercadoMVC.Controllers;

[Route("catalogos/{slug}")]
[ServiceFilter(typeof(DemoSessionFilter))]
public sealed class CatalogController(
    CatalogDataService catalog,
    ILogger<CatalogController> logger) : Controller
{
    private const int PageSize = 8;

    [HttpGet]
    public async Task<IActionResult> Index(string slug, string? buscar, string? estado, int pagina = 1, CancellationToken cancellationToken = default)
    {
        if (!CatalogDefinitions.TryGet(slug, out var definition)) return NotFound();
        buscar = (buscar ?? string.Empty).Trim();
        estado = NormalizeState(estado);
        if (buscar.Length > 100) buscar = buscar[..100];
        pagina = Math.Max(1, pagina);
        return View("Index", await BuildPageAsync(definition, buscar, estado, pagina, new CatalogInput(), cancellationToken));
    }

    [HttpPost("crear")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string slug, CatalogInput input, string? buscar, string? estado, int pagina = 1, CancellationToken cancellationToken = default)
    {
        if (!CatalogDefinitions.TryGet(slug, out var definition)) return NotFound();
        NormalizeInput(definition, input);
        ValidateInput(definition, input);
        if (slug.Equals("empleados", StringComparison.OrdinalIgnoreCase)) await ValidateSucursalAsync(input, cancellationToken);
        if (!ModelState.IsValid)
        {
            return View("Index", await BuildPageAsync(definition, buscar, estado, pagina, input, cancellationToken, "create"));
        }

        try
        {
            await catalog.CreateAsync(definition, input, cancellationToken);
            TempData["Notice"] = $"{definition.SingularTitle} agregado correctamente.";
            return RedirectToAction(nameof(Index), new { slug, buscar, estado, pagina });
        }
        catch (SqlException exception)
        {
            logger.LogWarning("SQL create failed for {Catalog}, error {ErrorNumber}.", slug, exception.Number);
            ModelState.AddModelError(string.Empty, SqlMessage(exception, "INSERT", definition));
            return View("Index", await BuildPageAsync(definition, buscar, estado, pagina, input, cancellationToken, "create"));
        }
        catch (Exception exception)
        {
            logger.LogWarning("Catalog create failed for {Catalog}: {ExceptionType}.", slug, exception.GetType().Name);
            ModelState.AddModelError(string.Empty, "No fue posible guardar el registro. Revisa la conexión e intenta nuevamente.");
            return View("Index", await BuildPageAsync(definition, buscar, estado, pagina, input, cancellationToken, "create"));
        }
    }

    [HttpPost("editar/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string slug, int id, CatalogInput input, string? buscar, string? estado, int pagina = 1, CancellationToken cancellationToken = default)
    {
        if (!CatalogDefinitions.TryGet(slug, out var definition)) return NotFound();
        NormalizeInput(definition, input);
        ValidateInput(definition, input);
        if (slug.Equals("empleados", StringComparison.OrdinalIgnoreCase))
        {
            input.Id = id;
            await ValidateSucursalAsync(input, cancellationToken);
        }
        if (!ModelState.IsValid)
        {
            return View("Index", await BuildPageAsync(definition, buscar, estado, pagina, input, cancellationToken, "edit"));
        }

        try
        {
            if (!await catalog.UpdateAsync(definition, id, input, cancellationToken))
            {
                TempData["Notice"] = "El registro ya no existe o fue modificado desde otra sesión.";
            }
            else
            {
                TempData["Notice"] = $"{definition.SingularTitle} actualizado correctamente.";
            }
            return RedirectToAction(nameof(Index), new { slug, buscar, estado, pagina });
        }
        catch (SqlException exception)
        {
            logger.LogWarning("SQL update failed for {Catalog}, error {ErrorNumber}.", slug, exception.Number);
            ModelState.AddModelError(string.Empty, SqlMessage(exception, "UPDATE", definition));
            input.Id = id;
            return View("Index", await BuildPageAsync(definition, buscar, estado, pagina, input, cancellationToken, "edit"));
        }
        catch (Exception exception)
        {
            logger.LogWarning("Catalog update failed for {Catalog}: {ExceptionType}.", slug, exception.GetType().Name);
            ModelState.AddModelError(string.Empty, "No fue posible actualizar el registro. Revisa la conexión e intenta nuevamente.");
            input.Id = id;
            return View("Index", await BuildPageAsync(definition, buscar, estado, pagina, input, cancellationToken, "edit"));
        }
    }

    [HttpPost("eliminar/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string slug, int id, string? buscar, string? estado, int pagina = 1, CancellationToken cancellationToken = default)
    {
        if (!CatalogDefinitions.TryGet(slug, out var definition)) return NotFound();
        try
        {
            TempData["Notice"] = await catalog.DeleteAsync(definition, id, cancellationToken)
                ? "Registro eliminado correctamente."
                : "El registro ya no existe.";
        }
        catch (SqlException exception)
        {
            logger.LogWarning("SQL delete failed for {Catalog}, error {ErrorNumber}.", slug, exception.Number);
            TempData["Error"] = SqlMessage(exception, "DELETE", definition);
        }
        catch (Exception exception)
        {
            logger.LogWarning("Catalog delete failed for {Catalog}: {ExceptionType}.", slug, exception.GetType().Name);
            TempData["Error"] = "No fue posible eliminar el registro. Revisa la conexión e intenta nuevamente.";
        }

        return RedirectToAction(nameof(Index), new { slug, buscar, estado, pagina });
    }

    private async Task<CatalogPageViewModel> BuildPageAsync(
        CatalogDefinition definition, string? search, string? state, int page, CatalogInput input,
        CancellationToken cancellationToken, string? openModal = null)
    {
        CatalogPageData? data = null;
        IReadOnlyList<SucursalOption> sucursales = [];
        string? error = null;
        try
        {
            page = Math.Max(1, page);
            data = await catalog.GetPageAsync(definition, search, NormalizeState(state), page, PageSize, cancellationToken);
            if (definition.Slug == "empleados") sucursales = await catalog.GetSucursalesAsync(cancellationToken);
            var lastPage = Math.Max(1, (int)Math.Ceiling((double)data.Total / PageSize));
            if (page > lastPage)
            {
                page = lastPage;
                data = await catalog.GetPageAsync(definition, search, NormalizeState(state), page, PageSize, cancellationToken);
            }
        }
        catch (Exception exception)
        {
            logger.LogWarning("Catalog read failed for {Catalog}: {ExceptionType}.", definition.Slug, exception.GetType().Name);
            error = "No se pudo consultar SQL Server. Revisa la configuración local y vuelve a intentarlo.";
        }

        return new CatalogPageViewModel
        {
            Sucursales = sucursales,
            Definition = definition,
            Rows = data?.Rows ?? [],
            Input = input,
            Search = search ?? string.Empty,
            State = NormalizeState(state),
            Page = page,
            PageSize = PageSize,
            Total = data?.Total ?? 0,
            Active = data?.Active ?? 0,
            Inactive = data?.Inactive ?? 0,
            Notice = TempData["Notice"] as string,
            Error = error ?? TempData["Error"] as string,
            OpenModal = openModal
        };
    }

    private void ValidateInput(CatalogDefinition definition, CatalogInput input)
    {
        foreach (var field in definition.Fields)
        {
            input.Fields.TryGetValue(field.Name, out var value);
            var key = $"Fields[{field.Name}]";
            if (field.Required && string.IsNullOrWhiteSpace(value))
            {
                ModelState.AddModelError(key, $"{field.Label} es obligatorio.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(value)) continue;
            value = value.Trim();
            if (field.Type == System.Data.SqlDbType.VarChar && value.Length > field.Size)
            {
                ModelState.AddModelError(key, $"{field.Label} permite un máximo de {field.Size} caracteres.");
            }
            else if (field.Type == System.Data.SqlDbType.Int &&
                     (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number) ||
                      (field.Minimum.HasValue && number < field.Minimum.Value)))
            {
                ModelState.AddModelError(key, field.Minimum.HasValue
                    ? $"{field.Label} debe ser un número igual o mayor que {field.Minimum.Value}."
                    : $"{field.Label} debe ser un número entero.");
            }
            else if (field.Type == System.Data.SqlDbType.Date && !DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            {
                ModelState.AddModelError(key, $"{field.Label} debe ser una fecha válida.");
            }
            else if (field.Type == System.Data.SqlDbType.Decimal &&
                     (!decimal.TryParse(value, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var salary) ||
                      salary <= 0 || salary > 9999999999.99m || decimal.Round(salary, 2) != salary))
            {
                ModelState.AddModelError(key, "Salario debe ser mayor que cero, con hasta 10 enteros y 2 decimales.");
            }
            else if (field.Name == "Estado" && value is not ("Activo" or "Inactivo"))
            {
                ModelState.AddModelError(key, "Selecciona un estado válido.");
            }
        }
    }

    private async Task ValidateSucursalAsync(CatalogInput input, CancellationToken cancellationToken)
    {
        try
        {
            var sucursales = await catalog.GetSucursalesAsync(cancellationToken);
            input.Fields.TryGetValue("CodigoSucursal", out var raw);
            if (!int.TryParse(raw, out var id) || !sucursales.Any(sucursal => sucursal.CodigoSucursal == id))
                ModelState.AddModelError("Fields[CodigoSucursal]", "Selecciona una sucursal existente.");
        }
        catch (Exception exception)
        {
            logger.LogWarning("Sucursal validation failed: {ExceptionType}.", exception.GetType().Name);
            ModelState.AddModelError("Fields[CodigoSucursal]", "No se pudieron cargar las sucursales. Intenta nuevamente.");
        }
    }

    private static void NormalizeInput(CatalogDefinition definition, CatalogInput input)
    {
        var normalized = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var field in definition.Fields)
        {
            input.Fields.TryGetValue(field.Name, out var value);
            if (value is null && field.DefaultValue is not null) value = field.DefaultValue;
            normalized[field.Name] = string.IsNullOrWhiteSpace(value) && !field.Required ? null : value?.Trim();
        }
        input.Fields = normalized;
    }

    private static string NormalizeState(string? value) => value is "Activo" or "Inactivo" ? value : string.Empty;

    private static string SqlMessage(SqlException exception, string operation, CatalogDefinition definition) =>
        exception.Number switch
        {
            2601 or 2627 when definition.Slug == "empleados" => "Ya existe un empleado con ese DPI. Revisa el valor ingresado.",
            547 when definition.Slug == "empleados" && operation == "UPDATE" => "No se pudo actualizar: verifica la sucursal. Si el empleado tiene compras, ventas o ajustes relacionados, no puede cambiarse su sucursal.",
            547 when definition.Slug == "empleados" && operation == "INSERT" => "La sucursal ya no existe o los datos incumplen una restricción del empleado. Revisa sucursal, salario y estado.",
            229 or 230 => $"El usuario de SQL Server no tiene permiso {operation} sobre {definition.Table}. Solicita únicamente ese permiso para esta tabla.",
            2601 or 2627 => "Ya existe un registro con ese valor único. Revisa el nombre o NIT.",
            547 when operation == "DELETE" => "No se puede eliminar: el registro está relacionado con información existente.",
            547 => "La operación no cumple una restricción de la base de datos.",
            _ => "SQL Server rechazó la operación. Revisa los datos e inténtalo nuevamente."
        };
}
