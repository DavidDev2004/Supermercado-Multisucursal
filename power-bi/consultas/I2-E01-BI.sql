SELECT
    CodigoCliente,
    Nombres+' '+Apellidos AS Cliente,
    Departamento,
    ISNULL(Correo,'No registrado') AS Correo,
    LEN(Nombres+' '+Apellidos) AS LongitudNombre,
    DATEDIFF(YEAR,FechaRegistro,'20260930')-
        CASE WHEN DATEADD(YEAR,DATEDIFF(YEAR,FechaRegistro,'20260930'),FechaRegistro)>'20260930'
             THEN 1 ELSE 0 END AS AntiguedadAnios
FROM Tbl_Clientes
WHERE Estado='Activo' AND FechaRegistro<'20260701'
    AND Departamento IN ('Guatemala','Quetzaltenango')
ORDER BY Departamento ASC, Cliente ASC;
