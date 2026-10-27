-- No se usa JOIN en la consulta principal; los EXISTS preservan una fila por sucursal.
SELECT s.CodigoSucursal, s.Nombre AS Sucursal, s.Municipio, s.Departamento
FROM dbo.Tbl_Sucursales AS s
WHERE s.Estado='Activo'
  AND EXISTS (
      SELECT 1 FROM dbo.Tbl_AjustesInventario AS a
      WHERE a.CodigoSucursal=s.CodigoSucursal AND a.Estado='Aplicado'
        AND a.FechaAjuste >= '20260901' AND a.FechaAjuste < '20261001'
        AND EXISTS (
            SELECT 1 FROM dbo.Tbl_AjustesInventarioDetalle AS d
            WHERE d.CodigoAjuste=a.CodigoAjuste AND d.Cantidad<0
              AND d.CodigoProducto IN (
                  SELECT CodigoProducto FROM dbo.Tbl_Productos WHERE Estado='Activo'
              )
        )
  )
ORDER BY s.Departamento, Sucursal;
