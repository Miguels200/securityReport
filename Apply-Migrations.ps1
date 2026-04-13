# =====================================================
# Script de Aplicación de Migraciones
# Security Report System
# =====================================================

Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "   SECURITY REPORT - APLICAR MIGRACIONES" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""

# Configuración
$InfrastructureProject = "src\Infrastructure"
$ApiProject = "src\Api"

# Verificar que dotnet CLI esté disponible
Write-Host "[1/5] Verificando .NET CLI..." -ForegroundColor Yellow
$dotnetVersion = dotnet --version
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ .NET CLI encontrado: $dotnetVersion" -ForegroundColor Green
} else {
    Write-Host "✗ Error: .NET CLI no encontrado" -ForegroundColor Red
    exit 1
}

# Verificar Entity Framework Core Tools
Write-Host ""
Write-Host "[2/5] Verificando EF Core Tools..." -ForegroundColor Yellow
$efVersion = dotnet ef --version 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ EF Core Tools encontrado" -ForegroundColor Green
    Write-Host $efVersion -ForegroundColor Gray
} else {
    Write-Host "✗ EF Core Tools no encontrado. Instalando..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ EF Core Tools instalado correctamente" -ForegroundColor Green
    } else {
        Write-Host "✗ Error al instalar EF Core Tools" -ForegroundColor Red
        exit 1
    }
}

# Listar migraciones disponibles
Write-Host ""
Write-Host "[3/5] Listando migraciones disponibles..." -ForegroundColor Yellow
dotnet ef migrations list --project $InfrastructureProject --startup-project $ApiProject --no-build

# Confirmar aplicación
Write-Host ""
Write-Host "[4/5] ¿Deseas aplicar las migraciones a la base de datos?" -ForegroundColor Yellow
Write-Host "      ADVERTENCIA: Esto modificará la estructura de la base de datos." -ForegroundColor Red
Write-Host ""
Write-Host "Opciones:" -ForegroundColor Cyan
Write-Host "  [1] Aplicar migraciones a la base de datos" -ForegroundColor White
Write-Host "  [2] Generar script SQL (sin aplicar)" -ForegroundColor White
Write-Host "  [3] Cancelar" -ForegroundColor White
Write-Host ""

$opcion = Read-Host "Selecciona una opción (1-3)"

switch ($opcion) {
    "1" {
        Write-Host ""
        Write-Host "[5/5] Aplicando migraciones..." -ForegroundColor Yellow
        dotnet ef database update --project $InfrastructureProject --startup-project $ApiProject --verbose
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host ""
            Write-Host "=================================================" -ForegroundColor Green
            Write-Host "   ✓ MIGRACIONES APLICADAS EXITOSAMENTE" -ForegroundColor Green
            Write-Host "=================================================" -ForegroundColor Green
        } else {
            Write-Host ""
            Write-Host "=================================================" -ForegroundColor Red
            Write-Host "   ✗ ERROR AL APLICAR MIGRACIONES" -ForegroundColor Red
            Write-Host "=================================================" -ForegroundColor Red
            exit 1
        }
    }
    "2" {
        Write-Host ""
        Write-Host "[5/5] Generando script SQL..." -ForegroundColor Yellow
        $scriptPath = "migration_script_$(Get-Date -Format 'yyyyMMdd_HHmmss').sql"
        dotnet ef migrations script --project $InfrastructureProject --startup-project $ApiProject --output $scriptPath
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host ""
            Write-Host "=================================================" -ForegroundColor Green
            Write-Host "   ✓ SCRIPT SQL GENERADO" -ForegroundColor Green
            Write-Host "=================================================" -ForegroundColor Green
            Write-Host "Archivo: $scriptPath" -ForegroundColor Cyan
        } else {
            Write-Host ""
            Write-Host "✗ Error al generar script SQL" -ForegroundColor Red
            exit 1
        }
    }
    "3" {
        Write-Host ""
        Write-Host "Operación cancelada por el usuario." -ForegroundColor Yellow
        exit 0
    }
    default {
        Write-Host ""
        Write-Host "Opción no válida. Operación cancelada." -ForegroundColor Red
        exit 1
    }
}

Write-Host ""
Write-Host "Presiona cualquier tecla para continuar..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
