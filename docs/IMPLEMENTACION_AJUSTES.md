# Ajustes de Inventario y Detalle — 2026-10-26

## Implementado

- Ruta `/ajustes`, con alias `/modulos/ajustes`; usa el filtro de autenticación existente.
- Listado, búsqueda por número/motivo/sucursal/empleado, filtros de estado y sucursal, paginación de ocho filas y cuatro KPI globales consultados mediante SELECT.
- Modales amplios Agregar, Editar y Consultar, selector de empleado por sucursal y múltiples productos con cantidades firmadas. El borrador puede no tener detalles; aplicarlo exige al menos uno.
- Validación de campos obligatorios, longitudes SQL, cantidades distintas de cero con hasta dos decimales, productos únicos y referencias reales. Antiforgery en todos los POST; los costos enviados por el cliente no se enlazan ni se utilizan.
- Guardado y eliminación únicamente de Borradores, con encabezado y detalles en una SqlTransaction. No modifican inventario.
- Aplicación Serializable: bloqueo UPDLOCK/HOLDLOCK del encabezado, inventarios en orden de producto, existencia no negativa ni fuera de DECIMAL(12,2), costo promedio positivo y captura de ese costo en cada detalle. Solo actualiza Existencia; conserva CostoPromedio y las otras columnas del inventario. La transición a Aplicado está en la misma transacción y condicionada a Borrador. Una repetición se rechaza antes de modificar inventario.
- Aplicados y Anulados son de solo consulta. No se implementa anulación. Falta de inventario se rechaza, sin crearlo. Históricos NULL se muestran como desconocidos, sin sustituirlos por costos actuales ni sumar valores parciales.
- Errores de duplicados, FK/CHECK, permisos, desbordamiento, esquema incompatible y deadlock tienen mensajes controlados.

## Archivos

Nuevos: Models/AjustesModels.cs, Data/AjustesDataService.cs, Controllers/AjustesController.cs, Views/Ajustes/Index.cshtml, wwwroot/js/ajustes.js y wwwroot/css/ajustes.css.

Integración: Program.cs (un registro de servicio), Views/Shared/_AppLayout.cshtml (un enlace), AGENTS.md (alcance de Ajustes). Se documenta esta etapa en MEMORY.md y COMANDOS_RAPIDOS.md. No se modifican los controladores, repositorios, vistas o estilos de otros módulos ni los scripts SQL.

## Pruebas ejecutadas por Codex

- Compilación final: `dotnet build --no-restore -p:UseAppHost=false -p:OutputPath=bin/AjustesCheck/`, 0 errores y 0 advertencias. El aislamiento bloqueó la primera escritura de bin/obj; la compilación se completó fuera del aislamiento, sin cambios de permisos o NuGet.
- `node --check` pasó para ajustes.js.
- Un ejecutable diagnóstico local en bin/AjustesCheck llamó exclusivamente BuscarAsync y ObtenerAsync, con User Secrets leídos en memoria. SELECT independiente contrastó los KPI: 7 ajustes, 1 Borrador, 5 Aplicados y 1 Anulado. Catálogos: 5 sucursales, 12 empleados y 13 productos. Los siete detalles leídos conservan costo NULL.
- Búsqueda por número, tres filtros de estado, filtros de sucursal 4 (2 ajustes) y 2 (1 ajuste), y correspondencia encabezado/detalles pasaron. La página solicitada fuera de rango se ajustó a 1/1; con siete registros no se pudo probar una segunda página real.
- Instancia de validación en localhost:5193: navegador real GET /ajustes redirigió a /acceso?returnUrl=%2Fajustes. No había sesión autenticada disponible; no se afirma haber comprobado visualmente la tabla ni los modales.
- GET sin sesión a Clientes, Empleados, Ventas, Pagos, Compras y los tres dashboards también redirigió al login existente. GET de ajustes.js y ajustes.css devolvió 200. No se comprobaron las pantallas protegidas de esos módulos con autenticación.
- La instancia principal anterior en 5192 no fue sustituida por la salida de validación. El proceso de validación se detuvo para la compilación final y se volvió a iniciar en http://localhost:5193/ajustes para las pruebas manuales autenticadas.
- No se ejecutaron INSERT, UPDATE ni DELETE de prueba, ni migraciones. La migración 04 fue declarada aplicada y verificada por el usuario; las lecturas del nuevo servicio confirmaron que la columna está disponible.

## Pendiente

Con sesión válida: comprobar tabla, filtros y modales en navegador, adición/eliminación de líneas y mensajes de validación. Con respaldo y autorización de prueba: persistencia de borradores, aplicación positiva/negativa, costos históricos, rechazo por stock insuficiente/costo cero/inventario faltante, duplicados, repetición/concurrencia y rollback forzado. La implementación transaccional revisada no equivale a pruebas de escritura ejecutadas.
