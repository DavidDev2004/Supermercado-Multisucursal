WITH Practicas AS (
    SELECT v.Decision, v.NumeroAjuste
    FROM (VALUES
        ('NO', 'I3-E15-E164A13A540348E8A8B38F'),
        ('SI', 'I3-E15-7F01845DDB7B4090BFA76E')
    ) AS v(Decision, NumeroAjuste)
)
SELECT p.Decision, p.NumeroAjuste,
       CASE WHEN a.CodigoAjuste IS NULL THEN 0 ELSE 1 END AS ExisteAjuste,
       (SELECT COUNT(*) FROM dbo.Tbl_AjustesInventarioDetalle AS d WHERE d.CodigoAjuste = a.CodigoAjuste) AS CantidadLineas,
       CAST(NULL AS decimal(18,2)) AS Antes,
       (SELECT i.Existencia FROM dbo.Tbl_Inventario AS i WHERE i.CodigoInventario = 1) AS Despues
FROM Practicas AS p
LEFT JOIN dbo.Tbl_AjustesInventario AS a ON a.NumeroAjuste = p.NumeroAjuste;
