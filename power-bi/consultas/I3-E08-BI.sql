SELECT a.Motivo, YEAR(a.FechaAjuste) AS Anio, MONTH(a.FechaAjuste) AS Mes,
             COUNT(*) AS CantidadLineas,
             SUM(ABS(d.Cantidad)) AS UnidadesPerdidas,
             CAST(ROUND(SUM(ABS(d.Cantidad)*d.CostoUnitario),2) AS decimal(18,2)) AS ValorPerdido
      FROM dbo.Tbl_AjustesInventario AS a
      JOIN dbo.Tbl_AjustesInventarioDetalle AS d ON d.CodigoAjuste=a.CodigoAjuste
      WHERE a.Estado='Aplicado' AND a.FechaAjuste>='20260101'
        AND a.FechaAjuste<'20270101' AND d.Cantidad<0
      GROUP BY a.Motivo,YEAR(a.FechaAjuste),MONTH(a.FechaAjuste)
      HAVING COUNT(*)>=2
         AND COUNT(d.CostoUnitario)=COUNT(*)
         AND SUM(ABS(d.Cantidad)*d.CostoUnitario)>20
      ORDER BY Anio, Mes, ValorPerdido DESC, a.Motivo;
