SELECT pp.CodigoProductoProveedor, pp.CodigoProveedor, pp.CodigoProducto,
       pr.Nombre AS Proveedor, p.Nombre AS Producto,
       CAST(pp.CostoReferencia AS decimal(18,2)) AS CostoReferencia
FROM dbo.Tbl_ProductoProveedor AS pp
JOIN dbo.Tbl_Proveedores AS pr ON pr.CodigoProveedor = pp.CodigoProveedor
JOIN dbo.Tbl_Productos AS p ON p.CodigoProducto = pp.CodigoProducto
WHERE pp.CodigoProductoProveedor = 1;
