# BORRADOR · M02 Clientes y Comportamiento de Compra

Integrante 1. Revisión visual previa documentada en VALIDACION_M07_M09.md; aprobación académica pendiente. Fuente: documento oficial 09 del integrante 1; no sustituye sus enunciados completos.

## Diseño propuesto

Cabecera azul marino y navegación lateral existente. Barra de filtros con fechas y dimensiones compatibles, botón Aplicar, Actualizar, impresión visual y sello de última actualización. Primera fila: cuatro tarjetas KPI. Segunda y tercera filas: dos gráficas por fila. En móvil: filtros y tarjetas apilados, gráficas en una columna y etiquetas legibles al tacto. Sin tablas de registros.

## Matriz de trazabilidad

| Enunciado | Visual | Tipo | Filtros previstos | Fuentes reales esperadas | Estado |
|---|---|---|---|---|---|
| I1-M02-E01 | Clientes con Compras | KPI | Fechas, sucursal, departamento, municipio | Tbl_Ventas + Tbl_VentasDetalle + Tbl_Clientes | Implementado; revisión visual previa documentada; aprobación pendiente |
| I1-M02-E02 | Clientes Recurrentes | KPI | Fechas, sucursal, departamento, municipio | Tbl_Ventas + Tbl_VentasDetalle + Tbl_Clientes | Implementado; revisión visual previa documentada; aprobación pendiente |
| I1-M02-E03 | Compras por Cliente | KPI | Fechas, sucursal, departamento, municipio | Tbl_Ventas + Tbl_VentasDetalle + Tbl_Clientes | Implementado; revisión visual previa documentada; aprobación pendiente |
| I1-M02-E04 | Gasto Promedio por Cliente | KPI | Fechas, sucursal, departamento, municipio | Tbl_Ventas + Tbl_VentasDetalle + Tbl_Clientes | Implementado; revisión visual previa documentada; aprobación pendiente |
| I1-M02-E05 | Clientes por Mes | Línea | Fechas, sucursal, departamento, municipio | Tbl_Ventas + Tbl_VentasDetalle + Tbl_Clientes | Implementado; revisión visual previa documentada; aprobación pendiente |
| I1-M02-E06 | Clientes con Mayor Gasto | Barras horizontales | Fechas, sucursal, departamento, municipio | Tbl_Ventas + Tbl_VentasDetalle + Tbl_Clientes | Implementado; revisión visual previa documentada; aprobación pendiente |
| I1-M02-E07 | Distribución Geográfica de Clientes | Dona | Fechas, sucursal, departamento, municipio | Tbl_Ventas + Tbl_VentasDetalle + Tbl_Clientes | Implementado; revisión visual previa documentada; aprobación pendiente |
| I1-M02-E08 | Clientes Nuevos y Recurrentes | Barras agrupadas | Fechas, sucursal, departamento, municipio | Tbl_Ventas + Tbl_VentasDetalle + Tbl_Clientes | Implementado; revisión visual previa documentada; aprobación pendiente |

## Criterios y decisiones pendientes

- Nuevo = primera venta confirmada histórica; recurrente mensual = había comprado antes. Se calcula por cliente distinto, sin multiplicar por detalles.

Verificar con SELECT los valores, fechas, estados, ausencia de duplicaciones y permisos del servidor. Los datos inexistentes se muestran como «Sin datos».
