# Comandos rápidos

Registro de comandos reales usados para preparar este proyecto. Las rutas indican el directorio desde el que se ejecutaron. No se incluyen secretos.

## Tres páginas de Inicio (2026-10-25)

- Desde `C:\BaseDatos_2026`: Get-Content y rg leyeron AGENTS, guía de diseño, script SQL y componentes MVC actuales. `New-Item -ItemType Directory -Path 'src/SupermercadoMVC/Views/Inicio' -Force` creó la carpeta de vistas.
- Desde `C:\BaseDatos_2026`, fuera del aislamiento: `Get-CimInstance Win32_Process -Filter "Name = 'SupermercadoMVC.exe'"` identificó la instancia 38920. Desde la carpeta MVC, Get-Process comprobó su ruta exacta bajo el proyecto y Stop-Process detuvo solo ese proceso antes de compilar.
- Desde `C:\BaseDatos_2026\src\SupermercadoMVC`, fuera del aislamiento: `dotnet build` detectó errores RZ por usar page como variable en Razor; tras renombrarla, el mismo comando compiló con 0 errores/0 advertencias. No se modificaron permisos, paquetes ni configuración global.
- Desde esa carpeta, `dotnet run --no-build --launch-profile http` inició localhost:5192 con la compilación nueva.
- Desde `C:\BaseDatos_2026`, fuera del aislamiento: `sqlcmd -S <servidor>,<puerto> -d <base> -U <usuario> -N o -C -b -W -s '|' -Q <SELECT>` contrastó los dashboards. Credenciales leídas en memoria desde CONEXION.md; SQLCMDPASSWORD temporal retirado al finalizar. Consultas usadas: fecha GETDATE como DATE; ventas confirmadas de hoy/mes con total agregado por venta; productos/clientes/sucursales activos; compras recibidas mensuales; ventas por sucursal; inventario distinto/bajo mínimo/cero y por categoría; compras por estado; pagos Aplicados de hoy y por método últimos 30 días; ventas con saldo positivo y top 5 productos por importe neto de líneas de seis meses. No se ejecutaron scripts del grupo ni DML.
- Navegador integrado: acceso demo existente por su formulario; tres rutas /Inicio/Administrador, /Inicio/Bodega y /Inicio/Caja. Lectura DOM contrastó KPI/gráficos con SELECT, búsqueda de productos con y sin coincidencias, cargas sin errores de consola, formato Q y estados informativos. Capturas a 614/1280 px y medición scrollWidth comprobaron que no había desbordamiento de página. Carga GET de /catalogos/clientes, /catalogos/empleados, /ventas y /pagos sin alertas. No se guardaron datos ni se enviaron POST de operaciones.

## Implementación Pagos y saldos (2026-10-25)

- Desde `C:\BaseDatos_2026`: Get-Content y rg sobre AGENTS, guías locales, scripts y MVC; lectura sin ejecutar scripts.
- Desde `C:\BaseDatos_2026\src\SupermercadoMVC`: `dotnet build --no-restore` dentro del aislamiento reprodujo acceso denegado en obj/Debug/net9.0/rpswa.dswa.cache.json.
- Desde `C:\BaseDatos_2026`: `Get-CimInstance Win32_Process -Filter "Name = 'SupermercadoMVC.exe'"` y `Get-NetTCPConnection -LocalPort 5192 -State Listen -ErrorAction SilentlyContinue`. CIM fue denegado dentro del aislamiento; fuera identificó la ruta del ejecutable del proyecto.
- Desde la carpeta MVC, fuera del aislamiento: `Get-Process -Id 33264` y `Stop-Process -Id 33264`, tras comprobar la ruta exacta, liberaron la compilación. `dotnet build`, incluida versión final, terminó con 0 errores/0 advertencias. No se modificaron permisos/configuraciones.
- Desde la misma carpeta: `dotnet run --no-build --launch-profile http` inició localhost:5192; Ctrl+C detuvo la instancia temporal antes del build final y el mismo comando volvió a iniciar la versión final.
- Desde `C:\BaseDatos_2026`, fuera del aislamiento: `sqlcmd -S <servidor>,<puerto> -d <base> -U <usuario> -N o -C -b -W -s '|' -Q <SELECT>` con valores leídos en memoria desde CONEXION.md y SQLCMDPASSWORD temporal, retirado al finalizar. Lecturas: total redondeado agregado por venta, pagos Aplicados, KPI de confirmadas, conteos/estados de pagos, saldos FV-2022/23/25 y negativos. En este contexto no hubo fallo TLS ni cambios globales.
- Misma conexión, solo SELECT: VALUES con dos importes DECIMAL 0.0050 dio total agregado 0.01; restas DECIMAL(37,2) conservaron 0.01 y -0.01. COUNT_BIG de Tbl_Pagos dio 35 y conteo del comprobante REVISION-SIN-GUARDAR dio 0.
- Navegador integrado: acceso demo normal, menú, búsqueda, filtros, paginación, historial y modal. Lectura DOM verificó POST /pagos/registrar/22, token presente, máximo de monto 100.00 y rechazo local de comisión 26 frente a abono 25. Comisión corregida a 1 en pantalla sin guardar. Consola sin errores observados y captura visual del modal. No se enviaron formularios de pago ni se ejecutó DML.

## Implementación Ventas (2026-10-25)

- Desde C:/BaseDatos_2026: Get-Content de AGENTS, guía crud-encabezado-detalle, scripts SQL y MVC; rg para relaciones y rutas. Lectura de archivos, sin ejecutar scripts.
- Get-CimInstance Win32_Process identificó la instancia SupermercadoMVC.exe por su ruta exacta dentro del proyecto; Stop-Process detuvo exclusivamente esa instancia para liberar los binarios. No se detuvo SQL Server.
- Desde C:/BaseDatos_2026/src/SupermercadoMVC: `dotnet build` fuera del aislamiento, incluida compilación final: 0 advertencias/0 errores. `dotnet run --no-build --launch-profile http` inició la aplicación en localhost:5192. Ctrl+C detuvo la instancia temporal antes de la compilación final y luego se volvió a iniciar.
- Desde C:/BaseDatos_2026: sqlcmd con credenciales leídas en memoria desde CONEXION.md y secreto en SQLCMDPASSWORD temporal, omitido de la salida. Consultas realmente usadas: SELECT Estado,COUNT_BIG(*) FROM dbo.Tbl_Ventas GROUP BY Estado; SELECT COUNT_BIG(*) FROM dbo.Tbl_VentasDetalle; SELECT SUM(d.Cantidad*d.PrecioUnitario-d.Descuento) con JOIN de ventas confirmadas; SELECT TOP(3) de venta/número/estado e importe agrupado. Resultado: 25 ventas, 39 detalles, 6050.0000 confirmado. No hubo DML.
- Navegador integrado: acceso demo (solo sesión), menú Ventas, búsqueda FV-2024, consulta confirmada y modal nuevo, añadir una línea sin guardar, selección de sucursal y producto; revisión de consola. No se enviaron formularios de guardar/confirmar/eliminar.

## Resolución del entorno y arranque (2026-10-25)

- Desde C:/BaseDatos_2026/src/SupermercadoMVC: `dotnet build --no-restore` dentro del aislamiento reprodujo acceso denegado a obj/Debug/net9.0/rpswa.dswa.cache.json.
- Desde C:/BaseDatos_2026: `Get-Acl src\SupermercadoMVC\obj` y `Get-Item` sobre cachés verificaron ACL/atributos; no se cambiaron permisos ni se eliminaron archivos.
- Desde C:/BaseDatos_2026/src/SupermercadoMVC: `dotnet build` ejecutado con autorización fuera del sandbox: restauración correcta, 0 advertencias/0 errores. No se alteró NuGet ni configuración global.
- Desde la misma carpeta: `dotnet run --no-build --launch-profile http` fuera del sandbox: aplicación iniciada en http://localhost:5192, entorno Development.
- Desde C:/BaseDatos_2026: `Invoke-WebRequest` con WebRequestSession para GET /acceso, POST /Access/Enter con token antiforgery y sin credenciales (solo sesión demo), y GET /catalogos/empleados. Resultado: acceso y catálogo HTTP 200; selector con 5 sucursales, 8 botones Editar y modal/token presentes; sin error de consulta SQL. No se enviaron POST de alta/edición/eliminación.

## Implementación Empleados (2026-10-25)

- Desde `C:\BaseDatos_2026`: `Get-Content` y `rg` para inspeccionar AGENTS.md, guías crud-foreign-key/diseno-ui, script del grupo, definiciones, controlador, servicio, modelos y vistas. Solo archivos locales; sin ejecutar SQL.
- Desde `C:\BaseDatos_2026\src\SupermercadoMVC`: `dotnet build` — falló por NU1301, SSL/autenticación a api.nuget.org y acceso denegado al archivo temporal bajo obj.
- Desde `C:\BaseDatos_2026\src\SupermercadoMVC`: `dotnet build --no-restore` — falló por acceso denegado a obj/Debug/net9.0/rpswa.dswa.cache.json.
- No se modificaron permisos/configuración ni se ejecutaron consultas SQL o DML de prueba. Compilación y verificación en navegador pendientes.

## Windows/PowerShell

- `Get-Location` — muestra el directorio actual. Ejecutado desde `C:\BaseDatos_2026`.
- `Test-Path -LiteralPath 'C:\BaseDatos_2026'` — comprueba si existe la ruta del proyecto. Ejecutado desde `C:\BaseDatos_2026`.
- `Get-ChildItem -LiteralPath 'C:\BaseDatos_2026' -Force` — inspecciona elementos existentes. Ejecutado desde `C:\BaseDatos_2026`.
- `New-Item -ItemType Directory -Path <ruta> -Force` — crea directorios de la estructura si faltan. Ejecutado desde `C:\BaseDatos_2026`.
- `Set-Content -LiteralPath <ruta> -Value <contenido> -Encoding utf8` — escribe archivos de texto nuevos, solo tras comprobar que no existen. Ejecutado desde `C:\BaseDatos_2026`.
- `New-Item -ItemType File -Path <ruta>` — crea archivos `.gitkeep` faltantes. Ejecutado desde `C:\BaseDatos_2026`.

## Docker

Aún no se ejecutaron comandos Docker.

## dotnet

Aún no se ejecutaron comandos dotnet.

## SQL Server

Aún no se ejecutaron comandos de SQL Server.

## Git

Aún no se ejecutaron comandos Git.

## Diagnóstico

- `Get-Location` — confirma el directorio de ejecución. Ejecutado desde `C:\BaseDatos_2026`.
- `Test-Path -LiteralPath 'C:\BaseDatos_2026'` — verifica la existencia de la carpeta objetivo. Ejecutado desde `C:\BaseDatos_2026`.
- `Get-ChildItem -LiteralPath 'C:\BaseDatos_2026' -Force` — permite revisar el contenido existente antes de crear archivos. Ejecutado desde `C:\BaseDatos_2026`.

## Solución de errores

Aún no se ejecutaron comandos para solucionar errores.

## Comandos registrados en la revisión del esquema

- `Get-ChildItem -LiteralPath 'C:\BaseDatos_2026\database' -Force` — revisó los scripts SQL y sus tamaños. Ejecutado desde `C:\BaseDatos_2026`.
- `Get-ChildItem -LiteralPath 'C:\BaseDatos_2026\docs' -Force -Recurse` — verificó la guía y el nombre de imagen presente. Ejecutado desde `C:\BaseDatos_2026`.
- `Get-Content -LiteralPath 'C:\BaseDatos_2026\database\01_Crear_Base_Datos.sql' -Raw` y `Get-Content -LiteralPath 'C:\BaseDatos_2026\database\02_Datos_Prueba.sql' -Raw` — leyeron los scripts sin ejecutarlos. Ejecutados desde `C:\BaseDatos_2026`.
- `Get-Content -LiteralPath 'C:\BaseDatos_2026\AGENTS.md' -Raw` y `Get-Content -LiteralPath 'C:\BaseDatos_2026\MEMORY.md' -Raw` — consultaron reglas y memoria previas. Ejecutados desde `C:\BaseDatos_2026`.
- `Get-Item -LiteralPath <rutas> | Select-Object FullName,Length` — verificó que el resumen, memoria, reglas y guía de comandos existen y tienen contenido. Ejecutado desde `C:\BaseDatos_2026`.
- `Get-Content -LiteralPath 'C:\BaseDatos_2026\AGENTS.md' -Raw`, `Get-Content -LiteralPath 'C:\BaseDatos_2026\MEMORY.md' -Raw` y `Get-Content -LiteralPath 'C:\BaseDatos_2026\docs\COMANDOS_RAPIDOS.md' -Tail 15` — revisó los documentos actualizados. Ejecutado desde `C:\BaseDatos_2026`.
- `Set-Content -LiteralPath <ruta> -Value <contenido> -Encoding utf8` — comando de escritura previsto para actualizar documentos del proyecto; no se ejecutó correctamente en esta sesión. Ejecutado desde `C:\BaseDatos_2026`.

## Diagnóstico del equipo Windows

Comandos realmente usados desde `C:\BaseDatos_2026` el 2026-10-11:

- `Get-Command code,codex,dotnet,docker,git,sqlcmd,sqlpackage,ssms,winget` (se consultaron uno por uno con `Get-Command ... -ErrorAction SilentlyContinue`) — comprobó la disponibilidad y ruta de los ejecutables.
- `code --version` — VS Code 1.140.0.
- `codex --version` — Codex CLI 0.162.0-alpha.17.2; ejecutable proporcionado por la extensión Codex de VS Code.
- `dotnet --info` y `dotnet --list-sdks` — SDKs .NET 10.0.300 y 10.0.302; runtimes .NET y ASP.NET Core 8.0.29/8.0.31 y 10.0.8/10.0.10.
- `git --version` — Git 2.56.0.windows.1.
- `docker --version`, `docker version` y `docker info --format '{{.ServerVersion}}'` — Docker CLI 29.8.1; el acceso al motor por named pipe fue denegado.
- `Get-Service -Name 'com.docker.service'` y `Get-Process -Name 'Docker Desktop','com.docker.backend','dockerd'` — servicio Docker detenido; no se encontraron esos procesos.
- `docker context ls` — falló porque el acceso a la configuración/contextos Docker del perfil fue denegado.
- `sqlcmd -?` — SQL Server Command Line Tool 17.0.1000.7.
- `Get-ItemProperty 'HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*',...` — detectó Docker Desktop 4.93.0, Git 2.56.0 y SQL Server Management Studio 22.6.0.
- `Get-Item <ruta a Ssms.exe>` con `.VersionInfo` — SSMS 22.6.11806.211.
- `winget --version` y `winget list` — el ejecutable existe, pero Windows denegó su ejecución en esta sesión.
- `Get-CimInstance Win32_OperatingSystem` — falló con acceso denegado; versión de Windows se consultó con `[Environment]::OSVersion.VersionString`.
- `Get-Process powershell,pwsh` y `WindowsPrincipal.IsInRole(Administrator)` — se comprobó que la sesión no es Administrador.

No se ejecutaron comandos de instalación ni se cambió el estado de servicios.

### Verificación de Docker reportada por el usuario

El usuario ejecutó estos comandos en Windows PowerShell desde `C:\WINDOWS\system32` y compartió la salida el 2026-10-11:

- `Start-Service -Name com.docker.service` — inició el servicio de Docker Desktop.
- `docker version` — confirmó Client 29.8.1 y Server Docker Desktop 4.93.0, Engine 29.8.1 (`desktop-linux`).

La captura de Docker Desktop muestra el motor activo, un contenedor SQL Server en ejecución y contenedores MySQL y Oracle detenidos. Los nombres aparecen truncados en la captura. Esta sesión no inició ni modificó contenedores.

## Implementación MVC y verificación (2026-10-18)

Comandos ejecutados desde las rutas indicadas. No contienen credenciales.

- Desde `C:\BaseDatos_2026\src`: `dotnet new mvc -n SupermercadoMVC -f net10.0 --no-restore` — falló porque el SDK/plantillas disponibles aquí son .NET 9; `dotnet new mvc -n SupermercadoMVC -f net9.0 --no-restore` — creó la base MVC.
- Desde `C:\BaseDatos_2026\src\SupermercadoMVC`: `dotnet add package Microsoft.Data.SqlClient --no-restore` — intentó añadir la referencia sin restaurar; la ejecución autorizada de `dotnet add package Microsoft.Data.SqlClient` resolvió/restauró Microsoft.Data.SqlClient 7.1.1.
- Desde `C:\BaseDatos_2026\src\SupermercadoMVC`: `dotnet user-secrets set --project <SupermercadoMVC.csproj> ConnectionStrings:Supermercado <valor leído de CONEXION.md>` — guardó la conexión en el almacén local de User Secrets, fuera del código fuente. No registrar ni imprimir el valor.
- Desde `C:\BaseDatos_2026`: `docker ps --format '{{.Names}}|{{.Status}}|{{.Ports}}'` — comprobó en solo lectura que `sqlserver_basedatos` estaba activo y publicaba el puerto 1435 a 1433.
- Desde `C:\BaseDatos_2026`: `sqlcmd -S <hostname>,<puerto> -d <base> -U <usuario> -P <secreto de CONEXION.md> -N o -C -b -Q <consultas SELECT>` — confirmó `SupermercadosGuate`, existencia de tablas y conteos KPI. Solo SELECT; el secreto no se incluyó en el registro.
- Desde `C:\BaseDatos_2026\src\SupermercadoMVC`: `dotnet build` y `dotnet build --no-restore` — compilación final exitosa con 0 advertencias y 0 errores; antes, el sandbox bloqueó `bin/obj` y se repitió con autorización.
- Desde `C:\BaseDatos_2026\src\SupermercadoMVC`: `dotnet run --no-build --launch-profile http` — inició la aplicación de desarrollo en `http://localhost:5192`.
- Desde `C:\BaseDatos_2026`: `Invoke-WebRequest <URL local> -WebSession <sesión>` — verificó acceso demo, dashboard y rutas de catálogo (HTTP 200); un POST con campo requerido vacío verificó validación sin llegar a una escritura SQL.
- Desde `C:\BaseDatos_2026`: `Invoke-WebRequest` a `/acceso`, `/Home/Index`, `/catalogos/categorias`, `/catalogos/sucursales`, `/catalogos/proveedores`, `/catalogos/clientes`, `/modulos/productos`, `/modulos/compras`, `/modulos/ventas`, `/modulos/inventario` y `/modulos/reportes` — verificó HTTP 200 en las once rutas.
- Desde `C:\BaseDatos_2026\src\SupermercadoMVC`: `dotnet build --no-restore` — compilación final tras el ajuste de etiquetas: 0 advertencias y 0 errores.
- Solución aplicada a la advertencia HTTPS del perfil HTTP: `app.UseHttpsRedirection()` se ejecuta solo fuera del entorno Development; producción conserva redirección HTTPS y el perfil académico local funciona en HTTP.

## Pruebas CRUD autorizadas (2026-10-18)

Ejecutadas desde `C:\BaseDatos_2026`; los secretos se leyeron localmente y se omitieron de los registros:

- `sqlcmd -S <hostname>,<puerto> -d <base> -U <usuario> -P <secreto> -N o -C -b -W -s '|' -Q "SELECT DB_NAME(), HAS_PERMS_BY_NAME(..., 'INSERT'), HAS_PERMS_BY_NAME(..., 'UPDATE'), HAS_PERMS_BY_NAME(..., 'DELETE')"` — verificación SELECT-only de la conexión y permisos efectivos. Resultado: base `SupermercadosGuate`; permisos de lectura y escritura presentes en las cuatro tablas.
- `Invoke-WebRequest http://localhost:5192/catalogos/<slug>` y POST antiforgery a `/crear`, `/editar/<id>` y `/eliminar/<id>` — pruebas a través de la aplicación con registros nuevos marcados `PRUEBA_CODEX`; `slug`: `clientes`, `proveedores`, `sucursales`, `categorias`. Cada alta/edición se confirmó con SELECT, y solo se borró la PK IDENTITY creada por esa prueba.
- `sqlcmd ... -Q "SELECT COUNT_BIG(*) ..."` — confirmó que cada ID temporal desapareció y que no quedó ninguna marca `PRUEBA_CODEX`; los conteos finales fueron Clientes 14, Proveedores 6, Sucursales 5, Categorías 6.
- Edge iniciado con `Start-Process <msedge.exe> -ArgumentList '--headless=new','--remote-debugging-port=9333',...` — verificación de los botones Agregar/Editar, apertura de modales y campos/PK precargados. Perfil temporal creado y eliminado dentro de `C:\BaseDatos_2026`.
- Desde `C:\BaseDatos_2026\src\SupermercadoMVC`: `dotnet restore` — paquetes actualizados; `dotnet build --no-restore` — 0 advertencias, 0 errores; `dotnet run --no-build --launch-profile http` — aplicación local en `http://localhost:5192`.

## Integración de Compras (2026-10-25)

- Ruta `C:\BaseDatos_2026`: `Get-Content <archivo>` y `rg -n <patrón> <rutas>` — lectura de esquema, datos de prueba, guías locales, componentes MVC y fuentes de la integrante extraídas en `C:\BaseDatos_2026_ANALISIS_INTEGRANTE_20261025`. No se ejecutaron scripts SQL.
- Ruta `C:\BaseDatos_2026\src\SupermercadoMVC`: `dotnet build --no-restore` — primer intento dentro del sandbox bloqueado al escribir `obj/Debug/net9.0/rpswa.dswa.cache.json`; fuera del aislamiento se corrigieron errores de compilación de la vista nueva. El ejecutable estaba bloqueado por la instancia MVC anterior.
- Ruta `C:\BaseDatos_2026`: `Get-CimInstance Win32_Process -Filter "ProcessId=31036"` y `Get-NetTCPConnection -LocalPort 5192 -State Listen -ErrorAction SilentlyContinue` — identificación de instancia MVC; no se tocaron procesos SQL.
- Ruta `C:\BaseDatos_2026\src\SupermercadoMVC`: `Get-Process -Id 31036`, comprobación de su ruta exacta y `Stop-Process -Id $purchaseApp.Id` — se detuvo únicamente la aplicación verificada para desbloquear su ejecutable.
- Misma ruta: `dotnet build --no-restore` — resultado final 0 errores, 0 advertencias; `dotnet run --no-build --launch-profile http` — aplicación nueva en http://localhost:5192, content root del proyecto principal.
- Ruta `C:\BaseDatos_2026`: consultas con `sqlcmd -S <servidor configurado> -d <base configurada> -U <usuario configurado> -N o -C -b -W -s '|' -Q <SELECT>` — credenciales tomadas en memoria de User Secrets y contraseña mediante SQLCMDPASSWORD, retirada del entorno al terminar; no registrada. Se reutilizó la configuración TLS local existente, sin cambios de certificados ni configuración global. Un primer intento falló antes de conectar por asignación al constructor de conexión; se utilizó después el constructor de SqlConnectionStringBuilder correctamente.
- SELECT ejecutados: DB_NAME(); conteo de Tbl_Compras por Estado; SUM(Cantidad*CostoUnitario-Descuento) sobre compras Recibidas; listado por compra con suma de detalles; lectura de sys.check_constraints para Tbl_ComprasDetalle; COUNT_BIG/SUM(Existencia)/SUM(CostoPromedio) de Tbl_Inventario. Solo lectura, sin DML.
- Ruta `C:\BaseDatos_2026`: `python -` con pathlib/hashlib — comparación de archivos funcionales contra el respaldo excluyendo bin/obj/.git, conteo de archivos del respaldo y comparación binaria de los dos scripts SQL. Sin usar Git.
- Navegador integrado: acceso demo normal, GET de Compras, búsqueda/filtro/paginación, apertura de modales, cálculo y validación visual sin guardar; GET de Clientes, Empleados, Ventas, Pagos, los tres dashboards y /modulos/compras. Sin POST CRUD.

- SELECT final con la misma conexión: conteo por Estado, COUNT_BIG de Tbl_ComprasDetalle, agregados de inventario y LEFT JOIN de compras con empleado/sucursal para detectar relaciones inválidas. Resultado: 9 compras, 20 detalles; controles de inventario sin cambios y cero relaciones inválidas.

## Integración de Login (2026-10-25)

Comandos realmente utilizados. La conexión SQL se tomó de User Secrets en memoria; no se registraron secretos.

- Ruta `C:\BaseDatos_2026`: `Get-Content -Raw <archivo>` y `rg -n <patrón> <rutas>` — lectura del script del grupo, Login del ZIP, controladores, vistas y reglas.
- Ruta `C:\BaseDatos_2026`: `docker inspect sqlserver_basedatos` — localizó el volumen de respaldo del contenedor SQL Server; no cambió el contenedor.
- SQL Server, mediante conexión configurada en User Secrets: `BACKUP DATABASE [SupermercadosGuate] TO DISK = N'/var/opt/mssql/backups/SupermercadosGuate_ANTES_LOGIN_20261025.bak' WITH COPY_ONLY, INIT, CHECKSUM, COMPRESSION` — creó respaldo independiente. `RESTORE VERIFYONLY FROM DISK = N'/var/opt/mssql/backups/SupermercadosGuate_ANTES_LOGIN_20261025.bak' WITH CHECKSUM` — validó el conjunto. No se aplicó ninguna migración.
- Ruta `C:\BaseDatos_2026`: `Get-FileHash 'C:\BASE_DATOS\Docker\Backups\SupermercadosGuate_ANTES_LOGIN_20261025.bak' -Algorithm SHA256` — calculó la huella del respaldo.
- Ruta `C:\BaseDatos_2026\src\SupermercadoMVC`: `dotnet build --no-restore -p:UseAppHost=false -p:OutputPath=bin/LoginCheck/` — compiló el Login sin sustituir el ejecutable que atiende el puerto 5192; 0 errores y 0 advertencias.
- Misma ruta: `dotnet .\bin\LoginCheck\SupermercadoMVC.dll --urls http://localhost:5193` — inició una instancia temporal para probar el Login. Se cerró después de las pruebas; no se tocó la instancia principal ni SQL Server.
- Ruta `C:\BaseDatos_2026`: `Invoke-WebRequest` con `WebRequestSession` contra `/acceso` y `/Access/Enter` de `localhost:5193` — comprobó token antiforgery, validaciones de campos vacíos y rechazo HTTP 400 sin token. No se enviaron operaciones CRUD.
- Navegador: GET a rutas de Clientes, Empleados, Ventas, Pagos, Compras y los tres dashboards en 5193 — redirección al Login sin cookie; GET a las mismas rutas en 5192 — comprobación de que la instancia anterior continúa operativa.


## Ajustes de Inventario (2026-10-26)

- Desde C:\BaseDatos_2026: Get-Content y rg leyeron AGENTS.md, guías locales, scripts SQL y componentes de Compras/Ventas. Se leyeron las reglas de la guía oficial del integrante 3 mediante XML del DOCX, sin modificarlo.
- Desde C:\BaseDatos_2026\src\SupermercadoMVC: dotnet build --no-restore -p:UseAppHost=false -p:OutputPath=bin/AjustesCheck/ — primer intento bloqueado por escritura en bin/obj; compilación fuera del aislamiento, incluida la final, 0 advertencias y 0 errores. No se cambió NuGet ni permisos.
- Desde C:\BaseDatos_2026: node --check src/SupermercadoMVC/wwwroot/js/ajustes.js — sintaxis correcta.
- Desde la carpeta MVC, con ASPNETCORE_ENVIRONMENT=Development: dotnet .\bin\AjustesCheck\SupermercadoMVC.dll --urls http://localhost:5193 — instancia de validación separada. Ctrl+C la detuvo antes de la compilación final; no se detuvo 5192.
- Desde C:\BaseDatos_2026: se compiló ReadOnlyAudit.cs en bin/AjustesCheck con el csc.dll del SDK existente y un archivo .rsp para evitar el límite de longitud de argumentos de Windows. dotnet exec --runtimeconfig src/SupermercadoMVC/bin/AjustesCheck/SupermercadoMVC.runtimeconfig.json --depsfile src/SupermercadoMVC/bin/AjustesCheck/SupermercadoMVC.deps.json src/SupermercadoMVC/bin/AjustesCheck/ReadOnlyAudit.dll ejecutó exclusivamente BuscarAsync/ObtenerAsync y un SELECT independiente de conteos por estado. User Secrets en memoria, sin imprimir secretos; no se utilizó sqlcmd ni se enviaron DML.
- GET mediante HttpClient sin seguir redirecciones: /ajustes, /ajustes?nuevo=true, /modulos/ajustes y rutas existentes — 302 al login; /js/ajustes.js y /css/ajustes.css — 200. Navegador real confirmó redirección de /ajustes al login; faltó sesión para validar visualmente los modales.
- Tras la compilación final se repitió el mismo comando de inicio en 5193; la compilación nueva quedó disponible para pruebas manuales con autenticación.

## Productos y Producto–Proveedor (2026-10-26)

- Ruta C:\BaseDatos_2026: Get-Content, rg, Get-ChildItem y Get-FileHash -Algorithm SHA256 — lectura de esquema/guías/código y comparación de archivos funcionales antes/después. DOCX oficiales leídos con System.IO.Compression.ZipFile y XPath; no se extrajeron ni alteraron.
- Ruta C:\BaseDatos_2026: node --check src/SupermercadoMVC/wwwroot/js/productos.js — sintaxis correcta.
- Ruta C:\BaseDatos_2026\src\SupermercadoMVC: dotnet build --no-restore -p:UseAppHost=false -p:OutputPath=bin/ProductosCheck/ — primer intento con acceso denegado del aislamiento a bin/obj; repetido fuera del aislamiento: 0 errores y 0 advertencias. Sin cambiar permisos, NuGet o instalaciones.
- Misma ruta: compilación del diagnóstico local ReadOnlyAudit.cs con csc.dll y @bin/ProductosCheck/ReadOnlyAudit.rsp; ejecución: dotnet exec --runtimeconfig bin/ProductosCheck/SupermercadoMVC.runtimeconfig.json --depsfile bin/ProductosCheck/SupermercadoMVC.deps.json bin/ProductosCheck/ReadOnlyAudit.dll — llamadas exclusivamente de lectura a ProductosDataService, SELECT independientes y validaciones de modelos en memoria; 13 productos, 16 relaciones y páginas de 8+5; KPI y filtros correctos. Los artefactos quedaron en bin ignorado. La escritura del .rsp también requirió salir del aislamiento.
- Misma ruta: ASPNETCORE_ENVIRONMENT=Development (variable temporal de la terminal) y dotnet .\bin\ProductosCheck\SupermercadoMVC.dll --urls http://localhost:5193 — inició la compilación nueva tras cerrar con Ctrl+C exclusivamente la instancia temporal anterior de 5193. Sin sustituir 5192.
- Ruta C:\BaseDatos_2026: HttpClient con AllowAutoRedirect=false y GET a Productos, modales, alias y rutas anteriores — 302 al acceso; /js/productos.js, /css/productos.css y /acceso: 200. Navegador de Codex confirmó el login; no había sesión autenticada.
- Misma ruta: System.Data.SqlClient usando User Secrets en memoria y SELECT COUNT_BIG de Tbl_Usuarios — 1 usuario total, 1 Activo. Sin mostrar nombres de usuario, hashes, contraseñas o cadena de conexión. No se hizo POST ni DML.

## Inventario de solo consulta (2026-10-26)

- Ruta C:\BaseDatos_2026: Get-Content AGENTS.md, scripts SQL y guía UI; Get-FileHash -Algorithm SHA256 sobre módulos funcionales antes/después — esquema y cambios de integración comprobados.
- Ruta C:\BaseDatos_2026\src\SupermercadoMVC: dotnet build --no-restore -p:UseAppHost=false -p:OutputPath=bin/InventarioCheck/ — primer intento en aislamiento recibió acceso denegado en bin/obj; repetido fuera de ese aislamiento compiló con 0 errores y 0 advertencias. No se cambiaron permisos ni configuración.
- Misma ruta: csc.dll con @bin/InventarioCheck/ReadOnlyAudit.rsp y dotnet exec --runtimeconfig bin/InventarioCheck/SupermercadoMVC.runtimeconfig.json --depsfile bin/InventarioCheck/SupermercadoMVC.deps.json bin/InventarioCheck/ReadOnlyAudit.dll — diagnóstico en bin ignorado, únicamente SELECT de Inventario y catálogos; KPI, nueve páginas, sucursales, categorías y filtros concordaron con SELECT independientes. No se imprimieron secretos.
- Misma ruta: ASPNETCORE_ENVIRONMENT=Development temporal y dotnet .\bin\InventarioCheck\SupermercadoMVC.dll --urls http://localhost:5193 — reemplazó solo la instancia temporal anterior de 5193 para navegar; 5192 no fue sustituida.
- Ruta C:\BaseDatos_2026: HttpClient con AllowAutoRedirect=false a /inventario, /modulos/inventario, rutas anteriores, /css/inventario.css y /acceso — módulos protegidos 302 al login; recurso y acceso 200. Navegador confirmó la redirección a acceso sin sesión válida.
