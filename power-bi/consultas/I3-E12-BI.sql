SELECT i.CodigoInventario, i.CodigoSucursal, i.CodigoProducto,
       i.FechaUltimoIngreso,
       CASE WHEN i.FechaUltimoIngreso IS NULL THEN NULL
            ELSE DATEDIFF(DAY,i.FechaUltimoIngreso,'20260930') END AS DiasSinIngreso,
       CASE WHEN i.FechaUltimoIngreso IS NULL THEN 'Sin ingreso'
            WHEN DATEDIFF(DAY,i.FechaUltimoIngreso,'20260930')<=30 THEN 'Reciente'
            WHEN DATEDIFF(DAY,i.FechaUltimoIngreso,'20260930')<=90 THEN 'Intermedio'
            ELSE 'Antiguo' END AS NivelAntiguedad
FROM dbo.Tbl_Inventario AS i
ORDER BY DiasSinIngreso DESC, i.CodigoInventario;
