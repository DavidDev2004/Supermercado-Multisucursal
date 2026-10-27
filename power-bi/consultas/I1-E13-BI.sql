WITH Sucursal AS (
    SELECT MIN(CodigoSucursal) AS CodigoSucursal FROM dbo.Tbl_Sucursales WHERE Estado = 'Activo'
), ComprasDocumento AS (
    SELECT c.CodigoCompra, c.CodigoSucursal,
           SUM(d.Cantidad * d.CostoUnitario - d.Descuento) AS Importe
    FROM dbo.Tbl_Compras AS c
    JOIN dbo.Tbl_ComprasDetalle AS d ON d.CodigoCompra = c.CodigoCompra
    WHERE c.Estado = 'Recibida' AND c.FechaCompra >= '20260901' AND c.FechaCompra < '20261001'
    GROUP BY c.CodigoCompra, c.CodigoSucursal
)
SELECT s.CodigoSucursal, CAST(10000 AS decimal(18,2)) AS Presupuesto,
       CAST(COALESCE(SUM(cd.Importe), 0) AS decimal(18,2)) AS TotalComprado,
       CAST(10000 - COALESCE(SUM(cd.Importe), 0) AS decimal(18,2)) AS Diferencia,
       CASE WHEN COALESCE(SUM(cd.Importe), 0) > 10000 THEN 'Excedido'
            ELSE 'Dentro del presupuesto' END AS EstadoPresupuesto
FROM Sucursal AS s
LEFT JOIN ComprasDocumento AS cd ON cd.CodigoSucursal = s.CodigoSucursal
WHERE s.CodigoSucursal IS NOT NULL
GROUP BY s.CodigoSucursal;
