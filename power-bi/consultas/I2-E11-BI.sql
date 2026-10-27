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
