WITH CompraronAntes AS (
    SELECT CodigoProveedor FROM dbo.Tbl_Compras
    WHERE Estado = 'Recibida' AND FechaCompra >= '20260101' AND FechaCompra < '20260701'
    EXCEPT
    SELECT CodigoProveedor FROM dbo.Tbl_Compras
    WHERE Estado = 'Recibida' AND FechaCompra >= '20260701' AND FechaCompra < '20261001'
),
SinCompras2026 AS (
    SELECT p.CodigoProveedor
    FROM dbo.Tbl_Proveedores AS p
    WHERE p.Estado = 'Activo'
      AND EXISTS (SELECT 1 FROM dbo.Tbl_ProductoProveedor pp
                  WHERE pp.CodigoProveedor = p.CodigoProveedor AND pp.Estado = 'Activo')
      AND NOT EXISTS (SELECT 1 FROM dbo.Tbl_Compras c
                      WHERE c.CodigoProveedor = p.CodigoProveedor
                        AND c.Estado = 'Recibida'
                        AND c.FechaCompra >= '20260101' AND c.FechaCompra < '20270101')
),
Unicos AS (
    SELECT CodigoProveedor FROM CompraronAntes
    UNION
    SELECT CodigoProveedor FROM SinCompras2026
)
SELECT p.CodigoProveedor, p.Nombre AS Proveedor
FROM Unicos AS u
JOIN dbo.Tbl_Proveedores AS p ON p.CodigoProveedor = u.CodigoProveedor
ORDER BY Proveedor;
