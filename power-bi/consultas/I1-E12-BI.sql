SELECT c.CodigoCompra, c.NumeroCompra, c.CodigoSucursal,
       DATEDIFF(DAY, c.FechaEntregaProgramada, c.FechaRecepcion) AS DiasAtraso,
       CASE WHEN c.FechaRecepcion <= c.FechaEntregaProgramada THEN 'A tiempo'
            WHEN DATEDIFF(DAY, c.FechaEntregaProgramada, c.FechaRecepcion) <= 2 THEN 'Leve'
            WHEN DATEDIFF(DAY, c.FechaEntregaProgramada, c.FechaRecepcion) <= 5 THEN 'Medio'
            ELSE 'Alto' END AS NivelAtraso
FROM dbo.Tbl_Compras AS c
WHERE c.Estado = 'Recibida' AND c.FechaRecepcion IS NOT NULL
ORDER BY DiasAtraso DESC, c.NumeroCompra;
