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
