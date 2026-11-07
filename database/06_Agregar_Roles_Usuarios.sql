/*
Migración de roles para Tbl_Usuarios.
Seleccionar explícitamente SupermercadosGuate o una copia aislada de roles
antes de ejecutar. No contiene contraseñas ni crea cuentas.
*/
SET XACT_ABORT ON;
IF DB_NAME() <> N'SupermercadosGuate'
   AND DB_NAME() NOT LIKE N'SupermercadosGuate[_]ROLES[_]PRUEBAS[_]%'
    THROW 50600, 'Selecciona SupermercadosGuate o una copia aislada de roles.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.Tbl_Usuarios', N'U') IS NULL
        THROW 50601, 'Tbl_Usuarios no existe.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.Tbl_Usuarios WHERE Usuario = N'David' AND Estado = N'Activo')
        THROW 50602, 'La cuenta activa David no existe; no se puede asignar Administrador automaticamente.', 1;

    IF COL_LENGTH(N'dbo.Tbl_Usuarios', N'Rol') IS NULL
        ALTER TABLE dbo.Tbl_Usuarios ADD Rol NVARCHAR(20) NULL;

    -- La compilación de este lote ocurre antes del ALTER TABLE. SQL dinámico
    -- compila las referencias a Rol después de agregarla y conserva la transacción.
    EXEC sys.sp_executesql N'
        IF EXISTS (SELECT 1 FROM dbo.Tbl_Usuarios WHERE Usuario = N''David'' AND Rol IS NOT NULL AND Rol <> N''Administrador'')
            THROW 50603, ''David ya tiene otro rol; revisar antes de continuar.'', 1;
        UPDATE dbo.Tbl_Usuarios SET Rol = N''Administrador''
        WHERE Usuario = N''David'' AND Rol IS NULL;
        IF EXISTS (SELECT 1 FROM dbo.Tbl_Usuarios WHERE Rol IS NULL)
            THROW 50604, ''Existen usuarios sin rol; asignarlos explicitamente antes de continuar.'', 1;
        IF EXISTS (SELECT 1 FROM dbo.Tbl_Usuarios WHERE Rol NOT IN (N''Administrador'', N''Bodega'', N''Caja''))
            THROW 50605, ''Existe un rol no permitido.'', 1;
    ';

    IF EXISTS (
        SELECT 1 FROM sys.columns
        WHERE object_id = OBJECT_ID(N'dbo.Tbl_Usuarios') AND name = N'Rol' AND is_nullable = 1
    )
        ALTER TABLE dbo.Tbl_Usuarios ALTER COLUMN Rol NVARCHAR(20) NOT NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE parent_object_id = OBJECT_ID(N'dbo.Tbl_Usuarios') AND name = N'CK_Usuarios_Rol')
        EXEC sys.sp_executesql N'ALTER TABLE dbo.Tbl_Usuarios WITH CHECK ADD CONSTRAINT CK_Usuarios_Rol
            CHECK (Rol IN (N''Administrador'', N''Bodega'', N''Caja''));';

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
