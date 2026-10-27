/*
  SUPERMERCADOS GUATE — FASE III — INTEGRANTE 1
  Ejercicios I1-F3-E01 a I1-F3-E15.
  Motor: Microsoft SQL Server; base: SupermercadosGuate.
  E01-E13: consultas de lectura. E14-E15: prácticas DML controladas.
  IMPORTANTE: no ejecutar E14-E15 en la base de trabajo sin respaldo,
  autorización y revisión de los datos elegidos. Decisión inicial NO.
  Los bloques GO permiten ejecutar un ejercicio por separado en SSMS.
*/
USE SupermercadosGuate;
GO

-- I1-F3-E01: proveedores activos, días de crédito y antigüedad completa.
DECLARE @Referencia01 date = '20260930';
SELECT pr.CodigoProveedor,
       UPPER(pr.Nombre) AS Proveedor,
       pr.Departamento,
       pr.DiasCredito,
       ISNULL(pr.Correo, 'No registrado') AS Correo,
       DATEDIFF(YEAR, pr.FechaRegistro, @Referencia01)
         - CASE WHEN DATEADD(YEAR, DATEDIFF(YEAR, pr.FechaRegistro, @Referencia01), pr.FechaRegistro) > @Referencia01 THEN 1 ELSE 0 END AS AntiguedadAnios
FROM dbo.Tbl_Proveedores AS pr
WHERE pr.Estado = 'Activo'
  AND pr.DiasCredito BETWEEN 15 AND 60
  AND (pr.Nombre LIKE '%Guatemala%' OR pr.Nombre LIKE '%GT%')
ORDER BY AntiguedadAnios DESC, Proveedor;
GO

-- I1-F3-E02: compras recibidas entre julio y septiembre, con atraso.
SELECT c.CodigoCompra, c.NumeroCompra,
       YEAR(c.FechaCompra) AS Anio, MONTH(c.FechaCompra) AS Mes,
       c.FechaEntregaProgramada, c.FechaRecepcion,
       DATEDIFF(DAY, c.FechaEntregaProgramada, c.FechaRecepcion) AS DiasAtraso,
       LEN(c.NumeroCompra) AS LongitudNumero
FROM dbo.Tbl_Compras AS c
WHERE c.Estado = 'Recibida'
  AND c.FechaCompra >= '20260701' AND c.FechaCompra < '20261001'
  AND c.FechaRecepcion > c.FechaEntregaProgramada
ORDER BY DiasAtraso DESC, c.NumeroCompra;
GO

-- I1-F3-E03: cada compra recibida de septiembre, sin duplicar cabeceras.
WITH Totales AS (
    SELECT CodigoCompra,
           SUM(Cantidad * CostoUnitario - Descuento) AS ImporteCompra
    FROM dbo.Tbl_ComprasDetalle
    GROUP BY CodigoCompra
)
SELECT c.CodigoCompra, c.NumeroCompra, c.FechaCompra,
       pr.Nombre AS Proveedor, s.Nombre AS Sucursal,
       e.Nombres + ' ' + e.Apellidos AS Empleado,
       CAST(ROUND(t.ImporteCompra, 2) AS decimal(18,2)) AS ImporteCompra
FROM dbo.Tbl_Compras AS c
JOIN Totales AS t ON t.CodigoCompra = c.CodigoCompra
JOIN dbo.Tbl_Proveedores AS pr ON pr.CodigoProveedor = c.CodigoProveedor
JOIN dbo.Tbl_Sucursales AS s ON s.CodigoSucursal = c.CodigoSucursal
JOIN dbo.Tbl_Empleados AS e ON e.CodigoEmpleado = c.CodigoEmpleado
WHERE c.Estado = 'Recibida'
  AND c.FechaCompra >= '20260901' AND c.FechaCompra < '20261001'
ORDER BY ImporteCompra DESC, c.CodigoCompra;
GO

-- I1-F3-E04: todos los proveedores activos, aunque no hayan recibido compras.
WITH Compras2026 AS (
    SELECT c.CodigoProveedor, c.NumeroCompra, c.FechaCompra,
           CAST(ROUND(SUM(d.Cantidad * d.CostoUnitario - d.Descuento), 2) AS decimal(18,2)) AS ImporteCompra
    FROM dbo.Tbl_Compras AS c
    JOIN dbo.Tbl_ComprasDetalle AS d ON d.CodigoCompra = c.CodigoCompra
    WHERE c.Estado = 'Recibida'
      AND c.FechaCompra >= '20260101' AND c.FechaCompra < '20270101'
    GROUP BY c.CodigoCompra, c.CodigoProveedor, c.NumeroCompra, c.FechaCompra
)
SELECT pr.CodigoProveedor, pr.Nombre AS Proveedor,
       co.NumeroCompra, co.FechaCompra, co.ImporteCompra
FROM dbo.Tbl_Proveedores AS pr
LEFT JOIN Compras2026 AS co ON co.CodigoProveedor = pr.CodigoProveedor
WHERE pr.Estado = 'Activo'
ORDER BY Proveedor, co.FechaCompra, co.NumeroCompra;
GO

-- I1-F3-E05: activos cuyo costo de algún proveedor activo supera el promedio.
SELECT p.CodigoProducto, p.Nombre AS Producto, p.Marca,
       CAST(p.PrecioVenta AS decimal(18,2)) AS PrecioVenta
FROM dbo.Tbl_Productos AS p
WHERE p.Estado = 'Activo'
  AND EXISTS (
      SELECT 1
      FROM dbo.Tbl_ProductoProveedor AS pp
      WHERE pp.CodigoProducto = p.CodigoProducto
        AND pp.Estado = 'Activo'
        AND pp.CodigoProveedor IN (
            SELECT CodigoProveedor FROM dbo.Tbl_Proveedores WHERE Estado = 'Activo'
        )
        AND pp.CostoReferencia > (
            SELECT AVG(CAST(CostoReferencia AS decimal(18,4)))
            FROM dbo.Tbl_ProductoProveedor
            WHERE Estado = 'Activo'
        )
  )
ORDER BY Producto, p.CodigoProducto;
GO

-- I1-F3-E06: proveedores con artículos debajo del mínimo (sin JOIN principal).
SELECT pr.CodigoProveedor, pr.Nombre AS Proveedor, pr.Contacto,
       pr.Telefono, pr.Departamento
FROM dbo.Tbl_Proveedores AS pr
WHERE pr.Estado = 'Activo'
  AND EXISTS (
      SELECT 1
      FROM dbo.Tbl_ProductoProveedor AS pp
      WHERE pp.CodigoProveedor = pr.CodigoProveedor
        AND pp.Estado = 'Activo'
        AND pp.CodigoProducto IN (
            SELECT i.CodigoProducto FROM dbo.Tbl_Inventario AS i
            WHERE i.Existencia < i.ExistenciaMinima
        )
  )
ORDER BY pr.Departamento, Proveedor;
GO

-- I1-F3-E07: compras recibidas por proveedor; promedio por documento.
WITH ComprasDocumento AS (
    SELECT c.CodigoCompra, c.CodigoProveedor,
           SUM(d.Cantidad * d.CostoUnitario - d.Descuento) AS ImporteCompra
    FROM dbo.Tbl_Compras AS c
    JOIN dbo.Tbl_ComprasDetalle AS d ON d.CodigoCompra = c.CodigoCompra
    WHERE c.Estado = 'Recibida'
      AND c.FechaCompra >= '20260101' AND c.FechaCompra < '20261001'
    GROUP BY c.CodigoCompra, c.CodigoProveedor
)
SELECT pr.CodigoProveedor, pr.Nombre AS Proveedor,
       COUNT(*) AS CantidadCompras,
       CAST(ROUND(SUM(cd.ImporteCompra), 2) AS decimal(18,2)) AS TotalComprado,
       CAST(ROUND(AVG(CAST(cd.ImporteCompra AS decimal(18,4))), 2) AS decimal(18,2)) AS PromedioCompra
FROM ComprasDocumento AS cd
JOIN dbo.Tbl_Proveedores AS pr ON pr.CodigoProveedor = cd.CodigoProveedor
GROUP BY pr.CodigoProveedor, pr.Nombre
HAVING COUNT(*) >= 2 AND SUM(cd.ImporteCompra) > 1000
ORDER BY TotalComprado DESC, Proveedor;
GO

-- I1-F3-E08: costo neto y unidades recibidas por categoría y mes.
SELECT p.CodigoCategoria, ca.Nombre AS Categoria,
       YEAR(c.FechaCompra) AS Anio, MONTH(c.FechaCompra) AS Mes,
       COUNT(*) AS CantidadLineas,
       SUM(d.Cantidad) AS UnidadesCompradas,
       CAST(ROUND(SUM(d.Cantidad * d.CostoUnitario - d.Descuento), 2) AS decimal(18,2)) AS CostoNeto
FROM dbo.Tbl_Compras AS c
JOIN dbo.Tbl_ComprasDetalle AS d ON d.CodigoCompra = c.CodigoCompra
JOIN dbo.Tbl_Productos AS p ON p.CodigoProducto = d.CodigoProducto
JOIN dbo.Tbl_Categorias AS ca ON ca.CodigoCategoria = p.CodigoCategoria
WHERE c.Estado = 'Recibida' AND c.FechaCompra >= '20260101' AND c.FechaCompra < '20270101'
GROUP BY p.CodigoCategoria, ca.Nombre, YEAR(c.FechaCompra), MONTH(c.FechaCompra)
HAVING COUNT(*) >= 2
   AND SUM(d.Cantidad * d.CostoUnitario - d.Descuento) > 500
ORDER BY Anio, Mes, Categoria;
GO

-- I1-F3-E09: bitácora septiembre; requiere las migraciones de CostoUnitario y FechaAplicacion.
-- Ajustes antiguos sin FechaAplicacion no se atribuyen a septiembre; no se inventan fechas.
WITH Movimientos AS (
    SELECT c.FechaRecepcion AS Fecha, c.NumeroCompra AS Documento,
           CAST('Compra recibida' AS varchar(20)) AS TipoEvento,
           c.CodigoSucursal,
           CAST(ROUND(SUM(d.Cantidad * d.CostoUnitario - d.Descuento),2) AS decimal(18,2)) AS Monto,
           c.Estado
    FROM dbo.Tbl_Compras AS c
    JOIN dbo.Tbl_ComprasDetalle AS d ON d.CodigoCompra = c.CodigoCompra
    WHERE c.Estado = 'Recibida' AND c.FechaRecepcion >= '20260901' AND c.FechaRecepcion < '20261001'
    GROUP BY c.CodigoCompra, c.FechaRecepcion, c.NumeroCompra, c.CodigoSucursal, c.Estado
    UNION ALL
    SELECT a.FechaAplicacion, a.NumeroAjuste, CAST('Ajuste aplicado' AS varchar(20)),
           a.CodigoSucursal,
           CASE WHEN COUNT(d.CostoUnitario) = COUNT(*)
                THEN CAST(ROUND(SUM(d.Cantidad * d.CostoUnitario), 2) AS decimal(18,2))
                ELSE NULL END,
           a.Estado
    FROM dbo.Tbl_AjustesInventario AS a
    JOIN dbo.Tbl_AjustesInventarioDetalle AS d ON d.CodigoAjuste = a.CodigoAjuste
    WHERE a.Estado = 'Aplicado' AND a.FechaAplicacion >= '20260901' AND a.FechaAplicacion < '20261001'
    GROUP BY a.CodigoAjuste, a.FechaAplicacion, a.NumeroAjuste, a.CodigoSucursal, a.Estado
)
SELECT Fecha, Documento, TipoEvento, CodigoSucursal, Monto, Estado
FROM Movimientos
ORDER BY Fecha, TipoEvento, Documento;
GO

-- I1-F3-E10: proveedores que aparecen en ambas etapas (INTERSECT real).
WITH Comunes AS (
    SELECT CodigoProveedor FROM dbo.Tbl_Compras
    WHERE Estado = 'Recibida' AND FechaCompra >= '20260101' AND FechaCompra < '20260701'
    INTERSECT
    SELECT CodigoProveedor FROM dbo.Tbl_Compras
    WHERE Estado = 'Recibida' AND FechaCompra >= '20260701' AND FechaCompra < '20261001'
)
SELECT p.CodigoProveedor, p.Nombre AS Proveedor
FROM Comunes AS c
JOIN dbo.Tbl_Proveedores AS p ON p.CodigoProveedor = c.CodigoProveedor
ORDER BY Proveedor;
GO

-- I1-F3-E11: proveedores antiguos exclusivos, y activos con productos sin compras recibidas.
WITH CompraronAntes AS (
    SELECT CodigoProveedor FROM dbo.Tbl_Compras
    WHERE Estado = 'Recibida' AND FechaCompra >= '20260101' AND FechaCompra < '20260701'
    EXCEPT
    SELECT CodigoProveedor FROM dbo.Tbl_Compras
    WHERE Estado = 'Recibida' AND FechaCompra >= '20260701' AND FechaCompra < '20261001'
),
SinCompras2026 AS (
    SELECT p.CodigoProveedor
    FROM dbo.Tbl_Proveedores AS p
    WHERE p.Estado = 'Activo'
      AND EXISTS (SELECT 1 FROM dbo.Tbl_ProductoProveedor pp
                  WHERE pp.CodigoProveedor = p.CodigoProveedor AND pp.Estado = 'Activo')
      AND NOT EXISTS (SELECT 1 FROM dbo.Tbl_Compras c
                      WHERE c.CodigoProveedor = p.CodigoProveedor
                        AND c.Estado = 'Recibida'
                        AND c.FechaCompra >= '20260101' AND c.FechaCompra < '20270101')
),
Unicos AS (
    SELECT CodigoProveedor FROM CompraronAntes
    UNION
    SELECT CodigoProveedor FROM SinCompras2026
)
SELECT p.CodigoProveedor, p.Nombre AS Proveedor
FROM Unicos AS u
JOIN dbo.Tbl_Proveedores AS p ON p.CodigoProveedor = u.CodigoProveedor
ORDER BY Proveedor;
GO

-- I1-F3-E12: clasificar atrasos de compras recibidas (variables).
DECLARE @LeveMax12 int = 2, @MedioMax12 int = 5;
SELECT c.CodigoCompra, c.NumeroCompra, c.CodigoSucursal,
       DATEDIFF(DAY, c.FechaEntregaProgramada, c.FechaRecepcion) AS DiasAtraso,
       CASE WHEN c.FechaRecepcion <= c.FechaEntregaProgramada THEN 'A tiempo'
            WHEN DATEDIFF(DAY, c.FechaEntregaProgramada, c.FechaRecepcion) <= @LeveMax12 THEN 'Leve'
            WHEN DATEDIFF(DAY, c.FechaEntregaProgramada, c.FechaRecepcion) <= @MedioMax12 THEN 'Medio'
            ELSE 'Alto' END AS NivelAtraso
FROM dbo.Tbl_Compras AS c
WHERE c.Estado = 'Recibida' AND c.FechaRecepcion IS NOT NULL
ORDER BY DiasAtraso DESC, c.NumeroCompra;
GO

-- I1-F3-E13: evaluación condicional de presupuesto por sucursal.
DECLARE @Sucursal13 int = (SELECT MIN(CodigoSucursal) FROM dbo.Tbl_Sucursales WHERE Estado = 'Activo');
DECLARE @Presupuesto13 decimal(18,2) = 10000.00, @Total13 decimal(18,2);
SELECT @Total13 = COALESCE(SUM(x.Importe), 0)
FROM (
    SELECT c.CodigoCompra, SUM(d.Cantidad * d.CostoUnitario - d.Descuento) AS Importe
    FROM dbo.Tbl_Compras AS c
    JOIN dbo.Tbl_ComprasDetalle AS d ON d.CodigoCompra = c.CodigoCompra
    WHERE c.CodigoSucursal = @Sucursal13 AND c.Estado = 'Recibida'
      AND c.FechaCompra >= '20260901' AND c.FechaCompra < '20261001'
    GROUP BY c.CodigoCompra
) AS x;
IF @Sucursal13 IS NULL
    PRINT 'Sin sucursales activas para evaluar.';
ELSE IF @Total13 > @Presupuesto13
    SELECT @Sucursal13 AS CodigoSucursal, @Presupuesto13 AS Presupuesto,
           @Total13 AS TotalComprado, CAST(@Presupuesto13 - @Total13 AS decimal(18,2)) AS Diferencia,
           CAST('Excedido' AS varchar(25)) AS EstadoPresupuesto;
ELSE
    SELECT @Sucursal13 AS CodigoSucursal, @Presupuesto13 AS Presupuesto,
           @Total13 AS TotalComprado, CAST(@Presupuesto13 - @Total13 AS decimal(18,2)) AS Diferencia,
           CAST('Dentro del presupuesto' AS varchar(25)) AS EstadoPresupuesto;
-- Para probar ambas ramas: usar una sucursal/escenario de prueba con datos suficientes.
GO

/* I1-F3-E14: ejecutar solo este bloque en SupermercadosGuate_PRUEBAS.
   Probar NO y luego SI; ninguna prueba deja costo alterado.
*/
SET XACT_ABORT ON;
DECLARE @Ejecutar14 varchar(2) = 'NO'; -- Cambiar a SI solo en PRUEBAS.
DECLARE @Paso14 tinyint = 1, @Decision14 varchar(2),
        @Relacion14 int, @CostoOriginal14 decimal(12,2);
DECLARE @Prueba14 table (
    Decision varchar(2), Etapa varchar(20), CodigoProductoProveedor int,
    CodigoProveedor int, CodigoProducto int, CostoReferencia decimal(12,2)
);
IF @Ejecutar14 = 'SI'
BEGIN
IF DB_NAME() <> N'SupermercadosGuate_PRUEBAS'
    THROW 51016, 'E14: ejecutar solo en SupermercadosGuate_PRUEBAS.', 1;
WHILE @Paso14 <= 2
BEGIN
    SET @Decision14 = CASE WHEN @Paso14 = 1 THEN 'NO' ELSE 'SI' END;
    SET @Relacion14 = NULL;
    BEGIN TRY
        BEGIN TRANSACTION;
        SELECT TOP (1) @Relacion14 = CodigoProductoProveedor,
                       @CostoOriginal14 = CostoReferencia
        FROM dbo.Tbl_ProductoProveedor WITH (UPDLOCK, HOLDLOCK)
        WHERE Estado = 'Activo' AND CostoReferencia > 0
          AND CostoReferencia <= 9999999998.99
        ORDER BY CodigoProductoProveedor;
        IF @Relacion14 IS NULL
        BEGIN
            PRINT 'E14: no existe relacion activa con costo positivo apto para la prueba.';
            ROLLBACK TRANSACTION;
            BREAK;
        END;
        INSERT INTO @Prueba14
        SELECT @Decision14, 'Antes', CodigoProductoProveedor, CodigoProveedor,
               CodigoProducto, CostoReferencia
        FROM dbo.Tbl_ProductoProveedor WHERE CodigoProductoProveedor = @Relacion14;

        SAVE TRANSACTION Cambio14;
        BEGIN TRANSACTION;
        UPDATE dbo.Tbl_ProductoProveedor
        SET CostoReferencia = CostoReferencia + 1.00
        WHERE CodigoProductoProveedor = @Relacion14 AND Estado = 'Activo'
          AND CostoReferencia = @CostoOriginal14;
        IF @@ROWCOUNT <> 1 THROW 51001, 'E14: la relacion cambio durante la prueba.', 1;
        INSERT INTO @Prueba14
        SELECT @Decision14, 'Temporal', CodigoProductoProveedor, CodigoProveedor,
               CodigoProducto, CostoReferencia
        FROM dbo.Tbl_ProductoProveedor WHERE CodigoProductoProveedor = @Relacion14;

        IF @Decision14 = 'NO'
            ROLLBACK TRANSACTION Cambio14;
        COMMIT TRANSACTION; -- cierra la transaccion interna de la prueba
        IF @Decision14 = 'SI'
        BEGIN
            -- Compensacion controlada antes del COMMIT protector.
            UPDATE dbo.Tbl_ProductoProveedor
            SET CostoReferencia = @CostoOriginal14
            WHERE CodigoProductoProveedor = @Relacion14 AND Estado = 'Activo'
              AND CostoReferencia = @CostoOriginal14 + 1.00;
            IF @@ROWCOUNT <> 1 THROW 51002, 'E14: no fue posible restaurar el costo.', 1;
        END;
        INSERT INTO @Prueba14
        SELECT @Decision14, 'Final', CodigoProductoProveedor, CodigoProveedor,
               CodigoProducto, CostoReferencia
        FROM dbo.Tbl_ProductoProveedor WHERE CodigoProductoProveedor = @Relacion14;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
    SET @Paso14 += 1;
END;
END
ELSE PRINT 'E14 omitido: requiere habilitacion explicita de la practica.';
SELECT Decision, Etapa, CodigoProductoProveedor, CodigoProveedor,
       CodigoProducto, CostoReferencia
FROM @Prueba14
ORDER BY CASE Decision WHEN 'NO' THEN 1 ELSE 2 END,
         CASE Etapa WHEN 'Antes' THEN 1 WHEN 'Temporal' THEN 2 ELSE 3 END;
GO

/* I1-F3-E15: ejecutar solo este bloque en una copia aislada de la base.
   NO revierte la insercion; SI la confirma y luego limpia en otra transaccion.
   Si la limpieza falla, se revierte y se reintenta solo con las llaves creadas.
*/
SET XACT_ABORT ON;
DECLARE @Ejecutar15 varchar(2) = 'NO'; -- Cambiar a SI solo en una copia aislada.
DECLARE @Paso15 tinyint = 1, @Decision15 varchar(2), @Numero15 varchar(30),
        @Sucursal15 int, @Empleado15 int, @Proveedor15 int, @Producto15 int,
        @Costo15 decimal(12,2), @Compra15 int, @Detalle15 int,
        @ExistenciaAntes15 decimal(12,2), @ExistenciaDespues15 decimal(12,2),
        @LineasTemporal15 int, @ImporteTemporal15 decimal(18,2),
        @Confirmada15 bit, @Limpia15 bit;
DECLARE @Evidencia15 table (
    Decision varchar(2), Etapa varchar(25), NumeroCompra varchar(30),
    ExisteCompra int, CantidadLineas int, ImporteCompra decimal(18,2),
    ExistenciaAntes decimal(12,2), ExistenciaDespues decimal(12,2)
);
IF @Ejecutar15 = 'SI'
BEGIN
IF DB_NAME() <> N'SupermercadosGuate_PRUEBAS'
    THROW 51010, 'E15: ejecutar solo en SupermercadosGuate_PRUEBAS.', 1;
IF @@TRANCOUNT <> 0
    THROW 51011, 'E15: ejecutar fuera de cualquier transaccion externa.', 1;
WHILE @Paso15 <= 2
BEGIN
    SET @Decision15 = CASE WHEN @Paso15 = 1 THEN 'NO' ELSE 'SI' END;
    SET @Numero15 = 'I1-E15-' + LEFT(REPLACE(CONVERT(varchar(36), NEWID()), '-', ''), 22);
    SET @Empleado15 = NULL;
    SET @Compra15 = NULL;
    SET @Detalle15 = NULL;
    SET @Confirmada15 = 0;
    SET @Limpia15 = 0;
    BEGIN TRY
        SELECT TOP (1) @Sucursal15 = e.CodigoSucursal, @Empleado15 = e.CodigoEmpleado,
               @Proveedor15 = pp.CodigoProveedor, @Producto15 = pp.CodigoProducto,
               @Costo15 = pp.CostoReferencia, @ExistenciaAntes15 = i.Existencia
        FROM dbo.Tbl_Empleados AS e
        JOIN dbo.Tbl_Sucursales AS s ON s.CodigoSucursal = e.CodigoSucursal AND s.Estado = 'Activo'
        CROSS JOIN dbo.Tbl_ProductoProveedor AS pp
        JOIN dbo.Tbl_Productos AS p ON p.CodigoProducto = pp.CodigoProducto AND p.Estado = 'Activo'
        JOIN dbo.Tbl_Proveedores AS pr ON pr.CodigoProveedor = pp.CodigoProveedor AND pr.Estado = 'Activo'
        JOIN dbo.Tbl_Inventario AS i WITH (UPDLOCK, HOLDLOCK)
          ON i.CodigoSucursal = e.CodigoSucursal AND i.CodigoProducto = pp.CodigoProducto
        WHERE e.Estado = 'Activo' AND pp.Estado = 'Activo' AND pp.CostoReferencia > 0
        ORDER BY e.CodigoEmpleado, pp.CodigoProductoProveedor;
        IF @Empleado15 IS NULL
        BEGIN
            PRINT 'E15: faltan empleado, proveedor, producto o inventario validos.';
            BREAK;
        END;
        INSERT INTO @Evidencia15
        VALUES (@Decision15, 'Antes', @Numero15, 0, 0, 0,
                @ExistenciaAntes15, @ExistenciaAntes15);

        BEGIN TRANSACTION;
        IF EXISTS (SELECT 1 FROM dbo.Tbl_Compras WITH (UPDLOCK, HOLDLOCK)
                   WHERE NumeroCompra = @Numero15)
            THROW 51003, 'E15: el numero de practica ya existe.', 1;
        INSERT dbo.Tbl_Compras
            (CodigoSucursal, CodigoProveedor, CodigoEmpleado, NumeroCompra,
             FechaCompra, FechaEntregaProgramada, FechaRecepcion, Estado, Observaciones)
        VALUES (@Sucursal15, @Proveedor15, @Empleado15, @Numero15,
                '20260930', '20260930', NULL, 'Borrador', 'Practica Fase III I1-E15');
        SET @Compra15 = CONVERT(int, SCOPE_IDENTITY());
        INSERT dbo.Tbl_ComprasDetalle
            (CodigoCompra, CodigoProducto, Cantidad, CostoUnitario, Descuento, Observaciones)
        VALUES (@Compra15, @Producto15, 2, @Costo15, 0, 'Practica Fase III');
        SET @Detalle15 = CONVERT(int, SCOPE_IDENTITY());
        SELECT @LineasTemporal15 = COUNT(*),
               @ImporteTemporal15 = CAST(ROUND(COALESCE(SUM(d.Cantidad * d.CostoUnitario - d.Descuento), 0), 2) AS decimal(18,2))
        FROM dbo.Tbl_ComprasDetalle AS d WHERE d.CodigoCompra = @Compra15;
        IF @Decision15 = 'NO'
            ROLLBACK TRANSACTION;
        ELSE
        BEGIN
            SET @Confirmada15 = 1;
            COMMIT TRANSACTION; -- la compra y el detalle ya son persistentes
        END;
        INSERT INTO @Evidencia15
        VALUES (@Decision15, 'Temporal', @Numero15, 1, @LineasTemporal15,
                @ImporteTemporal15, @ExistenciaAntes15, @ExistenciaAntes15);
        IF @Decision15 = 'SI'
        BEGIN
            IF @@TRANCOUNT <> 0 OR
               (SELECT COUNT(*) FROM dbo.Tbl_Compras
                WHERE CodigoCompra = @Compra15 AND NumeroCompra = @Numero15) <> 1 OR
               (SELECT COUNT(*) FROM dbo.Tbl_ComprasDetalle
                WHERE CodigoDetalleCompra = @Detalle15 AND CodigoCompra = @Compra15) <> 1
                THROW 51012, 'E15: la compra no quedo confirmada con su detalle.', 1;
            INSERT INTO @Evidencia15
            VALUES ('SI', 'Confirmada', @Numero15, 1, 1, @ImporteTemporal15,
                    @ExistenciaAntes15, @ExistenciaAntes15);
            BEGIN TRANSACTION; -- limpieza independiente del COMMIT anterior
            DELETE FROM dbo.Tbl_ComprasDetalle
            WHERE CodigoDetalleCompra = @Detalle15 AND CodigoCompra = @Compra15
              AND CodigoProducto = @Producto15 AND Cantidad = 2
              AND CostoUnitario = @Costo15 AND Descuento = 0
              AND Observaciones = 'Practica Fase III';
            IF @@ROWCOUNT <> 1 THROW 51004, 'E15: detalle de practica no encontrado.', 1;
            DELETE FROM dbo.Tbl_Compras
            WHERE CodigoCompra = @Compra15 AND NumeroCompra = @Numero15
              AND Estado = 'Borrador' AND Observaciones = 'Practica Fase III I1-E15';
            IF @@ROWCOUNT <> 1 THROW 51005, 'E15: encabezado de practica no encontrado.', 1;
            COMMIT TRANSACTION;
            SET @Limpia15 = 1;
        END;
        SELECT @ExistenciaDespues15 = Existencia
        FROM dbo.Tbl_Inventario
        WHERE CodigoSucursal = @Sucursal15 AND CodigoProducto = @Producto15;
        IF @ExistenciaDespues15 IS NULL OR @ExistenciaDespues15 <> @ExistenciaAntes15
            THROW 51006, 'E15: el inventario cambio durante la practica.', 1;
        INSERT INTO @Evidencia15
        SELECT @Decision15, 'Final', @Numero15,
               (SELECT COUNT(*) FROM dbo.Tbl_Compras WHERE CodigoCompra = @Compra15),
               (SELECT COUNT(*) FROM dbo.Tbl_ComprasDetalle WHERE CodigoCompra = @Compra15),
               CAST(COALESCE((SELECT SUM(d.Cantidad * d.CostoUnitario - d.Descuento)
                              FROM dbo.Tbl_ComprasDetalle AS d WHERE d.CodigoCompra = @Compra15), 0) AS decimal(18,2)),
               @ExistenciaAntes15, @ExistenciaDespues15;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        IF @Confirmada15 = 1 AND @Limpia15 = 0
        BEGIN
            BEGIN TRY
                BEGIN TRANSACTION; -- recuperacion tras fallo de limpieza
                IF EXISTS (SELECT 1 FROM dbo.Tbl_Compras
                           WHERE CodigoCompra = @Compra15 AND NumeroCompra = @Numero15
                             AND Estado = 'Borrador' AND Observaciones = 'Practica Fase III I1-E15')
                BEGIN
                    DELETE FROM dbo.Tbl_ComprasDetalle
                    WHERE CodigoDetalleCompra = @Detalle15 AND CodigoCompra = @Compra15
                      AND CodigoProducto = @Producto15 AND Cantidad = 2
                      AND CostoUnitario = @Costo15 AND Descuento = 0
                      AND Observaciones = 'Practica Fase III';
                    IF @@ROWCOUNT <> 1 THROW 51013, 'E15: recuperacion del detalle incompleta.', 1;
                    DELETE FROM dbo.Tbl_Compras
                    WHERE CodigoCompra = @Compra15 AND NumeroCompra = @Numero15
                      AND Estado = 'Borrador' AND Observaciones = 'Practica Fase III I1-E15';
                    IF @@ROWCOUNT <> 1 THROW 51014, 'E15: recuperacion de la compra incompleta.', 1;
                END
                ELSE IF EXISTS (SELECT 1 FROM dbo.Tbl_ComprasDetalle
                                WHERE CodigoDetalleCompra = @Detalle15 AND CodigoCompra = @Compra15)
                    THROW 51015, 'E15: detalle de practica sin encabezado esperado; revisar manualmente.', 1;
                COMMIT TRANSACTION;
                PRINT 'E15: limpieza recuperada; la causa original se informa a continuacion.';
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                SELECT @Numero15 AS NumeroCompraPractica, @Compra15 AS CodigoCompraPractica,
                       @Detalle15 AS CodigoDetallePractica;
                THROW; -- conservar la copia aislada para recuperar solo estas llaves
            END CATCH;
        END;
        THROW;
    END CATCH;
    SET @Paso15 += 1;
END;
END
ELSE PRINT 'E15 omitido: requiere habilitacion explicita de la practica.';
SELECT Decision, Etapa, NumeroCompra, ExisteCompra, CantidadLineas,
       ImporteCompra, ExistenciaAntes, ExistenciaDespues
FROM @Evidencia15
ORDER BY CASE Decision WHEN 'NO' THEN 1 ELSE 2 END,
         CASE Etapa WHEN 'Antes' THEN 1 WHEN 'Temporal' THEN 2
                    WHEN 'Confirmada' THEN 3 ELSE 4 END;
GO
