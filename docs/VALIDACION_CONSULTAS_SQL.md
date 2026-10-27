# Validación de consultas SQL transaccionales E14 y E15

**Ejecución:** 2026-10-26, hora de Guatemala (UTC−6). **Base:** `SupermercadosGuate_PRUEBAS`.

Se ejecutó cada bloque por separado mediante ADO.NET. Antes de cada ejecución se comprobó `DB_NAME() = 'SupermercadosGuate_PRUEBAS'`; se aisló el bloque E14 o E15 sin incluir las instrucciones `USE` del resto del archivo. La variable `@Ejecutar14` o `@Ejecutar15` se habilitó solo en el texto enviado para esa ejecución. Las guardas del propio SQL permanecieron activas. No se ejecutó DML en `SupermercadosGuate`.

| Ejercicio | Evidencia NO (ROLLBACK) | Evidencia SI (COMMIT y restauración/limpieza) | Comprobación final | Resultado |
|---|---|---|---|---|
| I1-E14 | Relación producto–proveedor `1`: costo Q3.00 → Q4.00 → Q3.00. | La misma relación: Q3.00 → Q4.00 → Q3.00. | `SELECT` independiente: costo Q3.00. | **APROBADO** |
| I1-E15 | Compra `I1-E15-3056D4F7E68445568C069E`: 1 detalle y Q6.00 temporalmente; después 0 compra y 0 detalles. | Compra `I1-E15-E7F1AC0CB0BB416AB825B7`: etapa **Confirmada** con 1 detalle y Q6.00; después 0 compra y 0 detalles. | `SELECT` independiente: ambas compras y detalles ausentes; existencia 150.00 antes y después. | **APROBADO** |
| I2-E14 | Empleado `1`: salario Q6,500.00 → Q6,600.00 → Q6,500.00. | El mismo empleado: Q6,500.00 → Q6,600.00 → Q6,500.00. | `SELECT` independiente: salario Q6,500.00. | **APROBADO** |
| I2-E15 | Cliente NIT `I2E15638B1CE7EB1542`, venta `I2-E15-3243CE0A052A4894844FA9`: ambos existieron temporalmente y terminaron ausentes. | Cliente NIT `I2E1548F48CBA74FE4B`, venta `I2-E15-9287DE7FEC6C4195A2A587`: ambos aparecieron en etapa **Confirmada** y terminaron ausentes. | `SELECT` independiente: 0 clientes y 0 ventas con esos identificadores. | **APROBADO** |
| I3-E14 | Inventario `1`: mínimo 30.00 → 32.00 → 30.00; existencia 150.00. | El mismo inventario: mínimo 30.00 → 32.00 → 30.00; existencia 150.00. | `SELECT` independiente: mínimo 30.00 y existencia 150.00. | **APROBADO** |
| I3-E15 | Ajuste `I3-E15-E164A13A540348E8A8B38F`: 1 detalle temporal; después 0 ajustes y 0 detalles. | Ajuste `I3-E15-7F01845DDB7B4090BFA76E`: etapa **Confirmada** con 1 detalle; después 0 ajustes y 0 detalles. | `SELECT` independiente: ambos ajustes y detalles ausentes; existencia 150.00 antes y después. | **APROBADO** |

Todos los ejercicios devolvieron `@@TRANCOUNT = 0`. Antes y después de cada ejecución, el inventario completo mantuvo 65 filas, suma de existencias **5383.00**, suma de mínimos **1234.00** y suma de costos promedio **802.80**. Al finalizar, los conteos de las tablas relevantes coincidieron entre la base principal y PRUEBAS: empleados 12, producto–proveedor 16, inventario 65, clientes 15, compras 9, ventas 26 y ajustes 8.

Las rutas normales de NO y SI quedaron probadas. No se provocó un fallo de limpieza para ejercitar su recuperación automática. Las inserciones pueden dejar saltos en `IDENTITY` aunque las filas de práctica se reviertan o eliminen; todas estas inserciones se ejecutaron exclusivamente en la copia de pruebas.
