-- Ajustes antiguos sin FechaAplicacion no se atribuyen a septiembre; no se inventan fechas.
WITH Movimientos AS (
    SELECT c.FechaRecepcion AS Fecha, c.NumeroCompra AS Documento,
           CAST('Compra recibida' AS varchar(20)) AS TipoEvento,
           c.CodigoSucursal,
           CAST(ROUND(SUM(d.Cantidad * d.CostoUnitario - d.Descuento),2) AS decimal(18,2)) AS Monto,
           c.Estado
    FROM dbo.Tbl_Compras AS c
    JOIN dbo.Tbl_ComprasDetalle AS d ON d.CodigoCompra = c.CodigoCompra
    WHERE c.Estado = 'Recibida' AND c.FechaRecepcion >= '20260901' AND c.FechaRecepcion < '20261001'
    GROUP BY c.CodigoCompra, c.FechaRecepcion, c.NumeroCompra, c.CodigoSucursal, c.Estado
    UNION ALL
    SELECT a.FechaAplicacion, a.NumeroAjuste, CAST('Ajuste aplicado' AS varchar(20)),
           a.CodigoSucursal,
           CASE WHEN COUNT(d.CostoUnitario) = COUNT(*)
                THEN CAST(ROUND(SUM(d.Cantidad * d.CostoUnitario), 2) AS decimal(18,2))
                ELSE NULL END,
           a.Estado
    FROM dbo.Tbl_AjustesInventario AS a
    JOIN dbo.Tbl_AjustesInventarioDetalle AS d ON d.CodigoAjuste = a.CodigoAjuste
    WHERE a.Estado = 'Aplicado' AND a.FechaAplicacion >= '20260901' AND a.FechaAplicacion < '20261001'
    GROUP BY a.CodigoAjuste, a.FechaAplicacion, a.NumeroAjuste, a.CodigoSucursal, a.Estado
)
SELECT Fecha, Documento, TipoEvento, CodigoSucursal, Monto, Estado
FROM Movimientos
ORDER BY Fecha, TipoEvento, Documento;
