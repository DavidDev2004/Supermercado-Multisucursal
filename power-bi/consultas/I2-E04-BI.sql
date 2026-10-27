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
