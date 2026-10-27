WITH Practicas AS (
    SELECT v.Decision, v.NIT, v.NumeroVenta
    FROM (VALUES
        ('NO', 'I2E15638B1CE7EB1542', 'I2-E15-3243CE0A052A4894844FA9'),
        ('SI', 'I2E1548F48CBA74FE4B', 'I2-E15-9287DE7FEC6C4195A2A587')
    ) AS v(Decision, NIT, NumeroVenta)
)
SELECT p.Decision, p.NIT, p.NumeroVenta,
       CASE WHEN c.CodigoCliente IS NULL THEN 0 ELSE 1 END AS ExisteCliente,
       CASE WHEN v.CodigoVenta IS NULL THEN 0 ELSE 1 END AS ExisteVenta
FROM Practicas AS p
LEFT JOIN dbo.Tbl_Clientes AS c ON c.NIT = p.NIT
LEFT JOIN dbo.Tbl_Ventas AS v ON v.NumeroVenta = p.NumeroVenta;
