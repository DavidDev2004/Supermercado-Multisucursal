# BORRADOR · M03 Cobros y Medios de Pago

Integrante 1. Revisión visual previa documentada en VALIDACION_M07_M09.md; aprobación académica pendiente. Fuente: documento oficial 09 del integrante 1; no sustituye sus enunciados completos.

## Diseño propuesto

Cabecera azul marino y navegación lateral existente. Barra de filtros con fechas y dimensiones compatibles, botón Aplicar, Actualizar, impresión visual y sello de última actualización. Primera fila: cuatro tarjetas KPI. Segunda y tercera filas: dos gráficas por fila. En móvil: filtros y tarjetas apilados, gráficas en una columna y etiquetas legibles al tacto. Sin tablas de registros.

## Matriz de trazabilidad

| Enunciado | Visual | Tipo | Filtros previstos | Fuentes reales esperadas | Estado |
|---|---|---|---|---|---|
| I1-M03-E01 | Total Cobrado | KPI | Fechas, sucursal, método, estado | Tbl_Pagos + Tbl_Ventas + Tbl_Sucursales | Implementado; revisión visual previa documentada; aprobación pendiente |
| I1-M03-E02 | Número de Cobros | KPI | Fechas, sucursal, método, estado | Tbl_Pagos + Tbl_Ventas + Tbl_Sucursales | Implementado; revisión visual previa documentada; aprobación pendiente |
| I1-M03-E03 | Cobro Promedio | KPI | Fechas, sucursal, método, estado | Tbl_Pagos + Tbl_Ventas + Tbl_Sucursales | Implementado; revisión visual previa documentada; aprobación pendiente |
| I1-M03-E04 | Comisiones | KPI | Fechas, sucursal, método, estado | Tbl_Pagos + Tbl_Ventas + Tbl_Sucursales | Implementado; revisión visual previa documentada; aprobación pendiente |
| I1-M03-E05 | Cobros por Método de Pago | Dona | Fechas, sucursal, método, estado | Tbl_Pagos + Tbl_Ventas + Tbl_Sucursales | Implementado; revisión visual previa documentada; aprobación pendiente |
| I1-M03-E06 | Cobros por Mes | Columnas | Fechas, sucursal, método, estado | Tbl_Pagos + Tbl_Ventas + Tbl_Sucursales | Implementado; revisión visual previa documentada; aprobación pendiente |
| I1-M03-E07 | Estado de Cobros | Dona | Fechas, sucursal, método, estado | Tbl_Pagos + Tbl_Ventas + Tbl_Sucursales | Implementado; revisión visual previa documentada; aprobación pendiente |
| I1-M03-E08 | Cobros por Sucursal | Barras horizontales | Fechas, sucursal, método, estado | Tbl_Pagos + Tbl_Ventas + Tbl_Sucursales | Implementado; revisión visual previa documentada; aprobación pendiente |

## Criterios y decisiones pendientes

- Los KPI monetarios y por comprobante usan solo pagos Aplicados; el gráfico de estados cuenta también Anulados. La comisión no reduce el abono.

Verificar con SELECT los valores, fechas, estados, ausencia de duplicaciones y permisos del servidor. Los datos inexistentes se muestran como «Sin datos».
