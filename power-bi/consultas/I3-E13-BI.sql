WITH Sucursal AS (
    SELECT MIN(CodigoSucursal) AS CodigoSucursal FROM dbo.Tbl_Sucursales WHERE Estado = 'Activo'
)
SELECT s.CodigoSucursal, CAST(2 AS int) AS Tolerancia,
       COUNT(i.CodigoInventario) AS ProductosBajoMinimo,
       CASE WHEN COUNT(i.CodigoInventario) > 2 THEN 'Reposición prioritaria'
            ELSE 'Seguimiento normal' END AS EstadoReposicion
FROM Sucursal AS s
LEFT JOIN dbo.Tbl_Inventario AS i ON i.CodigoSucursal = s.CodigoSucursal
    AND i.Existencia < i.ExistenciaMinima
    AND EXISTS (SELECT 1 FROM dbo.Tbl_Productos AS p
                WHERE p.CodigoProducto = i.CodigoProducto AND p.Estado = 'Activo')
WHERE s.CodigoSucursal IS NOT NULL
GROUP BY s.CodigoSucursal;
