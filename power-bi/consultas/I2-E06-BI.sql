SELECT
    CodigoCliente,
    Nombres+' '+Apellidos AS Cliente,
    Departamento,
    Telefono
FROM Tbl_Clientes
WHERE Estado='Activo' AND CodigoCliente IN
    (SELECT CodigoCliente FROM Tbl_Ventas
     WHERE Estado='Confirmada' AND CodigoVenta IN
        (SELECT CodigoVenta FROM Tbl_Pagos
         WHERE Estado='Aplicado' AND Monto>
            (SELECT AVG(Monto) FROM Tbl_Pagos WHERE Estado='Aplicado')))
ORDER BY Apellidos ASC;
