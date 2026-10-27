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
