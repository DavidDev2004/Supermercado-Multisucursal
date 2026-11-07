# BORRADOR · M05 Movimientos y Control de Inventario

Integrante 2. Revisión visual previa documentada en VALIDACION_M07_M09.md; aprobación académica pendiente. Fuente: documento oficial 09 del integrante 2; no sustituye sus enunciados completos.

## Diseño propuesto

Cabecera azul marino y navegación lateral existente. Barra de filtros con fechas y dimensiones compatibles, botón Aplicar, Actualizar, impresión visual y sello de última actualización. Primera fila: cuatro tarjetas KPI. Segunda y tercera filas: dos gráficas por fila. En móvil: filtros y tarjetas apilados, gráficas en una columna y etiquetas legibles al tacto. Sin tablas de registros.

## Matriz de trazabilidad

| Enunciado | Visual | Tipo | Filtros previstos | Fuentes reales esperadas | Estado |
|---|---|---|---|---|---|
| I2-M05-E01 | Unidades de Entrada | KPI | Fechas, sucursal, producto, tipo; NO existe tabla unificada de movimientos | Tbl_ComprasDetalle + Tbl_VentasDetalle + Tbl_AjustesInventarioDetalle y sus encabezados | Pendiente: historial incompleto |
| I2-M05-E02 | Unidades de Salida | KPI | Fechas, sucursal, producto, tipo; NO existe tabla unificada de movimientos | Tbl_ComprasDetalle + Tbl_VentasDetalle + Tbl_AjustesInventarioDetalle y sus encabezados | Pendiente: historial incompleto |
| I2-M05-E03 | Total de Movimientos | KPI | Fechas, sucursal, producto, tipo; NO existe tabla unificada de movimientos | Tbl_ComprasDetalle + Tbl_VentasDetalle + Tbl_AjustesInventarioDetalle y sus encabezados | Pendiente: historial incompleto |
| I2-M05-E04 | Valor de Movimientos | KPI | Fechas, sucursal, producto, tipo; NO existe tabla unificada de movimientos | Tbl_ComprasDetalle + Tbl_VentasDetalle + Tbl_AjustesInventarioDetalle y sus encabezados | Pendiente: historial incompleto |
| I2-M05-E05 | Movimientos por Fecha | Columnas agrupadas | Fechas, sucursal, producto, tipo; NO existe tabla unificada de movimientos | Tbl_ComprasDetalle + Tbl_VentasDetalle + Tbl_AjustesInventarioDetalle y sus encabezados | Pendiente: historial incompleto |
| I2-M05-E06 | Entradas frente a Salidas | Barras agrupadas | Fechas, sucursal, producto, tipo; NO existe tabla unificada de movimientos | Tbl_ComprasDetalle + Tbl_VentasDetalle + Tbl_AjustesInventarioDetalle y sus encabezados | Pendiente: historial incompleto |
| I2-M05-E07 | Movimientos por Tipo | Dona | Fechas, sucursal, producto, tipo; NO existe tabla unificada de movimientos | Tbl_ComprasDetalle + Tbl_VentasDetalle + Tbl_AjustesInventarioDetalle y sus encabezados | Pendiente: historial incompleto |
| I2-M05-E08 | Movimientos por Sucursal | Barras horizontales | Fechas, sucursal, producto, tipo; NO existe tabla unificada de movimientos | Tbl_ComprasDetalle + Tbl_VentasDetalle + Tbl_AjustesInventarioDetalle y sus encabezados | Pendiente: historial incompleto |

## Criterios y decisiones pendientes

- La ruta MVC presenta las ocho posiciones y explica por qué están pendientes. No hay historial único: ventas confirmadas carecen de fecha de confirmación, todos los ajustes Aplicados actuales carecen de FechaAplicacion y cinco detalles aplicados carecen de CostoUnitario histórico. Los costos tampoco tienen columna de moneda. No se presentan ceros como movimientos reales.

Verificar con SELECT los valores, fechas, estados, ausencia de duplicaciones y permisos del servidor. Los datos inexistentes se muestran como «Sin datos».
