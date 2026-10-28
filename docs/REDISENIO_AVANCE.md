# Rediseño visual: avance aislado

- Rama local: `redisenio-responsive`.
- Worktree: `C:\Users\html1\.codex\worktrees\redisenio-responsive\BaseDatos_2026`.
- La aplicación pública y el túnel ngrok continúan en `C:\BaseDatos_2026`; no se sustituyeron ni reiniciaron.
- Se copiaron al worktree los cambios locales de `.gitignore`, `Program.cs` y `deployment/Demo.ps1` sin copiar `deployment/private`.

## Implementado

- Menú móvil con fondo de cierre, botón de cierre, Escape, retorno del foco y navegación táctil.
- Navegación interna desplazable con cierre de sesión visible al pie; acceso adicional en la cabecera.
- Ajustes visuales globales para 390, 768, 1024 y 1440 px: fondos, color, superficies, botones, filtros, tablas con desplazamiento horizontal, tarjetas y modales de altura completa en teléfono.
- Corregidos los accesos visuales de Productos en Inicio y Reportes en Administrador.
- Títulos de los modales de catálogos con el nombre del registro; botón Guardar pago visible en el pie del modal móvil y asociado a su formulario POST original.

## Comprobado

- `dotnet build`: correcto, 0 advertencias y 0 errores.
- `node --check` de `wwwroot/js/app.js`: correcto.
- Vista previa autenticada en `http://127.0.0.1:5198`; 16 rutas cargaron desde el navegador sin desbordamiento horizontal de página a 390 px.
- Login medido a 390, 768, 1024 y 1440 px; dashboards, Inventario y Reportes medidos a 768, 1024 y 1440 px.
- Menú a 390×667: navegación interna desplazable y botón Cerrar sesión dentro del alto visible. Cierre mediante Escape y fondo; enlace del menú navega.
- Modales de Clientes, Productos, Compras, Ventas, Pagos y Ajustes abiertos en móvil; se agregó una línea de compra en el formulario sin guardar. El formulario de Pagos conservó método POST y token antiforgery. No hubo solicitudes POST ni cambios de datos.
- Reportes: se corrigió la altura excesiva de los selectores en móvil; consola del navegador sin errores ni advertencias durante las comprobaciones.

## Pendiente antes de integrar

- Revisión visual manual de los formularios restantes y de casos con tablas extensas en un dispositivo físico.
- Pruebas funcionales de escritura en un entorno de prueba, si el usuario las autoriza por separado.
- Aprobación del usuario para integrar la rama; no publicar ni fusionar antes.
