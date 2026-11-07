# Base de datos y migraciones

## Esquema

La fuente de verdad es `database/01_Crear_Base_Datos.sql`: crea `SupermercadosGuate` y 15 tablas de sucursales, categorías, proveedores, clientes, empleados, productos, relación producto–proveedor, inventario, compras y detalle, ventas y detalle, pagos, ajustes y detalle. Las PK, FK, columnas y restricciones están en ese script. `database/02_Datos_Prueba.sql` carga datos mediante INSERT y debe tratarse como script de escritura.

El [resumen del esquema](RESUMEN_ESQUEMA.md) describe la estructura inicial. El script actual además incluye `FechaAplicacion DATE NULL` en `Tbl_AjustesInventario` y `CostoUnitario DECIMAL(12,2) NULL` en `Tbl_AjustesInventarioDetalle`. Los valores NULL históricos se conservan sin inventar fecha ni costo.

| Archivo | Uso | Situación |
|---|---|---|
| `01_Crear_Base_Datos.sql` | Instalación nueva; crea la base y tablas. | No ejecutar en base existente. |
| `02_Datos_Prueba.sql` | Carga académica de prueba. | Contiene DML; opcional. |
| `03_Agregar_Usuarios_Login.sql` | Agrega `Tbl_Usuarios` independiente de las 15 tablas originales. | Existencia y lectura reconfirmadas mediante el login de las cuentas demo el 2026-11-07; no se ejecutó la migración. |
| `04_Agregar_Costo_Historico_Ajustes.sql` | Añade costo histórico nullable y restricción. | Aplicación anterior informada y verificada; no se ejecutó aquí. |
| `05_Agregar_Fecha_Aplicacion_Ajustes.sql` | Añade fecha de aplicación nullable. | Aplicación anterior informada y verificada; no se ejecutó aquí. |
| `06_Agregar_Roles_Usuarios.sql` | Añade Rol con Administrador/Bodega/Caja. | Aplicación y respaldo documentados en ROLES_PERMISOS; lectura actual de roles y login comprobados. No volver a ejecutar por rutina. |

La migración de Login crea `Tbl_Usuarios` con usuario único, hash y estado; la migración 06 incorpora el rol. Se conservan separadas del esquema académico original. Las migraciones requieren respaldo de SQL Server y revisión de la base de destino antes de ejecutarse. La preparación de entrega no ejecuta migraciones ni cambia fechas o costos históricos NULL. Las limitaciones de M05, M07-E03 y M08-E07 se documentan en [BI](inteligencia_negocios/README.md).

## Consultas académicas

`database/consultas/Consultas_Integrante_1.sql`, `Consultas_Integrante_2.sql` y `Consultas_Integrante_3.sql` tienen 15 ejercicios identificados y sin números repetidos por archivo. E01–E13 son consultas; E14/E15 incluyen DML de práctica y se protegieron para la copia `SupermercadosGuate_PRUEBAS`. La [evidencia anterior](VALIDACION_CONSULTAS_SQL.md) documenta SI/NO, ROLLBACK, COMMIT y limpieza de esos seis ejercicios en la copia. No ejecutar los archivos completos sobre producción.

Las consultas adaptadas para Power BI son SELECT independientes en `power-bi/consultas/`. E14/E15 representan el estado final de PRUEBAS; los visuales no ejecutan sus transacciones. La revisión actual verificó estructura y conteos de ejercicios, pero no reejecutó las 45 consultas contra SQL Server.
