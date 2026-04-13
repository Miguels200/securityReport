# Script para agregar roles Operario y Supervisor a SecurityReportDB
# Requiere sqlcmd instalado

$server = "158.220.114.156"
$database = "SecurityReportDB"
$username = "Fisco"
$password = "Fisco2019+"
$sqlFile = "Add-OperarioAndSupervisor-Roles.sql"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Agregando roles Operario y Supervisor" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar si existe sqlcmd
$sqlcmdExists = Get-Command sqlcmd -ErrorAction SilentlyContinue

if ($sqlcmdExists) {
    Write-Host "Ejecutando script SQL..." -ForegroundColor Yellow
    
    sqlcmd -S $server -d $database -U $username -P $password -i $sqlFile
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "✓ Roles agregados exitosamente!" -ForegroundColor Green
        Write-Host ""
        Write-Host "Roles disponibles:" -ForegroundColor Cyan
        Write-Host "  1. Administrador (ID: 11111111-1111-1111-1111-111111111111)" -ForegroundColor White
        Write-Host "  2. ResponsableSST (ID: 22222222-2222-2222-2222-222222222222)" -ForegroundColor White
        Write-Host "  3. Colaborador (ID: 33333333-3333-3333-3333-333333333333)" -ForegroundColor White
        Write-Host "  4. Operario (ID: 44444444-4444-4444-4444-444444444444)" -ForegroundColor Green
        Write-Host "  5. Supervisor (ID: 55555555-5555-5555-5555-555555555555)" -ForegroundColor Green
    } else {
        Write-Host ""
        Write-Host "✗ Error al ejecutar el script SQL" -ForegroundColor Red
    }
} else {
    Write-Host "sqlcmd no está instalado." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Por favor ejecuta el script manualmente:" -ForegroundColor Cyan
    Write-Host "  1. Abre SQL Server Management Studio (SSMS) o Azure Data Studio" -ForegroundColor White
    Write-Host "  2. Conéctate al servidor: $server" -ForegroundColor White
    Write-Host "  3. Abre el archivo: $sqlFile" -ForegroundColor White
    Write-Host "  4. Ejecuta el script (F5)" -ForegroundColor White
}

Write-Host ""
Write-Host "Presiona cualquier tecla para continuar..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
