# Módulo Inventario de solo consulta

Implementado el 2026-10-26 en `C:\BaseDatos_2026`. Fuentes: `AGENTS.md`, scripts del grupo, reglas oficiales del integrante III y diseño actual.

## Funcionalidad

- Ruta `/inventario`, con alias `/modulos/inventario`; protegido por la autenticación existente.
- Consulta ADO.NET con SELECT parametrizados de `Tbl_Inventario`, `Tbl_Productos`, `Tbl_Categorias` y `Tbl_Sucursales`.
- Tabla por combinación sucursal/producto: nombre y código de barras, categoría, sucursal, existencia, mínimo, costo promedio, fecha de último ingreso y último conteo. Las fechas NULL muestran «Sin registro».
- Búsqueda por código de barras, producto, marca, categoría o sucursal; filtros por sucursal, categoría y situación; paginación de 8 filas.
- Cuatro KPI globales obtenidos de `Tbl_Inventario`: registros, unidades, registros bajo mínimo (`Existencia < ExistenciaMinima`) y sin existencias (`Existencia = 0`). Una fila puede cumplir ambas situaciones. Ningún KPI inventa importes ni movimientos.
- Pantalla de solo lectura, sin botones de edición ni acciones POST. Existencias y costo promedio permanecen a cargo de los módulos transaccionales ya existentes.

## Archivos

Nuevos: `Models/InventarioModels.cs`, `Data/InventarioDataService.cs`, `Controllers/InventarioController.cs`, `Views/Inventario/Index.cshtml`, `wwwroot/css/inventario.css`.

Integración: `Program.cs` registra el servicio; `_AppLayout.cshtml` cambia el enlace lateral de Inventario; `AGENTS.md` amplía únicamente el alcance para este módulo de consulta. Comparación SHA-256 antes/después: los demás archivos funcionales preexistentes de controladores, datos, modelos, vistas y recursos no cambiaron.

## Comprobaciones realmente ejecutadas

- `dotnet build --no-restore -p:UseAppHost=false -p:OutputPath=bin/InventarioCheck/`: 0 errores y 0 advertencias fuera del aislamiento que impide escribir en bin/obj. Sin cambios en permisos o configuración.
- Diagnóstico local de solo lectura: ejecutó `InventarioDataService.ConsultarAsync` para las nueve páginas, todas las sucursales y categorías, filtros de bajo mínimo y sin existencias, búsqueda por código y paginación extrema. Contrastó KPI y conteos por sucursal/categoría con SELECT independientes. Resultado actual: 65 inventarios, 5 sucursales, 6 categorías, 5,383.00 unidades, 0 bajo mínimo y 0 sin existencias; páginas 8+8+8+8+8+8+8+8+1. Sin filas duplicadas.
- Instancia de validación en `http://localhost:5193`; la instancia 5192 no se sustituyó. GET a `/inventario`, filtros y alias: 302 al login con returnUrl; `/css/inventario.css` y `/acceso`: 200. Rutas anteriores mantuvieron la redirección de autenticación.
- Navegador real: `/inventario` mostró el login. No había sesión válida en ese navegador; la tabla protegida no pudo observarse visualmente. Queda pendiente comprobar navegación y aspecto con una sesión autorizada.
- No se enviaron POST ni se ejecutaron DML, migraciones, scripts de creación o Git.
