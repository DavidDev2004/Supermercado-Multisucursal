SELECT s.CodigoSucursal, s.Nombre AS Sucursal,
       COUNT(DISTINCT i.CodigoProducto) AS CantidadProductos,
       SUM(i.Existencia) AS TotalUnidades,
       CAST(ROUND(SUM(i.Existencia*i.CostoPromedio),2) AS decimal(18,2)) AS ValorInventario
FROM dbo.Tbl_Inventario AS i
JOIN dbo.Tbl_Sucursales AS s ON s.CodigoSucursal=i.CodigoSucursal AND s.Estado='Activo'
JOIN dbo.Tbl_Productos AS p ON p.CodigoProducto=i.CodigoProducto AND p.Estado='Activo'
GROUP BY s.CodigoSucursal, s.Nombre
HAVING COUNT(DISTINCT i.CodigoProducto)>=3
   AND SUM(i.Existencia*i.CostoPromedio)>500
ORDER BY ValorInventario DESC, Sucursal;
