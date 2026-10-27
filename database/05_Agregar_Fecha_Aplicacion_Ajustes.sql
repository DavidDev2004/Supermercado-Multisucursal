/*
Migracion independiente: fecha real de aplicacion de ajustes.
Aplicar solo con autorizacion y respaldo SQL verificado.
Los ajustes historicos conservan FechaAplicacion = NULL; no se infieren fechas.
Puede ejecutarse de nuevo sin alterar fechas ya registradas.
*/
USE [SupermercadosGuate];
GO

SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @TablaAjustes int = OBJECT_ID(N'dbo.Tbl_AjustesInventario', N'U');
    IF @TablaAjustes IS NULL
        THROW 54003, 'No existe dbo.Tbl_AjustesInventario; no se aplico la migracion.', 1;

    IF NOT EXISTS (
        SELECT 1 FROM sys.columns
        WHERE object_id = @TablaAjustes AND name = N'FechaAplicacion'
    )
        EXEC(N'ALTER TABLE dbo.Tbl_AjustesInventario
               ADD FechaAplicacion DATE NULL;');

    IF NOT EXISTS (
        SELECT 1 FROM sys.columns
        WHERE object_id = @TablaAjustes AND name = N'FechaAplicacion'
          AND system_type_id = TYPE_ID(N'date') AND is_nullable = 1
    )
        THROW 54004, 'FechaAplicacion existe con tipo o nulabilidad incompatible; revisar manualmente.', 1;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
