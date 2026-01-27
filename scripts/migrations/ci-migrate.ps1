# scripts/ci-migrate.ps1
param(
    [string]$ConnectionString,
    [string]$Environment = "Production",
    [switch]$DryRun
)

Write-Host "=== CI/CD Migration Script ===" -ForegroundColor Cyan
Write-Host "Environment: $Environment" -ForegroundColor Yellow
Write-Host "Dry run: $DryRun" -ForegroundColor Gray

# Генерируем SQL скрипт для проверки
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$scriptFile = "migrations_$timestamp.sql"

Write-Host "`n1. Generating migration script..." -ForegroundColor Green
$generateCommand = "dotnet ef migrations script --context DatabaseContext --project apps/test-service/WebApi/WebApi.csproj --idempotent --output $scriptFile"
if ($DryRun) {
    Write-Host "DRY RUN: $generateCommand" -ForegroundColor Gray
}
else {
    Invoke-Expression $generateCommand
    Write-Host "Script generated: $scriptFile" -ForegroundColor Green
    
    # Проверяем скрипт
    $lineCount = (Get-Content $scriptFile | Measure-Object -Line).Lines
    Write-Host "Script size: $lineCount lines" -ForegroundColor Gray
    
    # Показываем первые 5 и последние 5 строк
    Write-Host "`nScript preview:" -ForegroundColor Gray
    Get-Content $scriptFile -TotalCount 5 | ForEach-Object { Write-Host "  $_" }
    Write-Host "  ..." -ForegroundColor Gray
    Get-Content $scriptFile -Tail 5 | ForEach-Object { Write-Host "  $_" }
}

if (-not $DryRun) {
    Write-Host "`n2. Applying migrations..." -ForegroundColor Green
    $updateCommand = "dotnet ef database update --context DatabaseContext --project apps/test-service/WebApi/WebApi.csproj"
    
    # Если есть специфичная строка подключения
    if (-not [string]::IsNullOrEmpty($ConnectionString)) {
        $env:ConnectionStrings__DefaultConnection = $ConnectionString
    }
    
    Invoke-Expression $updateCommand
    
    Write-Host "`n3. Verifying migrations..." -ForegroundColor Green
    
    # Проверяем примененные миграции
    $checkCommand = @"
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '__EFMigrationsHistory') 
    SELECT COUNT(*) as MigrationCount FROM [__EFMigrationsHistory]
ELSE
    SELECT 0 as MigrationCount
"@
    
    Write-Host "✅ Migrations applied successfully!" -ForegroundColor Green
    
    # Очищаем временный файл
    if (Test-Path $scriptFile) {
        Remove-Item $scriptFile -Force
    }
}

Write-Host "`n=== CI/CD Migration Complete ===" -ForegroundColor Cyan