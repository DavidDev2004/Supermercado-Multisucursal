# BORRADOR · M04 Existencias y Disponibilidad

Integrante 2. Revisión visual previa documentada en VALIDACION_M07_M09.md; aprobación académica pendiente. Fuente: documento oficial 09 del integrante 2; no sustituye sus enunciados completos.

## Diseño propuesto

Cabecera azul marino y navegación lateral existente. Barra de filtros con sucursal, categoría, producto y condición de stock, botón Aplicar, Actualizar, impresión visual y sello de última actualización. Primera fila: cuatro tarjetas KPI. Segunda y tercera filas: dos gráficas por fila. En móvil: filtros y tarjetas apilados, gráficas en una columna y etiquetas legibles al tacto. Sin tablas de registros.

## Matriz de trazabilidad

| Enunciado | Visual | Tipo | Filtros previstos | Fuentes reales esperadas | Estado |
|---|---|---|---|---|---|
| I2-M04-E01 | Unidades Disponibles | KPI | Sucursal, categoría, producto, condición de stock | Tbl_Inventario + Tbl_Productos + Tbl_Categorias + Tbl_Sucursales | Implementado; revisión visual previa documentada; aprobación pendiente |
| I2-M04-E02 | Valor de Inventario | KPI | Sucursal, categoría, producto, condición de stock | Tbl_Inventario + Tbl_Productos + Tbl_Categorias + Tbl_Sucursales | Implementado; revisión visual previa documentada; aprobación pendiente |
| I2-M04-E03 | Productos Bajo Mínimo | KPI | Sucursal, categoría, producto, condición de stock | Tbl_Inventario + Tbl_Productos + Tbl_Categorias + Tbl_Sucursales | Implementado; revisión visual previa documentada; aprobación pendiente |
| I2-M04-E04 | Sin Existencia | KPI | Sucursal, categoría, producto, condición de stock | Tbl_Inventario + Tbl_Productos + Tbl_Categorias + Tbl_Sucursales | Implementado; revisión visual previa documentada; aprobación pendiente |
| I2-M04-E05 | Inventario por Categoría | Dona | Sucursal, categoría, producto, condición de stock | Tbl_Inventario + Tbl_Productos + Tbl_Categorias + Tbl_Sucursales | Implementado; revisión visual previa documentada; aprobación pendiente |
| I2-M04-E06 | Valor de Inventario por Sucursal | Barras | Sucursal, categoría, producto, condición de stock | Tbl_Inventario + Tbl_Productos + Tbl_Categorias + Tbl_Sucursales | Implementado; revisión visual previa documentada; aprobación pendiente |
| I2-M04-E07 | Productos Bajo Mínimo | Barras horizontales | Sucursal, categoría, producto, condición de stock | Tbl_Inventario + Tbl_Productos + Tbl_Categorias + Tbl_Sucursales | Implementado; revisión visual previa documentada; aprobación pendiente |
| I2-M04-E08 | Estado de Stock | Dona | Sucursal, categoría, producto, condición de stock | Tbl_Inventario + Tbl_Productos + Tbl_Categorias + Tbl_Sucursales | Implementado; revisión visual previa documentada; aprobación pendiente |

## Criterios y decisiones pendientes

- Implementado con inventario actual y SELECT parametrizado. Los totales de unidades pueden sumar unidades de medida distintas; el esquema no identifica moneda. El diseño sigue siendo BORRADOR hasta revisión.

Verificar con SELECT los valores, fechas, estados, ausencia de duplicaciones y permisos del servidor. Los datos inexistentes se muestran como «Sin datos».
