# BORRADOR · M06 Rotación y Reposición de Productos

Integrante 2. Revisión visual previa documentada en VALIDACION_M07_M09.md; aprobación académica pendiente. Fuente: documento oficial 09 del integrante 2; no sustituye sus enunciados completos.

## Diseño propuesto

Cabecera azul marino y navegación lateral existente. Barra de filtros con fechas y dimensiones compatibles, botón Aplicar, Actualizar, impresión visual y sello de última actualización. Primera fila: cuatro tarjetas KPI. Segunda y tercera filas: dos gráficas por fila. En móvil: filtros y tarjetas apilados, gráficas en una columna y etiquetas legibles al tacto. Sin tablas de registros.

## Matriz de trazabilidad

| Enunciado | Visual | Tipo | Filtros previstos | Fuentes reales esperadas | Estado |
|---|---|---|---|---|---|
| I2-M06-E01 | Productos con Ventas | KPI | Fechas, sucursal, categoría, producto | Tbl_Productos + Tbl_VentasDetalle + Tbl_Ventas + Tbl_Inventario + Tbl_Categorias | Implementado; revisión visual previa documentada; aprobación pendiente |
| I2-M06-E02 | Productos Sin Ventas | KPI | Fechas, sucursal, categoría, producto | Tbl_Productos + Tbl_VentasDetalle + Tbl_Ventas + Tbl_Inventario + Tbl_Categorias | Implementado; revisión visual previa documentada; aprobación pendiente |
| I2-M06-E03 | Unidades Vendidas | KPI | Fechas, sucursal, categoría, producto | Tbl_Productos + Tbl_VentasDetalle + Tbl_Ventas + Tbl_Inventario + Tbl_Categorias | Implementado; revisión visual previa documentada; aprobación pendiente |
| I2-M06-E04 | Requieren Reposición | KPI | Fechas, sucursal, categoría, producto | Tbl_Productos + Tbl_VentasDetalle + Tbl_Ventas + Tbl_Inventario + Tbl_Categorias | Implementado; revisión visual previa documentada; aprobación pendiente |
| I2-M06-E05 | Productos Más Vendidos | Barras horizontales | Fechas, sucursal, categoría, producto | Tbl_Productos + Tbl_VentasDetalle + Tbl_Ventas + Tbl_Inventario + Tbl_Categorias | Implementado; revisión visual previa documentada; aprobación pendiente |
| I2-M06-E06 | Productos de Baja Rotación | Barras horizontales | Fechas, sucursal, categoría, producto | Tbl_Productos + Tbl_VentasDetalle + Tbl_Ventas + Tbl_Inventario + Tbl_Categorias | Implementado; revisión visual previa documentada; aprobación pendiente |
| I2-M06-E07 | Rotación por Categoría | Barras | Fechas, sucursal, categoría, producto | Tbl_Productos + Tbl_VentasDetalle + Tbl_Ventas + Tbl_Inventario + Tbl_Categorias | Implementado; revisión visual previa documentada; aprobación pendiente |
| I2-M06-E08 | Ventas frente a Existencias | Columnas agrupadas | Fechas, sucursal, categoría, producto | Tbl_Productos + Tbl_VentasDetalle + Tbl_Ventas + Tbl_Inventario + Tbl_Categorias | Implementado; revisión visual previa documentada; aprobación pendiente |

## Criterios y decisiones pendientes

- Implementado con ventas Confirmadas y posiciones actuales de inventario, sin multiplicar las cantidades al unir fuentes. E06 mide ventas bajas, no una tasa de rotación; las unidades pueden tener medidas distintas. El diseño sigue siendo BORRADOR hasta revisión.

Verificar con SELECT los valores, fechas, estados, ausencia de duplicaciones y permisos del servidor. Los datos inexistentes se muestran como «Sin datos».
