"""Prepara SELECT de Power BI a partir de los ejercicios oficiales.

No se conecta a SQL Server. E14/E15 leen exclusivamente la copia PRUEBAS.
"""

from __future__ import annotations

import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parent
SQL_ROOT = ROOT.parent / "database" / "consultas"
OUT = ROOT / "consultas"

PAGES = [
    (1, 2, "Compras y movimientos", [
        (2, "Compras con recepción atrasada", "table", ["NumeroCompra", "Anio", "Mes", "DiasAtraso"]),
        (3, "Compras recibidas en septiembre", "table", ["NumeroCompra", "Proveedor", "Sucursal", "ImporteCompra"]),
        (7, "Compras por proveedor", "bar", ["Proveedor", "TotalComprado"]),
        (8, "Costo neto por categoría y mes", "line", ["Mes", "CostoNeto", "Categoria"]),
        (9, "Bitácora de movimientos", "table", ["Fecha", "Documento", "TipoEvento", "CodigoSucursal", "Monto"]),
    ]),
    (1, 3, "Seguimiento operativo", [
        (11, "Proveedores a contactar", "table", ["CodigoProveedor", "Proveedor"]),
        (12, "Prioridad por atraso", "table", ["NumeroCompra", "CodigoSucursal", "DiasAtraso", "NivelAtraso"]),
        (13, "Presupuesto de la sucursal evaluada", "cardVisual", ["TotalComprado", "Presupuesto", "Diferencia", "EstadoPresupuesto"]),
        (14, "Costo de referencia verificado", "table", ["CodigoProductoProveedor", "Proveedor", "Producto", "CostoReferencia"]),
        (15, "Compra de práctica verificada", "table", ["NumeroCompra", "ExisteCompra", "CantidadLineas", "ImporteCompra"]),
    ]),
    (2, 1, "Clientes y oportunidades comerciales", [
        (1, "Clientes registrados", "table", ["Cliente", "Departamento", "AntiguedadAnios"]),
        (4, "Clientes con y sin ventas", "table", ["Cliente", "NumeroVenta", "FechaVenta", "TotalVenta"]),
        (5, "Ventas superiores al promedio", "table", ["NumeroVenta", "FechaVenta", "CodigoSucursal", "TotalVenta"]),
        (6, "Clientes con pagos destacados", "table", ["Cliente", "Departamento", "Telefono"]),
        (10, "Clientes recurrentes", "table", ["CodigoCliente", "Cliente"]),
    ]),
    (2, 2, "Ventas y cobros", [
        (2, "Pagos aplicados", "table", ["Comprobante", "MetodoPago", "Monto", "Comision", "MontoNeto"]),
        (3, "Saldos por venta", "table", ["NumeroVenta", "Cliente", "TotalVenta", "TotalPagado", "Saldo"]),
        (7, "Ventas por sucursal", "bar", ["Sucursal", "TotalVendido"]),
        (8, "Venta neta por categoría y mes", "line", ["Mes", "VentaNeta", "Categoria"]),
        (9, "Bitácora de ventas y pagos", "table", ["Fecha", "Documento", "TipoEvento", "CodigoSucursal", "Monto"]),
    ]),
    (2, 3, "Seguimiento comercial y verificaciones", [
        (11, "Clientes para campaña", "table", ["CodigoCliente", "Cliente"]),
        (12, "Clasificación de ventas", "table", ["NumeroVenta", "CodigoSucursal", "TotalVenta", "NivelVenta"]),
        (13, "Meta de venta de la sucursal", "cardVisual", ["TotalVendido", "Meta", "Faltante", "EstadoMeta"]),
        (14, "Salario verificado", "table", ["Empleado", "CodigoSucursal", "Salario"]),
        (15, "Registros de práctica verificados", "table", ["NIT", "NumeroVenta", "ExisteCliente", "ExisteVenta"]),
    ]),
    (3, 1, "Productos y cobertura del inventario", [
        (1, "Productos registrados", "table", ["Producto", "Marca", "PrecioVenta", "DiasRegistrado"]),
        (4, "Productos con y sin inventario", "table", ["Producto", "CodigoSucursal", "Existencia", "ExistenciaMinima"]),
        (5, "Precios superiores al promedio", "table", ["CodigoProducto", "CodigoCategoria", "Producto", "PrecioVenta"]),
        (6, "Sucursales con pérdidas", "table", ["Sucursal", "Municipio", "Departamento"]),
        (9, "Cobertura territorial", "table", ["Municipio", "Departamento"]),
    ]),
    (3, 2, "Existencias y pérdidas", [
        (2, "Necesidades de reposición", "table", ["CodigoInventario", "Existencia", "ExistenciaMinima", "Faltante"]),
        (3, "Valoración del inventario", "table", ["Sucursal", "Producto", "ValorCosto", "ValorVenta"]),
        (7, "Valor del inventario por sucursal", "bar", ["Sucursal", "ValorInventario"]),
        (8, "Pérdidas por motivo y mes", "line", ["Mes", "ValorPerdido", "Motivo"]),
        (10, "Disponibilidad compartida", "table", ["CodigoProducto", "Producto"]),
    ]),
    (3, 3, "Reposición y verificaciones", [
        (11, "Disponibilidad exclusiva", "table", ["CodigoProducto", "Producto", "Disponibilidad"]),
        (12, "Antigüedad del inventario", "table", ["CodigoInventario", "FechaUltimoIngreso", "DiasSinIngreso", "NivelAntiguedad"]),
        (13, "Reposición de la sucursal", "cardVisual", ["CodigoSucursal", "Tolerancia", "ProductosBajoMinimo", "EstadoReposicion"]),
        (14, "Mínimo de inventario verificado", "table", ["CodigoInventario", "CodigoSucursal", "Existencia", "ExistenciaMinima"]),
        (15, "Ajuste de práctica verificado", "table", ["NumeroAjuste", "ExisteAjuste", "CantidadLineas", "Antes", "Despues"]),
    ]),
]

READ_ONLY_FINAL = {
    (1, 14): """SELECT pp.CodigoProductoProveedor, pp.CodigoProveedor, pp.CodigoProducto,
       pr.Nombre AS Proveedor, p.Nombre AS Producto,
       CAST(pp.CostoReferencia AS decimal(18,2)) AS CostoReferencia
FROM dbo.Tbl_ProductoProveedor AS pp
JOIN dbo.Tbl_Proveedores AS pr ON pr.CodigoProveedor = pp.CodigoProveedor
JOIN dbo.Tbl_Productos AS p ON p.CodigoProducto = pp.CodigoProducto
WHERE pp.CodigoProductoProveedor = 1;""",
    (1, 15): """WITH Practicas AS (
    SELECT v.Decision, v.NumeroCompra
    FROM (VALUES
        ('NO', 'I1-E15-3056D4F7E68445568C069E'),
        ('SI', 'I1-E15-E7F1AC0CB0BB416AB825B7')
    ) AS v(Decision, NumeroCompra)
)
SELECT p.Decision, p.NumeroCompra,
       CASE WHEN c.CodigoCompra IS NULL THEN 0 ELSE 1 END AS ExisteCompra,
       (SELECT COUNT(*) FROM dbo.Tbl_ComprasDetalle AS d WHERE d.CodigoCompra = c.CodigoCompra) AS CantidadLineas,
       CAST(COALESCE((SELECT SUM(d.Cantidad * d.CostoUnitario - d.Descuento)
                      FROM dbo.Tbl_ComprasDetalle AS d WHERE d.CodigoCompra = c.CodigoCompra), 0) AS decimal(18,2)) AS ImporteCompra
FROM Practicas AS p
LEFT JOIN dbo.Tbl_Compras AS c ON c.NumeroCompra = p.NumeroCompra;""",
    (2, 14): """SELECT e.CodigoEmpleado, e.CodigoSucursal,
       e.Nombres + ' ' + e.Apellidos AS Empleado,
       CAST(e.Salario AS decimal(18,2)) AS Salario
FROM dbo.Tbl_Empleados AS e
WHERE e.CodigoEmpleado = 1;""",
    (2, 15): """WITH Practicas AS (
    SELECT v.Decision, v.NIT, v.NumeroVenta
    FROM (VALUES
        ('NO', 'I2E15638B1CE7EB1542', 'I2-E15-3243CE0A052A4894844FA9'),
        ('SI', 'I2E1548F48CBA74FE4B', 'I2-E15-9287DE7FEC6C4195A2A587')
    ) AS v(Decision, NIT, NumeroVenta)
)
SELECT p.Decision, p.NIT, p.NumeroVenta,
       CASE WHEN c.CodigoCliente IS NULL THEN 0 ELSE 1 END AS ExisteCliente,
       CASE WHEN v.CodigoVenta IS NULL THEN 0 ELSE 1 END AS ExisteVenta
FROM Practicas AS p
LEFT JOIN dbo.Tbl_Clientes AS c ON c.NIT = p.NIT
LEFT JOIN dbo.Tbl_Ventas AS v ON v.NumeroVenta = p.NumeroVenta;""",
    (3, 14): """SELECT i.CodigoInventario, i.CodigoSucursal, i.CodigoProducto,
       i.Existencia, i.ExistenciaMinima
FROM dbo.Tbl_Inventario AS i
WHERE i.CodigoInventario = 1;""",
    (3, 15): """WITH Practicas AS (
    SELECT v.Decision, v.NumeroAjuste
    FROM (VALUES
        ('NO', 'I3-E15-E164A13A540348E8A8B38F'),
        ('SI', 'I3-E15-7F01845DDB7B4090BFA76E')
    ) AS v(Decision, NumeroAjuste)
)
SELECT p.Decision, p.NumeroAjuste,
       CASE WHEN a.CodigoAjuste IS NULL THEN 0 ELSE 1 END AS ExisteAjuste,
       (SELECT COUNT(*) FROM dbo.Tbl_AjustesInventarioDetalle AS d WHERE d.CodigoAjuste = a.CodigoAjuste) AS CantidadLineas,
       CAST(NULL AS decimal(18,2)) AS Antes,
       (SELECT i.Existencia FROM dbo.Tbl_Inventario AS i WHERE i.CodigoInventario = 1) AS Despues
FROM Practicas AS p
LEFT JOIN dbo.Tbl_AjustesInventario AS a ON a.NumeroAjuste = p.NumeroAjuste;""",
}

ADAPTED_13 = {
    1: """WITH Sucursal AS (
    SELECT MIN(CodigoSucursal) AS CodigoSucursal FROM dbo.Tbl_Sucursales WHERE Estado = 'Activo'
), ComprasDocumento AS (
    SELECT c.CodigoCompra, c.CodigoSucursal,
           SUM(d.Cantidad * d.CostoUnitario - d.Descuento) AS Importe
    FROM dbo.Tbl_Compras AS c
    JOIN dbo.Tbl_ComprasDetalle AS d ON d.CodigoCompra = c.CodigoCompra
    WHERE c.Estado = 'Recibida' AND c.FechaCompra >= '20260901' AND c.FechaCompra < '20261001'
    GROUP BY c.CodigoCompra, c.CodigoSucursal
)
SELECT s.CodigoSucursal, CAST(10000 AS decimal(18,2)) AS Presupuesto,
       CAST(COALESCE(SUM(cd.Importe), 0) AS decimal(18,2)) AS TotalComprado,
       CAST(10000 - COALESCE(SUM(cd.Importe), 0) AS decimal(18,2)) AS Diferencia,
       CASE WHEN COALESCE(SUM(cd.Importe), 0) > 10000 THEN 'Excedido'
            ELSE 'Dentro del presupuesto' END AS EstadoPresupuesto
FROM Sucursal AS s
LEFT JOIN ComprasDocumento AS cd ON cd.CodigoSucursal = s.CodigoSucursal
WHERE s.CodigoSucursal IS NOT NULL
GROUP BY s.CodigoSucursal;""",
    2: """WITH VentasDocumento AS (
    SELECT v.CodigoVenta, v.CodigoSucursal,
           SUM(d.Cantidad * d.PrecioUnitario - d.Descuento) AS Importe
    FROM dbo.Tbl_Ventas AS v
    JOIN dbo.Tbl_VentasDetalle AS d ON d.CodigoVenta = v.CodigoVenta
    WHERE v.Estado = 'Confirmada' AND v.FechaVenta >= '20260901' AND v.FechaVenta < '20261001'
    GROUP BY v.CodigoVenta, v.CodigoSucursal
)
SELECT CAST(1 AS int) AS CodigoSucursal, CAST(1000 AS decimal(18,2)) AS Meta,
       CAST(COALESCE(SUM(Importe), 0) AS decimal(18,2)) AS TotalVendido,
       CAST(CASE WHEN COALESCE(SUM(Importe), 0) >= 1000 THEN 0
                 ELSE 1000 - COALESCE(SUM(Importe), 0) END AS decimal(18,2)) AS Faltante,
       CASE WHEN COALESCE(SUM(Importe), 0) >= 1000 THEN 'Alcanzada'
            ELSE 'Pendiente' END AS EstadoMeta
FROM VentasDocumento WHERE CodigoSucursal = 1;""",
    3: """WITH Sucursal AS (
    SELECT MIN(CodigoSucursal) AS CodigoSucursal FROM dbo.Tbl_Sucursales WHERE Estado = 'Activo'
)
SELECT s.CodigoSucursal, CAST(2 AS int) AS Tolerancia,
       COUNT(i.CodigoInventario) AS ProductosBajoMinimo,
       CASE WHEN COUNT(i.CodigoInventario) > 2 THEN 'Reposición prioritaria'
            ELSE 'Seguimiento normal' END AS EstadoReposicion
FROM Sucursal AS s
LEFT JOIN dbo.Tbl_Inventario AS i ON i.CodigoSucursal = s.CodigoSucursal
    AND i.Existencia < i.ExistenciaMinima
    AND EXISTS (SELECT 1 FROM dbo.Tbl_Productos AS p
                WHERE p.CodigoProducto = i.CodigoProducto AND p.Estado = 'Activo')
WHERE s.CodigoSucursal IS NOT NULL
GROUP BY s.CodigoSucursal;""",
}


def source_block(integrante: int, ejercicio: int) -> str:
    text = (SQL_ROOT / f"Consultas_Integrante_{integrante}.sql").read_text(encoding="utf-8-sig")
    if integrante == 2:
        pattern = rf"(?ms)^/\* {ejercicio}\..*?\*/\s*(.*?)^GO\s*$"
    elif integrante == 3 and ejercicio == 8:
        pattern = r"(?s)EXEC sys\.sp_executesql N'(.*?)';"
        match = re.search(pattern, text)
        if not match:
            raise ValueError("No se encontró el SELECT dinámico de I3-E08")
        return match.group(1).replace("''", "'").strip()
    else:
        pattern = rf"(?ms)^-- I{integrante}-F3-E{ejercicio:02d}:.*?\r?\n(.*?)^GO\s*$"
    match = re.search(pattern, text)
    if not match:
        raise ValueError(f"No se encontró I{integrante}-E{ejercicio:02d}")
    return match.group(1).strip()


def query(integrante: int, ejercicio: int) -> str:
    if ejercicio in (14, 15):
        sql = READ_ONLY_FINAL[(integrante, ejercicio)]
    elif ejercicio == 13:
        sql = ADAPTED_13[integrante]
    else:
        sql = source_block(integrante, ejercicio)
        if integrante == 1 and ejercicio == 12:
            sql = re.sub(r"(?m)^DECLARE .*?;\s*", "", sql, count=1)
            sql = sql.replace("@LeveMax12", "2").replace("@MedioMax12", "5")
        elif integrante == 2 and ejercicio == 12:
            sql = re.sub(r"(?m)^DECLARE .*?;\s*", "", sql, count=2)
            sql = sql.replace("@LimiteBajo", "100").replace("@LimiteAlto", "500")
        elif integrante == 3 and ejercicio in (1, 2, 12):
            sql = re.sub(r"(?m)^DECLARE .*?;\s*", "", sql, count=1)
            for name, value in {
                "@Referencia01": "'20260930'", "@Referencia02": "'20260930'",
                "@Referencia12": "'20260930'", "@Reciente12": "30", "@Intermedio12": "90",
            }.items():
                sql = sql.replace(name, value)
        elif integrante == 3 and ejercicio in (10, 11):
            match = re.search(r"(?s)\bWITH\s+.*?ORDER BY\s+.*?;", sql)
            if not match:
                raise ValueError(f"No se pudo adaptar I3-E{ejercicio:02d}")
            sql = match.group(0)
            a = f"@SucursalA{ejercicio}"
            b = f"@SucursalB{ejercicio}"
            sql = sql.replace(b, "(SELECT MIN(CodigoSucursal) FROM dbo.Tbl_Sucursales WHERE CodigoSucursal <> (SELECT MIN(CodigoSucursal) FROM dbo.Tbl_Sucursales))")
            sql = sql.replace(a, "(SELECT MIN(CodigoSucursal) FROM dbo.Tbl_Sucursales)")
    sql = sql.strip()
    stripped = re.sub(r"--[^\n]*", "", sql)
    if not re.match(r"(?i)^(SELECT|WITH)\b", stripped.lstrip()):
        raise ValueError(f"I{integrante}-E{ejercicio:02d} no inicia con lectura")
    if re.search(r"(?i)\b(INSERT|UPDATE|DELETE|MERGE|CREATE|ALTER|DROP|EXEC|EXECUTE)\b", stripped):
        raise ValueError(f"I{integrante}-E{ejercicio:02d} contiene escritura o ejecución")
    return sql


def main() -> None:
    OUT.mkdir(exist_ok=True)
    manifest = []
    for integrante, pagina, titulo, visuals in PAGES:
        for ejercicio, visual_titulo, tipo, columns in visuals:
            db = "SupermercadosGuate_PRUEBAS" if ejercicio in (14, 15) else "SupermercadosGuate"
            sql = query(integrante, ejercicio)
            key = f"I{integrante}-E{ejercicio:02d}"
            (OUT / f"{key}-BI.sql").write_text(sql + "\n", encoding="utf-8")
            manifest.append({"key": key, "integrante": integrante, "pagina": pagina,
                             "pagina_titulo": titulo, "titulo": visual_titulo,
                             "tipo": tipo, "columnas": columns, "base": db})
    (OUT / "manifest.json").write_text(json.dumps(manifest, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"SELECT preparados: {len(manifest)}")


if __name__ == "__main__":
    main()
