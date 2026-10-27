WITH SoloA AS (
        SELECT CodigoProducto FROM dbo.Tbl_Inventario
        WHERE CodigoSucursal=(SELECT MIN(CodigoSucursal) FROM dbo.Tbl_Sucursales) AND Existencia>0
        EXCEPT
        SELECT CodigoProducto FROM dbo.Tbl_Inventario
        WHERE CodigoSucursal=(SELECT MIN(CodigoSucursal) FROM dbo.Tbl_Sucursales WHERE CodigoSucursal <> (SELECT MIN(CodigoSucursal) FROM dbo.Tbl_Sucursales)) AND Existencia>0
    ),
    SoloB AS (
        SELECT CodigoProducto FROM dbo.Tbl_Inventario
        WHERE CodigoSucursal=(SELECT MIN(CodigoSucursal) FROM dbo.Tbl_Sucursales WHERE CodigoSucursal <> (SELECT MIN(CodigoSucursal) FROM dbo.Tbl_Sucursales)) AND Existencia>0
        EXCEPT
        SELECT CodigoProducto FROM dbo.Tbl_Inventario
        WHERE CodigoSucursal=(SELECT MIN(CodigoSucursal) FROM dbo.Tbl_Sucursales) AND Existencia>0
    ),
    Exclusivos AS (
        SELECT CodigoProducto, CAST('Solo sucursal 1' AS varchar(30)) AS Disponibilidad FROM SoloA
        UNION ALL
        SELECT CodigoProducto, CAST('Solo sucursal 2' AS varchar(30)) AS Disponibilidad FROM SoloB
    )
    SELECT p.CodigoProducto, p.Nombre AS Producto, x.Disponibilidad
    FROM Exclusivos AS x
    JOIN dbo.Tbl_Productos AS p ON p.CodigoProducto=x.CodigoProducto
    ORDER BY x.Disponibilidad, Producto, p.CodigoProducto;
