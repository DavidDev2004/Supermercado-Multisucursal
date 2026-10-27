SELECT p.CodigoProducto, p.CodigoCategoria, p.Nombre AS Producto,
       CAST(p.PrecioVenta AS decimal(18,2)) AS PrecioVenta
FROM dbo.Tbl_Productos AS p
WHERE p.Estado='Activo'
  AND p.PrecioVenta > (
      SELECT AVG(CAST(p2.PrecioVenta AS decimal(18,4)))
      FROM dbo.Tbl_Productos AS p2
      WHERE p2.CodigoCategoria=p.CodigoCategoria AND p2.Estado='Activo'
  )
ORDER BY p.CodigoCategoria, p.PrecioVenta DESC, p.CodigoProducto;
