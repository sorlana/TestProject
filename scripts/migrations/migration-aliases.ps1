# scripts/migration-aliases.ps1
function mig-status { & ".\scripts\migrations\migrate.ps1" -Action status @args }
function mig-list { & ".\scripts\migrations\migrate.ps1" -Action list @args }
function mig-add { 
    param([string]$Name)
    & ".\scripts\migrations\migrate.ps1" -Action add -Name $Name @args 
}
function mig-up { & ".\scripts\migrations\migrate.ps1" -Action update @args }
function mig-update { mig-up @args }
function mig-remove { & ".\scripts\migrations\migrate.ps1" -Action remove @args }
function mig-script { & ".\scripts\migrations\migrate.ps1" -Action script @args }
function mig-reset { & ".\scripts\migrations\migrate.ps1" -Action reset @args }
function mig-help {
    Write-Host "=== Migration Aliases ===" -ForegroundColor Cyan
    Write-Host "mig-status          - Show migration status" -ForegroundColor Green
    Write-Host "mig-list            - List all migrations" -ForegroundColor Green
    Write-Host "mig-add <name>      - Create new migration" -ForegroundColor Green
    Write-Host "mig-up / mig-update - Apply all pending migrations" -ForegroundColor Green
    Write-Host "mig-remove [name]   - Remove last migration" -ForegroundColor Yellow
    Write-Host "mig-script          - Generate SQL script" -ForegroundColor Cyan
    Write-Host "mig-reset           - Reset database (DANGER!)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Examples:" -ForegroundColor Gray
    Write-Host "  mig-add AddEmailToUsers" -ForegroundColor Gray
    Write-Host "  mig-up" -ForegroundColor Gray
    Write-Host "  mig-script -FromMigration Initial -ToMigration AddEmail" -ForegroundColor Gray
}

# Автоматически показываем помощь при загрузке
mig-help