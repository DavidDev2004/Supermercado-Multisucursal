# BORRADOR · M01 Ventas y Tendencias Comerciales

Integrante 1. Revisión visual previa documentada en VALIDACION_M07_M09.md; aprobación académica pendiente. Fuente: documento oficial 09 del integrante 1; no sustituye sus enunciados completos.

## Diseño propuesto

Cabecera azul marino y navegación lateral existente. Barra de filtros con fechas y dimensiones compatibles, botón Aplicar, Actualizar, impresión visual y sello de última actualización. Primera fila: cuatro tarjetas KPI. Segunda y tercera filas: dos gráficas por fila. En móvil: filtros y tarjetas apilados, gráficas en una columna y etiquetas legibles al tacto. Sin tablas de registros.

## Matriz de trazabilidad

| Enunciado | Visual | Tipo | Filtros previstos | Fuentes reales esperadas | Estado |
|---|---|---|---|---|---|
| I1-M01-E01 | Ventas Netas | KPI | Fechas, sucursal, categoría, producto; canal ausente | Tbl_Ventas + Tbl_VentasDetalle + Tbl_Sucursales + Tbl_Productos + Tbl_Categorias | Implementado; revisión visual previa documentada; aprobación pendiente |
| I1-M01-E02 | Número de Ventas | KPI | Fechas, sucursal, categoría, producto; canal ausente | Tbl_Ventas + Tbl_VentasDetalle + Tbl_Sucursales + Tbl_Productos + Tbl_Categorias | Implementado; revisión visual previa documentada; aprobación pendiente |
| I1-M01-E03 | Ticket Promedio | KPI | Fechas, sucursal, categoría, producto; canal ausente | Tbl_Ventas + Tbl_VentasDetalle + Tbl_Sucursales + Tbl_Productos + Tbl_Categorias | Implementado; revisión visual previa documentada; aprobación pendiente |
| I1-M01-E04 | Unidades Vendidas | KPI | Fechas, sucursal, categoría, producto; canal ausente | Tbl_Ventas + Tbl_VentasDetalle + Tbl_Sucursales + Tbl_Productos + Tbl_Categorias | Implementado; revisión visual previa documentada; aprobación pendiente |
| I1-M01-E05 | Ventas por Mes | Línea | Fechas, sucursal, categoría, producto; canal ausente | Tbl_Ventas + Tbl_VentasDetalle + Tbl_Sucursales + Tbl_Productos + Tbl_Categorias | Implementado; revisión visual previa documentada; aprobación pendiente |
| I1-M01-E06 | Ventas por Sucursal | Barras | Fechas, sucursal, categoría, producto; canal ausente | Tbl_Ventas + Tbl_VentasDetalle + Tbl_Sucursales + Tbl_Productos + Tbl_Categorias | Implementado; revisión visual previa documentada; aprobación pendiente |
| I1-M01-E07 | Ventas por Categoría | Dona | Fechas, sucursal, categoría, producto; canal ausente | Tbl_Ventas + Tbl_VentasDetalle + Tbl_Sucursales + Tbl_Productos + Tbl_Categorias | Implementado; revisión visual previa documentada; aprobación pendiente |
| I1-M01-E08 | Productos Más Vendidos | Barras horizontales | Fechas, sucursal, categoría, producto; canal ausente | Tbl_Ventas + Tbl_VentasDetalle + Tbl_Sucursales + Tbl_Productos + Tbl_Categorias | Implementado; revisión visual previa documentada; aprobación pendiente |

## Criterios y decisiones pendientes

- Importe neto = cantidad × precio unitario − descuento de cada línea confirmada. Número de ventas = documentos distintos. Variación: período anterior de igual duración.
- No hay columna de canal de venta; se omite ese filtro.

Verificar con SELECT los valores, fechas, estados, ausencia de duplicaciones y permisos del servidor. Los datos inexistentes se muestran como «Sin datos».
