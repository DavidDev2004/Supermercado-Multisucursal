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
