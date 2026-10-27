using Microsoft.Data.SqlClient;
using System.Data;

namespace SupermercadoMVC.Data;

public sealed record CatalogField(
    string Name,
    string Label,
    SqlDbType Type,
    int Size = 0,
    bool Required = true,
    string InputType = "text",
    string? DefaultValue = null,
    int? Minimum = null);

public sealed record CatalogDefinition(
    string Slug,
    string Title,
    string Table,
    string IdColumn,
    IReadOnlyList<CatalogField> Fields,
    IReadOnlyList<string> SearchColumns)
{
    public IReadOnlyList<string> SelectColumns => [IdColumn, .. Fields.Select(field => field.Name)];
    public string SingularTitle => Slug switch
    {
        "categorias" => "Categoría",
        "sucursales" => "Sucursal",
        "proveedores" => "Proveedor",
        "clientes" => "Cliente",
        "empleados" => "Empleado",
        _ => Title
    };
}

public static class CatalogDefinitions
{
    private static CatalogField Text(string name, string label, int size, bool required = true) =>
        new(name, label, SqlDbType.VarChar, size, required);

    private static CatalogField Date(string name, string label) =>
        new(name, label, SqlDbType.Date, Required: true, InputType: "date");

    private static CatalogField Status() =>
        new("Estado", "Estado", SqlDbType.VarChar, 20, Required: true, InputType: "select", DefaultValue: "Activo");

    private static readonly IReadOnlyDictionary<string, CatalogDefinition> Items =
        new Dictionary<string, CatalogDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            ["empleados"] = new(
                "empleados", "Empleados", "dbo.Tbl_Empleados", "CodigoEmpleado",
                [new("CodigoSucursal", "Sucursal", SqlDbType.Int, InputType: "sucursal"),
                 Text("DPI", "DPI", 13), Text("Nombres", "Nombres", 100), Text("Apellidos", "Apellidos", 100),
                 Text("Puesto", "Puesto", 100), Date("FechaNacimiento", "Fecha de nacimiento"),
                 Date("FechaContratacion", "Fecha de contratación"), Text("Telefono", "Teléfono", 20, false),
                 Text("Correo", "Correo", 100, false), new("Salario", "Salario", SqlDbType.Decimal, InputType: "number"), Status()],
                ["DPI", "Nombres", "Apellidos", "Puesto"]),
            ["categorias"] = new(
                "categorias", "Categorías", "dbo.Tbl_Categorias", "CodigoCategoria",
                [Text("Nombre", "Nombre", 100), Text("Descripcion", "Descripción", 200, false), Status()],
                ["Nombre", "Descripcion"]),
            ["sucursales"] = new(
                "sucursales", "Sucursales", "dbo.Tbl_Sucursales", "CodigoSucursal",
                [Text("Nombre", "Nombre", 100), Text("Direccion", "Dirección", 200), Text("Municipio", "Municipio", 100),
                 Text("Departamento", "Departamento", 100), Text("Telefono", "Teléfono", 20), Date("FechaApertura", "Fecha de apertura"), Status()],
                ["Nombre", "Direccion", "Municipio", "Departamento"]),
            ["proveedores"] = new(
                "proveedores", "Proveedores", "dbo.Tbl_Proveedores", "CodigoProveedor",
                [Text("NIT", "NIT", 20), Text("Nombre", "Nombre comercial", 150), Text("Contacto", "Persona de contacto", 150),
                 Text("Municipio", "Municipio", 100), Text("Departamento", "Departamento", 100), Text("Telefono", "Teléfono", 20),
                 Text("Correo", "Correo", 100, false),
                 new("DiasCredito", "Días de crédito", SqlDbType.Int, Required: true, InputType: "number", DefaultValue: "0", Minimum: 0),
                 Date("FechaRegistro", "Fecha de registro"), Status()],
                ["NIT", "Nombre", "Contacto"]),
            ["clientes"] = new(
                "clientes", "Clientes", "dbo.Tbl_Clientes", "CodigoCliente",
                [Text("NIT", "NIT", 20), Text("Nombres", "Nombres", 100), Text("Apellidos", "Apellidos", 100),
                 Text("Municipio", "Municipio", 100), Text("Departamento", "Departamento", 100), Text("Telefono", "Teléfono", 20, false),
                 Text("Correo", "Correo", 100, false), Date("FechaRegistro", "Fecha de registro"), Status()],
                ["NIT", "Nombres", "Apellidos"])
        };

    public static bool TryGet(string? slug, out CatalogDefinition definition)
    {
        if (!string.IsNullOrWhiteSpace(slug) && Items.TryGetValue(slug, out definition!))
        {
            return true;
        }

        definition = null!;
        return false;
    }

    public static IEnumerable<CatalogDefinition> All => Items.Values;
}
