SELECT
    V.CodigoVenta,
    V.NumeroVenta,
    V.FechaVenta,
    C.Nombres+' '+C.Apellidos AS Cliente,
    S.Nombre AS Sucursal,
    E.Nombres+' '+E.Apellidos AS Empleado,
    CAST(ROUND((SELECT SUM(D.Cantidad*D.PrecioUnitario-D.Descuento)
           FROM Tbl_VentasDetalle D WHERE D.CodigoVenta=V.CodigoVenta),2) AS DECIMAL(18,2)) AS TotalVenta,
    CAST(ROUND(ISNULL((SELECT SUM(P.Monto) FROM Tbl_Pagos P
                  WHERE P.CodigoVenta=V.CodigoVenta AND P.Estado='Aplicado'),0),2) AS DECIMAL(18,2)) AS TotalPagado,
    CAST(ROUND((SELECT SUM(D.Cantidad*D.PrecioUnitario-D.Descuento)
           FROM Tbl_VentasDetalle D WHERE D.CodigoVenta=V.CodigoVenta)
          -ISNULL((SELECT SUM(P.Monto) FROM Tbl_Pagos P
                   WHERE P.CodigoVenta=V.CodigoVenta AND P.Estado='Aplicado'),0),2) AS DECIMAL(18,2)) AS Saldo
FROM Tbl_Ventas V
INNER JOIN Tbl_Clientes C ON C.CodigoCliente=V.CodigoCliente
INNER JOIN Tbl_Sucursales S ON S.CodigoSucursal=V.CodigoSucursal
INNER JOIN Tbl_Empleados E ON E.CodigoEmpleado=V.CodigoEmpleado
WHERE V.Estado='Confirmada' AND V.FechaVenta>='20260901' AND V.FechaVenta<'20261001'
ORDER BY Saldo DESC;
