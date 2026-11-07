# Roles y permisos del sistema

La autorización se aplica en el servidor a rutas GET, operaciones POST y descargas CSV. El menú muestra únicamente los módulos del rol. Los filtros por rol no cambian las reglas de negocio de cada CRUD.

| Área | Administrador | Bodega | Caja |
|---|---|---|---|
| Inicio | General y los tres dashboards | Bodega | Caja |
| Sucursales, Empleados | Sí | No | No |
| Categorías, Proveedores, Productos y relación Producto–Proveedor | Sí | Sí | No |
| Clientes | Sí | No | Sí |
| Compras, Inventario, Ajustes | Sí | Sí | No |
| Ventas, Pagos y Saldos | Sí | No | Sí |
| Reportes y CSV | Los cuatro tipos | Compras e Inventario | Ventas y Saldos |
| BI M01–M03 | Sí | No | Sí |
| BI M04–M08 | Sí | Sí | No |
| BI M09 | Sí | No | No |

Los módulos BI M01–M09 ya están implementados localmente con la distribución de la guía 09. `DemoSessionFilter` aplica la matriz mediante `RolePermissions`; ocultar un enlace del menú no es la única protección. La revisión final comprobó la matriz de los tres roles y los accesos HTTP permitidos/prohibidos de Bodega y Caja. El login HTTP de Administrador conserva su evidencia anterior. Véase [entrega](ENTREGA_20261107.md).

La actualización pública posterior ya incorpora estos módulos y permisos. Pasaron 78 comprobaciones HTTPS, incluidos login/cierre, dashboards y rutas permitidas/prohibidas de Bodega/Caja. Usuarios, estados, roles y huellas de hashes se conservaron; David sigue activo como Administrador. La comprobación manual de su login queda a cargo del usuario, que no compartió su contraseña. Véase [publicación de entrega](PUBLICACION_ENTREGA_20261107.md).

## Implementación y verificación de 2026-11-07

- Respaldo completo anterior a la migración: `C:\BASE_DATOS\Docker\Backups\SupermercadosGuate_ANTES_ROLES_20261107_122045.bak`; `RESTORE VERIFYONLY WITH CHECKSUM` aprobado. Copia SQL aislada restaurada con MDF/LDF distintos.
- `database/06_Agregar_Roles_Usuarios.sql` agrega `Rol NVARCHAR(20) NOT NULL`, permite únicamente los tres roles y asigna Administrador a David sin cambiar su hash. Pasó tres ejecuciones en la copia; las posteriores no modificaron filas.
- En la copia aislada, BodegaDemo y CajaDemo iniciaron sesión. Rutas propias, reportes y CSV devolvieron 200; rutas y POST ajenos devolvieron 403. Menú sin enlaces ajenos y cierre de sesión comprobados. David inició sesión manualmente en esa copia y confirmó el inicio de Administrador.
- En la base principal quedaron David, BodegaDemo y CajaDemo activos, uno por rol. El hash de David se comparó internamente con la copia previa y permaneció igual. No se modificaron tablas de negocio.
- Release local en `127.0.0.1:5197` y HTTPS ngrok: login de Bodega/Caja, rutas propias y bloqueos comprobados. Ngrok devuelve 401 sin Basic Auth; con Basic Auth carga login y CSS. El usuario confirmó manualmente que David inició sesión en la URL pública y abrió el inicio de Administrador y Clientes.
- Las contraseñas de demostración se generaron aleatoriamente y están cifradas con DPAPI en `deployment/private/usuarios-demo.dpapi.json`, excluido de Git. Para verlas **solo en el terminal local**: `C:\BaseDatos_2026\deployment\UsuariosDemo.ps1 -Action MostrarClaves`. Nunca compartir el archivo cifrado ni las claves en un repositorio.
