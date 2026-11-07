# Publicación de entrega del 7 de noviembre de 2026

**Actualización de guía 10:** el acceso por Cloudflare Tunnel rápido ya está funcionando; se validó el mismo sistema por HTTPS con 78 pruebas adicionales y redirección desde HTTP. El usuario reportó ingreso, dashboard y cierre de David desde datos móviles. Ngrok se detuvo después; se repitieron 78 pruebas Cloudflare y 14 de exposición sin fallos. La alternativa y sus pendientes están en [Guía 10: Cloudflare Tunnel](GUIA10_CLOUDFLARE_20261107.md). El relato de ngrok siguiente documenta la publicación anterior.

El usuario autorizó el respaldo final, commit/push y actualización de la versión pública conservando la configuración. Se aceptan como pendientes M05 (datos históricos), M07-E03 (Compras Pendientes) y M08-E07 (comparación de costos sin moneda). No se inventan fechas, costos ni estados para completarlos. La aprobación académica de las matrices y la modalidad de guía 10 sigue correspondiendo al docente.

## Respaldo final

- Archivo: `C:\BASE_DATOS\Docker\Backups\SupermercadosGuate_ENTREGA_20261107_1d75c781.bak`.
- Operación: `BACKUP DATABASE` con `COPY_ONLY`, `CHECKSUM` y `COMPRESSION`, en archivo nuevo. No se sobrescribió otro respaldo.
- Resultado: `RESTORE VERIFYONLY WITH CHECKSUM` correcto; archivo de 991232 bytes.
- SHA-256: `019243B8805B613C6D2320158B5B7D7EDC10AA9F066A3B01964C2759FF3E0C84`.
- No se restauró sobre la base activa ni se ejecutaron migraciones o DML. Falta una copia fuera del anfitrión y una restauración de prueba de este respaldo final si el docente la exige; VERIFYONLY no equivale a esa restauración.

David se confirmó activo como Administrador antes de actualizar. Una referencia privada de usuarios, estados, roles y huellas de sus hashes permite comprobar después que no se cambiaron cuentas ni contraseñas. No se publica esa referencia.

## Revisión previa

Los 31 KPI calculables volvieron a coincidir con SELECT independientes. Los nueve módulos BI y las consultas de catálogos, ventas, compras, pagos, inventario, ajustes y reportes respondieron; 90 comprobaciones HTTP locales sin fallos. El candidato Release arrancó separado con login y recursos BI accesibles. Se revisan el índice Git, sus exclusiones y los secretos conocidos antes de subir.

La consulta de diagnóstico incluida en la migración 03 ahora lista solo código, usuario y estado, sin mostrar PasswordHash. Esto solo modifica el archivo fuente: no se ejecutó la migración.

## Resultado de publicación

La rama `main` de GitHub recibió el commit de código `bcf5e987871fbb414ec59751b88b2f359c61b441` y se confirmó el mismo identificador en el remoto. Se revisaron 459 archivos de texto del índice, rutas excluidas e historial: sin coincidencias de los secretos conocidos ni patrones de claves detectados. No se subieron respaldos, archivos privados, configuraciones locales, documentos originales ni binarios de publicación. La revisión no equivale a certificar que todo dato de todo el historial sea público.

La versión Release candidata se activó correctamente en el puerto 5197, manteniendo el túnel ngrok existente, entorno Production, conexión SQL, User Secrets, Basic Auth y archivos cifrados. El proceso nuevo es PID 50036; SQL Server sigue expuesto solo en 127.0.0.1:1435. Los hashes de configuración se conservaron. La DLL activa coincide con el candidato: SHA-256 `794716F8DD349233CA20E2F0D4855AF4D299DAC9BC36371A17E4206A343020EA`.

Antes de reemplazar el proceso se guardó una copia nueva en `deployment/private/entrega-rollback-final-20261107-a1853565/`. Se verificó su DLL contra la versión anterior. No hubo fallo de publicación y no fue necesario activar el rollback. La versión anterior permanece recuperable; el túnel no se detuvo ni se cambiaron credenciales.

## Comprobación del sistema publicado

- **78 comprobaciones HTTPS, cero fallos**, usando la URL pública con Basic Auth y sesiones propias de Bodega/Caja. El acceso sin Basic Auth devuelve 401; sin sesión MVC las rutas protegidas redirigen al login.
- Login y cierre de sesión de Bodega/Caja correctos. Bodega accede a M04–M08 y Caja a M01–M03; los demás módulos BI, M09 y los inicios/catálogos ajenos quedan prohibidos según el rol.
- Dashboards propios, consultas de catálogos accesibles, productos/relaciones, compras, ventas, pagos, inventario, ajustes y reportes/CSV permitidos respondieron sin mensajes de error SQL. Los filtros funcionaron y los ilegibles mostraron validación.
- M05 conserva sus ocho posiciones pendientes. M07-E03 y M08-E07 mantienen la explicación correspondiente. No se rellenaron cifras, moneda ni historia inexistente.
- Usuarios, estados, roles y huellas de hashes coinciden con la referencia anterior a la publicación. **David continúa activo como Administrador**. Su contraseña no se cambió. El usuario se comprometió a comprobar manualmente el login real de David; no se manejó su contraseña ni se simuló una sesión de ese usuario.
- Las pruebas se ejecutaron por HTTP desde el anfitrión contra el enlace HTTPS público; no equivalen a prueba desde un segundo dispositivo/red ni a una revisión visual nueva. No se hicieron operaciones CRUD de escritura ni movimientos de inventario.

## Pendientes que se entregan declarados

1. M05, M07-E03 y M08-E07 permanecen pendientes por decisión del usuario; falta cierre académico de esas limitaciones y aprobación de las matrices/diseños.
2. La guía 10 ofrece **Tailscale o Cloudflare**, como alternativas. El sistema actual usa **ngrok**: falta aceptación del docente de esa modalidad o una configuración posterior autorizada de una de las opciones oficiales. No se instalaron ni sustituyeron túneles.
3. Completar evidencia desde un segundo equipo/red, teléfono/tableta y confirmar el login manual de David. Las evidencias visuales anteriores se conservan; no se generaron nuevas.
4. Guardar una copia del respaldo final fuera del anfitrión; realizar restauración de prueba aislada de este archivo si se exige. El respaldo final sí pasó VERIFYONLY WITH CHECKSUM.
5. Las pruebas de escritura, errores, concurrencia y rollback de negocio siguen pendientes en una copia aislada autorizada; no se hicieron sobre información real. El reinicio y rollback web no se provocaron como prueba porque la publicación fue correcta.

El repositorio de código es `DavidDev2004/Supermercado-Multisucursal`. La URL vigente y los registros operativos se mantienen en los archivos privados de despliegue y se comunican al usuario por el chat; no se publican las claves de acceso. El anfitrión, Docker, SQL y el túnel deben permanecer disponibles durante la entrega. Los documentos de preparación previos describen la etapa anterior; este registro prevalece para el resultado de publicación.
