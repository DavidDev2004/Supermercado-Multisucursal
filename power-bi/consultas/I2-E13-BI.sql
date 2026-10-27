WITH VentasDocumento AS (
    SELECT v.CodigoVenta, v.CodigoSucursal,
           SUM(d.Cantidad * d.PrecioUnitario - d.Descuento) AS Importe
    FROM dbo.Tbl_Ventas AS v
    JOIN dbo.Tbl_VentasDetalle AS d ON d.CodigoVenta = v.CodigoVenta
    WHERE v.Estado = 'Confirmada' AND v.FechaVenta >= '20260901' AND v.FechaVenta < '20261001'
    GROUP BY v.CodigoVenta, v.CodigoSucursal
)
SELECT CAST(1 AS int) AS CodigoSucursal, CAST(1000 AS decimal(18,2)) AS Meta,
       CAST(COALESCE(SUM(Importe), 0) AS decimal(18,2)) AS TotalVendido,
       CAST(CASE WHEN COALESCE(SUM(Importe), 0) >= 1000 THEN 0
                 ELSE 1000 - COALESCE(SUM(Importe), 0) END AS decimal(18,2)) AS Faltante,
       CASE WHEN COALESCE(SUM(Importe), 0) >= 1000 THEN 'Alcanzada'
            ELSE 'Pendiente' END AS EstadoMeta
FROM VentasDocumento WHERE CodigoSucursal = 1;
