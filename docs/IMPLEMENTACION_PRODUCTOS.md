# Productos y relación Producto–Proveedor

Implementación autorizada el 2026-10-26. Fuente de reglas: `01_Proyecto_Final_Integrante_3.docx` y `03. CRUD MVC Foreign Key.docx` en Downloads, scripts del grupo y guías locales FK/UI. Se conserva el diseño coral/turquesa.

## Implementado

- `/productos`, con alias `/modulos/productos`, protegido por la autenticación existente.
- Consulta, búsqueda por código de barras/nombre/marca/categoría, filtros de estado y categoría, paginación de 8 y cuatro KPI globales: productos, activos, inactivos y relaciones con proveedores.
- Modales de producto y administración de proveedores por producto. Altas/ediciones de la relación con todos los campos reales: proveedor, código externo, costo de referencia, plazo de entrega, fecha y estado. Desplegables SELECT de categorías y proveedores existentes.
- ADO.NET, parámetros tipados, antiforgery y validación en servidor: obligatorios, longitudes VARCHAR, FK existente, unicidad de código de barras y pareja producto/proveedor, precio/costo positivos DECIMAL(12,2), plazo entero no negativo, fechas sin hora y estados Activo/Inactivo. Sin reglas de activación o fechas futuras adicionales.
- Escrituras implementadas con SqlTransaction Serializable, COMMIT/ROLLBACK y bloqueo del producto. Las restricciones SQL respaldan la validación de duplicados y FK. Identificadores editables obtenidos desde la ruta; no se confía en IDs ocultos.
- Eliminación requiere confirmación de registro de práctica. Un producto con inventario, relaciones o detalles de documentos no se elimina; se informa que debe conservarse/inactivarse. Sin cascadas automáticas. Las relaciones con compras tampoco se eliminan ni cambian de proveedor; pueden editar sus datos e inactivarse. El esquema no tiene una marca de práctica: la confirmación declara esa condición, y el servidor comprueba las dependencias.
- No crea inventarios ni modifica compras, ventas, ajustes, precios/costos históricos o saldos.

## Archivos

Nuevos: `Models/ProductosModels.cs`, `Data/ProductosDataService.cs`, `Controllers/ProductosController.cs`, `Views/Productos/Index.cshtml`, `Views/Productos/_Filters.cshtml`, `wwwroot/js/productos.js`, `wwwroot/css/productos.css`.

Integración: Program.cs registra el servicio; _AppLayout cambia exclusivamente el enlace Productos; AGENTS.md amplía exclusivamente el alcance autorizado. MEMORY.md y COMANDOS_RAPIDOS.md registran los resultados. Comparación SHA-256: los archivos funcionales preexistentes, excepto el layout indicado, no cambiaron; Program.cs solo recibió el registro de servicio.

## Verificaciones ejecutadas por Codex

- Build final en `bin/ProductosCheck`: 0 errores, 0 advertencias. El aislamiento negó escritura en bin/obj; la misma compilación fuera de ese aislamiento pasó. No se cambiaron ACL, NuGet ni instalaciones.
- `node --check wwwroot/js/productos.js`: correcto.
- Diagnóstico local en bin/ProductosCheck, sin modificar el código funcional: invocó solamente BuscarAsync, ObtenerAsync, RelacionesAsync y SELECT independientes. SQL real: 13 productos, todos activos, 0 inactivos, 16 relaciones, 6 categorías y 6 proveedores. Dos páginas de 8 y 5 registros; los 13 detalles y sus relaciones coinciden con el listado. Filtros por ambos estados y las 6 categorías, búsqueda por código de barras y ajuste al último número de página correctos. KPI contrastados con SELECT independientes. Validaciones de modelos comprobadas en memoria, sin guardar.
- Instancia nueva iniciada en `http://localhost:5193`, reemplazando únicamente la instancia temporal anterior de ese puerto; 5192 conservada.
- HTTP GET: Productos, modal nuevo, proveedores y alias redirigen correctamente al login sin cookie. JS/CSS y `/acceso` responden 200. Las rutas de Clientes, Empleados, Ventas, Pagos, Compras, Ajustes y los tres dashboards conservan sus redirecciones de autenticación. Esto no acredita la navegación autenticada de esos módulos.
- Navegador real: `/productos` mostró el login con returnUrl; no había sesión válida en el navegador de Codex. No se comprobaron visualmente tabla, modales ni botones autenticados. SELECT actual: Tbl_Usuarios existe y tiene 1 usuario Activo; no se accedió a su contraseña ni se inició sesión.
- No se ejecutaron POST, INSERT, UPDATE, DELETE, migraciones ni Git.

## Pendiente

Iniciar sesión con el usuario existente y comprobar visualmente listado, modales, edición precargada, catálogos, mensajes, filtros y paginación. Con autorización y respaldo previo: persistencia de productos/relaciones de práctica, duplicados, FK inexistentes, protección de dependencias, concurrencia y rollback. Las escrituras están implementadas, no probadas en SQL Server.
