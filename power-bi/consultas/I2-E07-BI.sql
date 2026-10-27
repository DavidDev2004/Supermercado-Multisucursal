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
