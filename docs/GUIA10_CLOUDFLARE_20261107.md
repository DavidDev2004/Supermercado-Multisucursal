# Guía 10: publicación web con Cloudflare Tunnel

Estado al 7 de noviembre de 2026: **túnel rápido Cloudflare funcionando para la demostración**. Esta es la alternativa «Publicación Web: Servidor Local y Cloudflare Tunnel» del documento oficial local. No se compró dominio ni se creó una zona Cloudflare. La dirección `trycloudflare.com` es temporal y cambia cuando se reinicia el túnel. La URL actual y las pruebas con fecha se guardan en `deployment/private/cloudflare-publicacion.json` y `deployment/private/cloudflare-http-final.json`; se comparten con el docente por un canal directo, no como dirección permanente en GitHub. [Cloudflare describe esta modalidad y sus límites](https://developers.cloudflare.com/tunnel/get-started/quick-tunnels/).

## Sistema y arquitectura comprobados

| Elemento | Resultado |
|---|---|
| Anfitrión | Windows; ASP.NET Core MVC `.NET 9` en Production, `http://127.0.0.1:5197`. Responsable de mantenerlo encendido y conectado: **por designar por el grupo**. |
| Aplicación | El mismo Release del supermercado, con login por cookie, contraseña con hash, antiforgery, roles y nueve módulos BI. Solo se añadió redirección HTTPS para hostnames públicos en `Program.cs`. |
| SQL Server | Contenedor `sqlserver_basedatos_ipv4_only`, base `SupermercadosGuate`, puerto del host `127.0.0.1:1435`. El túnel solo apunta al puerto web 5197. |
| Copia SQL | `SupermercadosGuate_ENTREGA_20261107_1d75c781.bak`, 991232 bytes, SHA-256 `019243B8805B613C6D2320158B5B7D7EDC10AA9F066A3B01964C2759FF3E0C84`; `RESTORE VERIFYONLY WITH CHECKSUM` correcto. Falta copia fuera del anfitrión y restauración aislada de prueba. |
| Cloudflare | `cloudflared` 2026.10.7 oficial, SHA-256 del ejecutable verificado; transporte QUIC. Túnel rápido sin dominio, cuenta ni puertos entrantes abiertos. |
| Recuperación web | Copia previa en `deployment/private/cloudflare-rollback-20261107-2de87c02/`. La activación probó ambos enlaces y conservó los hashes de configuración y las cuentas; no fue necesario recuperarla. |

La aplicación procesa `X-Forwarded-Proto` solo desde el proxy local de loopback. Las solicitudes con hostname público y esquema HTTP redirigen a HTTPS; el acceso local por `127.0.0.1` conserva HTTP para diagnóstico. La sesión usa cookie `Secure` y `HttpOnly`. El enlace público HTTPS envía HSTS. No se cambiaron credenciales, roles, consultas BI, datos ni esquema.

Para reiniciar manualmente después de cerrar sesión o reiniciar Windows, primero iniciar Docker y la aplicación Production en `127.0.0.1:5197` con sus secretos locales existentes. En una segunda consola de PowerShell desde la raíz del proyecto:

```powershell
.\deployment\tools\cloudflared.exe tunnel --url http://127.0.0.1:5197 --protocol quic
```

Copiar la **nueva** URL HTTPS que aparece en la consola. Mantener el proceso y el anfitrión encendidos durante la demostración. `deployment/tools/` y `deployment/private/` están excluidos de Git. QUIC se eligió porque la inspección TLS local de Avast rechazó la conexión HTTP/2 al extremo de Cloudflare; el túnel QUIC sí registró la conexión sin desactivar la verificación TLS. La documentación oficial de [instalación](https://developers.cloudflare.com/tunnel/downloads/) y [túneles rápidos](https://developers.cloudflare.com/tunnel/get-started/quick-tunnels/) describe estos pasos.

## Pruebas efectuadas

| Prueba | Resultado |
|---|---|
| Compilación Release aislada y arranque en puerto separado | Correctos; HTTP local `200`, petición pública simulada HTTP `307` a HTTPS y HTTPS `200` con HSTS. |
| Cloudflare HTTPS por URL pública | **78 comprobaciones, 0 fallos** desde el anfitrión pasando por el extremo de Cloudflare: login/cierre real de Bodega y Caja, dashboards, nueve rutas BI por rol, KPI y gráficas, consultas, filtros, CSV y rutas directas prohibidas. |
| Ngrok conservado durante la migración | **78 comprobaciones, 0 fallos** después del nuevo despliegue; sirvió como respaldo hasta completar la prueba externa. |
| Invitado | Login y recursos públicos `200`; Inicio, CRUD, BI y reportes protegidos redirigen al acceso. |
| Bodega | Inicio Bodega, M04–M08 y sus módulos/CSV autorizados `200`; rutas de Caja, Administrador y M09 denegadas `403`. |
| Caja | Inicio Caja, M01–M03 y sus módulos/CSV autorizados `200`; rutas de Bodega, Administrador y M09 denegadas `403`. |
| Archivos y diagnósticos | `/swagger`, `/health`, `/.env`, `/appsettings.json`, ruta privada de despliegue, script SQL y ruta inexistente: `404`; no se expone respaldo ni listado. |
| Exposición de puertos | Kestrel y métricas de `cloudflared` solo en loopback; SQL Server solo `127.0.0.1:1435`; Docker no expone otros puertos del proyecto. |
| Integridad de usuarios | Estados, roles y huellas de hashes iguales a la referencia anterior; David activo como Administrador. Su login real requiere comprobación manual por David. |

Las 78 pruebas usan HTTP sobre la URL pública desde **el mismo anfitrión**; son evidencia funcional de la ruta por Cloudflare, pero no sustituyen la prueba exigida desde otra red. No se ejecutaron POST de negocio, inserciones, ediciones ni eliminaciones de prueba.

## Evidencia pendiente para cerrar el documento del profesor

1. Desde datos móviles con Wi-Fi apagado u otra red externa: abrir la URL HTTPS actual, iniciar y cerrar sesión, entrar al dashboard propio y anotar fecha, dispositivo, red, rol y resultado. El grupo debe guardar capturas sin contraseñas ni datos personales. Responsable de la prueba remota: **por designar**.
2. Capturas de acceso anónimo redirigido al login y de acceso directo prohibido (`403`) con Bodega/Caja. El informe HTTP anterior registra las respuestas, pero no hay capturas visuales nuevas.
3. Probar Android, iPhone, tableta de ~768 px y escritorio; revisar menú, filtros, KPI, gráficas y modales a 360 px, 768 px y escritorio. CSS contiene reglas responsivas, pero falta constatación visual en esos dispositivos.
4. La guía solicita alta, consulta y edición con datos ficticios, encabezado y detalle, y recuperación/reinicio. Por instrucción del usuario no se altera la base vigente: estas pruebas de escritura y restauración quedan para una copia aislada autorizada. La parada del túnel no se provocó mientras el profesor necesita acceso; [Cloudflare confirma](https://developers.cloudflare.com/tunnel/get-started/quick-tunnels/) que la URL deja de funcionar al parar y cambia al reiniciar.
5. El docente debe confirmar que acepta la **modalidad temporal** para la demostración. Si exige dirección estable, habrá que configurar un túnel administrado con una zona/dominio del grupo. No se presume que un dominio sea gratis.

M05 (histórico), M07-E03 y M08-E07 siguen declarados pendientes por falta de datos fieles. El resultado técnico de la publicación está en [el informe previo](PUBLICACION_ENTREGA_20261107.md); este documento actualiza exclusivamente la modalidad de acceso de la guía 10.
