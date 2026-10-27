SELECT
    V.CodigoVenta,
    V.NumeroVenta,
    V.CodigoSucursal,
    CAST(ROUND(SUM(D.Cantidad*D.PrecioUnitario-D.Descuento),2) AS DECIMAL(18,2)) AS TotalVenta,
    CASE
        WHEN SUM(D.Cantidad*D.PrecioUnitario-D.Descuento)<100 THEN 'Baja'
        WHEN SUM(D.Cantidad*D.PrecioUnitario-D.Descuento)<=500 THEN 'Media'
        ELSE 'Alta'
    END AS NivelVenta
FROM Tbl_Ventas V
INNER JOIN Tbl_VentasDetalle D ON D.CodigoVenta=V.CodigoVenta
WHERE V.Estado='Confirmada'
GROUP BY V.CodigoVenta,V.NumeroVenta,V.CodigoSucursal
ORDER BY TotalVenta DESC;
