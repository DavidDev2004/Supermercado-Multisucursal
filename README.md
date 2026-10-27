# Supermercado Multisucursal

Proyecto académico de Base de Datos I: aplicación ASP.NET Core MVC, SQL Server y proyecto Power BI. El código actual incluye autenticación por cookies, catálogos, productos y proveedores, compras, ventas, pagos y saldos, inventario, ajustes, reportes y tres páginas de inicio.

## Estado verificado el 27 de octubre de 2026

- `dotnet build` del proyecto MVC: correcto, sin errores ni advertencias, usando una salida separada porque otra instancia mantenía bloqueado el ejecutable habitual.
- Validador estructural del PBIP: 9 páginas, 45 visuales de datos, 45 tablas TMDL y navegación interna válida. Power BI Desktop y la actualización de datos no se probaron en esta auditoría.
- `http://localhost:5193/acceso` respondió HTTP 200 y `/modulos/reportes` redirigió al acceso sin sesión. No se verificaron pantallas autenticadas ni nuevas operaciones de escritura en esta auditoría.
- Los tres archivos de Fase III contienen 15 ejercicios identificados y únicos cada uno. Las pruebas transaccionales E14/E15 constan en la copia aislada `SupermercadosGuate_PRUEBAS`; no se ejecutaron aquí.

Consulta [el estado académico](docs/CUMPLIMIENTO_ACADEMICO.md) para distinguir implementación, evidencia previa y pruebas pendientes. Los documentos originales de los integrantes, la guía 07 y los prototipos se conservan solo localmente y no se distribuyen en el repositorio. Faltan guías posteriores y pruebas finales para certificar toda la entrega.

## Documentación

- [Instalación y ejecución](docs/INSTALACION.md)
- [Arquitectura y rutas](docs/ARQUITECTURA.md)
- [Base de datos y migraciones](docs/BASE_DATOS.md)
- [Cumplimiento y pruebas](docs/CUMPLIMIENTO_ACADEMICO.md)
- [Preparación para publicación](docs/PUBLICACION_SEGURIDAD.md)
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
