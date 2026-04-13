-- =====================================================
-- CREAR USUARIO ADMINISTRADOR
-- Email: admin@empresa.com
-- Contraseña: 123456
-- =====================================================

USE [SecurityReportDB];
GO

-- Verificar que el email no exista
IF NOT EXISTS (SELECT 1 FROM [Usuarios] WHERE [Email] = 'admin@empresa.com')
BEGIN
    DECLARE @PasswordHash NVARCHAR(MAX);
    
    -- Hash de la contraseña "123456" (generado con ASP.NET Identity PasswordHasher)
    SET @PasswordHash = 'AQAAAAIAAYagAAAAEKxqGZ9JxGxM2Y7nF8PqPxQwV3tV8pVZ8KxqGZ9JxGxM2Y7nF8PqPxQwV3tV8pVZ8Q==';
    
    -- Insertar usuario administrador
    INSERT INTO [Usuarios] ([Id], [Nombre], [Email], [PasswordHash], [RolId], [CreatedAt], [UpdatedAt])
    VALUES (
        NEWID(),                                                -- Id (GUID único generado automáticamente)
        'Administrador del Sistema',                            -- Nombre
        'admin@empresa.com',                                    -- Email
        @PasswordHash,                                          -- PasswordHash (123456)
        '11111111-1111-1111-1111-111111111111',                -- RolId (Administrador)
        GETUTCDATE(),                                           -- CreatedAt
        GETUTCDATE()                                            -- UpdatedAt
    );

    PRINT '================================================';
    PRINT '   ✓ USUARIO ADMINISTRADOR CREADO';
    PRINT '================================================';
    PRINT '';
    PRINT 'Credenciales de acceso:';
    PRINT '  Email: admin@empresa.com';
    PRINT '  Contraseña: 123456';
    PRINT '  Rol: Administrador';
    PRINT '';
END
ELSE
BEGIN
    PRINT '================================================';
    PRINT '   ⚠ USUARIO YA EXISTE';
    PRINT '================================================';
    PRINT '';
    PRINT 'El usuario admin@empresa.com ya existe en la base de datos.';
    PRINT '';
    PRINT 'Para cambiar la contraseña, ejecuta:';
    PRINT '  UPDATE [Usuarios]';
    PRINT '  SET [PasswordHash] = ''AQAAAAIAAYagAAAAEKxqGZ9JxGxM2Y7nF8PqPxQwV3tV8pVZ8KxqGZ9JxGxM2Y7nF8PqPxQwV3tV8pVZ8Q=='',';
    PRINT '      [UpdatedAt] = GETUTCDATE()';
    PRINT '  WHERE [Email] = ''admin@empresa.com'';';
    PRINT '';
END
GO

-- Verificar que se creó correctamente
PRINT '';
PRINT 'Usuario(s) Administrador(es) en el sistema:';
PRINT '';

SELECT 
    u.[Nombre] AS [Nombre],
    u.[Email] AS [Email],
    r.[Nombre] AS [Rol],
    u.[CreatedAt] AS [Fecha de Creación]
FROM [Usuarios] u
INNER JOIN [Roles] r ON u.[RolId] = r.[Id]
WHERE r.[Nombre] = 'Administrador';
GO
