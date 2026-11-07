# Validación inicial de Inteligencia de Negocios M01–M03

Fecha: 2026-11-07. Estado: implementación local; diseños de los nueve módulos pendientes de revisión. La aplicación pública de ngrok no se actualizó.

## Pruebas ejecutadas

| Comprobación | Resultado |
|---|---|
| Compilación Debug en `bin/BiCheck` y Release en `bin/BiReleaseCheck` | Ambas: 0 errores, 0 advertencias |
| Sintaxis de `business-intelligence.js` | `node --check` sin errores |
| Inicio de sesión de CajaDemo en instancia aislada `127.0.0.1:5201` | Correcto, sin imprimir contraseña |
| GET M01, M02 y M03 con CajaDemo | HTTP 200; 4 KPI y 4 gráficas HTML en cada módulo; sin mensaje de error SQL |
| Datos de las 12 gráficas en el período inicial | Ninguna mostró «Sin datos»; M01: 1 dona, 1 línea, 2 barras; M02: 1 dona, 1 línea, 2 barras; M03: 2 donas, 1 columna, 1 barra |
| Filtros de M01 (sucursal, categoría, producto), M02 (departamento, municipio) y M03 (estado) | HTTP 200 y cuatro KPI por módulo |
| Fechas invertidas | Mensaje de validación visible en HTML |
| Acceso de BodegaDemo a M01, M02 y M03 | HTTP 403 en las tres rutas |
| Rutas previas de Bodega `/Inicio/Bodega`, `/compras`, `/inventario` | HTTP 200 |
| Visitante sin sesión en M01 | HTTP 302 hacia `/acceso` |
| CSS y JavaScript nuevos | HTTP 200 |
| SELECT independiente M01, período por defecto | 17 ventas, Q 4,805.00 netos, 229 unidades; la página mostró los mismos valores |
| SELECT independiente M02, período por defecto | 9 compradores, 4 recurrentes, 17 documentos; la página mostró los mismos valores y promedio 1.89 |
| SELECT independiente M03, período por defecto | 28 pagos Aplicados, Q 4,290.00 de monto y Q 40.50 de comisiones; la página mostró los mismos valores |

El primer intento de cargar M02/M03 devolvió SQL 209 por columnas duplicadas en las listas de filtros; se corrigieron los alias y se repitieron las tres peticiones satisfactoriamente. La primera vista previa aislada sufrió un error local de DPAPI del sandbox; la segunda se inició fuera del sandbox, en el mismo equipo y puerto 5201, sin tocar Production.

## Pendiente de revisión

- Inspección visual autenticada en navegador a tamaños de computadora, tableta y teléfono; comprobar menús, filtros cruzados, impresión y autoactualización desde la interfaz.
- Confirmación manual del diseño y matriz de los nueve módulos. M04–M09 no se implementaron.
- Contrastar cada una de las doce gráficas con SELECT independientes y validar datos vacíos en todos los filtros.
- Probar HTTP de Administrador en M01–M03; la regla de autorización está revisada en código, mientras que Caja y Bodega sí se probaron mediante HTTP.

No se ejecutó INSERT, UPDATE, DELETE, migración ni script SQL. Se realizaron SELECT de solo lectura y POST de inicio de sesión para la prueba autenticada.
