SELECT i.CodigoInventario, s.CodigoSucursal, s.Nombre AS Sucursal,
       p.CodigoProducto, p.Nombre AS Producto, ca.Nombre AS Categoria,
       i.Existencia,
       CAST(ROUND(i.Existencia*i.CostoPromedio,2) AS decimal(18,2)) AS ValorCosto,
       CAST(ROUND(i.Existencia*p.PrecioVenta,2) AS decimal(18,2)) AS ValorVenta
FROM dbo.Tbl_Inventario AS i
JOIN dbo.Tbl_Sucursales AS s ON s.CodigoSucursal=i.CodigoSucursal AND s.Estado='Activo'
JOIN dbo.Tbl_Productos AS p ON p.CodigoProducto=i.CodigoProducto AND p.Estado='Activo'
JOIN dbo.Tbl_Categorias AS ca ON ca.CodigoCategoria=p.CodigoCategoria AND ca.Estado='Activo'
ORDER BY ValorCosto DESC, Sucursal, Producto;
