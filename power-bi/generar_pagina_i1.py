"""Genera las cuatro tablas y visuales nuevos de la primera página PBIR/TMDL.

Uso: python generar_pagina_i1.py
No se conecta a SQL Server ni modifica la base de datos.
"""

from __future__ import annotations

import copy
import hashlib
import json
import re
import uuid
from pathlib import Path


ROOT = Path(__file__).resolve().parent
PROJECT = ROOT.parent
SQL_FILE = PROJECT / "database" / "consultas" / "Consultas_Integrante_1.sql"
MODEL_DIR = ROOT / "Supermercado_FaseIV.SemanticModel" / "definition"
PAGE_DIR = (
    ROOT
    / "Supermercado_FaseIV.Report"
    / "definition"
    / "pages"
    / "b1ff16fab4f768d75370"
)
EXISTING_VISUAL = PAGE_DIR / "visuals" / "f93480705cbc4f329087" / "visual.json"
MODEL_FILE = MODEL_DIR / "model.tmdl"
E01_FILE = MODEL_DIR / "tables" / "I1-E01_Proveedores_Credito.tmdl"

SPECS = [
    {
        "exercise": "E01",
        "table": "I1-E01_Proveedores_Credito",
        "title": "E01 · Proveedores con crédito",
        "columns": ["CodigoProveedor", "Proveedor", "Departamento", "DiasCredito", "Correo", "AntiguedadAnios"],
        "types": ["int64", "string", "string", "int64", "string", "int64"],
        "position": (45, 35, 900, 475),
    },
    {
        "exercise": "E04",
        "table": "I1-E04_Proveedores_Compras",
        "title": "E04 · Compras por proveedor",
        "columns": ["CodigoProveedor", "Proveedor", "NumeroCompra", "FechaCompra", "ImporteCompra"],
        "types": ["int64", "string", "string", "dateTime", "decimal"],
        "position": (975, 35, 900, 475),
    },
    {
        "exercise": "E05",
        "table": "I1-E05_Productos_CostoReferencia",
        "title": "E05 · Productos sobre costo promedio",
        "columns": ["CodigoProducto", "Producto", "Marca", "PrecioVenta"],
        "types": ["int64", "string", "string", "decimal"],
        "position": (45, 540, 590, 480),
    },
    {
        "exercise": "E06",
        "table": "I1-E06_Proveedores_BajoMinimo",
        "title": "E06 · Proveedores para reposición",
        "columns": ["CodigoProveedor", "Proveedor", "Contacto", "Telefono", "Departamento"],
        "types": ["int64", "string", "string", "string", "string"],
        "position": (665, 540, 590, 480),
    },
    {
        "exercise": "E10",
        "table": "I1-E10_Proveedores_DosPeriodos",
        "title": "E10 · Proveedores en ambos periodos",
        "columns": ["CodigoProveedor", "Proveedor"],
        "types": ["int64", "string"],
        "position": (1285, 540, 590, 480),
    },
]


def literal(value: str) -> dict:
    return {"expr": {"Literal": {"Value": value}}}


def color(value: str) -> dict:
    return {"solid": {"color": literal(f"'{value}'")}}


def sql_for(exercise: str, source: str) -> str:
    pattern = rf"(?ms)^-- I1-F3-{exercise}:.*?\r?\n(.*?)^GO\s*$"
    match = re.search(pattern, source)
    if match is None:
        raise ValueError(f"No se encontró el bloque I1-{exercise}")
    sql = match.group(1).strip()
    if not re.match(r"(?is)^(SELECT|WITH)\b", sql):
        raise ValueError(f"I1-{exercise} no empieza con SELECT o WITH")
    if re.search(r"(?i)\b(INSERT|UPDATE|DELETE|MERGE|ALTER|DROP|CREATE|EXEC|EXECUTE)\b", sql):
        raise ValueError(f"I1-{exercise} contiene una operación no permitida")
    return sql


def tmdl_table(spec: dict, sql: str) -> str:
    table = spec["table"]
    lines = [f"table {table}", f"\tlineageTag: {uuid.uuid5(uuid.NAMESPACE_URL, table)}", ""]
    for column, data_type in zip(spec["columns"], spec["types"]):
        lines += [
            f"\tcolumn {column}",
            f"\t\tdataType: {data_type}",
        ]
        if data_type == "decimal":
            lines.append("\t\tformatString: #,0.00")
        elif data_type == "int64":
            lines.append("\t\tformatString: 0")
        lines += [
            f"\t\tlineageTag: {uuid.uuid5(uuid.NAMESPACE_URL, table + '.' + column)}",
            "\t\tsummarizeBy: none",
            f"\t\tsourceColumn: {column}",
            "",
        ]
    m_sql = sql.replace('"', '""').replace("\r\n", "\n").replace("\n", "#(lf)")
    lines += [
        f"\tpartition {table} = m",
        "\t\tmode: import",
        "\t\tsource =",
        "\t\t\t\tlet",
        f'\t\t\t\t    Origen = Sql.Database("localhost,1435", "SupermercadosGuate", [Query="{m_sql}"])',
        "\t\t\t\tin",
        "\t\t\t\t    Origen",
        "",
        "\tannotation PBI_ResultType = Table",
        "",
    ]
    return "\n".join(lines)


def projection(table: str, column: str) -> dict:
    return {
        "field": {
            "Column": {
                "Expression": {"SourceRef": {"Entity": table}},
                "Property": column,
            }
        },
        "queryRef": f"{table}.{column}",
        "nativeQueryRef": column,
    }


def visual_id(exercise: str) -> str:
    return hashlib.sha1(f"I1-{exercise}-pagina-1".encode()).hexdigest()[:20]


def build_visual(template: dict, spec: dict) -> dict:
    visual = copy.deepcopy(template)
    exercise = spec["exercise"]
    visual["name"] = template["name"] if exercise == "E01" else visual_id(exercise)
    x, y, width, height = spec["position"]
    visual["position"] = {
        "x": x,
        "y": y,
        "z": 0,
        "height": height,
        "width": width,
        "tabOrder": SPECS.index(spec),
    }
    visual["visual"]["query"] = {
        "queryState": {
            "Values": {
                "projections": [projection(spec["table"], col) for col in spec["columns"]]
            }
        }
    }
    visual["visual"]["visualContainerObjects"] = {
        "title": [
            {
                "properties": {
                    "show": literal("true"),
                    "text": literal(f"'{spec['title']}'"),
                    "fontColor": color("#12304A"),
                    "fontSize": literal("15D"),
                    "bold": literal("true"),
                }
            }
        ],
        "background": [
            {
                "properties": {
                    "show": literal("true"),
                    "color": color("#FFFFFF"),
                    "transparency": literal("0D"),
                }
            }
        ],
        "border": [
            {
                "properties": {
                    "show": literal("true"),
                    "color": color("#19A7A0"),
                    "radius": literal("8D"),
                }
            }
        ],
    }
    visual.pop("filterConfig", None)
    return visual


def write_json(path: Path, data: dict) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def main() -> None:
    if not all(p.exists() for p in (SQL_FILE, MODEL_FILE, E01_FILE, EXISTING_VISUAL)):
        raise FileNotFoundError("Faltan archivos fuente del PBIP o del SQL")
    source = SQL_FILE.read_text(encoding="utf-8-sig")
    original_e01 = E01_FILE.read_bytes()
    template = json.loads(EXISTING_VISUAL.read_text(encoding="utf-8-sig"))
    if template.get("visual", {}).get("visualType") != "tableEx":
        raise ValueError("E01 dejó de ser un visual de tabla")

    for spec in SPECS[1:]:
        sql = sql_for(spec["exercise"], source)
        (MODEL_DIR / "tables" / f"{spec['table']}.tmdl").write_text(
            tmdl_table(spec, sql), encoding="utf-8"
        )

    model = MODEL_FILE.read_text(encoding="utf-8-sig")
    names = [s["table"] for s in SPECS]
    order = "annotation PBI_QueryOrder = " + json.dumps(names, ensure_ascii=False)
    if order not in model:
        raise ValueError("model.tmdl no tiene el orden de consultas esperado")
    for table in names[1:]:
        if f"ref table {table}" not in model:
            raise ValueError(f"Falta ref table {table} en model.tmdl")

    for spec in SPECS[1:]:
        result = build_visual(template, spec)
        path = PAGE_DIR / "visuals" / result["name"] / "visual.json"
        write_json(path, result)

    if E01_FILE.read_bytes() != original_e01:
        raise RuntimeError("Se alteró la consulta E01")
    print("Página 1 generada: E01, E04, E05, E06, E10; consulta E01 intacta.")


if __name__ == "__main__":
    main()
