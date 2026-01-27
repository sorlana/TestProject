# scripts/migrations/migrate.ps1
param(
    [Parameter(Mandatory=$false)]
    [ValidateSet('add', 'update', 'list', 'remove', 'script', 'status', 'reset')]
    [string]$Action = 'status',
    
    [string]$Name = "",
    [string]$FromMigration = "",
    [string]$ToMigration = "0",
    [switch]$Force,
    [switch]$Verbose,
    [string]$Environment = "Development"
)

$ErrorActionPreference = "Stop"

Write-Host "=== EF Core Migration Tool ===" -ForegroundColor Cyan
Write-Host "Action: $Action" -ForegroundColor Yellow
Write-Host "Environment: $Environment" -ForegroundColor Green

# Настройки проекта
$projectPath = "apps/test-service/WebApi/WebApi.csproj"
$startupProjectPath = "apps/test-service/WebApi/WebApi.csproj"
$context = "DatabaseContext"
$migrationsDir = "apps/test-service/Infrastructure/Infrastructure.EntityFramework/Migrations"

# Функция для выполнения команд в контейнере
function Invoke-InContainer {
    param([string]$Command)
    
    if ($Verbose) {
        Write-Host "Executing: $Command" -ForegroundColor Gray
    }
    
    docker-compose exec test-service $Command
}

# Функция для выполнения команд локально
function Invoke-Local {
    param([string]$Command)
    
    if ($Verbose) {
        Write-Host "Executing: $Command" -ForegroundColor Gray
    }
    
    Invoke-Expression $Command
}

# Обработка действий
switch ($Action) {
    'status' {
        Write-Host "`n=== Migration Status ===" -ForegroundColor Green
        
        # Проверка через API
        try {
            $response = Invoke-WebRequest -Uri "http://localhost:5000/diagnostics/migrations" -UseBasicParsing
            $data = $response.Content | ConvertFrom-Json
            
            Write-Host "Database connected: $($data.databaseConnected)" -ForegroundColor $(if ($data.databaseConnected) {"Green"} else {"Red"})
            Write-Host "AutoMigrate enabled: $($data.autoMigrateEnabled)" -ForegroundColor Yellow
            Write-Host "Applied migrations: $($data.appliedCount)" -ForegroundColor Green
            Write-Host "Pending migrations: $($data.pendingCount)" -ForegroundColor $(if ($data.pendingCount -gt 0) {"Red"} else {"Green"})
            
            if ($data.pendingMigrations.Count -gt 0) {
                Write-Host "`nPending migrations:" -ForegroundColor Red
                foreach ($migration in $data.pendingMigrations) {
                    Write-Host "  - $migration" -ForegroundColor Red
                }
            }
            
            Write-Host "`nApplied migrations:" -ForegroundColor Green
            foreach ($migration in $data.appliedMigrations) {
                Write-Host "  - $migration" -ForegroundColor Green
            }
        }
        catch {
            Write-Host "Cannot connect to API: $_" -ForegroundColor Red
        }
        
        # Проверка через базу данных
        Write-Host "`n=== Database Check ===" -ForegroundColor Green
        docker-compose exec postgres-test psql -U test_user -d testdb -c "SELECT * FROM public.\"__EFMigrationsHistory\" ORDER BY \"MigrationId\";"
    }
    
    'list' {
        Write-Host "`n=== Available Migrations ===" -ForegroundColor Green
        
        # В проекте
        if (Test-Path $migrationsDir) {
            Write-Host "`nIn project:" -ForegroundColor Yellow
            Get-ChildItem $migrationsDir -Filter *.cs | 
                Where-Object { $_.Name -notlike "*.Designer.cs" -and $_.Name -notlike "*Snapshot.cs" } |
                Sort-Object Name |
                ForEach-Object {
                    $migrationName = $_.BaseName
                    $datePart = $migrationName.Substring(0, 14)
                    $description = $migrationName.Substring(15)
                    try {
                        $date = [DateTime]::ParseExact($datePart, "yyyyMMddHHmmss", $null)
                        Write-Host "  $($date.ToString('yyyy-MM-dd HH:mm')) - $description" -ForegroundColor Cyan
                    }
                    catch {
                        Write-Host "  $migrationName" -ForegroundColor Cyan
                    }
                }
        }
        
        # В базе данных
        Write-Host "`nIn database:" -ForegroundColor Yellow
        docker-compose exec postgres-test psql -U test_user -d testdb -t -c "SELECT '\"''' || \"MigrationId\" || '\"''' FROM public.\"__EFMigrationsHistory\" ORDER BY \"MigrationId\";"
    }
    
    'add' {
        if ([string]::IsNullOrEmpty($Name)) {
            $Name = Read-Host "Enter migration name"
        }
        
        if ([string]::IsNullOrEmpty($Name)) {
            Write-Host "Migration name is required!" -ForegroundColor Red
            exit 1
        }
        
        Write-Host "`nCreating migration: $Name" -ForegroundColor Green
        
        # Создаем миграцию
        $command = "dotnet ef migrations add $Name --context $context --project $projectPath --startup-project $startupProjectPath --output-dir ../Infrastructure/Infrastructure.EntityFramework/Migrations"
        
        try {
            Write-Host "Executing: $command" -ForegroundColor Gray
            Invoke-Local $command
            
            Write-Host "`n✅ Migration created successfully!" -ForegroundColor Green
            Write-Host "Location: $migrationsDir" -ForegroundColor Yellow
            
            # Показываем созданные файлы
            $newFiles = Get-ChildItem $migrationsDir -Filter "*$Name*" | Select-Object -ExpandProperty Name
            if ($newFiles) {
                Write-Host "Created files:" -ForegroundColor Cyan
                foreach ($file in $newFiles) {
                    Write-Host "  - $file" -ForegroundColor Cyan
                }
            }
        }
        catch {
            Write-Host "Error creating migration: $_" -ForegroundColor Red
            exit 1
        }
    }
    
    'update' {
        Write-Host "`nApplying migrations..." -ForegroundColor Green
        
        if ([string]::IsNullOrEmpty($ToMigration)) {
            $ToMigration = "0"  # 0 означает последнюю миграцию
        }
        
        $command = "dotnet ef database update $ToMigration --context $context --project $projectPath --startup-project $startupProjectPath"
        
        try {
            Write-Host "Executing: $command" -ForegroundColor Gray
            Invoke-Local $command
            
            Write-Host "`n✅ Migrations applied successfully!" -ForegroundColor Green
            
            # Проверяем статус
            & $PSCommandPath -Action status
        }
        catch {
            Write-Host "Error applying migrations: $_" -ForegroundColor Red
            exit 1
        }
    }
    
    'remove' {
        if ([string]::IsNullOrEmpty($Name)) {
            $Name = Read-Host "Enter migration name to remove (last if empty)"
        }
        
        Write-Host "`nRemoving migration: $Name" -ForegroundColor Yellow
        
        if ($Force -or (Read-Host "Are you sure? Type 'yes' to continue") -eq 'yes') {
            $command = "dotnet ef migrations remove --context $context --project $projectPath --startup-project $startupProjectPath --force"
            
            try {
                Write-Host "Executing: $command" -ForegroundColor Gray
                Invoke-Local $command
                
                Write-Host "`n✅ Migration removed successfully!" -ForegroundColor Green
            }
            catch {
                Write-Host "Error removing migration: $_" -ForegroundColor Red
                exit 1
            }
        }
        else {
            Write-Host "Operation cancelled" -ForegroundColor Yellow
        }
    }
    
    'script' {
        Write-Host "`nGenerating SQL script..." -ForegroundColor Green
        
        if ([string]::IsNullOrEmpty($FromMigration)) {
            $FromMigration = "0"
        }
        
        $outputFile = "migration_script_$(Get-Date -Format 'yyyyMMdd_HHmmss').sql"
        $command = "dotnet ef migrations script $FromMigration $ToMigration --context $context --project $projectPath --startup-project $startupProjectPath --output $outputFile"
        
        try {
            Write-Host "Executing: $command" -ForegroundColor Gray
            Invoke-Local $command
            
            if (Test-Path $outputFile) {
                $lineCount = (Get-Content $outputFile | Measure-Object -Line).Lines
                Write-Host "`n✅ SQL script generated successfully!" -ForegroundColor Green
                Write-Host "File: $outputFile" -ForegroundColor Yellow
                Write-Host "Lines: $lineCount" -ForegroundColor Cyan
                
                # Показываем первые 10 строк
                Write-Host "`nPreview (first 10 lines):" -ForegroundColor Gray
                Get-Content $outputFile -TotalCount 10 | ForEach-Object { Write-Host "  $_" -ForegroundColor Gray }
            }
        }
        catch {
            Write-Host "Error generating script: $_" -ForegroundColor Red
            exit 1
        }
    }
    
    'reset' {
        Write-Host "`n⚠️  RESET DATABASE ⚠️" -ForegroundColor Red
        Write-Host "This will DELETE ALL DATA in the database!" -ForegroundColor Red
        
        if ($Force -or (Read-Host "Type 'RESET' to confirm") -eq 'RESET') {
            Write-Host "`nStopping containers..." -ForegroundColor Yellow
            docker-compose down
            
            Write-Host "Removing database volume..." -ForegroundColor Red
            docker volume rm testproject-postgres-data
            
            Write-Host "Clearing migrations..." -ForegroundColor Yellow
            if (Test-Path $migrationsDir) {
                Remove-Item "$migrationsDir\*" -Recurse -Force -ErrorAction SilentlyContinue
            }
            
            Write-Host "Creating initial migration..." -ForegroundColor Green
            $command = "dotnet ef migrations add InitialCreate --context $context --project $projectPath --startup-project $startupProjectPath --output-dir ../Infrastructure/Infrastructure.EntityFramework/Migrations"
            Invoke-Local $command
            
            Write-Host "Starting services..." -ForegroundColor Green
            docker-compose up -d
            
            Write-Host "Waiting for services to start..." -ForegroundColor Yellow
            Start-Sleep -Seconds 30
            
            Write-Host "`n✅ Database reset complete!" -ForegroundColor Green
            Write-Host "Database is now empty with initial migration only." -ForegroundColor Yellow
        }
        else {
            Write-Host "Operation cancelled" -ForegroundColor Yellow
        }
    }
}

Write-Host "`n=== Operation Complete ===" -ForegroundColor Cyan