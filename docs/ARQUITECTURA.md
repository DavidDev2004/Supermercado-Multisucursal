# Arquitectura del sistema

## Componentes

| Capa | Ubicación | Función |
|---|---|---|
| MVC | `src/SupermercadoMVC/Controllers`, `Views`, `Models` | Rutas, validación de formularios y presentación coral/turquesa. |
| Acceso a datos | `src/SupermercadoMVC/Data` | ADO.NET con `Microsoft.Data.SqlClient` y parámetros tipados. |
| SQL Server | `database/` | Esquema académico, migraciones y ejercicios. |
| Power BI | `power-bi/Supermercado_FaseIV.pbip` | Modelo TMDL y reporte PBIR independientes de MVC. |

`Program.cs` registra servicios con alcance de solicitud y configura cookie de autenticación, sesión y rutas MVC. `DatabaseConnectionFactory` lee `ConnectionStrings:Supermercado` de configuración local. Los controladores protegidos usan `DemoSessionFilter`, cuyo nombre es histórico: exige identidad autenticada y aplica `RolePermissions` en el servidor, incluidas rutas directas, operaciones POST y reportes CSV. La cookie se revalida contra el usuario activo y su rol actual.

## Rutas principales

| Área | Ruta |
|---|---|
| Acceso y salida | `/acceso`; salida por POST con antiforgery. |
| Categorías, Sucursales, Proveedores, Clientes, Empleados | `/catalogos/{slug}`. |
| Productos y relación con proveedores | `/productos`. |
| Compras y detalle | `/compras`. |
| Ventas y detalle | `/ventas`. |
| Pagos y saldos | `/pagos`. |
| Inventario | `/inventario`. |
| Ajustes y detalle | `/ajustes`. |
| Reportes de lectura y CSV | `/modulos/reportes`, `/modulos/reportes/csv`. |
| Inicio por área | `/Inicio/Administrador`, `/Inicio/Bodega`, `/Inicio/Caja`. |
| Inteligencia de Negocios web | `/inteligencia-negocios/m01` hasta `/inteligencia-negocios/m09`. |

El acceso y la raíz redirigen a Bodega/Caja según el rol; Administrador conserva el panel general y puede consultar los tres inicios. BI usa SELECT parametrizados: Administrador M01–M09, Caja M01–M03 y Bodega M04–M08. M09 es exclusivo de Administrador. Véase [Roles](ROLES_PERMISOS.md).

Compras, Ventas y Ajustes mantienen borradores de encabezado y detalle y usan transacciones SQL para recepción, confirmación o aplicación. Pagos vuelve a comprobar el saldo en SQL Server antes de registrar. Inventario y Reportes son módulos de consulta. Los importes de ventas, compras y saldos se agregan desde sus detalles o pagos por separado para evitar duplicar importes. M05, M07-E03 y M08-E07 identifican los datos insuficientes sin inventar resultados.

## Límites actuales

No se identificó una suite de pruebas automatizadas MVC en el proyecto. Las pruebas manuales y SQL previas se documentan por separado; la existencia de un controlador o ruta no prueba que todos sus flujos funcionen en una sesión autenticada. Power BI conserva conexiones locales a SQL Server, por lo que requiere configuración adicional fuera de este equipo.
