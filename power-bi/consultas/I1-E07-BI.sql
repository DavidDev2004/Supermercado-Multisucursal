WITH ComprasDocumento AS (
    SELECT c.CodigoCompra, c.CodigoProveedor,
           SUM(d.Cantidad * d.CostoUnitario - d.Descuento) AS ImporteCompra
    FROM dbo.Tbl_Compras AS c
    JOIN dbo.Tbl_ComprasDetalle AS d ON d.CodigoCompra = c.CodigoCompra
    WHERE c.Estado = 'Recibida'
      AND c.FechaCompra >= '20260101' AND c.FechaCompra < '20261001'
    GROUP BY c.CodigoCompra, c.CodigoProveedor
)
SELECT pr.CodigoProveedor, pr.Nombre AS Proveedor,
       COUNT(*) AS CantidadCompras,
       CAST(ROUND(SUM(cd.ImporteCompra), 2) AS decimal(18,2)) AS TotalComprado,
       CAST(ROUND(AVG(CAST(cd.ImporteCompra AS decimal(18,4))), 2) AS decimal(18,2)) AS PromedioCompra
FROM ComprasDocumento AS cd
JOIN dbo.Tbl_Proveedores AS pr ON pr.CodigoProveedor = cd.CodigoProveedor
GROUP BY pr.CodigoProveedor, pr.Nombre
HAVING COUNT(*) >= 2 AND SUM(cd.ImporteCompra) > 1000
ORDER BY TotalComprado DESC, Proveedor;
