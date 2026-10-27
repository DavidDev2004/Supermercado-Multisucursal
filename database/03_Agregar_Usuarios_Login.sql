/*
Migración independiente del Login. Ejecutar solo tras autorización explícita.
Respaldo previo verificado: SupermercadosGuate_ANTES_LOGIN_20261025.bak.
Basada en Tbl_Usuarios del ZIP de la integrante; no incluye datos ni contraseñas.
*/
USE [SupermercadosGuate];
GO

IF OBJECT_ID(N'dbo.Tbl_Usuarios', N'U') IS NOT NULL
BEGIN
    THROW 50001, 'Tbl_Usuarios ya existe. Revisa su estructura antes de aplicar esta migración.', 1;
END;

CREATE TABLE dbo.Tbl_Usuarios
(
    CodigoUsuario INT IDENTITY(1,1) CONSTRAINT PK_Usuarios PRIMARY KEY,
    Usuario NVARCHAR(50) NOT NULL CONSTRAINT UQ_Usuarios_Usuario UNIQUE,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    Estado NVARCHAR(20) NOT NULL CONSTRAINT DF_Usuarios_Estado DEFAULT N'Activo'
);
GO
