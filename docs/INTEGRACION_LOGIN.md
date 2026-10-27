# Integración del Login (2026-10-25)

> Registro histórico de la etapa inicial. En esa fecha la migración 03 estaba pendiente. Documentación posterior indica que `Tbl_Usuarios` fue instalada y se observó un usuario activo; la auditoría del 2026-10-27 no pudo reconfirmar SQL Server. Para el estado y las pruebas vigentes, consulta [Cumplimiento académico](CUMPLIMIENTO_ACADEMICO.md).

## Procedencia y alcance

- Fuente: archivo local `2026_BaseDatos integrante incorporacion.zip` (SHA-256 `FCCA65850C35F94640C9446687ED42D7E97709812041B098C5F94CF3C72DE3A7`). Se adaptaron el controlador de acceso, repositorio de usuarios y modelos al MVC principal. No se incorporaron el `Program.cs`, layout, scripts de recreación ni credenciales de la integrante.
- Se conservó `/acceso` y el diseño coral/turquesa. El acceso usa cookie de ASP.NET Core, verificación de contraseña con `PasswordHasher<T>`, antiforgery y retorno solo a URL local. El cierre de sesión invalida la cookie. No se añadieron roles ni permisos.
- `database/03_Agregar_Usuarios_Login.sql` es una migración mínima independiente basada en la tabla del ZIP: `CodigoUsuario`, `Usuario` único, `PasswordHash` y `Estado`. **Está preparada, no ejecutada.** No altera tablas existentes ni contiene usuarios o contraseñas.

## Respaldos previos

- Proyecto principal: `C:\BaseDatos_2026_RESPALDO_ANTES_INTEGRACION_20261025`, 1320 archivos; se comprobó su presencia y no se modificó.
- SQL Server: `C:\BASE_DATOS\Docker\Backups\SupermercadosGuate_ANTES_LOGIN_20261025.bak`, respaldo `COPY_ONLY` con `CHECKSUM`; `RESTORE VERIFYONLY WITH CHECKSUM` confirmó que el conjunto es válido. SHA-256: `7B5DD4B36B69526FE22DF9AA64610D18555C05C7807ED83568F7F952E16672B8`. El respaldo no modifica datos ni esquema.
- Se comprobó mediante lectura que `dbo.Tbl_Usuarios` aún no existe en `SupermercadosGuate`.

## Archivos funcionales

- Nuevos: `Models/LoginModels.cs`, `Data/UsuarioRepository.cs`, `Data/UsuarioInicialProvisioner.cs` y la migración SQL preparada.
- Adaptados: `Controllers/AccessController.cs`, `Filters/DemoSessionFilter.cs`, `Views/Access/Index.cshtml`, `Views/Shared/_AppLayout.cshtml` y `Program.cs`. Se actualizó solamente el alcance de autenticación en `AGENTS.md`.
- El aprovisionador crea un usuario inicial únicamente al ejecutar explícitamente `--crear-usuario-inicial` desde una terminal interactiva después de autorizar y aplicar la migración. Solicita usuario y contraseña sin mostrarla y guarda solo un hash. No se ha ejecutado.

## Pruebas realizadas

- `dotnet build --no-restore -p:UseAppHost=false -p:OutputPath=bin/LoginCheck/`: 0 errores y 0 advertencias. Se compiló en salida separada para conservar la instancia de demostración en el puerto 5192.
- Instancia de prevalidación `http://localhost:5193/acceso`: el formulario y diseño cargan; token antiforgery presente; usuario y contraseña vacíos muestran validaciones; POST sin token devuelve 400. Credenciales ficticias muestran un error controlado porque falta `Tbl_Usuarios`; no se creó ningún registro.
- En navegador, las rutas protegidas de Clientes, Empleados, Ventas, Pagos, Compras y los tres dashboards redirigen a `/acceso` con `returnUrl` local cuando no hay cookie autenticada. La cookie antigua de demostración no concede acceso a la nueva compilación.
- En la instancia anterior de 5192, se comprobaron en navegador las rutas de Clientes, Empleados, Ventas con detalle, Pagos/Saldos, Compras con detalle y los tres dashboards. Cargaron con sus datos y sin alertas SQL. Esto verifica la instancia anterior, **no** una sesión autenticada de la nueva compilación.
- No se ejecutaron scripts del ZIP ni INSERT/UPDATE/DELETE de prueba.

## Pendiente para activar el Login

1. Obtener autorización explícita para ejecutar `database/03_Agregar_Usuarios_Login.sql`; el respaldo SQL ya fue creado y verificado. La restricción vigente de `AGENTS.md` impide aplicar cambios de esquema sin esa autorización.
2. Crear el usuario inicial mediante la opción interactiva, sin enviar contraseña por chat. La creación inserta exclusivamente una fila nueva en `Tbl_Usuarios`.
3. Sustituir de manera controlada la instancia de 5192 por la compilación nueva. Probar inicio correcto e incorrecto, persistencia y cierre de sesión, acceso a todos los módulos con cookie, formularios GET/validaciones y errores. Las pruebas DML de módulos quedan pendientes y se harán solo con datos de prueba y respaldo previo.

URL del Login al activar la nueva compilación: `http://localhost:5192/acceso`. Mientras no se aplique la migración ni se cree un usuario, la nueva compilación no permite iniciar sesión; la instancia anterior de 5192 continúa funcional con acceso demostrativo.
