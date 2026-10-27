SELECT e.CodigoEmpleado, e.CodigoSucursal,
       e.Nombres + ' ' + e.Apellidos AS Empleado,
       CAST(e.Salario AS decimal(18,2)) AS Salario
FROM dbo.Tbl_Empleados AS e
WHERE e.CodigoEmpleado = 1;
