/*
Migracion independiente para futuros ajustes de inventario.
Aplicar solo con autorizacion y respaldo SQL verificado.
Los detalles anteriores conservan CostoUnitario = NULL: no existe una fuente
fiable para reconstruir su costo historico.
*/
USE [SupermercadosGuate];
GO

SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @TablaAjustesDetalle int = OBJECT_ID(N'dbo.Tbl_AjustesInventarioDetalle', N'U');
    IF @TablaAjustesDetalle IS NULL
        THROW 54001, 'No existe dbo.Tbl_AjustesInventarioDetalle; no se aplico la migracion.', 1;

    IF NOT EXISTS (
        SELECT 1 FROM sys.columns
        WHERE object_id = @TablaAjustesDetalle AND name = N'CostoUnitario'
    )
        EXEC(N'ALTER TABLE dbo.Tbl_AjustesInventarioDetalle
               ADD CostoUnitario DECIMAL(12,2) NULL;');

    IF NOT EXISTS (
        SELECT 1 FROM sys.columns
        WHERE object_id = @TablaAjustesDetalle AND name = N'CostoUnitario'
          AND system_type_id = 106 AND precision = 12 AND scale = 2
          AND is_nullable = 1
    )
        THROW 54002, 'CostoUnitario existe con un tipo o nulabilidad incompatible; revisar manualmente.', 1;

    IF NOT EXISTS (
        SELECT 1 FROM sys.check_constraints
        WHERE parent_object_id = @TablaAjustesDetalle
          AND name = N'CK_AjustesDetalle_CostoUnitario'
    )
        EXEC(N'ALTER TABLE dbo.Tbl_AjustesInventarioDetalle WITH CHECK
               ADD CONSTRAINT CK_AjustesDetalle_CostoUnitario
               CHECK (CostoUnitario IS NULL OR CostoUnitario > 0);');
    ELSE
        EXEC(N'ALTER TABLE dbo.Tbl_AjustesInventarioDetalle WITH CHECK
               CHECK CONSTRAINT CK_AjustesDetalle_CostoUnitario;');

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
