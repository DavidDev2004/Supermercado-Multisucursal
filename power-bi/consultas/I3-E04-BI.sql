SELECT p.CodigoProducto, p.Nombre AS Producto,
       i.CodigoSucursal, i.Existencia, i.ExistenciaMinima
FROM dbo.Tbl_Productos AS p
LEFT JOIN dbo.Tbl_Inventario AS i ON i.CodigoProducto=p.CodigoProducto
WHERE p.Estado='Activo'
ORDER BY Producto, i.CodigoSucursal;
