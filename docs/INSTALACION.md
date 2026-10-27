# Instalación y ejecución local

Esta guía describe una instalación nueva. No ejecutar los scripts de creación o datos de prueba sobre la base productiva existente.

## Requisitos

- Windows con .NET SDK 9 para compilar `src/SupermercadoMVC/SupermercadoMVC.csproj` (`net9.0`).
- SQL Server accesible desde la aplicación y una base `SupermercadosGuate` preparada con el esquema del proyecto.
- Power BI Desktop solo para abrir `power-bi/Supermercado_FaseIV.pbip`.

## Base de datos

En una instalación vacía, revisar primero `database/01_Crear_Base_Datos.sql`; crea la base y las 15 tablas académicas. `database/02_Datos_Prueba.sql` es opcional y **contiene INSERT**. La tabla de autenticación se define separadamente en `database/03_Agregar_Usuarios_Login.sql`. Las migraciones `04` y `05` son para bases antiguas que aún no tienen `CostoUnitario` de ajustes y `FechaAplicacion`; el script de creación actual ya incluye ambas columnas. Requieren respaldo, revisión y autorización antes de ejecutarse. Véase [Base de datos](BASE_DATOS.md).

## Configuración MVC

Desde la raíz de una copia o clon del proyecto, entra en `src/SupermercadoMVC`. El archivo `appsettings.example.json` muestra las claves y opciones locales esperadas; es solo una plantilla y ASP.NET Core no lo carga automáticamente. Configura el valor real mediante User Secrets en tu equipo:

```powershell
cd src/SupermercadoMVC
dotnet restore
dotnet user-secrets set "ConnectionStrings:Supermercado" "<cadena SQL Server local>"
dotnet build --no-restore
dotnet run --launch-profile http
```

La cadena real debe introducirse localmente y no copiarse a documentación, código ni capturas. Para la conexión de desarrollo con un certificado SQL Server no confiable se puede configurar la opción de confianza solo en el secreto local; para publicar se necesita TLS y un certificado válido. No se incluye una cuenta ni contraseña inicial.

Prepara una instancia SQL Server propia antes de iniciar la aplicación. Revisa el script `database/01_Crear_Base_Datos.sql` y úsalo únicamente para una instalación vacía; `02_Datos_Prueba.sql` agrega datos de práctica de forma opcional. Revisa las migraciones independientes `03`–`05` y aplica solo las que falten en esa instalación. El puerto `1435` de la plantilla corresponde al Docker local de este equipo: cámbialo en tu secreto si tu servidor usa otro puerto. Ningún respaldo `.bak`, usuario SQL ni dato productivo se distribuye con el repositorio.

Abrir `http://localhost:5192/acceso`. El inicio de sesión requiere `Tbl_Usuarios` y al menos un usuario activo con contraseña hash. `UsuarioInicialProvisioner` solo se ejecuta con la opción explícita `--crear-usuario-inicial`, tras preparar la tabla y un respaldo; solicita los datos de forma interactiva. No ejecutar esa opción durante una simple compilación.

## Comprobación sin escritura

`dotnet build --no-restore` verifica C# y Razor. La ruta `/acceso` puede abrirse sin sesión; las demás rutas redirigen allí hasta autenticarse. Los reportes y dashboards requieren conexión SQL Server. La validación estructural del PBIP se ejecuta desde la raíz con el Python disponible: `python power-bi/validar_fase_iv.py`; no consulta la base. Las operaciones de los formularios sí escriben datos y no forman parte de esta comprobación.

## Power BI

Abrir `power-bi/Supermercado_FaseIV.pbip` en Desktop. Las 39 consultas E01–E13 apuntan a `SupermercadosGuate`; los seis visuales E14/E15 consultan por SELECT la copia `SupermercadosGuate_PRUEBAS`. La dirección `localhost,1435` es local y deberá adaptarse para otro equipo. Revisar las conexiones en Desktop antes de actualizar; el PBIP no incluye credenciales.
