# Preparación para GitHub y despliegue

El proyecto **no se publicó** durante esta auditoría. La compilación local funciona, pero faltan comprobaciones antes de exponerlo en Internet.

## Archivos y secretos

La conexión MVC se obtiene de .NET User Secrets y no aparece con valor en `appsettings.json`. Una búsqueda de asignaciones sensibles en archivos de texto del proyecto no encontró una contraseña o cadena de conexión incrustada; las coincidencias del código eran referencias al campo de contraseña y al nombre de la configuración. Esta búsqueda no garantiza que todos los binarios o documentos estén libres de datos privados.

`.gitignore` excluye `database/CONEXION.md`, `MEMORY.md`, configuraciones locales `appsettings.*.json`, `.env`, perfiles de publicación, claves, `bin/`, `obj/`, el perfil temporal de navegador, registros, cachés `.pbi`, respaldos y `.pbix` con posibles datos importados. También excluye copias de bases SQL y exportaciones tabulares. Los SQL académicos, el PBIP editable y `appsettings.example.json` sin credenciales permanecen como fuentes del proyecto. Antes de publicar, revisar de nuevo el contenido candidato, en particular datos de prueba, prototipos, consultas y documentos. Un patrón de exclusión no retira automáticamente archivos ya rastreados.

El repositorio local se inició sin commits, archivos rastreados ni remoto. Por ello no existe historial Git local que depurar. La auditoría de contenido encontró únicamente referencias de código al manejo de contraseñas y los marcadores de la plantilla; no detectó un valor de credencial en los archivos de texto candidatos. Los documentos e imágenes deben revisarse también por su contenido académico y derechos de distribución antes de hacer público el repositorio.

`database/02_Datos_Prueba.sql` conserva el conjunto académico y sus relaciones. Los nombres de 13 clientes y 10 empleados, seis contactos, 10 DPI, 19 NIT, 34 teléfonos y 24 correos se sustituyeron únicamente en el script fuente por marcadores inequívocos de prueba; los correos usan `@example.com`. No se cambiaron registros de SQL Server.

Las dos imágenes de `docs/prototipos/`, las nueve de `power-bi/referencias/` y el documento `docs/07_Dashboard_paginas_Inicio.docx` son material de referencia sin autorización de distribución documentada. El favicon local tampoco tiene procedencia documentada. Se excluyen del repositorio público mediante `.gitignore` y permanecen en el equipo. Las bibliotecas de `wwwroot/lib/` incluyen sus archivos de licencia MIT.

Una contraseña de una cuenta externa fue compartida anteriormente por chat. Debe cambiarse desde ese servicio antes de usar la cuenta para publicar; no se reproduce aquí ni se incorpora al proyecto.

## Bloqueos de despliegue

- Las consultas de Power BI usan `localhost,1435` y E14/E15 dependen de `SupermercadosGuate_PRUEBAS`; no funcionarán en un servicio remoto sin rediseñar la conexión o suministrar una copia autorizada.
- La aplicación necesita SQL Server, tabla `Tbl_Usuarios`, usuario inicial y un secreto de conexión configurado en el host. No incluir esos valores en el repositorio ni en variables públicas.
- El código exige identidad autenticada, pero no tiene autorización por rol. Las tres rutas de Inicio son independientes y accesibles a cualquier usuario autenticado. Si se requieren permisos diferenciados, esto es un requisito funcional pendiente.
- Revisar configuración de HTTPS, certificado SQL Server confiable, persistencia de claves de protección de datos de ASP.NET Core y encabezados de proxy antes de publicar. La prueba aislada anterior falló al descifrar claves DPAPI en el entorno de ejecución; ese resultado no certifica el host final.
- El login no muestra un mecanismo de limitación de intentos o bloqueo de cuenta en el código revisado. Debe evaluarse antes de exponerlo públicamente.
- La guía de publicación, el proveedor gratuito y el procedimiento de despliegue definitivo no están definidos en los documentos disponibles.

**Conclusión:** el conjunto de archivos candidato quedó preparado para un primer commit local, con datos de prueba sintéticos y referencias de distribución incierta excluidas. No se hizo commit ni publicación. El despliegue público requiere todavía configuración segura del host y pruebas autenticadas; la preparación del repositorio no certifica el servicio web.
