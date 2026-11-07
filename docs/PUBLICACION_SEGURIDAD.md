# Preparación para GitHub y despliegue

**Actualización de guía 10:** el túnel rápido de Cloudflare funciona sobre el Release con redirección HTTPS pública. Véase [Guía 10: Cloudflare Tunnel](GUIA10_CLOUDFLARE_20261107.md). El procedimiento de ngrok más abajo corresponde a la etapa anterior.

**Estado al 2026-11-07:** la instancia Production en `127.0.0.1:5197`, expuesta por ngrok con Basic Auth y login propio, ya se actualizó con el candidato Release. El respaldo SQL final pasó VERIFYONLY WITH CHECKSUM; 78 comprobaciones HTTPS terminaron sin fallos y se conservaron configuración y cuentas. Se guardó copia nueva para recuperación y no fue necesario activarla. Véase [resultado de publicación y pendientes de guía 10](PUBLICACION_ENTREGA_20261107.md).

## Archivos y secretos

La conexión MVC se obtiene de .NET User Secrets y no aparece con valor en `appsettings.json`. Una búsqueda de asignaciones sensibles en archivos de texto del proyecto no encontró una contraseña o cadena de conexión incrustada; las coincidencias del código eran referencias al campo de contraseña y al nombre de la configuración. Esta búsqueda no garantiza que todos los binarios o documentos estén libres de datos privados.

La preparación final comparó en memoria los secretos conocidos de SQL, ngrok, Basic Auth y las cuentas demo contra 441 archivos de texto candidatos del repositorio y paquete; no hubo coincidencias. Se comprobaron las exclusiones de archivos privados y configuraciones locales. No se auditó exhaustivamente el historial Git ni el contenido de todos los binarios. El sitio existente conservó proceso, User Secrets y archivos privados; login/CSS públicos respondieron correctamente con Basic Auth y el acceso anónimo obtuvo 401.

`.gitignore` excluye `database/CONEXION.md`, `MEMORY.md`, configuraciones locales `appsettings.*.json`, `.env`, perfiles de publicación, claves, `bin/`, `obj/`, el perfil temporal de navegador, registros, cachés `.pbi`, respaldos y `.pbix` con posibles datos importados. También excluye copias de bases SQL y exportaciones tabulares. Los SQL académicos, el PBIP editable y `appsettings.example.json` sin credenciales permanecen como fuentes del proyecto. Antes de publicar, revisar de nuevo el contenido candidato, en particular datos de prueba, prototipos, consultas y documentos. Un patrón de exclusión no retira automáticamente archivos ya rastreados.

El repositorio tiene historial y remoto `origin` en GitHub; la rama es `main`. El commit revisado de roles/BI y preparación de entrega se subió correctamente y se confirmó por lectura del identificador remoto. El respaldo, configuración privada, credenciales y paquete Release permanecen excluidos de Git.

`database/02_Datos_Prueba.sql` conserva el conjunto académico y sus relaciones. Los nombres de 13 clientes y 10 empleados, seis contactos, 10 DPI, 19 NIT, 34 teléfonos y 24 correos se sustituyeron únicamente en el script fuente por marcadores inequívocos de prueba; los correos usan `@example.com`. No se cambiaron registros de SQL Server.

Los originales de `docs/documentos-oficiales/`, las imágenes de `docs/prototipos/` y `power-bi/referencias/`, y `docs/07_Dashboard_paginas_Inicio.docx` se conservan localmente y se excluyen de Git por no tener autorización documentada de redistribución. Los diseños SVG y matrices BI creados para el proyecto sí pueden formar parte del código candidato. Las bibliotecas de `wwwroot/lib/` conservan sus licencias MIT.

Una contraseña de una cuenta externa fue compartida anteriormente por chat. Debe cambiarse desde ese servicio antes de usar la cuenta para publicar; no se reproduce aquí ni se incorpora al proyecto.

## Bloqueos de despliegue

- Las consultas de Power BI usan `localhost,1435` y E14/E15 dependen de `SupermercadosGuate_PRUEBAS`; no funcionarán en un servicio remoto sin rediseñar la conexión o suministrar una copia autorizada.
- La aplicación necesita SQL Server, tabla `Tbl_Usuarios`, usuario inicial y un secreto de conexión configurado en el host. No incluir esos valores en el repositorio ni en variables públicas.
- Identidad y roles están aplicados por `DemoSessionFilter` y `RolePermissions`; las rutas directas y CSV de Bodega/Caja se comprobaron por HTTP. La cookie revalida usuario y rol contra SQL Server. El login HTTP de Administrador conserva evidencia anterior y debe repetirse en la versión final antes de la entrega remota.
- Revisar configuración de HTTPS, certificado SQL Server confiable, persistencia de claves de protección de datos de ASP.NET Core y encabezados de proxy antes de publicar. La prueba aislada anterior falló al descifrar claves DPAPI en el entorno de ejecución; ese resultado no certifica el host final.
- El login no muestra un mecanismo de limitación de intentos o bloqueo de cuenta en el código revisado. Debe evaluarse antes de exponerlo públicamente.
- Las dos guías 10 disponibles plantean Tailscale o Cloudflare como alternativas. Ngrok es el mecanismo actual y requiere confirmación de aceptación docente. No se instalaron túneles, no se cambió firewall y no se abrió SQL Server a Internet; Docker publica SQL solo en `127.0.0.1:1435`.

## Actualizar conservando la configuración

El paquete candidato local es `src/SupermercadoMVC/bin/EntregaPublicable20261107/`; no es la carpeta Release que usa el proceso actual. No compilar ni publicar directamente sobre `bin/Release/net9.0` mientras esté sirviendo. No reutilizar el script privado `activate-redesign.ps1`: contiene rutas de candidato y rollback de octubre.

Se preparó una copia web de recuperación en `deployment/private/entrega-rollback-20261107-ea9c35da/`, con binarios activos, wwwroot y appsettings locales; la DLL copiada coincide por SHA-256. No incluye un respaldo SQL nuevo. Si el sitio cambia antes de actualizar, refrescar la copia con otro nombre único; no sobrescribir el respaldo existente.

Tras autorización, guardar una copia completa nueva de la versión activa, sus archivos estáticos y configuraciones locales. Conservar User Secrets, claves ASP.NET Data Protection del mismo usuario Windows y `deployment/private/` (token, Basic Auth, usuarios demo cifrados y registros). No copiar estas carpetas al repositorio ni reemplazarlas con la plantilla. Mantener el puerto 5197, el usuario anfitrión y el túnel actual; detener únicamente el proceso web identificado por el listener y `app.pid`, actualizar desde el candidato y arrancar con la misma configuración SQL/Production. Validar login, roles y BI antes de declarar éxito. Si falla, detener el nuevo proceso y recuperar la copia completa anterior con la misma configuración. No se ejecutó este procedimiento.

Actualizar la aplicación no requiere ejecutar creación de base, datos de prueba ni migraciones ya aplicadas. La copia SQL existente anterior a roles tiene verificación histórica; antes del cambio remoto conviene un respaldo nuevo autorizado y recuperable. Faltan pruebas finales desde otra red, evidencia de dispositivos y cierre de pruebas transaccionales en una copia aislada. La preparación local no certifica todos esos puntos.
