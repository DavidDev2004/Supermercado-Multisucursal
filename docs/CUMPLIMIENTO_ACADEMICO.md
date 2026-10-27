# Cumplimiento académico y estado de pruebas

**Corte:** 27 de octubre de 2026. Esta auditoría no ejecutó DML, migraciones ni cambios en SQL Server. «Implementado» significa que existen ruta, controlador, vista y servicio; no equivale a prueba funcional completa.

## Fuentes de requisitos

Se leyeron los tres `01_Proyecto_Final_Integrante_*.docx` y las guías 01–05 en la carpeta local `Downloads`, además de la guía 07 local. Los documentos originales y las imágenes de prototipos se conservan solo localmente; se excluyen del repositorio público por falta de autorización de distribución documentada. Las guías 06 y 08–10 no se encontraron. Los tres PNG específicos de Inicio que nombra la guía 07 tampoco estaban disponibles; se revisaron dos imágenes generales y nueve prototipos de Fase IV locales. El documento de requisitos de Centro Automotriz pertenece a otro proyecto y no se utilizó para esta comparación.

| Fuente oficial revisada | Requisito principal | Estado comprobable |
|---|---|---|
| Tres documentos de integrantes, Fase I | 15 tablas comunes, PK/FK, unicidad, estados, costos históricos, pagos y operaciones transaccionales de inventario. | El script contiene las 15 tablas y sus restricciones; costo y fecha histórica de Ajustes se añadieron mediante migraciones. La base viva no se reconfirmó hoy. |
| Guía 01 | Entorno, script real del grupo, conexión y MVC compilable. | Estructura y compilación verificadas; SQL actual no respondió al diagnóstico de lectura. |
| Guía 02 | CRUD simple, KPI, búsqueda, modales y prueba DML con registros de práctica. | Implementado en catálogos; evidencia anterior de prueba en navegador y SELECT. |
| Guía 03 | CRUD con FK y selector real; integridad referencial. | Implementado en Empleados y Productos; las pruebas de errores FK de la versión actual faltan. |
| Guía 04 | Encabezado/detalle, modales, KPI y transacciones. | Implementado en Compras, Ventas y Ajustes; pruebas transaccionales negativas aún pendientes. |
| Guía 05 | Integración y matriz de acciones, resultado esperado/obtenido y evidencia. | Existe evidencia parcial en documentación; no hay una matriz completa con capturas de todos los módulos actuales. |
| Guía 07 | Inicio Administrador/Bodega/Caja, KPI y gráficos SQL y comparación SELECT. | Rutas y código presentes; usuario reportó seis SELECT concordantes. Faltan sus tres prototipos específicos y una inspección visual final. |
| Guías 06, 08, 09 y 10 | No disponibles. | Cumplimiento no evaluable sin los documentos. |

## Módulos MVC

| Módulo | Implementación revisada | Evidencia disponible | Pendiente principal |
|---|---|---|---|
| Login y cierre | Cookie, hash, antiforgery, retorno local y POST de salida. | Código revisado; `/acceso` devolvió 200 hoy. Historial documenta pruebas previas de formulario y redirección. | Probar login y salida en la instancia final con usuario real; revisar protección para publicación. |
| Inicio y dashboards Administrador, Bodega, Caja | Tres rutas, KPI, gráficos, tablas y accesos reales o avisos cuando falta dato. | Usuario reportó coincidencia de seis SELECT en SSMS; guía 07 y código revisados. | Revisar visualmente las tres rutas autenticadas en la versión final. |
| Categorías, Sucursales, Proveedores, Clientes | CRUD simple, filtros, KPI, paginación y modales. | Historial registra altas, ediciones y eliminación de datos temporales en etapa anterior. | Regresión autenticada de la versión actual. |
| Empleados | CRUD con FK a sucursal. | Usuario informó alta, edición y eliminación del empleado de prueba, confirmado con SELECT. | Reprobar validaciones negativas y regresión actual. |
| Productos y Producto–Proveedor | CRUD, relación FK y validaciones de duplicados. | Código y SELECT previos; sin prueba de escritura registrada aquí. | Pruebas de guardado y errores con datos exclusivos de prueba. |
| Compras y detalle | Borradores, recepción transaccional, costo ponderado e inventario. | Listado, modales y SELECT previos según documentación. | Recepción, repetición/concurrencia, inventario faltante y rollback. |
| Ventas y detalle | Borradores y confirmación transaccional; confirmadas/anuladas de consulta. | Usuario informó venta 1002 y descuento de inventario, verificados en SSMS. | Repetición/concurrencia y errores forzados. |
| Pagos y saldos | Pago de confirmadas, saldo calculado y control transaccional. | Usuario informó pagos 1002/1003 y saldo Q0.00 verificados. | Sobrepago, comprobante duplicado, concurrencia y rollback. |
| Inventario | Consulta con filtros, paginación, mínimos, costos y KPI. | SELECT y recorrido de páginas anteriores documentados; sin DML. | Pantalla autenticada en la versión final. |
| Ajustes y detalle | Borradores, aplicación transaccional y costo histórico. | Código, SELECT y compilación anteriores; la migración 04/05 fue informada como aplicada. | Aplicación manual controlada, doble aplicación y rollback. |
| Reportes | Ventas, compras, inventario y saldos; filtros, totales, páginas y CSV de lectura. | Código compilado; hoy la ruta sin cookie redirigió al login. | Verificar datos, filtros, CSV y errores con sesión autenticada. |

## Fase III y Fase IV

- **Fase III:** conteo estático actual de 15 ejercicios únicos por integrante (45). E14/E15 de los tres integrantes tienen evidencia previa de NO, SI, limpieza y `@@TRANCOUNT = 0` en `SupermercadosGuate_PRUEBAS`; consultar [validación SQL](VALIDACION_CONSULTAS_SQL.md). E01–E13 no se reejecutaron hoy. La revisión actual no certifica de nuevo los resultados de cada ejercicio ni reemplaza las consignas originales ausentes.
- **Fase IV:** los títulos de las nueve páginas PBIR coinciden con los tres títulos de página de cada documento de integrante: Proveedores y abastecimiento / Compras y movimientos / Seguimiento operativo; Clientes y oportunidades comerciales / Ventas y cobros / Seguimiento comercial y verificaciones; Productos y cobertura del inventario / Existencias y pérdidas / Reposición y verificaciones. `power-bi/validar_fase_iv.py` terminó correctamente hoy: 9 páginas, 45 visuales de datos, 45 tablas TMDL, 40 SELECT nuevos, 54 enlaces válidos y sin solapamientos según coordenadas. El primer conjunto de cinco visuales completa los 45. El usuario había confirmado anteriormente apertura y actualización en Power BI Desktop; esta auditoría no abrió Desktop ni reconfirmó sus resultados. Los seis visuales E14/E15 usan SELECT sobre la copia de pruebas.

## Pruebas ejecutadas en esta auditoría

| Comprobación | Resultado | Alcance |
|---|---|---|
| `dotnet build --no-restore -p:UseAppHost=false -p:OutputPath=bin/AuditoriaFinal/` | 0 errores, 0 advertencias. | Compilación C# y Razor; no prueba SQL ni formularios. |
| `power-bi/validar_fase_iv.py` | Correcto. | Estructura PBIR/TMDL, referencias y disposición; no Desktop ni actualización. |
| Conteo de marcadores E01–E15 | 15 únicos por cada archivo. | Integridad de numeración, no exactitud de resultados. |
| `node --check` de `wwwroot/js` | 9 archivos, 0 fallos. | Sintaxis JavaScript; no prueba interacción de navegador. |
| HTTP local | `/acceso` en 5193: 200; `/modulos/reportes`: 302 hacia `/acceso`. | Autenticación sin sesión; no pantalla autenticada. |
| Diagnóstico SELECT de Inventario | Falló al conectar: `SqlException` 20. | No se leyeron resultados SQL actuales ni se alteraron datos. |

No apareció un proyecto de pruebas automatizadas MVC. El diagnóstico de Inventario ya compilado, inspeccionado y limitado a SELECT devolvió `SqlException` número 20; el puerto SQL respondió a TCP, pero eso no demuestra autenticación ni lectura. En una sesión previa, `sqlcmd` había fallado por TLS. Las verificaciones SQL citadas en la tabla de módulos son **evidencias de sesiones anteriores o pruebas manuales reportadas por el usuario**.

## Pendientes por prioridad

1. **Alta:** obtener las guías 06 y 08–10, completar la matriz de evidencias de la guía 05 y ejecutar una regresión autenticada, sobre todo de Reportes, Productos, Compras y Ajustes. No declarar cumplimiento final antes de ello.
2. **Alta para publicación pública:** definir hosting y SQL Server accesible con TLS válido, secretos fuera del paquete, protección del login ante intentos repetidos y revisión del acceso por roles si el profesor lo exige. Véase [publicación](PUBLICACION_SEGURIDAD.md).
3. **Media:** ejecutar escenarios negativos y de concurrencia/rollback en una copia aislada con datos de prueba y respaldo; la lectura del código no certifica esos flujos.
4. **Media:** abrir el PBIP final en Power BI Desktop y comprobar los nueve prototipos, refresco y resultados; el validador local cubre únicamente estructura.
5. **Media:** armonizar `AGENTS.md`, que aún contiene restricciones redactadas para una etapa inicial, con el alcance ya autorizado e implementado. Se conservó sin cambios porque esta auditoría no modifica reglas operativas.
