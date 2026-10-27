/*
  SUPERMERCADOS GUATE — FASE III — INTEGRANTE 3
  Ejercicios I3-F3-E01 a I3-F3-E15.
  Motor: Microsoft SQL Server; base: SupermercadosGuate.
  E01-E13: consultas de lectura (E08 usa el costo historico si esta disponible).
  E14-E15: prácticas DML controladas; NO ejecutar sin respaldo SQL.
  El esquema original no conserva CostoUnitario historico. Aplicar primero
  database/04_Agregar_Costo_Historico_Ajustes.sql en bases existentes.
  Los ajustes anteriores permanecen con costo NULL, sin valorar inventando datos.
*/
USE SupermercadosGuate;
GO

-- I3-F3-E01: productos activos de marca GT/Guatemala, precio Q10–Q100.
DECLARE @Referencia01 date = '20260930';
SELECT p.CodigoProducto, UPPER(p.Nombre) AS Producto,
       LOWER(p.Marca) AS Marca,
       CAST(p.PrecioVenta AS decimal(18,2)) AS PrecioVenta,
       LEN(p.Nombre) AS LongitudNombre,
       DATEDIFF(DAY,p.FechaRegistro,@Referencia01) AS DiasRegistrado
FROM dbo.Tbl_Productos AS p
WHERE p.Estado='Activo' AND p.PrecioVenta BETWEEN 10 AND 100
  AND (p.Marca LIKE '%GT%' OR p.Marca LIKE '%Guatemala%')
ORDER BY DiasRegistrado DESC, Producto;
GO

-- I3-F3-E02: inventarios en mínimo o por debajo y días desde ingreso.
DECLARE @Referencia02 date = '20260930';
SELECT i.CodigoInventario, i.CodigoSucursal, i.CodigoProducto,
       i.Existencia, i.ExistenciaMinima,
       CASE WHEN i.Existencia < i.ExistenciaMinima
            THEN i.ExistenciaMinima-i.Existencia ELSE CAST(0 AS decimal(12,2)) END AS Faltante,
       CAST(ROUND((CASE WHEN i.Existencia<i.ExistenciaMinima THEN i.ExistenciaMinima-i.Existencia ELSE 0 END)*i.CostoPromedio,2) AS decimal(18,2)) AS ValorFaltante,
       CASE WHEN i.FechaUltimoIngreso IS NULL THEN NULL
            ELSE DATEDIFF(DAY,i.FechaUltimoIngreso,@Referencia02) END AS DiasSinIngreso,
       CASE WHEN i.FechaUltimoIngreso IS NULL THEN 'Sin ingreso'
            ELSE 'Con ingreso' END AS SituacionIngreso
FROM dbo.Tbl_Inventario AS i
WHERE i.Existencia<=i.ExistenciaMinima AND i.CostoPromedio>0
ORDER BY ValorFaltante DESC, i.CodigoInventario;
GO

-- I3-F3-E03: inventarios por sucursal, producto y categoría activos.
SELECT i.CodigoInventario, s.CodigoSucursal, s.Nombre AS Sucursal,
       p.CodigoProducto, p.Nombre AS Producto, ca.Nombre AS Categoria,
       i.Existencia,
       CAST(ROUND(i.Existencia*i.CostoPromedio,2) AS decimal(18,2)) AS ValorCosto,
       CAST(ROUND(i.Existencia*p.PrecioVenta,2) AS decimal(18,2)) AS ValorVenta
FROM dbo.Tbl_Inventario AS i
JOIN dbo.Tbl_Sucursales AS s ON s.CodigoSucursal=i.CodigoSucursal AND s.Estado='Activo'
JOIN dbo.Tbl_Productos AS p ON p.CodigoProducto=i.CodigoProducto AND p.Estado='Activo'
JOIN dbo.Tbl_Categorias AS ca ON ca.CodigoCategoria=p.CodigoCategoria AND ca.Estado='Activo'
ORDER BY ValorCosto DESC, Sucursal, Producto;
GO

-- I3-F3-E04: todos los productos activos, con o sin inventario.
SELECT p.CodigoProducto, p.Nombre AS Producto,
       i.CodigoSucursal, i.Existencia, i.ExistenciaMinima
FROM dbo.Tbl_Productos AS p
LEFT JOIN dbo.Tbl_Inventario AS i ON i.CodigoProducto=p.CodigoProducto
WHERE p.Estado='Activo'
ORDER BY Producto, i.CodigoSucursal;
GO

-- I3-F3-E05: productos por encima del promedio de su categoría (subconsulta correlacionada).
SELECT p.CodigoProducto, p.CodigoCategoria, p.Nombre AS Producto,
       CAST(p.PrecioVenta AS decimal(18,2)) AS PrecioVenta
FROM dbo.Tbl_Productos AS p
WHERE p.Estado='Activo'
  AND p.PrecioVenta > (
      SELECT AVG(CAST(p2.PrecioVenta AS decimal(18,4)))
      FROM dbo.Tbl_Productos AS p2
      WHERE p2.CodigoCategoria=p.CodigoCategoria AND p2.Estado='Activo'
  )
ORDER BY p.CodigoCategoria, p.PrecioVenta DESC, p.CodigoProducto;
GO

-- I3-F3-E06: sucursales con ajustes negativos aplicados durante septiembre.
-- No se usa JOIN en la consulta principal; los EXISTS preservan una fila por sucursal.
SELECT s.CodigoSucursal, s.Nombre AS Sucursal, s.Municipio, s.Departamento
FROM dbo.Tbl_Sucursales AS s
WHERE s.Estado='Activo'
  AND EXISTS (
      SELECT 1 FROM dbo.Tbl_AjustesInventario AS a
      WHERE a.CodigoSucursal=s.CodigoSucursal AND a.Estado='Aplicado'
        AND a.FechaAjuste >= '20260901' AND a.FechaAjuste < '20261001'
        AND EXISTS (
            SELECT 1 FROM dbo.Tbl_AjustesInventarioDetalle AS d
            WHERE d.CodigoAjuste=a.CodigoAjuste AND d.Cantidad<0
              AND d.CodigoProducto IN (
                  SELECT CodigoProducto FROM dbo.Tbl_Productos WHERE Estado='Activo'
              )
        )
  )
ORDER BY s.Departamento, Sucursal;
GO

-- I3-F3-E07: inventario de productos activos en sucursales activas.
SELECT s.CodigoSucursal, s.Nombre AS Sucursal,
       COUNT(DISTINCT i.CodigoProducto) AS CantidadProductos,
       SUM(i.Existencia) AS TotalUnidades,
       CAST(ROUND(SUM(i.Existencia*i.CostoPromedio),2) AS decimal(18,2)) AS ValorInventario
FROM dbo.Tbl_Inventario AS i
JOIN dbo.Tbl_Sucursales AS s ON s.CodigoSucursal=i.CodigoSucursal AND s.Estado='Activo'
JOIN dbo.Tbl_Productos AS p ON p.CodigoProducto=i.CodigoProducto AND p.Estado='Activo'
GROUP BY s.CodigoSucursal, s.Nombre
HAVING COUNT(DISTINCT i.CodigoProducto)>=3
   AND SUM(i.Existencia*i.CostoPromedio)>500
ORDER BY ValorInventario DESC, Sucursal;
GO

/* I3-F3-E08: perdidas por ajustes negativos al costo historico del detalle.
   Los grupos con costos historicos incompletos no pueden compararse con Q20
   y se excluyen; no se inventa el costo de ajustes antiguos.
*/
IF COL_LENGTH('dbo.Tbl_AjustesInventarioDetalle','CostoUnitario') IS NULL
BEGIN
    PRINT 'I3-F3-E08 pendiente: no existe CostoUnitario histórico en Tbl_AjustesInventarioDetalle. No se inventan pérdidas.';
    SELECT CAST(NULL AS varchar(200)) AS Motivo, CAST(NULL AS int) AS Anio,
           CAST(NULL AS int) AS Mes, CAST(NULL AS int) AS CantidadLineas,
           CAST(NULL AS decimal(18,2)) AS UnidadesPerdidas,
           CAST(NULL AS decimal(18,2)) AS ValorPerdido
    WHERE 1=0;
END
ELSE
BEGIN
    EXEC sys.sp_executesql N'
      SELECT a.Motivo, YEAR(a.FechaAjuste) AS Anio, MONTH(a.FechaAjuste) AS Mes,
             COUNT(*) AS CantidadLineas,
             SUM(ABS(d.Cantidad)) AS UnidadesPerdidas,
             CAST(ROUND(SUM(ABS(d.Cantidad)*d.CostoUnitario),2) AS decimal(18,2)) AS ValorPerdido
      FROM dbo.Tbl_AjustesInventario AS a
      JOIN dbo.Tbl_AjustesInventarioDetalle AS d ON d.CodigoAjuste=a.CodigoAjuste
      WHERE a.Estado=''Aplicado'' AND a.FechaAjuste>=''20260101''
        AND a.FechaAjuste<''20270101'' AND d.Cantidad<0
      GROUP BY a.Motivo,YEAR(a.FechaAjuste),MONTH(a.FechaAjuste)
      HAVING COUNT(*)>=2
         AND COUNT(d.CostoUnitario)=COUNT(*)
         AND SUM(ABS(d.Cantidad)*d.CostoUnitario)>20
      ORDER BY Anio, Mes, ValorPerdido DESC, a.Motivo;';
END;
GO

-- I3-F3-E09: catálogo geográfico único (UNION elimina duplicados).
SELECT Municipio, Departamento FROM dbo.Tbl_Sucursales WHERE Estado='Activo'
UNION
SELECT Municipio, Departamento FROM dbo.Tbl_Proveedores WHERE Estado='Activo'
ORDER BY Departamento, Municipio;
GO

-- I3-F3-E10: productos con existencia positiva en dos sucursales (INTERSECT).
DECLARE @SucursalA10 int = (SELECT MIN(CodigoSucursal) FROM dbo.Tbl_Sucursales);
DECLARE @SucursalB10 int = (SELECT MIN(CodigoSucursal) FROM dbo.Tbl_Sucursales WHERE CodigoSucursal<>@SucursalA10);
IF @SucursalB10 IS NULL
    PRINT 'E10 requiere dos sucursales diferentes.';
ELSE
BEGIN
    WITH Compartidos AS (
        SELECT CodigoProducto FROM dbo.Tbl_Inventario
        WHERE CodigoSucursal=@SucursalA10 AND Existencia>0
        INTERSECT
        SELECT CodigoProducto FROM dbo.Tbl_Inventario
        WHERE CodigoSucursal=@SucursalB10 AND Existencia>0
    )
    SELECT p.CodigoProducto, p.Nombre AS Producto
    FROM Compartidos AS c
    JOIN dbo.Tbl_Productos AS p ON p.CodigoProducto=c.CodigoProducto
    ORDER BY Producto, p.CodigoProducto;
END;
GO

-- I3-F3-E11: exclusivos de cada sucursal (EXCEPT en ambos sentidos).
DECLARE @SucursalA11 int = (SELECT MIN(CodigoSucursal) FROM dbo.Tbl_Sucursales);
DECLARE @SucursalB11 int = (SELECT MIN(CodigoSucursal) FROM dbo.Tbl_Sucursales WHERE CodigoSucursal<>@SucursalA11);
IF @SucursalB11 IS NULL
    PRINT 'E11 requiere dos sucursales diferentes.';
ELSE
BEGIN
    WITH SoloA AS (
        SELECT CodigoProducto FROM dbo.Tbl_Inventario
        WHERE CodigoSucursal=@SucursalA11 AND Existencia>0
        EXCEPT
        SELECT CodigoProducto FROM dbo.Tbl_Inventario
        WHERE CodigoSucursal=@SucursalB11 AND Existencia>0
    ),
    SoloB AS (
        SELECT CodigoProducto FROM dbo.Tbl_Inventario
        WHERE CodigoSucursal=@SucursalB11 AND Existencia>0
        EXCEPT
        SELECT CodigoProducto FROM dbo.Tbl_Inventario
        WHERE CodigoSucursal=@SucursalA11 AND Existencia>0
    ),
    Exclusivos AS (
        SELECT CodigoProducto, CAST('Solo sucursal 1' AS varchar(30)) AS Disponibilidad FROM SoloA
        UNION ALL
        SELECT CodigoProducto, CAST('Solo sucursal 2' AS varchar(30)) AS Disponibilidad FROM SoloB
    )
    SELECT p.CodigoProducto, p.Nombre AS Producto, x.Disponibilidad
    FROM Exclusivos AS x
    JOIN dbo.Tbl_Productos AS p ON p.CodigoProducto=x.CodigoProducto
    ORDER BY x.Disponibilidad, Producto, p.CodigoProducto;
END;
GO

-- I3-F3-E12: antigüedad del inventario al 30/09/2026.
DECLARE @Referencia12 date='20260930', @Reciente12 int=30, @Intermedio12 int=90;
SELECT i.CodigoInventario, i.CodigoSucursal, i.CodigoProducto,
       i.FechaUltimoIngreso,
       CASE WHEN i.FechaUltimoIngreso IS NULL THEN NULL
            ELSE DATEDIFF(DAY,i.FechaUltimoIngreso,@Referencia12) END AS DiasSinIngreso,
       CASE WHEN i.FechaUltimoIngreso IS NULL THEN 'Sin ingreso'
            WHEN DATEDIFF(DAY,i.FechaUltimoIngreso,@Referencia12)<=@Reciente12 THEN 'Reciente'
            WHEN DATEDIFF(DAY,i.FechaUltimoIngreso,@Referencia12)<=@Intermedio12 THEN 'Intermedio'
            ELSE 'Antiguo' END AS NivelAntiguedad
FROM dbo.Tbl_Inventario AS i
ORDER BY DiasSinIngreso DESC, i.CodigoInventario;
GO

-- I3-F3-E13: reposición condicional con tolerancia de dos productos.
DECLARE @Sucursal13 int=(SELECT MIN(CodigoSucursal) FROM dbo.Tbl_Sucursales WHERE Estado='Activo');
DECLARE @Tolerancia13 int=2, @Debajo13 int;
SELECT @Debajo13=COUNT(*)
FROM dbo.Tbl_Inventario AS i
JOIN dbo.Tbl_Productos AS p ON p.CodigoProducto=i.CodigoProducto
WHERE i.CodigoSucursal=@Sucursal13 AND i.Existencia<i.ExistenciaMinima AND p.Estado='Activo';
IF @Sucursal13 IS NULL
    PRINT 'E13: no se encontró sucursal activa.';
ELSE IF @Debajo13>@Tolerancia13
    SELECT @Sucursal13 AS CodigoSucursal,@Tolerancia13 AS Tolerancia,
           @Debajo13 AS ProductosBajoMinimo,
           CAST('Reposición prioritaria' AS varchar(30)) AS EstadoReposicion;
ELSE
    SELECT @Sucursal13 AS CodigoSucursal,@Tolerancia13 AS Tolerancia,
           @Debajo13 AS ProductosBajoMinimo,
           CAST('Seguimiento normal' AS varchar(30)) AS EstadoReposicion;
-- Para documentar ambas ramas, preparar escenarios distintos y repetir sin alterar producción.
GO

/* I3-F3-E14: ejecutar solo este bloque en SupermercadosGuate_PRUEBAS.
   Probar NO y SI; restaurar minimo sin modificar Existencia.
*/
SET XACT_ABORT ON;
DECLARE @Ejecutar14 varchar(2) = 'NO'; -- Cambiar a SI solo en PRUEBAS.
DECLARE @Paso14 tinyint = 1, @Decision14 varchar(2),
        @Inventario14 int, @MinimoOriginal14 decimal(12,2),
        @ExistenciaOriginal14 decimal(12,2);
DECLARE @Evidencia14 table (
    Decision varchar(2), Etapa varchar(20), CodigoInventario int,
    CodigoSucursal int, CodigoProducto int,
    Existencia decimal(12,2), ExistenciaMinima decimal(12,2)
);
IF @Ejecutar14 = 'SI'
BEGIN
IF DB_NAME() <> N'SupermercadosGuate_PRUEBAS'
    THROW 53016, 'E14: ejecutar solo en SupermercadosGuate_PRUEBAS.', 1;
WHILE @Paso14 <= 2
BEGIN
    SET @Decision14 = CASE WHEN @Paso14 = 1 THEN 'NO' ELSE 'SI' END;
    SET @Inventario14 = NULL;
    BEGIN TRY
        BEGIN TRANSACTION;
        SELECT TOP (1) @Inventario14 = CodigoInventario,
               @MinimoOriginal14 = ExistenciaMinima,
               @ExistenciaOriginal14 = Existencia
        FROM dbo.Tbl_Inventario WITH (UPDLOCK, HOLDLOCK)
        WHERE ExistenciaMinima <= 9999999997.99
        ORDER BY CodigoInventario;
        IF @Inventario14 IS NULL
        BEGIN
            PRINT 'E14: no existe inventario apto para la prueba.';
            ROLLBACK TRANSACTION;
            BREAK;
        END;
        INSERT INTO @Evidencia14
        SELECT @Decision14, 'Antes', CodigoInventario, CodigoSucursal,
               CodigoProducto, Existencia, ExistenciaMinima
        FROM dbo.Tbl_Inventario WHERE CodigoInventario = @Inventario14;

        SAVE TRANSACTION Cambio14;
        BEGIN TRANSACTION;
        UPDATE dbo.Tbl_Inventario SET ExistenciaMinima = ExistenciaMinima + 2
        WHERE CodigoInventario = @Inventario14
          AND ExistenciaMinima = @MinimoOriginal14
          AND Existencia = @ExistenciaOriginal14;
        IF @@ROWCOUNT <> 1 THROW 53001, 'E14: el inventario cambio durante la prueba.', 1;
        INSERT INTO @Evidencia14
        SELECT @Decision14, 'Temporal', CodigoInventario, CodigoSucursal,
               CodigoProducto, Existencia, ExistenciaMinima
        FROM dbo.Tbl_Inventario WHERE CodigoInventario = @Inventario14;

        IF @Decision14 = 'NO'
            ROLLBACK TRANSACTION Cambio14;
        COMMIT TRANSACTION; -- cierra la transaccion interna de la prueba
        IF @Decision14 = 'SI'
        BEGIN
            UPDATE dbo.Tbl_Inventario SET ExistenciaMinima = @MinimoOriginal14
            WHERE CodigoInventario = @Inventario14
              AND ExistenciaMinima = @MinimoOriginal14 + 2
              AND Existencia = @ExistenciaOriginal14;
            IF @@ROWCOUNT <> 1 THROW 53002, 'E14: no fue posible restaurar el minimo.', 1;
        END;
        INSERT INTO @Evidencia14
        SELECT @Decision14, 'Final', CodigoInventario, CodigoSucursal,
               CodigoProducto, Existencia, ExistenciaMinima
        FROM dbo.Tbl_Inventario WHERE CodigoInventario = @Inventario14;
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
SELECT Decision, Etapa, CodigoInventario, CodigoSucursal,
       CodigoProducto, Existencia, ExistenciaMinima
FROM @Evidencia14
ORDER BY CASE Decision WHEN 'NO' THEN 1 ELSE 2 END,
         CASE Etapa WHEN 'Antes' THEN 1 WHEN 'Temporal' THEN 2 ELSE 3 END;
GO

/* I3-F3-E15: ejecutar solo este bloque en una copia aislada de la base.
   NO revierte el ajuste; SI confirma Borrador y detalle firmado y limpia
   en otra transaccion. Si la limpieza falla, se reintenta por llaves creadas.
   Requiere CostoUnitario; el borrador no cambia existencia fisica.
*/
SET XACT_ABORT ON;
DECLARE @Ejecutar15 varchar(2) = 'NO'; -- Cambiar a SI solo en una copia aislada.
DECLARE @Paso15 tinyint = 1, @Decision15 varchar(2), @Numero15 varchar(30),
        @Sucursal15 int, @Empleado15 int, @Producto15 int,
        @Ajuste15 int, @Detalle15 int, @Costo15 decimal(12,2),
        @ExistenciaAntes15 decimal(12,2), @ExistenciaDespues15 decimal(12,2),
        @LineasTemporal15 int, @Confirmada15 bit, @Limpia15 bit;
DECLARE @Evidencia15 table (
    Decision varchar(2), Etapa varchar(25), NumeroAjuste varchar(30),
    ExisteAjuste int, CantidadLineas int,
    ExistenciaAntes decimal(12,2), ExistenciaDespues decimal(12,2)
);
IF @Ejecutar15 = 'SI'
BEGIN
IF DB_NAME() <> N'SupermercadosGuate_PRUEBAS'
    THROW 53010, 'E15: ejecutar solo en SupermercadosGuate_PRUEBAS.', 1;
IF @@TRANCOUNT <> 0
    THROW 53011, 'E15: ejecutar fuera de cualquier transaccion externa.', 1;
IF COL_LENGTH('dbo.Tbl_AjustesInventarioDetalle', 'CostoUnitario') IS NULL
    THROW 53007, 'E15: aplicar primero la migracion del costo historico de ajustes.', 1;
WHILE @Paso15 <= 2
BEGIN
    SET @Decision15 = CASE WHEN @Paso15 = 1 THEN 'NO' ELSE 'SI' END;
    SET @Numero15 = 'I3-E15-' + LEFT(REPLACE(CONVERT(varchar(36), NEWID()), '-', ''), 22);
    SET @Empleado15 = NULL;
    SET @Ajuste15 = NULL;
    SET @Detalle15 = NULL;
    SET @Confirmada15 = 0;
    SET @Limpia15 = 0;
    BEGIN TRY
        SELECT TOP (1) @Sucursal15 = i.CodigoSucursal, @Producto15 = i.CodigoProducto,
               @Empleado15 = e.CodigoEmpleado, @Costo15 = i.CostoPromedio,
               @ExistenciaAntes15 = i.Existencia
        FROM dbo.Tbl_Inventario AS i WITH (UPDLOCK, HOLDLOCK)
        JOIN dbo.Tbl_Sucursales AS s ON s.CodigoSucursal = i.CodigoSucursal AND s.Estado = 'Activo'
        JOIN dbo.Tbl_Empleados AS e ON e.CodigoSucursal = i.CodigoSucursal AND e.Estado = 'Activo'
        JOIN dbo.Tbl_Productos AS p ON p.CodigoProducto = i.CodigoProducto AND p.Estado = 'Activo'
        WHERE i.CostoPromedio > 0 AND i.Existencia >= 1
        ORDER BY i.CodigoInventario, e.CodigoEmpleado;
        IF @Empleado15 IS NULL
        BEGIN
            PRINT 'E15: no hay inventario con costo positivo y empleado valido.';
            BREAK;
        END;
        INSERT INTO @Evidencia15
        VALUES (@Decision15, 'Antes', @Numero15, 0, 0,
                @ExistenciaAntes15, @ExistenciaAntes15);

        BEGIN TRANSACTION;
        IF EXISTS (SELECT 1 FROM dbo.Tbl_AjustesInventario WITH (UPDLOCK, HOLDLOCK)
                   WHERE NumeroAjuste = @Numero15)
            THROW 53003, 'E15: el numero de practica ya existe.', 1;
        INSERT dbo.Tbl_AjustesInventario
            (CodigoSucursal, CodigoEmpleado, NumeroAjuste, FechaAjuste,
             Motivo, Estado, Observaciones)
        VALUES (@Sucursal15, @Empleado15, @Numero15, '20260930',
                'Conteo de practica', 'Borrador', 'Practica Fase III I3-E15');
        SET @Ajuste15 = CONVERT(int, SCOPE_IDENTITY());
        EXEC sys.sp_executesql
            N'INSERT dbo.Tbl_AjustesInventarioDetalle
                (CodigoAjuste, CodigoProducto, Cantidad, CostoUnitario)
              VALUES (@Ajuste, @Producto, -1, @Costo);
              SET @Detalle = CONVERT(int, SCOPE_IDENTITY());',
            N'@Ajuste int, @Producto int, @Costo decimal(12,2), @Detalle int OUTPUT',
            @Ajuste = @Ajuste15, @Producto = @Producto15,
            @Costo = @Costo15, @Detalle = @Detalle15 OUTPUT;
        SELECT @LineasTemporal15 = COUNT(*)
        FROM dbo.Tbl_AjustesInventarioDetalle WHERE CodigoAjuste = @Ajuste15;
        IF @Decision15 = 'NO'
            ROLLBACK TRANSACTION;
        ELSE
        BEGIN
            SET @Confirmada15 = 1;
            COMMIT TRANSACTION; -- ajuste y detalle ya son persistentes
        END;
        INSERT INTO @Evidencia15
        VALUES (@Decision15, 'Temporal', @Numero15, 1, @LineasTemporal15,
                @ExistenciaAntes15, @ExistenciaAntes15);
        IF @Decision15 = 'SI'
        BEGIN
            IF @@TRANCOUNT <> 0 OR
               (SELECT COUNT(*) FROM dbo.Tbl_AjustesInventario
                WHERE CodigoAjuste = @Ajuste15 AND NumeroAjuste = @Numero15) <> 1 OR
               (SELECT COUNT(*) FROM dbo.Tbl_AjustesInventarioDetalle
                WHERE CodigoDetalleAjuste = @Detalle15 AND CodigoAjuste = @Ajuste15) <> 1
                THROW 53012, 'E15: ajuste y detalle no quedaron confirmados.', 1;
            INSERT INTO @Evidencia15
            VALUES ('SI', 'Confirmada', @Numero15, 1, 1,
                    @ExistenciaAntes15, @ExistenciaAntes15);
            BEGIN TRANSACTION; -- limpieza independiente del COMMIT anterior
            DELETE FROM dbo.Tbl_AjustesInventarioDetalle
            WHERE CodigoDetalleAjuste = @Detalle15 AND CodigoAjuste = @Ajuste15
              AND CodigoProducto = @Producto15 AND Cantidad = -1
              AND CostoUnitario = @Costo15;
            IF @@ROWCOUNT <> 1 THROW 53004, 'E15: detalle de practica no encontrado.', 1;
            DELETE FROM dbo.Tbl_AjustesInventario
            WHERE CodigoAjuste = @Ajuste15 AND NumeroAjuste = @Numero15
              AND Estado = 'Borrador' AND Motivo = 'Conteo de practica'
              AND Observaciones = 'Practica Fase III I3-E15';
            IF @@ROWCOUNT <> 1 THROW 53005, 'E15: encabezado de practica no encontrado.', 1;
            COMMIT TRANSACTION;
            SET @Limpia15 = 1;
        END;
        SELECT @ExistenciaDespues15 = Existencia
        FROM dbo.Tbl_Inventario
        WHERE CodigoSucursal = @Sucursal15 AND CodigoProducto = @Producto15;
        IF @ExistenciaDespues15 IS NULL OR @ExistenciaDespues15 <> @ExistenciaAntes15
            THROW 53006, 'E15: el inventario cambio durante la practica.', 1;
        INSERT INTO @Evidencia15
        SELECT @Decision15, 'Final', @Numero15,
               (SELECT COUNT(*) FROM dbo.Tbl_AjustesInventario WHERE CodigoAjuste = @Ajuste15),
               (SELECT COUNT(*) FROM dbo.Tbl_AjustesInventarioDetalle WHERE CodigoAjuste = @Ajuste15),
               @ExistenciaAntes15, @ExistenciaDespues15;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        IF @Confirmada15 = 1 AND @Limpia15 = 0
        BEGIN
            BEGIN TRY
                BEGIN TRANSACTION; -- recuperacion tras fallo de limpieza
                IF EXISTS (SELECT 1 FROM dbo.Tbl_AjustesInventario
                           WHERE CodigoAjuste = @Ajuste15 AND NumeroAjuste = @Numero15
                             AND Estado = 'Borrador' AND Motivo = 'Conteo de practica'
                             AND Observaciones = 'Practica Fase III I3-E15')
                BEGIN
                    DELETE FROM dbo.Tbl_AjustesInventarioDetalle
                    WHERE CodigoDetalleAjuste = @Detalle15 AND CodigoAjuste = @Ajuste15
                      AND CodigoProducto = @Producto15 AND Cantidad = -1
                      AND CostoUnitario = @Costo15;
                    IF @@ROWCOUNT <> 1 THROW 53013, 'E15: recuperacion del detalle incompleta.', 1;
                    DELETE FROM dbo.Tbl_AjustesInventario
                    WHERE CodigoAjuste = @Ajuste15 AND NumeroAjuste = @Numero15
                      AND Estado = 'Borrador' AND Motivo = 'Conteo de practica'
                      AND Observaciones = 'Practica Fase III I3-E15';
                    IF @@ROWCOUNT <> 1 THROW 53014, 'E15: recuperacion del ajuste incompleta.', 1;
                END
                ELSE IF EXISTS (SELECT 1 FROM dbo.Tbl_AjustesInventarioDetalle
                                WHERE CodigoDetalleAjuste = @Detalle15 AND CodigoAjuste = @Ajuste15)
                    THROW 53015, 'E15: detalle de practica sin encabezado esperado; revisar manualmente.', 1;
                COMMIT TRANSACTION;
                PRINT 'E15: limpieza recuperada; la causa original se informa a continuacion.';
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                SELECT @Numero15 AS NumeroAjustePractica, @Ajuste15 AS CodigoAjustePractica,
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
SELECT Decision, Etapa, NumeroAjuste, ExisteAjuste,
       CantidadLineas, ExistenciaAntes, ExistenciaDespues
FROM @Evidencia15
ORDER BY CASE Decision WHEN 'NO' THEN 1 ELSE 2 END,
         CASE Etapa WHEN 'Antes' THEN 1 WHEN 'Temporal' THEN 2
                    WHEN 'Confirmada' THEN 3 ELSE 4 END;
GO
