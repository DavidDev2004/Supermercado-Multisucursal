# Base de datos y migraciones

## Esquema

La fuente de verdad es `database/01_Crear_Base_Datos.sql`: crea `SupermercadosGuate` y 15 tablas de sucursales, categorías, proveedores, clientes, empleados, productos, relación producto–proveedor, inventario, compras y detalle, ventas y detalle, pagos, ajustes y detalle. Las PK, FK, columnas y restricciones están en ese script. `database/02_Datos_Prueba.sql` carga datos mediante INSERT y debe tratarse como script de escritura.

El [resumen del esquema](RESUMEN_ESQUEMA.md) describe la estructura inicial. El script actual además incluye `FechaAplicacion DATE NULL` en `Tbl_AjustesInventario` y `CostoUnitario DECIMAL(12,2) NULL` en `Tbl_AjustesInventarioDetalle`. Los valores NULL históricos se conservan sin inventar fecha ni costo.

| Archivo | Uso | Situación |
|---|---|---|
| `01_Crear_Base_Datos.sql` | Instalación nueva; crea la base y tablas. | No ejecutar en base existente. |
| `02_Datos_Prueba.sql` | Carga académica de prueba. | Contiene DML; opcional. |
| `03_Agregar_Usuarios_Login.sql` | Agrega `Tbl_Usuarios` independiente de las 15 tablas originales. | El historial indica que existe en SQL Server; esta auditoría no lo reconfirmó mediante SELECT. |
| `04_Agregar_Costo_Historico_Ajustes.sql` | Añade costo histórico nullable y restricción. | Aplicación anterior informada y verificada; no se ejecutó aquí. |
| `05_Agregar_Fecha_Aplicacion_Ajustes.sql` | Añade fecha de aplicación nullable. | Aplicación anterior informada y verificada; no se ejecutó aquí. |

La migración de Login crea `Tbl_Usuarios` con usuario único, hash y estado. Se conserva separada del esquema académico original. Las migraciones requieren respaldo de SQL Server y revisión de la base de destino antes de ejecutarse.

## Consultas académicas

`database/consultas/Consultas_Integrante_1.sql`, `Consultas_Integrante_2.sql` y `Consultas_Integrante_3.sql` tienen 15 ejercicios identificados y sin números repetidos por archivo. E01–E13 son consultas; E14/E15 incluyen DML de práctica y se protegieron para la copia `SupermercadosGuate_PRUEBAS`. La [evidencia anterior](VALIDACION_CONSULTAS_SQL.md) documenta SI/NO, ROLLBACK, COMMIT y limpieza de esos seis ejercicios en la copia. No ejecutar los archivos completos sobre producción.

Las consultas adaptadas para Power BI son SELECT independientes en `power-bi/consultas/`. E14/E15 representan el estado final de PRUEBAS; los visuales no ejecutan sus transacciones. La revisión actual verificó estructura y conteos de ejercicios, pero no reejecutó las 45 consultas contra SQL Server.
