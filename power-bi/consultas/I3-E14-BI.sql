SELECT i.CodigoInventario, i.CodigoSucursal, i.CodigoProducto,
       i.Existencia, i.ExistenciaMinima
FROM dbo.Tbl_Inventario AS i
WHERE i.CodigoInventario = 1;
