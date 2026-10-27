# Resumen de validación manual

**Fecha de registro:** 2026-10-25  
**Responsable de las pruebas descritas:** usuario del proyecto  
**Medios indicados:** navegador y SQL Server Management Studio (SSMS)

Estas son verificaciones manuales realizadas y reportadas por el usuario. Codex no realizó las operaciones de escritura ni ejecutó DML para esta validación.

## Resultados reportados

- **Empleados:** el registro 1003 se insertó, editó y posteriormente eliminó. Los cambios se verificaron mediante SELECT en SSMS.
- **Venta:** la venta 1002 se creó inicialmente como Borrador sin detalles; después se agregaron dos productos por un total de **Q30.00**.
- **Inventario:** tras confirmar la venta, Arroz cambió de **85 a 83** y Galletas de **80 a 79** unidades, verificado por el usuario en SSMS.
- **Pagos:** se registraron los pagos 1002 por **Q10.00** y 1003 por **Q20.00**.
- **Saldo de la venta:** total **Q30.00**, pagado **Q30.00**, saldo **Q0.00**.
- **Dashboards:** se ejecutaron seis consultas SELECT en SSMS, una de KPI y una de gráfico por cada página (Administrador, Bodega y Caja). El usuario reportó que los resultados coincidieron con los indicadores y gráficos esperados.

## Casos todavía no probados

- Rechazo de pagos que exceden el saldo.
- Rechazo de comprobantes duplicados.
- Concurrencia entre operaciones simultáneas.
- ROLLBACK comprobado ante una falla dentro de la transacción.
- Errores forzados de SQL Server o de conexión durante las operaciones.

La existencia de validaciones en el código no sustituye la prueba de estos escenarios. Este resumen no afirma que los casos pendientes hayan sido ejecutados.