WITH Compartidos AS (
        SELECT CodigoProducto FROM dbo.Tbl_Inventario
        WHERE CodigoSucursal=(SELECT MIN(CodigoSucursal) FROM dbo.Tbl_Sucursales) AND Existencia>0
        INTERSECT
        SELECT CodigoProducto FROM dbo.Tbl_Inventario
        WHERE CodigoSucursal=(SELECT MIN(CodigoSucursal) FROM dbo.Tbl_Sucursales WHERE CodigoSucursal <> (SELECT MIN(CodigoSucursal) FROM dbo.Tbl_Sucursales)) AND Existencia>0
    )
    SELECT p.CodigoProducto, p.Nombre AS Producto
    FROM Compartidos AS c
    JOIN dbo.Tbl_Productos AS p ON p.CodigoProducto=c.CodigoProducto
    ORDER BY Producto, p.CodigoProducto;
