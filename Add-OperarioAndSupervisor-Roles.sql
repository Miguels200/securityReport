-- Script para agregar los roles Operario y Supervisor a la base de datos SecurityReportDB
-- Fecha: 2026-02-09

USE SecurityReportDB;
GO

-- Verificar si los roles ya existen y agregarlos si no están
IF NOT EXISTS (SELECT 1 FROM [dbo].[Roles] WHERE Id = '44444444-4444-4444-4444-444444444444')
BEGIN
    INSERT INTO [dbo].[Roles] (Id, Nombre)
    VALUES ('44444444-4444-4444-4444-444444444444', 'Operario');
    PRINT 'Rol "Operario" agregado exitosamente.';
END
ELSE
BEGIN
    PRINT 'El rol "Operario" ya existe.';
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Roles] WHERE Id = '55555555-5555-5555-5555-555555555555')
BEGIN
    INSERT INTO [dbo].[Roles] (Id, Nombre)
    VALUES ('55555555-5555-5555-5555-555555555555', 'Supervisor');
    PRINT 'Rol "Supervisor" agregado exitosamente.';
END
ELSE
BEGIN
    PRINT 'El rol "Supervisor" ya existe.';
END

-- Verificar todos los roles
SELECT * FROM [dbo].[Roles];
GO
