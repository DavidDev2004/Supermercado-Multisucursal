WITH Practicas AS (
    SELECT v.Decision, v.NumeroCompra
    FROM (VALUES
        ('NO', 'I1-E15-3056D4F7E68445568C069E'),
        ('SI', 'I1-E15-E7F1AC0CB0BB416AB825B7')
    ) AS v(Decision, NumeroCompra)
)
SELECT p.Decision, p.NumeroCompra,
       CASE WHEN c.CodigoCompra IS NULL THEN 0 ELSE 1 END AS ExisteCompra,
       (SELECT COUNT(*) FROM dbo.Tbl_ComprasDetalle AS d WHERE d.CodigoCompra = c.CodigoCompra) AS CantidadLineas,
       CAST(COALESCE((SELECT SUM(d.Cantidad * d.CostoUnitario - d.Descuento)
                      FROM dbo.Tbl_ComprasDetalle AS d WHERE d.CodigoCompra = c.CodigoCompra), 0) AS decimal(18,2)) AS ImporteCompra
FROM Practicas AS p
LEFT JOIN dbo.Tbl_Compras AS c ON c.NumeroCompra = p.NumeroCompra;
