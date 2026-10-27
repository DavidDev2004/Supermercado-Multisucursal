SELECT p.CodigoCategoria, ca.Nombre AS Categoria,
       YEAR(c.FechaCompra) AS Anio, MONTH(c.FechaCompra) AS Mes,
       COUNT(*) AS CantidadLineas,
       SUM(d.Cantidad) AS UnidadesCompradas,
       CAST(ROUND(SUM(d.Cantidad * d.CostoUnitario - d.Descuento), 2) AS decimal(18,2)) AS CostoNeto
FROM dbo.Tbl_Compras AS c
JOIN dbo.Tbl_ComprasDetalle AS d ON d.CodigoCompra = c.CodigoCompra
JOIN dbo.Tbl_Productos AS p ON p.CodigoProducto = d.CodigoProducto
JOIN dbo.Tbl_Categorias AS ca ON ca.CodigoCategoria = p.CodigoCategoria
WHERE c.Estado = 'Recibida' AND c.FechaCompra >= '20260101' AND c.FechaCompra < '20270101'
GROUP BY p.CodigoCategoria, ca.Nombre, YEAR(c.FechaCompra), MONTH(c.FechaCompra)
HAVING COUNT(*) >= 2
   AND SUM(d.Cantidad * d.CostoUnitario - d.Descuento) > 500
ORDER BY Anio, Mes, Categoria;
