# BORRADOR · M08 Proveedores y Costos de Adquisición

Integrante 3. Revisión visual previa documentada en VALIDACION_M07_M09.md; aprobación académica pendiente. Fuente: documento oficial 09 del integrante 3; no sustituye sus enunciados completos.

## Diseño propuesto

Cabecera azul marino y navegación lateral existente. Barra de filtros con fechas y dimensiones compatibles, botón Aplicar, Actualizar, impresión visual y sello de última actualización. Primera fila: cuatro tarjetas KPI. Segunda y tercera filas: dos gráficas por fila. En móvil: filtros y tarjetas apilados, gráficas en una columna y etiquetas legibles al tacto. Sin tablas de registros.

## Matriz de trazabilidad

| Enunciado | Visual | Tipo | Filtros previstos | Fuentes reales esperadas | Estado |
|---|---|---|---|---|---|
| I3-M08-E01 | Proveedores Activos | KPI | Fechas, proveedor, producto, categoría, sucursal; moneda no disponible | Tbl_Proveedores + Tbl_ProductoProveedor + Tbl_Compras + Tbl_ComprasDetalle + Tbl_Productos | Implementado; revisión visual previa documentada; aprobación pendiente |
| I3-M08-E02 | Productos con Proveedor | KPI | Fechas, proveedor, producto, categoría, sucursal; moneda no disponible | Tbl_Proveedores + Tbl_ProductoProveedor + Tbl_Compras + Tbl_ComprasDetalle + Tbl_Productos | Implementado; revisión visual previa documentada; aprobación pendiente |
| I3-M08-E03 | Costo Promedio de Adquisición | KPI | Fechas, proveedor, producto, categoría, sucursal; moneda no disponible | Tbl_Proveedores + Tbl_ProductoProveedor + Tbl_Compras + Tbl_ComprasDetalle + Tbl_Productos | Implementado; revisión visual previa documentada; aprobación pendiente |
| I3-M08-E04 | Plazo Promedio de Entrega | KPI | Fechas, proveedor, producto, categoría, sucursal; moneda no disponible | Tbl_Proveedores + Tbl_ProductoProveedor + Tbl_Compras + Tbl_ComprasDetalle + Tbl_Productos | Implementado; revisión visual previa documentada; aprobación pendiente |
| I3-M08-E05 | Proveedores por Volumen de Compras | Barras horizontales | Fechas, proveedor, producto, categoría, sucursal; moneda no disponible | Tbl_Proveedores + Tbl_ProductoProveedor + Tbl_Compras + Tbl_ComprasDetalle + Tbl_Productos | Implementado; revisión visual previa documentada; aprobación pendiente |
| I3-M08-E06 | Evolución del Costo de Productos | Línea | Fechas, proveedor, producto, categoría, sucursal; moneda no disponible | Tbl_Proveedores + Tbl_ProductoProveedor + Tbl_Compras + Tbl_ComprasDetalle + Tbl_Productos | Implementado; revisión visual previa documentada; aprobación pendiente |
| I3-M08-E07 | Comparativo de Costos por Proveedor | Barras | Fechas, proveedor, producto, categoría, sucursal; moneda no disponible | Tbl_Proveedores + Tbl_ProductoProveedor + Tbl_Compras + Tbl_ComprasDetalle + Tbl_Productos | Pendiente: no existe moneda ni tipo de cambio |
| I3-M08-E08 | Plazos de Entrega | Barras horizontales | Fechas, proveedor, producto, categoría, sucursal; moneda no disponible | Tbl_Proveedores + Tbl_ProductoProveedor + Tbl_Compras + Tbl_ComprasDetalle + Tbl_Productos | Implementado; revisión visual previa documentada; aprobación pendiente |

## Criterios y decisiones pendientes

- E07 se muestra Pendiente porque Tbl_ProductoProveedor carece de moneda y no es seguro comparar costos entre proveedores. Los otros siete enunciados usan proveedores y relaciones activas, compras Recibidas y fechas reales. E06 dibuja hasta cinco series y deja huecos en meses sin compras. El diseño sigue en BORRADOR.

Verificar con SELECT los valores, fechas, estados, ausencia de duplicaciones y permisos del servidor. Los datos inexistentes se muestran como «Sin datos».
