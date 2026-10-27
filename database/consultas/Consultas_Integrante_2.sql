USE SupermercadosGuate;
GO

-- CONSULTAS SIMPLES

/* 1.
   Mostrar clientes activos registrados antes de julio de 2026 de Guatemala
   o Quetzaltenango.
*/
SELECT
    CodigoCliente,
    Nombres+' '+Apellidos AS Cliente,
    Departamento,
    ISNULL(Correo,'No registrado') AS Correo,
    LEN(Nombres+' '+Apellidos) AS LongitudNombre,
    DATEDIFF(YEAR,FechaRegistro,'20260930')-
        CASE WHEN DATEADD(YEAR,DATEDIFF(YEAR,FechaRegistro,'20260930'),FechaRegistro)>'20260930'
             THEN 1 ELSE 0 END AS AntiguedadAnios
FROM Tbl_Clientes
WHERE Estado='Activo' AND FechaRegistro<'20260701'
    AND Departamento IN ('Guatemala','Quetzaltenango')
ORDER BY Departamento ASC, Cliente ASC;
GO


USE SupermercadosGuate;
GO

/* 2.
   Mostrar pagos aplicados de julio a septiembre por efectivo o tarjeta,
   mayores a Q50.
*/
SELECT
    CodigoPago,
    UPPER(Comprobante) AS Comprobante,
    FechaPago,
    YEAR(FechaPago) AS Anio,
    MONTH(FechaPago) AS Mes,
    MetodoPago,
    ISNULL(Autorizacion,'Sin autorizacion') AS Autorizacion,
    ROUND(Monto,2) AS Monto,
    ROUND(Comision,2) AS Comision,
    ROUND(Monto-Comision,2) AS MontoNeto
FROM Tbl_Pagos
WHERE Estado='Aplicado'
    AND FechaPago>='20260701' AND FechaPago<'20261001'
    AND MetodoPago IN ('Efectivo','Tarjeta') AND Monto>50
ORDER BY FechaPago DESC;
GO


USE SupermercadosGuate;
GO

-- CONSULTAS RELACIONADAS Y SUBCONSULTAS

/* 3.
   Mostrar ventas confirmadas de septiembre con cliente, sucursal, empleado,
   total de venta, total pagado y saldo.
*/
SELECT
    V.CodigoVenta,
    V.NumeroVenta,
    V.FechaVenta,
    C.Nombres+' '+C.Apellidos AS Cliente,
    S.Nombre AS Sucursal,
    E.Nombres+' '+E.Apellidos AS Empleado,
    CAST(ROUND((SELECT SUM(D.Cantidad*D.PrecioUnitario-D.Descuento)
           FROM Tbl_VentasDetalle D WHERE D.CodigoVenta=V.CodigoVenta),2) AS DECIMAL(18,2)) AS TotalVenta,
    CAST(ROUND(ISNULL((SELECT SUM(P.Monto) FROM Tbl_Pagos P
                  WHERE P.CodigoVenta=V.CodigoVenta AND P.Estado='Aplicado'),0),2) AS DECIMAL(18,2)) AS TotalPagado,
    CAST(ROUND((SELECT SUM(D.Cantidad*D.PrecioUnitario-D.Descuento)
           FROM Tbl_VentasDetalle D WHERE D.CodigoVenta=V.CodigoVenta)
          -ISNULL((SELECT SUM(P.Monto) FROM Tbl_Pagos P
                   WHERE P.CodigoVenta=V.CodigoVenta AND P.Estado='Aplicado'),0),2) AS DECIMAL(18,2)) AS Saldo
FROM Tbl_Ventas V
INNER JOIN Tbl_Clientes C ON C.CodigoCliente=V.CodigoCliente
INNER JOIN Tbl_Sucursales S ON S.CodigoSucursal=V.CodigoSucursal
INNER JOIN Tbl_Empleados E ON E.CodigoEmpleado=V.CodigoEmpleado
WHERE V.Estado='Confirmada' AND V.FechaVenta>='20260901' AND V.FechaVenta<'20261001'
ORDER BY Saldo DESC;
GO


USE SupermercadosGuate;
GO

/* 4.
   Mostrar todos los clientes activos y sus ventas confirmadas de 2026,
   incluyendo clientes que no compraron.
*/
SELECT
    C.CodigoCliente,
    C.Nombres+' '+C.Apellidos AS Cliente,
    V.NumeroVenta,
    V.FechaVenta,
    CAST(ROUND((SELECT SUM(D.Cantidad*D.PrecioUnitario-D.Descuento)
           FROM Tbl_VentasDetalle D WHERE D.CodigoVenta=V.CodigoVenta),2) AS DECIMAL(18,2)) AS TotalVenta
FROM Tbl_Clientes C
LEFT JOIN Tbl_Ventas V ON V.CodigoCliente=C.CodigoCliente
    AND V.Estado='Confirmada'
    AND V.FechaVenta>='20260101' AND V.FechaVenta<'20270101'
WHERE C.Estado='Activo'
ORDER BY Cliente ASC, V.FechaVenta ASC;
GO


USE SupermercadosGuate;
GO

/* 5.
   Mostrar ventas confirmadas cuyo total sea mayor al promedio de los
   totales de ventas confirmadas.
*/
SELECT
    CodigoVenta,
    NumeroVenta,
    FechaVenta,
    CodigoSucursal,
    CAST(ROUND((SELECT SUM(Cantidad*PrecioUnitario-Descuento)
           FROM Tbl_VentasDetalle D WHERE D.CodigoVenta=V.CodigoVenta),2) AS DECIMAL(18,2)) AS TotalVenta
FROM Tbl_Ventas V
WHERE Estado='Confirmada'
    AND (SELECT SUM(Cantidad*PrecioUnitario-Descuento)
         FROM Tbl_VentasDetalle D WHERE D.CodigoVenta=V.CodigoVenta)>
        (SELECT AVG(TotalVenta)
         FROM
            (SELECT SUM(D2.Cantidad*D2.PrecioUnitario-D2.Descuento) AS TotalVenta
             FROM Tbl_Ventas V2
             INNER JOIN Tbl_VentasDetalle D2 ON D2.CodigoVenta=V2.CodigoVenta
             WHERE V2.Estado='Confirmada'
             GROUP BY V2.CodigoVenta) T)
ORDER BY TotalVenta DESC;
GO


USE SupermercadosGuate;
GO

/* 6.
   Mostrar clientes activos que tengan una venta confirmada con un pago
   aplicado mayor al promedio de todos los pagos aplicados.
*/
SELECT
    CodigoCliente,
    Nombres+' '+Apellidos AS Cliente,
    Departamento,
    Telefono
FROM Tbl_Clientes
WHERE Estado='Activo' AND CodigoCliente IN
    (SELECT CodigoCliente FROM Tbl_Ventas
     WHERE Estado='Confirmada' AND CodigoVenta IN
        (SELECT CodigoVenta FROM Tbl_Pagos
         WHERE Estado='Aplicado' AND Monto>
            (SELECT AVG(Monto) FROM Tbl_Pagos WHERE Estado='Aplicado')))
ORDER BY Apellidos ASC;
GO


USE SupermercadosGuate;
GO

-- CONSULTAS RESUMIDAS

/* 7.
   Resumir ventas confirmadas de enero a septiembre por sucursal.
*/
SELECT
    S.CodigoSucursal,
    S.Nombre AS Sucursal,
    COUNT(DISTINCT V.CodigoVenta) AS CantidadVentas,
    CAST(ROUND(SUM(D.Cantidad*D.PrecioUnitario-D.Descuento),2) AS DECIMAL(18,2)) AS TotalVendido,
    CAST(ROUND(SUM(D.Cantidad*D.PrecioUnitario-D.Descuento)/COUNT(DISTINCT V.CodigoVenta),2) AS DECIMAL(18,2)) AS TicketPromedio
FROM Tbl_Sucursales S
INNER JOIN Tbl_Ventas V ON V.CodigoSucursal=S.CodigoSucursal
INNER JOIN Tbl_VentasDetalle D ON D.CodigoVenta=V.CodigoVenta
WHERE V.Estado='Confirmada'
    AND V.FechaVenta>='20260101' AND V.FechaVenta<'20261001'
GROUP BY S.CodigoSucursal,S.Nombre
HAVING COUNT(DISTINCT V.CodigoVenta)>=3
    AND SUM(D.Cantidad*D.PrecioUnitario-D.Descuento)>500
ORDER BY TotalVendido DESC;
GO


USE SupermercadosGuate;
GO

/* 8.
   Resumir ventas confirmadas por categoria y mes de 2026.
*/
SELECT
    C.CodigoCategoria,
    C.Nombre AS Categoria,
    YEAR(V.FechaVenta) AS Anio,
    MONTH(V.FechaVenta) AS Mes,
    SUM(D.Cantidad) AS UnidadesVendidas,
    CAST(ROUND(SUM(D.Cantidad*D.PrecioUnitario-D.Descuento),2) AS DECIMAL(18,2)) AS VentaNeta,
    CAST(ROUND(SUM((D.Cantidad*D.PrecioUnitario-D.Descuento)
        -(D.Cantidad*D.CostoUnitarioHistorico)),2) AS DECIMAL(18,2)) AS Utilidad
FROM Tbl_Ventas V
INNER JOIN Tbl_VentasDetalle D ON D.CodigoVenta=V.CodigoVenta
INNER JOIN Tbl_Productos P ON P.CodigoProducto=D.CodigoProducto
INNER JOIN Tbl_Categorias C ON C.CodigoCategoria=P.CodigoCategoria
WHERE V.Estado='Confirmada'
    AND V.FechaVenta>='20260101' AND V.FechaVenta<'20270101'
GROUP BY C.CodigoCategoria,C.Nombre,YEAR(V.FechaVenta),MONTH(V.FechaVenta)
HAVING COUNT(D.CodigoDetalleVenta)>=2
    AND SUM(D.Cantidad*D.PrecioUnitario-D.Descuento)>200
ORDER BY Anio ASC, Mes ASC, Categoria ASC;
GO


USE SupermercadosGuate;
GO

-- OPERADORES DE CONJUNTOS

/* 9.
   Construir una bitacora de septiembre con ventas confirmadas y pagos aplicados.
*/
SELECT
    V.FechaVenta AS Fecha,
    V.NumeroVenta AS Documento,
    'Venta' AS TipoEvento,
    V.CodigoSucursal,
    CAST(ROUND(SUM(D.Cantidad*D.PrecioUnitario-D.Descuento),2) AS DECIMAL(18,2)) AS Monto,
    V.Estado
FROM Tbl_Ventas V
INNER JOIN Tbl_VentasDetalle D ON D.CodigoVenta=V.CodigoVenta
WHERE V.Estado='Confirmada'
    AND V.FechaVenta>='20260901' AND V.FechaVenta<'20261001'
GROUP BY V.FechaVenta,V.NumeroVenta,V.CodigoSucursal,V.Estado

UNION ALL

SELECT
    P.FechaPago,
    P.Comprobante,
    'Pago',
    V.CodigoSucursal,
    CAST(ROUND(P.Monto,2) AS DECIMAL(18,2)),
    P.Estado
FROM Tbl_Pagos P
INNER JOIN Tbl_Ventas V ON V.CodigoVenta=P.CodigoVenta
WHERE P.Estado='Aplicado'
    AND P.FechaPago>='20260901' AND P.FechaPago<'20261001'
ORDER BY Fecha ASC, TipoEvento ASC;
GO


USE SupermercadosGuate;
GO

/* 10.
   Mostrar clientes que compraron de enero a junio y tambien de julio
   a septiembre.
*/
SELECT
    C.CodigoCliente,
    C.Nombres+' '+C.Apellidos AS Cliente
FROM Tbl_Clientes C
INNER JOIN Tbl_Ventas V ON V.CodigoCliente=C.CodigoCliente
WHERE V.Estado='Confirmada'
    AND V.FechaVenta>='20260101' AND V.FechaVenta<'20260701'

INTERSECT

SELECT
    C.CodigoCliente,
    C.Nombres+' '+C.Apellidos AS Cliente
FROM Tbl_Clientes C
INNER JOIN Tbl_Ventas V ON V.CodigoCliente=C.CodigoCliente
WHERE V.Estado='Confirmada'
    AND V.FechaVenta>='20260701' AND V.FechaVenta<'20261001'
ORDER BY Cliente ASC;
GO


USE SupermercadosGuate;
GO

/* 11.
   Lista de clientes para campaña: clientes del primer periodo que ya no
   compraron y clientes nuevos de 2026 sin ventas confirmadas.
*/
SELECT
    C.CodigoCliente,
    C.Nombres+' '+C.Apellidos AS Cliente
FROM Tbl_Clientes C
INNER JOIN Tbl_Ventas V ON V.CodigoCliente=C.CodigoCliente
WHERE V.Estado='Confirmada'
    AND V.FechaVenta>='20260101' AND V.FechaVenta<'20260701'

EXCEPT

SELECT
    C.CodigoCliente,
    C.Nombres+' '+C.Apellidos AS Cliente
FROM Tbl_Clientes C
INNER JOIN Tbl_Ventas V ON V.CodigoCliente=C.CodigoCliente
WHERE V.Estado='Confirmada'
    AND V.FechaVenta>='20260701' AND V.FechaVenta<'20261001'

UNION

SELECT
    CodigoCliente,
    Nombres+' '+Apellidos AS Cliente
FROM Tbl_Clientes C
WHERE Estado='Activo'
    AND FechaRegistro>='20260101' AND FechaRegistro<'20270101'
    AND CodigoCliente NOT IN
        (SELECT CodigoCliente FROM Tbl_Ventas
         WHERE Estado='Confirmada'
            AND FechaVenta>='20260101' AND FechaVenta<'20270101')
ORDER BY Cliente ASC;
GO


USE SupermercadosGuate;
GO

-- VARIABLES Y CONSULTAS CONDICIONALES

/* 12.
   Clasificar cada venta confirmada como Baja, Media o Alta.
*/
DECLARE @LimiteBajo DECIMAL(12,2)=100;
DECLARE @LimiteAlto DECIMAL(12,2)=500;

SELECT
    V.CodigoVenta,
    V.NumeroVenta,
    V.CodigoSucursal,
    CAST(ROUND(SUM(D.Cantidad*D.PrecioUnitario-D.Descuento),2) AS DECIMAL(18,2)) AS TotalVenta,
    CASE
        WHEN SUM(D.Cantidad*D.PrecioUnitario-D.Descuento)<@LimiteBajo THEN 'Baja'
        WHEN SUM(D.Cantidad*D.PrecioUnitario-D.Descuento)<=@LimiteAlto THEN 'Media'
        ELSE 'Alta'
    END AS NivelVenta
FROM Tbl_Ventas V
INNER JOIN Tbl_VentasDetalle D ON D.CodigoVenta=V.CodigoVenta
WHERE V.Estado='Confirmada'
GROUP BY V.CodigoVenta,V.NumeroVenta,V.CodigoSucursal
ORDER BY TotalVenta DESC;
GO


USE SupermercadosGuate;
GO

/* 13.
   Comprobar si una sucursal alcanzo la meta de Q1000 en septiembre.
*/
DECLARE @CodigoSucursal INT=1;
DECLARE @Meta DECIMAL(12,2)=1000;
DECLARE @TotalVendido DECIMAL(12,2);

SELECT @TotalVendido=ISNULL(SUM(D.Cantidad*D.PrecioUnitario-D.Descuento),0)
FROM Tbl_Ventas V
INNER JOIN Tbl_VentasDetalle D ON D.CodigoVenta=V.CodigoVenta
WHERE V.CodigoSucursal=@CodigoSucursal AND V.Estado='Confirmada'
    AND V.FechaVenta>='20260901' AND V.FechaVenta<'20261001';

IF @TotalVendido>=@Meta
    SELECT @CodigoSucursal AS CodigoSucursal, @Meta AS Meta,
        @TotalVendido AS TotalVendido, CAST(0 AS DECIMAL(18,2)) AS Faltante, 'Alcanzada' AS EstadoMeta;
ELSE
    SELECT @CodigoSucursal AS CodigoSucursal, @Meta AS Meta,
        @TotalVendido AS TotalVendido, CAST(@Meta-@TotalVendido AS DECIMAL(18,2)) AS Faltante,
        'Pendiente' AS EstadoMeta;
GO

-- Cambiar el codigo de sucursal para probar la otra condicion.
DECLARE @CodigoSucursal INT=2;
DECLARE @Meta DECIMAL(12,2)=1000;
DECLARE @TotalVendido DECIMAL(12,2);

SELECT @TotalVendido=ISNULL(SUM(D.Cantidad*D.PrecioUnitario-D.Descuento),0)
FROM Tbl_Ventas V
INNER JOIN Tbl_VentasDetalle D ON D.CodigoVenta=V.CodigoVenta
WHERE V.CodigoSucursal=@CodigoSucursal AND V.Estado='Confirmada'
    AND V.FechaVenta>='20260901' AND V.FechaVenta<'20261001';

IF @TotalVendido>=@Meta
    SELECT @CodigoSucursal AS CodigoSucursal, @Meta AS Meta,
        @TotalVendido AS TotalVendido, CAST(0 AS DECIMAL(18,2)) AS Faltante, 'Alcanzada' AS EstadoMeta;
ELSE
    SELECT @CodigoSucursal AS CodigoSucursal, @Meta AS Meta,
        @TotalVendido AS TotalVendido, CAST(@Meta-@TotalVendido AS DECIMAL(18,2)) AS Faltante,
        'Pendiente' AS EstadoMeta;
GO


USE SupermercadosGuate;
GO

-- VARIABLES Y TRANSACCIONES

/* 14. Ejecutar solo este bloque en SupermercadosGuate_PRUEBAS.
   Probar NO y despues SI sobre un empleado activo; restaurar su salario.
*/
SET XACT_ABORT ON;
DECLARE @Ejecutar14 varchar(2) = 'NO'; -- Cambiar a SI solo en PRUEBAS.
DECLARE @Paso14 tinyint = 1, @Decision14 varchar(2),
        @Empleado14 int, @SalarioOriginal14 decimal(12,2);
DECLARE @Evidencia14 table (
    Decision varchar(2), Etapa varchar(20), CodigoEmpleado int,
    CodigoSucursal int, Empleado varchar(201), Salario decimal(12,2)
);
IF @Ejecutar14 = 'SI'
BEGIN
IF DB_NAME() <> N'SupermercadosGuate_PRUEBAS'
    THROW 52016, 'E14: ejecutar solo en SupermercadosGuate_PRUEBAS.', 1;
WHILE @Paso14 <= 2
BEGIN
    SET @Decision14 = CASE WHEN @Paso14 = 1 THEN 'NO' ELSE 'SI' END;
    SET @Empleado14 = NULL;
    BEGIN TRY
        BEGIN TRANSACTION;
        SELECT TOP (1) @Empleado14 = CodigoEmpleado, @SalarioOriginal14 = Salario
        FROM dbo.Tbl_Empleados WITH (UPDLOCK, HOLDLOCK)
        WHERE Estado = 'Activo' AND Salario > 0 AND Salario <= 9999999899.99
        ORDER BY CodigoEmpleado;
        IF @Empleado14 IS NULL
        BEGIN
            PRINT 'E14: no existe empleado activo apto para la prueba.';
            ROLLBACK TRANSACTION;
            BREAK;
        END;
        INSERT INTO @Evidencia14
        SELECT @Decision14, 'Antes', CodigoEmpleado, CodigoSucursal,
               Nombres + ' ' + Apellidos, Salario
        FROM dbo.Tbl_Empleados WHERE CodigoEmpleado = @Empleado14;

        SAVE TRANSACTION Cambio14;
        BEGIN TRANSACTION;
        UPDATE dbo.Tbl_Empleados SET Salario = Salario + 100.00
        WHERE CodigoEmpleado = @Empleado14 AND Estado = 'Activo'
          AND Salario = @SalarioOriginal14;
        IF @@ROWCOUNT <> 1 THROW 52001, 'E14: el empleado cambio durante la prueba.', 1;
        INSERT INTO @Evidencia14
        SELECT @Decision14, 'Temporal', CodigoEmpleado, CodigoSucursal,
               Nombres + ' ' + Apellidos, Salario
        FROM dbo.Tbl_Empleados WHERE CodigoEmpleado = @Empleado14;

        IF @Decision14 = 'NO'
            ROLLBACK TRANSACTION Cambio14;
        COMMIT TRANSACTION; -- cierra la transaccion interna de la prueba
        IF @Decision14 = 'SI'
        BEGIN
            UPDATE dbo.Tbl_Empleados SET Salario = @SalarioOriginal14
            WHERE CodigoEmpleado = @Empleado14 AND Estado = 'Activo'
              AND Salario = @SalarioOriginal14 + 100.00;
            IF @@ROWCOUNT <> 1 THROW 52002, 'E14: no fue posible restaurar el salario.', 1;
        END;
        INSERT INTO @Evidencia14
        SELECT @Decision14, 'Final', CodigoEmpleado, CodigoSucursal,
               Nombres + ' ' + Apellidos, Salario
        FROM dbo.Tbl_Empleados WHERE CodigoEmpleado = @Empleado14;
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
SELECT Decision, Etapa, CodigoEmpleado, CodigoSucursal, Empleado,
       CAST(Salario AS decimal(12,2)) AS Salario
FROM @Evidencia14
ORDER BY CASE Decision WHEN 'NO' THEN 1 ELSE 2 END,
         CASE Etapa WHEN 'Antes' THEN 1 WHEN 'Temporal' THEN 2 ELSE 3 END;
GO

/* 15. Ejecutar solo este bloque en una copia aislada de la base.
   NO revierte cliente y venta Borrador; SI los confirma y luego limpia
   en otra transaccion. No se insertan detalles ni pagos.
*/
SET XACT_ABORT ON;
DECLARE @Ejecutar15 varchar(2) = 'NO'; -- Cambiar a SI solo en una copia aislada.
DECLARE @Paso15 tinyint = 1, @Decision15 varchar(2), @NIT15 varchar(20),
        @Numero15 varchar(30), @Sucursal15 int, @Empleado15 int,
        @Cliente15 int, @Venta15 int, @Confirmada15 bit, @Limpia15 bit,
        @ExisteClienteTemporal15 int, @ExisteVentaTemporal15 int;
DECLARE @Evidencia15 table (
    Decision varchar(2), Etapa varchar(25), NIT varchar(20),
    NumeroVenta varchar(30), ExisteCliente int, ExisteVenta int
);
IF @Ejecutar15 = 'SI'
BEGIN
IF DB_NAME() <> N'SupermercadosGuate_PRUEBAS'
    THROW 52010, 'E15: ejecutar solo en SupermercadosGuate_PRUEBAS.', 1;
IF @@TRANCOUNT <> 0
    THROW 52011, 'E15: ejecutar fuera de cualquier transaccion externa.', 1;
WHILE @Paso15 <= 2
BEGIN
    SET @Decision15 = CASE WHEN @Paso15 = 1 THEN 'NO' ELSE 'SI' END;
    SET @NIT15 = 'I2E15' + LEFT(REPLACE(CONVERT(varchar(36), NEWID()), '-', ''), 14);
    SET @Numero15 = 'I2-E15-' + LEFT(REPLACE(CONVERT(varchar(36), NEWID()), '-', ''), 22);
    SET @Empleado15 = NULL;
    SET @Cliente15 = NULL;
    SET @Venta15 = NULL;
    SET @Confirmada15 = 0;
    SET @Limpia15 = 0;
    BEGIN TRY
        SELECT TOP (1) @Sucursal15 = e.CodigoSucursal, @Empleado15 = e.CodigoEmpleado
        FROM dbo.Tbl_Empleados AS e
        JOIN dbo.Tbl_Sucursales AS s ON s.CodigoSucursal = e.CodigoSucursal
        WHERE e.Estado = 'Activo' AND s.Estado = 'Activo'
        ORDER BY e.CodigoEmpleado;
        IF @Empleado15 IS NULL
        BEGIN
            PRINT 'E15: no existe empleado activo en sucursal activa.';
            BREAK;
        END;
        INSERT INTO @Evidencia15 VALUES (@Decision15, 'Antes', @NIT15, @Numero15, 0, 0);
        BEGIN TRANSACTION;
        IF EXISTS (SELECT 1 FROM dbo.Tbl_Clientes WITH (UPDLOCK, HOLDLOCK) WHERE NIT = @NIT15)
           OR EXISTS (SELECT 1 FROM dbo.Tbl_Ventas WITH (UPDLOCK, HOLDLOCK) WHERE NumeroVenta = @Numero15)
            THROW 52003, 'E15: alguno de los identificadores de practica ya existe.', 1;
        INSERT dbo.Tbl_Clientes
            (NIT, Nombres, Apellidos, Municipio, Departamento, Telefono,
             Correo, FechaRegistro, Estado)
        VALUES (@NIT15, 'Cliente', 'Practica', 'Guatemala', 'Guatemala',
                NULL, NULL, '20260930', 'Activo');
        SET @Cliente15 = CONVERT(int, SCOPE_IDENTITY());
        INSERT dbo.Tbl_Ventas
            (CodigoCliente, CodigoSucursal, CodigoEmpleado, NumeroVenta,
             FechaVenta, Estado, Observaciones)
        VALUES (@Cliente15, @Sucursal15, @Empleado15, @Numero15,
                '20260930', 'Borrador', 'Practica Fase III I2-E15');
        SET @Venta15 = CONVERT(int, SCOPE_IDENTITY());
        SELECT @ExisteClienteTemporal15 = (SELECT COUNT(*) FROM dbo.Tbl_Clientes WHERE CodigoCliente = @Cliente15),
               @ExisteVentaTemporal15 = (SELECT COUNT(*) FROM dbo.Tbl_Ventas WHERE CodigoVenta = @Venta15);
        IF @Decision15 = 'NO'
            ROLLBACK TRANSACTION;
        ELSE
        BEGIN
            SET @Confirmada15 = 1;
            COMMIT TRANSACTION; -- cliente y venta ya son persistentes
        END;
        INSERT INTO @Evidencia15
        VALUES (@Decision15, 'Temporal', @NIT15, @Numero15,
                @ExisteClienteTemporal15, @ExisteVentaTemporal15);
        IF @Decision15 = 'SI'
        BEGIN
            IF @@TRANCOUNT <> 0 OR
               (SELECT COUNT(*) FROM dbo.Tbl_Clientes
                WHERE CodigoCliente = @Cliente15 AND NIT = @NIT15) <> 1 OR
               (SELECT COUNT(*) FROM dbo.Tbl_Ventas
                WHERE CodigoVenta = @Venta15 AND NumeroVenta = @Numero15
                  AND CodigoCliente = @Cliente15) <> 1
                THROW 52012, 'E15: cliente y venta no quedaron confirmados.', 1;
            INSERT INTO @Evidencia15
            VALUES ('SI', 'Confirmada', @NIT15, @Numero15, 1, 1);
            BEGIN TRANSACTION; -- limpieza independiente del COMMIT anterior
            DELETE FROM dbo.Tbl_Ventas
            WHERE CodigoVenta = @Venta15 AND CodigoCliente = @Cliente15
              AND NumeroVenta = @Numero15 AND Estado = 'Borrador'
              AND Observaciones = 'Practica Fase III I2-E15';
            IF @@ROWCOUNT <> 1 THROW 52004, 'E15: no se encontro la venta de practica.', 1;
            DELETE FROM dbo.Tbl_Clientes
            WHERE CodigoCliente = @Cliente15 AND NIT = @NIT15
              AND Nombres = 'Cliente' AND Apellidos = 'Practica';
            IF @@ROWCOUNT <> 1 THROW 52005, 'E15: no se encontro el cliente de practica.', 1;
            COMMIT TRANSACTION;
            SET @Limpia15 = 1;
        END;
        INSERT INTO @Evidencia15
        SELECT @Decision15, 'Final', @NIT15, @Numero15,
               (SELECT COUNT(*) FROM dbo.Tbl_Clientes WHERE CodigoCliente = @Cliente15),
               (SELECT COUNT(*) FROM dbo.Tbl_Ventas WHERE CodigoVenta = @Venta15);
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        IF @Confirmada15 = 1 AND @Limpia15 = 0
        BEGIN
            BEGIN TRY
                BEGIN TRANSACTION; -- recuperacion tras fallo de limpieza
                IF EXISTS (SELECT 1 FROM dbo.Tbl_Clientes
                           WHERE CodigoCliente = @Cliente15 AND NIT = @NIT15
                             AND Nombres = 'Cliente' AND Apellidos = 'Practica')
                BEGIN
                    DELETE FROM dbo.Tbl_Ventas
                    WHERE CodigoVenta = @Venta15 AND CodigoCliente = @Cliente15
                      AND NumeroVenta = @Numero15 AND Estado = 'Borrador'
                      AND Observaciones = 'Practica Fase III I2-E15';
                    IF @@ROWCOUNT <> 1 THROW 52013, 'E15: recuperacion de la venta incompleta.', 1;
                    DELETE FROM dbo.Tbl_Clientes
                    WHERE CodigoCliente = @Cliente15 AND NIT = @NIT15
                      AND Nombres = 'Cliente' AND Apellidos = 'Practica';
                    IF @@ROWCOUNT <> 1 THROW 52014, 'E15: recuperacion del cliente incompleta.', 1;
                END
                ELSE IF EXISTS (SELECT 1 FROM dbo.Tbl_Ventas
                                WHERE CodigoVenta = @Venta15 AND NumeroVenta = @Numero15)
                    THROW 52015, 'E15: venta de practica sin cliente esperado; revisar manualmente.', 1;
                COMMIT TRANSACTION;
                PRINT 'E15: limpieza recuperada; la causa original se informa a continuacion.';
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                SELECT @NIT15 AS NITPractica, @Numero15 AS NumeroVentaPractica,
                       @Cliente15 AS CodigoClientePractica, @Venta15 AS CodigoVentaPractica;
                THROW; -- conservar la copia aislada para recuperar solo estas llaves
            END CATCH;
        END;
        THROW;
    END CATCH;
    SET @Paso15 += 1;
END;
END
ELSE PRINT 'E15 omitido: requiere habilitacion explicita de la practica.';
SELECT Decision, Etapa, NIT, NumeroVenta, ExisteCliente, ExisteVenta
FROM @Evidencia15
ORDER BY CASE Decision WHEN 'NO' THEN 1 ELSE 2 END,
         CASE Etapa WHEN 'Antes' THEN 1 WHEN 'Temporal' THEN 2
                    WHEN 'Confirmada' THEN 3 ELSE 4 END;
GO
