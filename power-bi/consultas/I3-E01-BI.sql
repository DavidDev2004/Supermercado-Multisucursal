SELECT p.CodigoProducto, UPPER(p.Nombre) AS Producto,
       LOWER(p.Marca) AS Marca,
       CAST(p.PrecioVenta AS decimal(18,2)) AS PrecioVenta,
       LEN(p.Nombre) AS LongitudNombre,
       DATEDIFF(DAY,p.FechaRegistro,'20260930') AS DiasRegistrado
FROM dbo.Tbl_Productos AS p
WHERE p.Estado='Activo' AND p.PrecioVenta BETWEEN 10 AND 100
  AND (p.Marca LIKE '%GT%' OR p.Marca LIKE '%Guatemala%')
ORDER BY DiasRegistrado DESC, Producto;
