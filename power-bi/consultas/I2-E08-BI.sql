SELECT
    C.CodigoCategoria,
    C.Nombre AS Categoria,
    YEAR(V.FechaVenta) AS Anio,
    MONTH(V.FechaVenta) AS Mes,
    SUM(D.Cantidad) AS UnidadesVendidas,
    CAST(ROUND(SUM(D.Cantidad*D.PrecioUnitario-D.Descuento),2) AS DECIMAL(18,2)) AS VentaNeta,
    CAST(ROUND(SUM((D.Cantidad*D.PrecioUnitario-D.Descuento)
        -(D.Cantidad*D.CostoUnitarioHistorico)),2) AS DECIMAL(18,2)) AS Utilidad
FROM Tbl_Ventas V
INNER JOIN Tbl_VentasDetalle D ON D.CodigoVenta=V.CodigoVenta
INNER JOIN Tbl_Productos P ON P.CodigoProducto=D.CodigoProducto
INNER JOIN Tbl_Categorias C ON C.CodigoCategoria=P.CodigoCategoria
WHERE V.Estado='Confirmada'
    AND V.FechaVenta>='20260101' AND V.FechaVenta<'20270101'
GROUP BY C.CodigoCategoria,C.Nombre,YEAR(V.FechaVenta),MONTH(V.FechaVenta)
HAVING COUNT(D.CodigoDetalleVenta)>=2
    AND SUM(D.Cantidad*D.PrecioUnitario-D.Descuento)>200
ORDER BY Anio ASC, Mes ASC, Categoria ASC;
