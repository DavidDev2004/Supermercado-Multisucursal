WITH Totales AS (
    SELECT CodigoCompra,
           SUM(Cantidad * CostoUnitario - Descuento) AS ImporteCompra
    FROM dbo.Tbl_ComprasDetalle
    GROUP BY CodigoCompra
)
SELECT c.CodigoCompra, c.NumeroCompra, c.FechaCompra,
       pr.Nombre AS Proveedor, s.Nombre AS Sucursal,
       e.Nombres + ' ' + e.Apellidos AS Empleado,
       CAST(ROUND(t.ImporteCompra, 2) AS decimal(18,2)) AS ImporteCompra
FROM dbo.Tbl_Compras AS c
JOIN Totales AS t ON t.CodigoCompra = c.CodigoCompra
JOIN dbo.Tbl_Proveedores AS pr ON pr.CodigoProveedor = c.CodigoProveedor
JOIN dbo.Tbl_Sucursales AS s ON s.CodigoSucursal = c.CodigoSucursal
JOIN dbo.Tbl_Empleados AS e ON e.CodigoEmpleado = c.CodigoEmpleado
WHERE c.Estado = 'Recibida'
  AND c.FechaCompra >= '20260901' AND c.FechaCompra < '20261001'
ORDER BY ImporteCompra DESC, c.CodigoCompra;
