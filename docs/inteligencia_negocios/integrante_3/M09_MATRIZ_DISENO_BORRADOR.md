# BORRADOR · M09 Gestión y Desempeño de Sucursales

Integrante 3. Revisión visual previa documentada en VALIDACION_M07_M09.md; aprobación académica pendiente. Fuente: documento oficial 09 del integrante 3; no sustituye sus enunciados completos.

## Diseño propuesto

Cabecera azul marino y navegación lateral existente. Barra de filtros con fechas y dimensiones compatibles, botón Aplicar, Actualizar, impresión visual y sello de última actualización. Primera fila: cuatro tarjetas KPI. Segunda y tercera filas: dos gráficas por fila. En móvil: filtros y tarjetas apilados, gráficas en una columna y etiquetas legibles al tacto. Sin tablas de registros.

## Matriz de trazabilidad

| Enunciado | Visual | Tipo | Filtros previstos | Fuentes reales esperadas | Estado |
|---|---|---|---|---|---|
| I3-M09-E01 | Sucursales Activas | KPI | Fechas, sucursal, departamento | Tbl_Sucursales + Tbl_Ventas + Tbl_VentasDetalle + Tbl_Compras + Tbl_ComprasDetalle + Tbl_Empleados + Tbl_Inventario | Implementado; revisión visual previa documentada; aprobación pendiente |
| I3-M09-E02 | Ventas Totales | KPI | Fechas, sucursal, departamento | Tbl_Sucursales + Tbl_Ventas + Tbl_VentasDetalle + Tbl_Compras + Tbl_ComprasDetalle + Tbl_Empleados + Tbl_Inventario | Implementado; revisión visual previa documentada; aprobación pendiente |
| I3-M09-E03 | Compras Totales | KPI | Fechas, sucursal, departamento | Tbl_Sucursales + Tbl_Ventas + Tbl_VentasDetalle + Tbl_Compras + Tbl_ComprasDetalle + Tbl_Empleados + Tbl_Inventario | Implementado; revisión visual previa documentada; aprobación pendiente |
| I3-M09-E04 | Empleados Activos | KPI | Fechas, sucursal, departamento | Tbl_Sucursales + Tbl_Ventas + Tbl_VentasDetalle + Tbl_Compras + Tbl_ComprasDetalle + Tbl_Empleados + Tbl_Inventario | Implementado; revisión visual previa documentada; aprobación pendiente |
| I3-M09-E05 | Ventas frente a Compras por Sucursal | Barras agrupadas | Fechas, sucursal, departamento | Tbl_Sucursales + Tbl_Ventas + Tbl_VentasDetalle + Tbl_Compras + Tbl_ComprasDetalle + Tbl_Empleados + Tbl_Inventario | Implementado; revisión visual previa documentada; aprobación pendiente |
| I3-M09-E06 | Evolución de Ventas por Sucursal | Línea | Fechas, sucursal, departamento | Tbl_Sucursales + Tbl_Ventas + Tbl_VentasDetalle + Tbl_Compras + Tbl_ComprasDetalle + Tbl_Empleados + Tbl_Inventario | Implementado; revisión visual previa documentada; aprobación pendiente |
| I3-M09-E07 | Distribución de Empleados | Dona | Fechas, sucursal, departamento | Tbl_Sucursales + Tbl_Ventas + Tbl_VentasDetalle + Tbl_Compras + Tbl_ComprasDetalle + Tbl_Empleados + Tbl_Inventario | Implementado; revisión visual previa documentada; aprobación pendiente |
| I3-M09-E08 | Inventario Valorizado por Sucursal | Barras | Fechas, sucursal, departamento | Tbl_Sucursales + Tbl_Ventas + Tbl_VentasDetalle + Tbl_Compras + Tbl_ComprasDetalle + Tbl_Empleados + Tbl_Inventario | Implementado; revisión visual previa documentada; aprobación pendiente |

## Criterios y decisiones pendientes

- Implementación MVC local con importes de ventas Confirmadas y compras Recibidas calculados por separado; empleados e inventario son saldos actuales. El diseño sigue en BORRADOR hasta revisión.

Verificar con SELECT los valores, fechas, estados, ausencia de duplicaciones y permisos del servidor. Los datos inexistentes se muestran como «Sin datos».
