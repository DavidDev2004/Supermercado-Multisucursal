# Publicación de entrega del 7 de noviembre de 2026

El usuario autorizó el respaldo final, commit/push y actualización de la versión pública conservando la configuración. Se aceptan como pendientes M05 (datos históricos), M07-E03 (Compras Pendientes) y M08-E07 (comparación de costos sin moneda). No se inventan fechas, costos ni estados para completarlos. La aprobación académica de las matrices y la modalidad de guía 10 sigue correspondiendo al docente.

## Respaldo final

- Archivo: `C:\BASE_DATOS\Docker\Backups\SupermercadosGuate_ENTREGA_20261107_1d75c781.bak`.
- Operación: `BACKUP DATABASE` con `COPY_ONLY`, `CHECKSUM` y `COMPRESSION`, en archivo nuevo. No se sobrescribió otro respaldo.
- Resultado: `RESTORE VERIFYONLY WITH CHECKSUM` correcto; archivo de 991232 bytes.
- SHA-256: `019243B8805B613C6D2320158B5B7D7EDC10AA9F066A3B01964C2759FF3E0C84`.
- No se restauró sobre la base activa ni se ejecutaron migraciones o DML. Falta una copia fuera del anfitrión y una restauración de prueba de este respaldo final si el docente la exige; VERIFYONLY no equivale a esa restauración.

David se confirmó activo como Administrador antes de actualizar. Una referencia privada de usuarios, estados, roles y huellas de sus hashes permite comprobar después que no se cambiaron cuentas ni contraseñas. No se publica esa referencia.

## Revisión previa

Los 31 KPI calculables volvieron a coincidir con SELECT independientes. Los nueve módulos BI y las consultas de catálogos, ventas, compras, pagos, inventario, ajustes y reportes respondieron; 90 comprobaciones HTTP locales sin fallos. El candidato Release arrancó separado con login y recursos BI accesibles. Se revisan el índice Git, sus exclusiones y los secretos conocidos antes de subir.

La consulta de diagnóstico incluida en la migración 03 ahora lista solo código, usuario y estado, sin mostrar PasswordHash. Esto solo modifica el archivo fuente: no se ejecutó la migración.

El resultado de actualización pública, verificaciones y commit se registrará al finalizar. Los documentos de preparación previos conservan su carácter de evidencia anterior a la autorización.
