# Скрипт проверки развертывания Landing CMS
Write-Host "=== Проверка Landing CMS ===" -ForegroundColor Cyan
Write-Host ""

# 1. Проверка структуры проекта
Write-Host "1. Проверка структуры проекта..." -ForegroundColor Yellow
$requiredFiles = @(
    "Program.cs",
    "Dockerfile",
    "LandingCms.csproj",
    "Controllers/LandingController.cs",
    "Controllers/TariffController.cs",
    "Models/LandingPage.cs",
    "Views/Shared/_Layout.cshtml",
    "Views/Landing/Index.cshtml"
)

$allFilesExist = $true
foreach ($file in $requiredFiles) {
    if (Test-Path $file) {
        Write-Host "  OK $file" -ForegroundColor Green
    } else {
        Write-Host "  FAIL $file" -ForegroundColor Red
        $allFilesExist = $false
    }
}
Write-Host ""

# 2. Проверка docker-compose
Write-Host "2. Проверка docker-compose.yaml..." -ForegroundColor Yellow
if (Test-Path "../../docker-compose.yaml") {
    $dockerCompose = Get-Content "../../docker-compose.yaml" -Raw
    if ($dockerCompose -match "landing-cms:") {
        Write-Host "  OK Сервис landing-cms найден" -ForegroundColor Green
    }
}
Write-Host ""

# 3. Проверка контроллеров
Write-Host "3. Проверка контроллеров..." -ForegroundColor Yellow
if (Test-Path "Controllers/TariffController.cs") {
    $tariffController = Get-Content "Controllers/TariffController.cs" -Raw
    if ($tariffController -match "/app/payment") {
        Write-Host "  OK Редирект на /app/payment" -ForegroundColor Green
    }
}

if (Test-Path "Views/Shared/_Layout.cshtml") {
    $layout = Get-Content "Views/Shared/_Layout.cshtml" -Raw
    if ($layout -match 'href="/app"') {
        Write-Host "  OK Кнопка Личный кабинет -> /app" -ForegroundColor Green
    }
}
Write-Host ""

# 4. Проверка Health Checks
Write-Host "4. Проверка Health Checks..." -ForegroundColor Yellow
if (Test-Path "Program.cs") {
    $program = Get-Content "Program.cs" -Raw
    if ($program -match "AddHealthChecks") {
        Write-Host "  OK Health Checks настроены" -ForegroundColor Green
    }
    if ($program -match "/health/ready") {
        Write-Host "  OK Endpoint /health/ready" -ForegroundColor Green
    }
    if ($program -match "/health/live") {
        Write-Host "  OK Endpoint /health/live" -ForegroundColor Green
    }
}
Write-Host ""

# 5. Проверка Piranha Manager
Write-Host "5. Проверка Piranha Manager..." -ForegroundColor Yellow
if (Test-Path "Program.cs") {
    $program = Get-Content "Program.cs" -Raw
    if ($program -match "UseManager") {
        Write-Host "  OK Piranha Manager включен" -ForegroundColor Green
    }
    if ($program -match "SeedData") {
        Write-Host "  OK Seed data настроен" -ForegroundColor Green
    }
}
Write-Host ""

Write-Host "=== Проверка завершена ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Следующие шаги:" -ForegroundColor Yellow
Write-Host "1. docker-compose up -d" -ForegroundColor White
Write-Host "2. curl http://localhost:3000/health" -ForegroundColor White
Write-Host "3. Откройте http://localhost:3000" -ForegroundColor White
Write-Host "4. Откройте http://localhost:3000/manager" -ForegroundColor White
Write-Host ""
