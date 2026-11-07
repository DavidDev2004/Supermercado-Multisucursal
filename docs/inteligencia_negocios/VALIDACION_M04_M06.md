# Validación local BI M04–M06 · 2026-11-07

Los diseños y matrices siguen identificados como **BORRADOR**. No se actualizó Production ni ngrok. La prueba usó una instancia aislada en `127.0.0.1:5202`, el código compilado localmente y SELECT de solo lectura a `SupermercadosGuate`.

| Comprobación ejecutada | Resultado |
|---|---|
| `dotnet build` Debug, salida `bin/BiCheckM456` | 0 errores, 0 advertencias. Un primer intento hacia `bin/BiCheck` falló porque la vista previa anterior mantenía el DLL abierto; se usó salida separada. |
| Inicio de sesión BodegaDemo y GET M04, M05, M06 | HTTP 200; cada ruta devuelve 4 tarjetas y 4 secciones de gráficas, sin error SQL. |
| Inicio de sesión CajaDemo y GET M04, M05, M06 | HTTP 403 en las tres rutas; M01 sigue con HTTP 200 para Caja. |
| BodegaDemo en M01 | HTTP 403. |
| M04, inventario actual | SELECT independiente: 65 posiciones, 5,381.00 unidades agregadas, 59,960.00 de `Existencia*CostoPromedio`, 0 bajo mínimo, 0 sin stock. Los cuatro KPI HTML muestran 5,381.00, Q 59,960.00, 0 y 0. El esquema no guarda código de moneda; `Q` mantiene la convención visual existente, sin conversión de divisas. Las unidades agregadas incluyen distintas `UnidadMedida`. |
| M06, período predeterminado 01/05/2026–27/10/2026 | SELECT independiente: 13 productos con ventas Confirmadas, 0 productos Activos sin ventas, 229.00 unidades vendidas, 0 posiciones bajo mínimo. Los cuatro KPI HTML concuerdan. |
| Filtros M04 y M06 | GET con sucursal/categoría/producto, condición Sin Stock y rango de fechas: HTTP 200 sin error SQL. Fechas invertidas muestran validación. |
| Gráfica M06-E08 | HTML contiene columnas agrupadas con las dos series, ventas del período y existencia actual. |
| Recursos y diseño adaptable | CSS y JavaScript BI: HTTP 200; `node --check` sin errores. La hoja contiene puntos de adaptación a 1050, 750 y 430 píxeles y desplazamiento horizontal para columnas. Esto es comprobación de código, no inspección visual autenticada. |
| M05 | Cuatro KPI y cuatro visuales presentan «Pendiente», no cero ficticio. SELECT confirmó 6 ajustes Aplicados sin FechaAplicacion y 5 detalles Aplicados sin CostoUnitario. Las 25 ventas Confirmadas no tienen columna de fecha de confirmación en el esquema. Compras Recibidas sí tienen FechaRecepcion, pero no cubren todas las salidas y ajustes. |
| Compras y monedas | SELECT: 7 Compras Recibidas con FechaRecepcion, 1 Borrador y 1 Anulada sin ella. Borrador/Anulada se excluyen de movimientos efectivos. El esquema no contiene columna de moneda ni tipo de cambio en Compras o Inventario; no se realizó conversión. |
| Publicación previa | GET local del login de Production en 5197: HTTP 200; no se reinició ni reemplazó esa instancia. |

**Pendientes:** revisión visual manual autenticada en navegador a tamaños de escritorio, tableta y teléfono; contraste independiente de cada punto de las ocho gráficas activas; definición académica de la fuente histórica para completar M05. La comprobación de Administrador en M04–M06 es estática mediante `RolePermissions`; se ejecutaron HTTP autenticados para Bodega y Caja, no para David. No se ejecutó DML, migración ni cambio a SQL Server.
