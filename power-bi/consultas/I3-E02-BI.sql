SELECT i.CodigoInventario, i.CodigoSucursal, i.CodigoProducto,
       i.Existencia, i.ExistenciaMinima,
       CASE WHEN i.Existencia < i.ExistenciaMinima
            THEN i.ExistenciaMinima-i.Existencia ELSE CAST(0 AS decimal(12,2)) END AS Faltante,
       CAST(ROUND((CASE WHEN i.Existencia<i.ExistenciaMinima THEN i.ExistenciaMinima-i.Existencia ELSE 0 END)*i.CostoPromedio,2) AS decimal(18,2)) AS ValorFaltante,
       CASE WHEN i.FechaUltimoIngreso IS NULL THEN NULL
            ELSE DATEDIFF(DAY,i.FechaUltimoIngreso,'20260930') END AS DiasSinIngreso,
       CASE WHEN i.FechaUltimoIngreso IS NULL THEN 'Sin ingreso'
            ELSE 'Con ingreso' END AS SituacionIngreso
FROM dbo.Tbl_Inventario AS i
WHERE i.Existencia<=i.ExistenciaMinima AND i.CostoPromedio>0
ORDER BY ValorFaltante DESC, i.CodigoInventario;
