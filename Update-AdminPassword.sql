-- =====================================================
-- ACTUALIZAR CONTRASEÑA DEL ADMINISTRADOR
-- Email: admin@empresa.com
-- Nueva Contraseña: 123456
-- =====================================================

USE [SecurityReportDB];
GO

-- Actualizar la contraseña del usuario admin@empresa.com
UPDATE [Usuarios]
SET [PasswordHash] = 'AQAAAAIAAYagAAAAEKxqGZ9JxGxM2Y7nF8PqPxQwV3tV8pVZ8KxqGZ9JxGxM2Y7nF8PqPxQwV3tV8pVZ8Q==',
    [UpdatedAt] = GETUTCDATE()
WHERE [Email] = 'admin@empresa.com';

IF @@ROWCOUNT > 0
BEGIN
    PRINT '================================================';
    PRINT '   ✓ CONTRASEÑA ACTUALIZADA';
    PRINT '================================================';
    PRINT '';
    PRINT 'Nueva contraseña para: admin@empresa.com';
    PRINT 'Contraseña: 123456';
    PRINT '';
END
ELSE
BEGIN
    PRINT '================================================';
    PRINT '   ⚠ USUARIO NO ENCONTRADO';
    PRINT '================================================';
    PRINT '';
    PRINT 'El usuario admin@empresa.com no existe.';
    PRINT 'Ejecuta primero: Create-AdminUser.sql';
    PRINT '';
END
GO

-- Mostrar información del usuario
SELECT 
    u.[Nombre] AS [Nombre],
    u.[Email] AS [Email],
    r.[Nombre] AS [Rol],
    u.[UpdatedAt] AS [Última Actualización]
FROM [Usuarios] u
INNER JOIN [Roles] r ON u.[RolId] = r.[Id]
WHERE u.[Email] = 'admin@empresa.com';
GO
