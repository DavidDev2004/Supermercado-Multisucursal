# Arquitectura del sistema

## Componentes

| Capa | Ubicación | Función |
|---|---|---|
| MVC | `src/SupermercadoMVC/Controllers`, `Views`, `Models` | Rutas, validación de formularios y presentación coral/turquesa. |
| Acceso a datos | `src/SupermercadoMVC/Data` | ADO.NET con `Microsoft.Data.SqlClient` y parámetros tipados. |
| SQL Server | `database/` | Esquema académico, migraciones y ejercicios. |
| Power BI | `power-bi/Supermercado_FaseIV.pbip` | Modelo TMDL y reporte PBIR independientes de MVC. |

`Program.cs` registra servicios con alcance de solicitud y configura cookie de autenticación, sesión y rutas MVC. `DatabaseConnectionFactory` lee `ConnectionStrings:Supermercado` de configuración local. Los controladores protegidos usan `DemoSessionFilter`, cuyo nombre es histórico: actualmente exige una identidad autenticada; no implementa permisos por rol.

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

Las páginas de inicio tienen rutas independientes; no hay selección por rol. Compras, Ventas y Ajustes mantienen borradores de encabezado y detalle y usan transacciones SQL para recepción, confirmación o aplicación. Pagos vuelve a comprobar el saldo en SQL Server antes de registrar. Inventario y Reportes son módulos de consulta. Los importes de ventas, compras y saldos se agregan desde sus detalles o pagos por separado para evitar duplicar importes.

## Límites actuales

No se identificó una suite de pruebas automatizadas MVC en el proyecto. Las pruebas manuales y SQL previas se documentan por separado; la existencia de un controlador o ruta no prueba que todos sus flujos funcionen en una sesión autenticada. Power BI conserva conexiones locales a SQL Server, por lo que requiere configuración adicional fuera de este equipo.
