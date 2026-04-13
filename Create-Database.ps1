# =====================================================
# CREAR BASE DE DATOS - SecurityReport
# =====================================================

Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "   SECURITY REPORT - CREAR BASE DE DATOS" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Servidor: 158.220.114.156" -ForegroundColor Yellow
Write-Host "Base de datos: SecurityReportDB" -ForegroundColor Yellow
Write-Host ""

Write-Host "[1/3] Aplicando migraciones..." -ForegroundColor Green
dotnet ef database update --project src\Infrastructure --startup-project src\Api --verbose

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "=================================================" -ForegroundColor Green
    Write-Host "   ✓ BASE DE DATOS CREADA EXITOSAMENTE" -ForegroundColor Green
    Write-Host "=================================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "📊 Tablas creadas: 13" -ForegroundColor Cyan
    Write-Host "🔐 Roles seed: 3 (Administrador, ResponsableSST, Colaborador)" -ForegroundColor Cyan
    Write-Host "📋 Estados seed: 3 (Abierto, EnProgreso, Cerrado)" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "🎯 Siguiente paso: Ejecuta la API con F5" -ForegroundColor Yellow
} else {
    Write-Host ""
    Write-Host "=================================================" -ForegroundColor Red
    Write-Host "   ✗ ERROR AL CREAR LA BASE DE DATOS" -ForegroundColor Red
    Write-Host "=================================================" -ForegroundColor Red
}

Write-Host ""
Write-Host "Presiona cualquier tecla para continuar..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
