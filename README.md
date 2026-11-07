# Supermercado Multisucursal

Proyecto académico de Base de Datos I: aplicación ASP.NET Core MVC, SQL Server y proyecto Power BI. El código actual incluye autenticación por cookies y roles, catálogos, productos y proveedores, compras, ventas, pagos y saldos, inventario, ajustes, reportes, tres páginas de inicio e Inteligencia de Negocios web M01–M09.

## Estado verificado el 7 de noviembre de 2026

- Compilación local aislada: 0 errores y 0 advertencias; sintaxis del JavaScript propio correcta.
- M01–M09: cuatro posiciones KPI y cuatro gráficas por módulo. Los 31 KPI calculables coinciden con ocho SELECT independientes. M05 conserva ocho visuales pendientes; M07-E03 y M08-E07 también permanecen pendientes, con su motivo visible.
- Roles del servidor: Administrador M01–M09, Caja M01–M03 y Bodega M04–M08. Login y cierre de Bodega/Caja, rutas permitidas/prohibidas, filtros y reportes/CSV: 90 comprobaciones HTTP locales sin fallos. Administrador: consultas de inicio y matriz de permisos comprobadas; no se repitió su login HTTP.
- Catálogos y módulos principales: consultas reales correctas. No se hicieron altas, ediciones, eliminaciones ni pruebas de concurrencia/rollback sobre la base.
- Se conserva la evidencia anterior de Power BI y Fase III; no se modificó Power BI ni se repitieron operaciones SQL de escritura.

Consulta [la preparación de entrega y guía 10](docs/ENTREGA_20261107.md) y [el estado académico](docs/CUMPLIMIENTO_ACADEMICO.md). Las guías 09 y las dos alternativas de guía 10 están disponibles localmente; los originales y prototipos se excluyen de Git. La actualización posterior autorizada ya se completó: código subido a GitHub, respaldo SQL final verificado y Release público con 78 comprobaciones HTTPS sin fallos. Se conservaron configuración y cuentas.

El resultado y los pendientes, incluidas las limitaciones de BI aceptadas por el usuario, están en [Publicación de entrega](docs/PUBLICACION_ENTREGA_20261107.md). La alternativa oficial de [guía 10 con Cloudflare Tunnel](docs/GUIA10_CLOUDFLARE_20261107.md) ya funciona con una URL temporal. Se confirmó por relato del usuario el login, dashboard y cierre de David desde datos móviles; faltan capturas, pruebas en más dispositivos y el cierre académico de las pruebas no realizadas sobre la base vigente.

## Documentación

- [Instalación y ejecución](docs/INSTALACION.md)
- [Arquitectura y rutas](docs/ARQUITECTURA.md)
- [Base de datos y migraciones](docs/BASE_DATOS.md)
- [Cumplimiento y pruebas](docs/CUMPLIMIENTO_ACADEMICO.md)
- [Preparación para publicación](docs/PUBLICACION_SEGURIDAD.md)
- [Guía 10: Cloudflare Tunnel y evidencia](docs/GUIA10_CLOUDFLARE_20261107.md)
- [Inteligencia de Negocios y limitaciones](docs/inteligencia_negocios/README.md)
- [Comandos usados durante el proyecto](docs/COMANDOS_RAPIDOS.md)

Para preparar una copia local nueva, sigue [Instalación y ejecución](docs/INSTALACION.md). La plantilla [appsettings.example.json](src/SupermercadoMVC/appsettings.example.json) contiene marcadores, no credenciales; la conexión real se configura con .NET User Secrets. Los respaldos SQL, datos importados de Power BI y archivos de conexión locales quedan fuera del repositorio.

## Estructura

| Carpeta | Contenido |
|---|---|
| `src/SupermercadoMVC/` | Aplicación MVC .NET 9 y acceso a datos ADO.NET. |
| `database/` | Esquema, datos de prueba y migraciones independientes. |
| `database/consultas/` | 15 ejercicios SQL por integrante. |
| `power-bi/` | Proyecto `Supermercado_FaseIV.pbip`, 9 páginas y consultas de lectura. |
| `docs/` | Evidencias y documentación técnica; referencias originales excluidas del repositorio. |
| `ia/skills/` | Guías internas de CRUD y diseño. |

La fuente de verdad del esquema es [01_Crear_Base_Datos.sql](database/01_Crear_Base_Datos.sql), complementada por las migraciones documentadas. No ejecutes el script de creación sobre una base existente. La conexión local se configura mediante .NET User Secrets; no se guardan credenciales en este repositorio.
