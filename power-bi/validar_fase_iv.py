"""Validación local de estructura PBIR/TMDL y consultas de Fase IV.

No abre conexiones ni ejecuta SQL. Los conteos SELECT se verifican aparte.
"""

from __future__ import annotations

import json
import re

import generar_ocho_paginas as g

def run() -> None:
    manifest = json.loads((g.QUERIES / "manifest.json").read_text(encoding="utf-8-sig"))
    meta = {m["key"]: m for m in json.loads((g.QUERIES / "metadata.json").read_text(encoding="utf-8-sig"))}
    counts = {c["key"]: c for c in json.loads((g.QUERIES / "conteos_validacion.json").read_text(encoding="utf-8-sig"))}
    pages_meta = json.loads((g.REPORT / "pages" / "pages.json").read_text(encoding="utf-8-sig"))
    model = (g.MODEL / "model.tmdl").read_text(encoding="utf-8-sig")
    assert len(manifest) == len(meta) == len(counts) == 40
    assert len(pages_meta["pageOrder"]) == len(set(pages_meta["pageOrder"])) == 9
    assert pages_meta["pageOrder"][0] == "b1ff16fab4f768d75370"
    table_files = list((g.MODEL / "tables").glob("*.tmdl"))
    assert len(table_files) == 45
    model_refs = [line for line in model.splitlines() if line.startswith("ref table ")]
    expected_refs = {f"ref table {path.stem}" for path in table_files}
    assert len(model_refs) == len(set(model_refs)) == 45
    assert set(model_refs) == expected_refs
    for path in g.MODEL.rglob("*.tmdl"):
        lines = path.read_text(encoding="utf-8-sig").splitlines()
        assert not any(line.startswith(("+", "-")) for line in lines), path
        if path.parent.name == "tables":
            assert lines[0] == f"table {path.stem}", path
            assert any(line.startswith("\tpartition ") for line in lines), path

    navigation = {}
    for page_index, page_id in enumerate(pages_meta["pageOrder"]):
        folder = g.REPORT / "pages" / page_id
        page = json.loads((folder / "page.json").read_text(encoding="utf-8-sig"))
        assert page["name"] == page_id and page["width"] == 1920 and page["height"] == 1080
        assert page["objects"]["background"][0]["properties"]["color"]["solid"]["color"]["expr"]["Literal"]["Value"] == "'#EDF4F7'"
        visuals = list((folder / "visuals").glob("*/visual.json"))
        assert len(visuals) == 15, (page_id, len(visuals))
        foreground = []
        data_count = 0
        nav_targets = []
        decorative_types = []
        tab_orders = []
        for file in visuals:
            v = json.loads(file.read_text(encoding="utf-8-sig"))
            assert v["name"] == file.parent.name
            pos = v["position"]
            assert 0 <= pos["x"] and 0 <= pos["y"]
            assert pos["x"] + pos["width"] <= 1920
            assert pos["y"] + pos["height"] <= 1080
            tab_orders.append(pos["tabOrder"])
            kind = v["visual"]["visualType"]
            if kind in {"tableEx", "clusteredBarChart", "lineChart", "cardVisual"}:
                data_count += 1
                foreground.append(pos)
            else:
                assert kind in {"shape", "textbox", "actionButton"}, file
                decorative_types.append(kind)
                if kind == "shape":
                    assert pos["x"] == pos["y"] == 0 and pos["width"] == 1920 and pos["height"] == 180
                else:
                    foreground.append(pos)
                if kind == "actionButton":
                    link = v["visual"]["visualContainerObjects"]["visualLink"][0]["properties"]
                    assert link["type"]["expr"]["Literal"]["Value"] == "'PageNavigation'"
                    target = link["navigationSection"]["expr"]["Literal"]["Value"].strip("'")
                    assert target in pages_meta["pageOrder"]
                    nav_targets.append(target)
        assert data_count == 5
        assert len(tab_orders) == len(set(tab_orders)), page_id
        assert sorted(decorative_types) == sorted(["shape"] + ["textbox"] * 3 + ["actionButton"] * 6)
        group = page_index // 3
        expected_targets = [pages_meta["pageOrder"][n * 3] for n in range(3)]
        expected_targets += pages_meta["pageOrder"][group * 3:group * 3 + 3]
        assert sorted(nav_targets) == sorted(expected_targets)
        navigation[page_id] = set(nav_targets)
        for i, a in enumerate(foreground):
            for b in foreground[i + 1:]:
                overlap = (a["x"] < b["x"] + b["width"] and a["x"] + a["width"] > b["x"]
                           and a["y"] < b["y"] + b["height"] and a["y"] + a["height"] > b["y"])
                assert not overlap, page_id

    for start in pages_meta["pageOrder"]:
        reachable = {start}
        pending = [start]
        while pending:
            for target in navigation[pending.pop()]:
                if target not in reachable:
                    reachable.add(target)
                    pending.append(target)
        assert reachable == set(pages_meta["pageOrder"]), start

    for item in manifest:
        key, table = item["key"], item["key"] + "_BI"
        assert f"ref table {table}" in model_refs
        sql = (g.QUERIES / f"{key}-BI.sql").read_text(encoding="utf-8-sig").strip()
        clean_sql = re.sub(r"(?m)^\s*--[^\n]*\n", "", sql).lstrip()
        assert re.match(r"(?i)^(SELECT|WITH)\b", clean_sql), key
        assert not re.search(r"(?i)\b(INSERT|UPDATE|DELETE|MERGE|CREATE|ALTER|DROP|EXEC|EXECUTE)\b", clean_sql), key
        tmdl = (g.MODEL / "tables" / f"{table}.tmdl").read_text(encoding="utf-8-sig")
        match = re.search(r'Sql\.Database\("localhost,1435", "([^\"]+)", \[Query="(.*?)"\]\)', tmdl, re.S)
        assert match and match.group(1) == item["base"]
        assert match.group(2).replace("#(lf)", "\n").replace('""', '"') == sql
        assert (key in counts and counts[key]["database"] == item["base"] and counts[key]["rows"] >= 0)
        actual = {c["name"] for c in meta[key]["columns"]}
        assert set(item["columnas"]).issubset(actual)
        for col in actual:
            assert f"column {col}\n" in tmdl
        folder = g.REPORT / "pages" / g.stable_id(f"pagina-i{item['integrante']}-p{item['pagina']}")
        file = folder / "visuals" / g.stable_id("visual-" + key) / "visual.json"
        v = json.loads(file.read_text(encoding="utf-8-sig"))
        expected_type = {"table": "tableEx", "bar": "clusteredBarChart",
                         "line": "lineChart", "cardVisual": "cardVisual"}[item["tipo"]]
        assert v["visual"]["visualType"] == expected_type
        roles = v["visual"]["query"]["queryState"]
        expected_roles = {"table": {"Values"}, "bar": {"Category", "Y"},
                          "line": {"Category", "Y", "Series"}, "cardVisual": {"Data"}}[item["tipo"]]
        assert set(roles) == expected_roles
        projected = {}
        for role, value in roles.items():
            projected[role] = []
            for p in value["projections"]:
                field = p["field"]
                column = field.get("Column") or field["Aggregation"]["Expression"]["Column"]
                assert column["Expression"]["SourceRef"]["Entity"] == table
                projected[role].append(column["Property"])
        cols = item["columnas"]
        if item["tipo"] == "table":
            expected_fields = {"Values": cols}
        elif item["tipo"] == "bar":
            expected_fields = {"Category": [cols[0]], "Y": [cols[1]]}
        elif item["tipo"] == "line":
            expected_fields = {"Category": [cols[0]], "Y": [cols[1]], "Series": [cols[2]]}
        else:
            expected_fields = {"Data": cols}
        assert projected == expected_fields, key
        assert "'#12304A'" in file.read_text(encoding="utf-8")
        assert "'#19A7A0'" in file.read_text(encoding="utf-8")

    all_text = "\n".join(p.read_text(encoding="utf-8-sig") for p in (g.MODEL / "tables").glob("*.tmdl"))
    assert not re.search(r"(?i)password\s*=|pwd\s*=|user\s+id\s*=", all_text)
    print("VALIDACIÓN OK: 9 páginas, 45 visuales de datos, 45 tablas, 40 SELECT nuevos y cero solapamientos.")
    print("Modelo TMDL, consultas y visuales validados desde los archivos fuente; 54 enlaces válidos.")


if __name__ == "__main__":
    run()
