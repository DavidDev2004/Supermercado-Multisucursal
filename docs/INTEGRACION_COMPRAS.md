# Integración de Compras y Detalle de Compras — 2026-10-25

## Procedencia y alcance

- Base: archivo local `2026_BaseDatos integrante incorporacion.zip`, 271 archivos; SHA-256 `FCCA65850C35F94640C9446687ED42D7E97709812041B098C5F94CF3C72DE3A7`.
- Se adaptaron los modelos, flujo y repositorio de Compras de `Supermercado.Web` al esquema y componentes de `SupermercadoMVC`. Se reutilizó la presentación del módulo Ventas sin modificar sus archivos.
- No se incorporaron login, usuarios, configuración, paquetes ni scripts SQL de la integrante.
- Respaldo conservado en `C:\BaseDatos_2026_RESPALDO_ANTES_INTEGRACION_20261025` (1,320 archivos). No se escribió en él.

## Implementación

- `/compras`: consulta, búsqueda por número/proveedor/NIT, filtro por estado, paginación de ocho filas, cuatro KPI y modales amplios de encabezado y múltiples detalles.
- `/modulos/compras` redirige a Compras desde el nuevo controlador, sin cambiar ModulesController.
- Proveedores, sucursales, empleados y productos provienen de SELECT. Se muestra el nombre y se guarda el código; empleado validado en su sucursal.
- Se usan `NumeroCompra`, `FechaCompra`, `FechaEntregaProgramada`, `FechaRecepcion`, observaciones del encabezado y líneas, y las columnas reales de `Tbl_ComprasDetalle`. No existen Total/Subtotal persistidos: importes = cantidad × costo − descuento.
- Borradores admiten cero detalles; guardar, editar y eliminar usan SqlTransaction y no modifican inventario. El estado nuevo es siempre Borrador.
- Recepción exige Borrador, detalles válidos y fecha no anterior a la compra. Se verifica en el servidor, independientemente del formulario.
- Todos los mutadores bloquean primero el encabezado con UPDLOCK/HOLDLOCK bajo Serializable. La recepción bloquea inventarios en orden de producto; existencias, costos, fecha de ingreso y transición de estado quedan en la misma transacción. COMMIT o ROLLBACK completo; recepciones repetidas encuentran Recibida y se rechazan.
- Costo promedio autorizado: `ROUND(((Existencia * CostoPromedio) + (Cantidad * CostoUnitario - Descuento)) / (Existencia + Cantidad), 2)`; implementación C# decimal y parámetros DECIMAL(12,2), sin float. Se comprueban límites antes de guardar.
- Descuento monetario: el script y el CHECK real permiten `0 <= Descuento <= Cantidad * CostoUnitario`, sin representación porcentual. Si falta inventario sucursal/producto se rechaza la recepción; no se crea inventario ni se modifica ExistenciaMinima.
- Recibidas y Anuladas solo consulta, también al solicitar `modo=editar`; el servidor rechaza mutaciones. Sin anulación.
- Validación de obligatorios, longitudes, fechas, decimales positivos, descuento, productos únicos y FK. Antiforgery en todos los POST, mensajes seguros para duplicados, FK, permisos y errores de transacción.

## Verificaciones ejecutadas por Codex, sin DML

- Compilación `dotnet build --no-restore`: 0 errores, 0 advertencias. El sandbox bloqueó obj; compilar fuera de él permitió continuar sin alterar permisos. Se detuvo solo la instancia MVC anterior que bloqueaba el ejecutable y se inició la compilación nueva en localhost:5192.
- SQL Server, SELECT: 9 compras (7 Recibida, 1 Borrador, 1 Anulada), importe recibido Q14,567.50. FC-1008 permanece Borrador con Q600.00; FC-1007 Recibida con Q2,167.50; FC-1009 Anulada con Q150.00.
- Se consultó `sys.check_constraints` y se confirmó el CHECK monetario. Inventario observado: 65 filas, suma de existencias 5,382.00 y suma de costos promedio 802.80 (esta suma es solo control de lectura, no un KPI económico).
- Navegador integrado real: sesión demo normal; listado y KPI concordantes; búsqueda FC-1008 (una fila); filtro Recibida (siete filas sin acciones de escritura); página 2 (FC-1001); modales Agregar vacío y Editar FC-1008 precargado.
- Se cargaron seis proveedores y cinco sucursales; empleados filtrados por sucursal. Se agregó una línea únicamente en el formulario, sin guardar: 2 × 3 − 1 = 5 y total visual 605. Descuento 7 produjo mensaje de validación. Token presente, método POST y acción `/compras/guardar/8` comprobados sin enviar.
- Modales de Recibida y Anulada con fieldset deshabilitado y sin botón Guardar, incluso solicitando edición por URL.
- Clientes, Empleados, Ventas, Pagos y los tres dashboards cargaron en navegador sin aviso SQL; `/modulos/compras` también abre Compras. Consola sin errores/advertencias observados.

## Pendiente de autorización y prueba manual

Crear, editar, eliminar borradores y recibir una compra; comprobar persistencia, fechas, costo ponderado y existencias mediante SELECT antes/después. Probar recepción repetida/concurrente, inventario ausente, duplicados, errores forzados y ROLLBACK. Las garantías transaccionales se revisaron en código, pero no se ejecutaron operaciones de escritura ni se provocaron errores SQL.

Verificación SELECT final tras navegar: se mantienen 7 Recibidas, 1 Borrador y 1 Anulada; 20 detalles, 65 inventarios, suma de existencias 5382.00 y suma de costos 802.80. Cero compras con relación empleado/sucursal inválida. No se ejecutaron escrituras.
