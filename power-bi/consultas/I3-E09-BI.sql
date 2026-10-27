SELECT Municipio, Departamento FROM dbo.Tbl_Sucursales WHERE Estado='Activo'
UNION
SELECT Municipio, Departamento FROM dbo.Tbl_Proveedores WHERE Estado='Activo'
ORDER BY Departamento, Municipio;
