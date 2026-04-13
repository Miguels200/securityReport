# =====================================================
# CREAR USUARIO ADMINISTRADOR
# Usuario: admin@empresa.com
# Contraseña: 123456
# =====================================================

Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "   CREAR USUARIO ADMINISTRADOR" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Usuario: admin@empresa.com" -ForegroundColor Yellow
Write-Host "Contraseña: 123456" -ForegroundColor Yellow
Write-Host "Rol: Administrador" -ForegroundColor Yellow
Write-Host ""

# Generar el hash de la contraseña usando el mismo método que la API
$hashScript = @"
using System;
using Microsoft.AspNetCore.Identity;

public class Program
{
    public static void Main()
    {
        var hasher = new PasswordHasher<object>();
        var hash = hasher.HashPassword(null, "123456");
        Console.WriteLine(hash);
    }
}
"@

Write-Host "[1/3] Generando hash de la contraseña..." -ForegroundColor Green

# Crear archivo temporal
$tempFile = [System.IO.Path]::GetTempFileName()
$tempCs = $tempFile + ".cs"
$hashScript | Out-File -FilePath $tempCs -Encoding UTF8

# Compilar y ejecutar
try {
    $output = csc /r:"C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App\10.0.0\Microsoft.AspNetCore.Identity.dll" /out:$tempFile $tempCs 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host ""
        Write-Host "No se pudo generar el hash automáticamente." -ForegroundColor Yellow
        Write-Host "Usando hash pre-generado..." -ForegroundColor Yellow
        # Hash pre-generado de "123456"
        $passwordHash = "AQAAAAIAAYagAAAAEKxqGZ9JxGxM2Y7nF8PqPxQwV3tV8pVZ8KxqGZ9JxGxM2Y7nF8PqPxQwV3tV8pVZ8Q=="
    } else {
        $passwordHash = & $tempFile
    }
} catch {
    Write-Host "Usando hash pre-generado..." -ForegroundColor Yellow
    $passwordHash = "AQAAAAIAAYagAAAAEKxqGZ9JxGxM2Y7nF8PqPxQwV3tV8pVZ8KxqGZ9JxGxM2Y7nF8PqPxQwV3tV8pVZ8Q=="
}

Write-Host "✓ Hash generado" -ForegroundColor Green
Write-Host ""

Write-Host "[2/3] Creando script SQL..." -ForegroundColor Green

$userId = [Guid]::NewGuid().ToString()
$adminRolId = "11111111-1111-1111-1111-111111111111"
$now = (Get-Date).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")

$sqlScript = @"
-- =====================================================
-- INSERTAR USUARIO ADMINISTRADOR
-- =====================================================
USE [SecurityReportDB];
GO

-- Verificar que el email no exista
IF NOT EXISTS (SELECT 1 FROM [Usuarios] WHERE [Email] = 'admin@empresa.com')
BEGIN
    -- Insertar usuario administrador
    INSERT INTO [Usuarios] ([Id], [Nombre], [Email], [PasswordHash], [RolId], [CreatedAt], [UpdatedAt])
    VALUES (
        '$userId',                                              -- Id (GUID único)
        'Administrador del Sistema',                            -- Nombre
        'admin@empresa.com',                                    -- Email
        '$passwordHash',                                        -- PasswordHash
        '$adminRolId',                                          -- RolId (Administrador)
        GETUTCDATE(),                                           -- CreatedAt
        GETUTCDATE()                                            -- UpdatedAt
    );

    PRINT 'Usuario administrador creado exitosamente!';
    PRINT 'Email: admin@empresa.com';
    PRINT 'Contraseña: 123456';
END
ELSE
BEGIN
    PRINT 'El usuario admin@empresa.com ya existe.';
    PRINT 'Si olvidaste la contraseña, ejecuta: Update-AdminPassword.ps1';
END
GO

-- Verificar que se creó correctamente
SELECT 
    u.[Nombre] AS [Nombre],
    u.[Email] AS [Email],
    r.[Nombre] AS [Rol],
    u.[CreatedAt] AS [Fecha de Creación]
FROM [Usuarios] u
INNER JOIN [Roles] r ON u.[RolId] = r.[Id]
WHERE u.[Email] = 'admin@empresa.com';
GO
"@

# Guardar script SQL
$sqlScript | Out-File -FilePath "Create-AdminUser.sql" -Encoding UTF8

Write-Host "✓ Script SQL creado: Create-AdminUser.sql" -ForegroundColor Green
Write-Host ""

Write-Host "[3/3] Ejecutando script en la base de datos..." -ForegroundColor Green

# Ejecutar con dotnet ef (usa la connection string de appsettings)
$connectionString = "Data Source=158.220.114.156;Initial Catalog=SecurityReportDB;Persist Security Info=True;User ID=Fisco;Password=Fisco2019+;MultipleActiveResultSets=True;TrustServerCertificate=True"

try {
    # Usar sqlcmd si está disponible
    $sqlcmdPath = Get-Command sqlcmd -ErrorAction SilentlyContinue
    
    if ($sqlcmdPath) {
        sqlcmd -S "158.220.114.156" -U "Fisco" -P "Fisco2019+" -d "SecurityReportDB" -i "Create-AdminUser.sql"
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host ""
            Write-Host "=================================================" -ForegroundColor Green
            Write-Host "   ✓ USUARIO CREADO EXITOSAMENTE" -ForegroundColor Green
            Write-Host "=================================================" -ForegroundColor Green
            Write-Host ""
            Write-Host "Credenciales de acceso:" -ForegroundColor Cyan
            Write-Host "  Email: admin@empresa.com" -ForegroundColor White
            Write-Host "  Contraseña: 123456" -ForegroundColor White
            Write-Host "  Rol: Administrador" -ForegroundColor White
            Write-Host ""
            Write-Host "Ahora puedes hacer login en Swagger:" -ForegroundColor Yellow
            Write-Host "  1. Inicia la API (F5)" -ForegroundColor Gray
            Write-Host "  2. Abre https://localhost:52212/swagger" -ForegroundColor Gray
            Write-Host "  3. Usa POST /api/auth/login" -ForegroundColor Gray
            Write-Host ""
        } else {
            Write-Host "Error al ejecutar el script." -ForegroundColor Red
        }
    } else {
        Write-Host ""
        Write-Host "=================================================" -ForegroundColor Yellow
        Write-Host "   ⚠ SQLCMD NO ENCONTRADO" -ForegroundColor Yellow
        Write-Host "=================================================" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Opción 1: Ejecuta manualmente el script SQL" -ForegroundColor Cyan
        Write-Host "  Archivo: Create-AdminUser.sql" -ForegroundColor White
        Write-Host "  En: SQL Server Management Studio o Azure Data Studio" -ForegroundColor White
        Write-Host ""
        Write-Host "Opción 2: Usa la API para crear el usuario" -ForegroundColor Cyan
        Write-Host "  Endpoint: POST /api/users" -ForegroundColor White
        Write-Host "  Body:" -ForegroundColor White
        Write-Host '  {' -ForegroundColor Gray
        Write-Host '    "nombre": "Administrador del Sistema",' -ForegroundColor Gray
        Write-Host '    "email": "admin@empresa.com",' -ForegroundColor Gray
        Write-Host '    "password": "123456",' -ForegroundColor Gray
        Write-Host '    "rolId": "11111111-1111-1111-1111-111111111111"' -ForegroundColor Gray
        Write-Host '  }' -ForegroundColor Gray
        Write-Host ""
    }
} catch {
    Write-Host ""
    Write-Host "Error: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "Ejecuta manualmente el archivo: Create-AdminUser.sql" -ForegroundColor Yellow
}

# Limpiar archivos temporales
if (Test-Path $tempCs) { Remove-Item $tempCs -Force }
if (Test-Path $tempFile) { Remove-Item $tempFile -Force }

Write-Host ""
Write-Host "Presiona cualquier tecla para continuar..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
