SELECT c.CodigoCompra, c.NumeroCompra,
       YEAR(c.FechaCompra) AS Anio, MONTH(c.FechaCompra) AS Mes,
       c.FechaEntregaProgramada, c.FechaRecepcion,
       DATEDIFF(DAY, c.FechaEntregaProgramada, c.FechaRecepcion) AS DiasAtraso,
       LEN(c.NumeroCompra) AS LongitudNumero
FROM dbo.Tbl_Compras AS c
WHERE c.Estado = 'Recibida'
  AND c.FechaCompra >= '20260701' AND c.FechaCompra < '20261001'
  AND c.FechaRecepcion > c.FechaEntregaProgramada
ORDER BY DiasAtraso DESC, c.NumeroCompra;
