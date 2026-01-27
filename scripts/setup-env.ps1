# scripts/setup-env.ps1
param(
    [Parameter(Mandatory=$false)]
    [ValidateSet('Development', 'Staging', 'Production')]
    [string]$Environment = 'Development'
)

Write-Host "Setting up environment: $Environment" -ForegroundColor Green

# Копируем соответствующий .env файл
$envFile = ".env.$Environment"
if (Test-Path $envFile) {
    Copy-Item $envFile ".env" -Force
    Write-Host "Copied $envFile to .env" -ForegroundColor Green
} else {
    Write-Host "Environment file $envFile not found" -ForegroundColor Yellow
    Write-Host "Using default .env file" -ForegroundColor Yellow
}

# Проверяем наличие обязательных переменных
if ($Environment -eq "Production") {
    Write-Host "`n=== PRODUCTION WARNING ===" -ForegroundColor Red
    Write-Host "1. Update all passwords and secrets" -ForegroundColor Yellow
    Write-Host "2. Set proper CORS origins" -ForegroundColor Yellow
    Write-Host "3. Disable Swagger in production" -ForegroundColor Yellow
    Write-Host "4. Configure SSL certificates" -ForegroundColor Yellow
}