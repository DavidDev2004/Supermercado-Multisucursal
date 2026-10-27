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
