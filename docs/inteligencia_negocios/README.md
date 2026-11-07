# Inteligencia de Negocios · borradores

Nueve diseños SVG y nueve matrices de trazabilidad, uno por módulo M01–M09, organizados por integrante. Todos requieren revisión académica; no había nueve maquetas aprobadas en `docs/prototipos/`. Los tres documentos individuales incluyen la misma imagen de 250 × 251 píxeles, pero no se halló una colección de nueve prototipos. M01–M04 y M06–M09 tienen implementación MVC local. M05 muestra sus ocho visuales como pendientes por falta de historial suficiente; M07-E03 y M08-E07 también indican sus limitaciones en lugar de inventar cifras. Los documentos oficiales completos están en `docs/documentos-oficiales/`.

| Integrante | Módulos | Rol autorizado | Estado |
|---|---|---|---|
| 1 | M01–M03 | Administrador y Caja | Implementación local; revisión visual en navegador realizada |
| 2 | M04–M06 | Administrador y Bodega | M04 y M06 implementados y revisados visualmente; M05 visible con ocho posiciones pendientes; diseños en borrador |
| 3 | M07–M08 | Administrador y Bodega | Implementación local y revisión visual realizada; E03 de M07 y E07 de M08 pendientes; diseños en borrador |
| 3 | M09 | Solo Administrador | Implementación local y revisión visual realizada; diseño en borrador |

M05 no puede reconstruir por fecha todos los movimientos: la venta Confirmada no guarda fecha de confirmación, los ajustes Aplicados actuales tienen FechaAplicacion NULL y cinco detalles Aplicados carecen de costo histórico. No se sustituyen esas fechas por la fecha del documento. M07-E03 necesita decidir cómo representar «pendientes» porque el esquema solo distingue Borrador, Recibida y Anulada. M08-E07 debe evitar comparar monedas sin tipo de cambio. Estas decisiones constan en sus matrices; no se inventaron cifras.

## Preparación final del 7 de noviembre de 2026

Se comprobaron de nuevo los nueve servicios sin filtro y con sucursal, las cuatro posiciones KPI y cuatro paneles por módulo y los permisos de los tres roles. Los 31 KPI calculables coincidieron con ocho SELECT independientes. Pasaron 90 comprobaciones HTTP en una instancia separada con Bodega y Caja, incluidos filtros, rutas prohibidas y reportes/CSV. El inicio y las consultas de Administrador se comprobaron directamente; su login HTTP conserva la evidencia previa. No se repitieron capturas ni pruebas visuales.

M05 conserva **8 de 8 visuales pendientes**; M07 conserva **E03 pendiente** y M08 conserva **E07 pendiente**. Sus ceros internos son marcadores del modelo: la interfaz muestra «Pendiente» y su motivo, sin presentarlos como mediciones. Para cerrar estos puntos hace falta una definición/fuente académica autorizada; no basta rellenar datos históricos o asumir una moneda. E04 de M08 mide días reales y E08 usa plazos estimados, identificados en la interfaz.

La compilación local pasó y hay un candidato Release separado. Las matrices/SVG siguen en BORRADOR hasta aprobación académica. La página pública no se actualizó; véanse [entrega y guía 10](../ENTREGA_20261107.md). No se ejecutó DML ni se modificó Power BI.
