# BORRADOR · M07 Compras y Abastecimiento

Integrante 3. Revisión visual previa documentada en VALIDACION_M07_M09.md; aprobación académica pendiente. Fuente: documento oficial 09 del integrante 3; no sustituye sus enunciados completos.

## Diseño propuesto

Cabecera azul marino y navegación lateral existente. Barra de filtros con fechas y dimensiones compatibles, botón Aplicar, Actualizar, impresión visual y sello de última actualización. Primera fila: cuatro tarjetas KPI. Segunda y tercera filas: dos gráficas por fila. En móvil: filtros y tarjetas apilados, gráficas en una columna y etiquetas legibles al tacto. Sin tablas de registros.

## Matriz de trazabilidad

| Enunciado | Visual | Tipo | Filtros previstos | Fuentes reales esperadas | Estado |
|---|---|---|---|---|---|
| I3-M07-E01 | Total de Compras | KPI | Fechas, sucursal, proveedor, producto, estado; Borrador no equivale a orden pendiente formal | Tbl_Compras + Tbl_ComprasDetalle + Tbl_Sucursales + Tbl_Proveedores | Implementado; revisión visual previa documentada; aprobación pendiente |
| I3-M07-E02 | Número de Órdenes | KPI | Fechas, sucursal, proveedor, producto, estado; Borrador no equivale a orden pendiente formal | Tbl_Compras + Tbl_ComprasDetalle + Tbl_Sucursales + Tbl_Proveedores | Implementado; revisión visual previa documentada; aprobación pendiente |
| I3-M07-E03 | Compras Pendientes | KPI | Fechas, sucursal, proveedor, producto, estado; Borrador no equivale a orden pendiente formal | Tbl_Compras + Tbl_ComprasDetalle + Tbl_Sucursales + Tbl_Proveedores | Pendiente: no existe estado de orden formal pendiente |
| I3-M07-E04 | Compras Recibidas | KPI | Fechas, sucursal, proveedor, producto, estado; Borrador no equivale a orden pendiente formal | Tbl_Compras + Tbl_ComprasDetalle + Tbl_Sucursales + Tbl_Proveedores | Implementado; revisión visual previa documentada; aprobación pendiente |
| I3-M07-E05 | Compras por Mes | Columnas | Fechas, sucursal, proveedor, producto, estado; Borrador no equivale a orden pendiente formal | Tbl_Compras + Tbl_ComprasDetalle + Tbl_Sucursales + Tbl_Proveedores | Implementado; revisión visual previa documentada; aprobación pendiente |
| I3-M07-E06 | Compras por Estado | Dona | Fechas, sucursal, proveedor, producto, estado; Borrador no equivale a orden pendiente formal | Tbl_Compras + Tbl_ComprasDetalle + Tbl_Sucursales + Tbl_Proveedores | Implementado; revisión visual previa documentada; aprobación pendiente |
| I3-M07-E07 | Compras por Sucursal | Barras | Fechas, sucursal, proveedor, producto, estado; Borrador no equivale a orden pendiente formal | Tbl_Compras + Tbl_ComprasDetalle + Tbl_Sucursales + Tbl_Proveedores | Implementado; revisión visual previa documentada; aprobación pendiente |
| I3-M07-E08 | Compras por Proveedor | Barras horizontales | Fechas, sucursal, proveedor, producto, estado; Borrador no equivale a orden pendiente formal | Tbl_Compras + Tbl_ComprasDetalle + Tbl_Sucursales + Tbl_Proveedores | Implementado; revisión visual previa documentada; aprobación pendiente |

## Criterios y decisiones pendientes

- La ruta MVC muestra E03 como Pendiente, sin sustituir Borrador por orden formal. Los otros siete enunciados usan FechaCompra para órdenes y FechaRecepcion para importes y recepciones. El diseño sigue en BORRADOR.

Verificar con SELECT los valores, fechas, estados, ausencia de duplicaciones y permisos del servidor. Los datos inexistentes se muestran como «Sin datos».
