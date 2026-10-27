"""Genera ocho páginas PBIR y cuarenta tablas TMDL para Fase IV.

Lee consultas/manifest.json, consultas/metadata.json y los SELECT -BI.sql.
No abre conexiones ni ejecuta SQL. Conserva la primera página existente.
"""

from __future__ import annotations

import copy
import hashlib
import json
import uuid
from pathlib import Path

ROOT = Path(__file__).resolve().parent
QUERIES = ROOT / "consultas"
REPORT = ROOT / "Supermercado_FaseIV.Report" / "definition"
MODEL = ROOT / "Supermercado_FaseIV.SemanticModel" / "definition"
EXISTING_PAGE = REPORT / "pages" / "b1ff16fab4f768d75370"
EXISTING_VISUAL = EXISTING_PAGE / "visuals" / "f93480705cbc4f329087" / "visual.json"
SLOTS = [
    (45, 35, 900, 310),
    (975, 35, 900, 310),
    (45, 365, 900, 310),
    (975, 365, 900, 310),
    (45, 695, 1830, 340),
]
TYPE_MAP = {"Int32": "int64", "Int64": "int64", "Decimal": "decimal",
            "Double": "double", "String": "string", "DateTime": "dateTime",
            "Boolean": "boolean"}


def stable_id(text: str) -> str:
    return hashlib.sha1(text.encode("utf-8")).hexdigest()[:20]


def projection(table: str, column: str, aggregate: bool = False) -> dict:
    field = {"Column": {"Expression": {"SourceRef": {"Entity": table}}, "Property": column}}
    if aggregate:
        field = {"Aggregation": {"Expression": field, "Function": 0}}
    return {"field": field,
            "queryRef": f"Sum({table}.{column})" if aggregate else f"{table}.{column}",
            "nativeQueryRef": column}


def visual(item: dict, template: dict, index: int) -> dict:
    key, table = item["key"], item["key"] + "_BI"
    x, y, width, height = SLOTS[index]
    result = {"$schema": template["$schema"],
              "name": stable_id("visual-" + key),
              "position": {"x": x, "y": y, "z": 0, "height": height,
                           "width": width, "tabOrder": index}}
    title = copy.deepcopy(template["visual"]["visualContainerObjects"])
    title["title"][0]["properties"]["text"]["expr"]["Literal"]["Value"] = (
        f"'{key.replace('-', '-F3-', 1)} · {item['titulo']}'"
    )
    kind = item["tipo"]
    cols = item["columnas"]
    if kind == "table":
        visual_type = "tableEx"
        state = {"Values": {"projections": [projection(table, c) for c in cols]}}
    elif kind == "bar":
        visual_type = "clusteredBarChart"
        state = {"Category": {"projections": [projection(table, cols[0])]},
                 "Y": {"projections": [projection(table, cols[1], aggregate=True)]}}
    elif kind == "line":
        visual_type = "lineChart"
        state = {"Category": {"projections": [projection(table, cols[0])]},
                 "Y": {"projections": [projection(table, cols[1], aggregate=True)]},
                 "Series": {"projections": [projection(table, cols[2])]}}
    elif kind == "cardVisual":
        visual_type = "cardVisual"
        state = {"Data": {"projections": [projection(table, c) for c in cols]}}
    else:
        raise ValueError(f"Tipo de visual desconocido: {kind}")
    result["visual"] = {"visualType": visual_type,
                        "query": {"queryState": state},
                        "visualContainerObjects": title,
                        "drillFilterOtherVisuals": True}
    return result


def table_tmdl(item: dict, metadata: dict) -> str:
    table = item["key"] + "_BI"
    db = item["base"]
    sql = (QUERIES / f"{item['key']}-BI.sql").read_text(encoding="utf-8-sig").strip()
    if db not in ("SupermercadosGuate", "SupermercadosGuate_PRUEBAS"):
        raise ValueError(f"Base no autorizada para {table}")
    if (item["key"].endswith("E14") or item["key"].endswith("E15")) != (db == "SupermercadosGuate_PRUEBAS"):
        raise ValueError(f"Origen incorrecto para {table}")
    columns = metadata[item["key"]]["columns"]
    lines = [f"table {table}", f"\tlineageTag: {uuid.uuid5(uuid.NAMESPACE_URL, table)}", ""]
    for col in columns:
        name, data_type = col["name"], TYPE_MAP[col["type"]]
        lines += [f"\tcolumn {name}", f"\t\tdataType: {data_type}"]
        if data_type == "decimal":
            lines.append("\t\tformatString: #,0.00")
        elif data_type == "int64":
            lines.append("\t\tformatString: 0")
        lines += [f"\t\tlineageTag: {uuid.uuid5(uuid.NAMESPACE_URL, table + '.' + name)}",
                  f"\t\tsummarizeBy: {'sum' if data_type in ('decimal', 'double') else 'none'}",
                  f"\t\tsourceColumn: {name}", ""]
    query = sql.replace('"', '""').replace("\r\n", "\n").replace("\n", "#(lf)")
    lines += [f"\tpartition {table} = m", "\t\tmode: import", "\t\tsource =",
              "\t\t\t\tlet",
              f'\t\t\t\t    Origen = Sql.Database("localhost,1435", "{db}", [Query="{query}"])',
              "\t\t\t\tin", "\t\t\t\t    Origen", "", "\tannotation PBI_ResultType = Table", ""]
    return "\n".join(lines)


def save_json(path: Path, value: dict) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(value, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def main() -> None:
    manifest = json.loads((QUERIES / "manifest.json").read_text(encoding="utf-8-sig"))
    metadata_list = json.loads((QUERIES / "metadata.json").read_text(encoding="utf-8-sig"))
    metadata = {m["key"]: m for m in metadata_list}
    if len(manifest) != 40 or len(metadata) != 40:
        raise ValueError("Se esperaban 40 consultas nuevas")
    template = json.loads(EXISTING_VISUAL.read_text(encoding="utf-8-sig"))
    pages = {}
    for item in manifest:
        key = (item["integrante"], item["pagina"])
        pages.setdefault(key, []).append(item)
    if len(pages) != 8 or any(len(items) != 5 for items in pages.values()):
        raise ValueError("Se esperaban ocho páginas de cinco visuales")
    first_page_hash = hashlib.sha256((EXISTING_PAGE / "page.json").read_bytes()).hexdigest()
    first_visual_hash = hashlib.sha256(EXISTING_VISUAL.read_bytes()).hexdigest()

    for item in manifest:
        table = item["key"] + "_BI"
        (MODEL / "tables" / f"{table}.tmdl").write_text(
            table_tmdl(item, metadata), encoding="utf-8"
        )

    for (integrante, numero), items in pages.items():
        page_id = stable_id(f"pagina-i{integrante}-p{numero}")
        folder = REPORT / "pages" / page_id
        save_json(folder / "page.json", {
            "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/report/definition/page/2.1.0/schema.json",
            "name": page_id,
            "displayName": items[0]["pagina_titulo"],
            "displayOption": "FitToPage",
            "height": 1080,
            "width": 1920,
        })
        for index, item in enumerate(items):
            v = visual(item, template, index)
            save_json(folder / "visuals" / v["name"] / "visual.json", v)

    if hashlib.sha256((EXISTING_PAGE / "page.json").read_bytes()).hexdigest() != first_page_hash:
        raise RuntimeError("La página 1 existente cambió")
    if hashlib.sha256(EXISTING_VISUAL.read_bytes()).hexdigest() != first_visual_hash:
        raise RuntimeError("El visual E01 existente cambió")
    print(f"Generadas {len(pages)} páginas, {len(manifest)} tablas y {len(manifest)} visuales.")


if __name__ == "__main__":
    main()
