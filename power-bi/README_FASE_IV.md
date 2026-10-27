# Fase IV — proyecto Power BI

Abrir `Supermercado_FaseIV.pbip` en Power BI Desktop. Los archivos `.pbix` de esta carpeta son copias anteriores y no contienen las ocho páginas añadidas al PBIP.

Las imágenes originales usadas como referencia visual se conservan localmente en `referencias/` y no se distribuyen en el repositorio público. Las páginas PBIR y el modelo TMDL sí forman parte del proyecto.

## Alcance

- 9 páginas y 45 visualizaciones: cinco ejercicios por página, conforme a los prototipos de los tres integrantes.
- Tablas, gráficos de barras, gráficos de líneas y tarjetas según el prototipo.
- Los 39 ejercicios E01–E13 leen `SupermercadosGuate`.
- E14 y E15 leen **solo** `SupermercadosGuate_PRUEBAS` mediante SELECT finales. La evidencia de las decisiones SI/NO y su limpieza está en `../docs/VALIDACION_CONSULTAS_SQL.md`.
- E15 del integrante 3 deja `Antes` en NULL porque la existencia histórica anterior no quedó almacenada en la base. `Despues` muestra la existencia actual de la copia de pruebas; no se atribuye un valor histórico inventado.

## Consultas y validación

Los SELECT de las ocho páginas nuevas están en `consultas/I*-E*-BI.sql`. `consultas/manifest.json` registra página, visual, columnas y base. `consultas/metadata.json` registra los tipos observados mediante inspección de esquema SQL; `consultas/conteos_validacion.json` conserva únicamente el número de filas leído en cada consulta. Ninguno contiene credenciales.

`preparar_consultas_fase_iv.py` genera los SELECT, `generar_ocho_paginas.py` genera las nuevas tablas TMDL y páginas PBIR, y `validar_fase_iv.py` comprueba la estructura local. Los generadores no se conectan a SQL Server. Para aplicar cambios hechos externamente mientras Desktop está abierto, usar **Aplicar cambios externos** y después **Actualizar**.

E06 y E10 de la primera página devolvieron cero filas en la verificación actual: no había inventarios bajo el mínimo y ningún proveedor tenía compras recibidas en ambos periodos de la consulta. Las tablas vacías deben conservarse como resultados legítimos.

La validación estructural local no reemplaza la revisión visual final en Power BI Desktop.
