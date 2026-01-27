# scripts/dev-migrate.ps1
function Show-Menu {
    Clear-Host
    Write-Host "=== EF Core Migration Manager ===" -ForegroundColor Cyan
    Write-Host "1. Check status" -ForegroundColor Green
    Write-Host "2. List migrations" -ForegroundColor Green
    Write-Host "3. Create migration" -ForegroundColor Green
    Write-Host "4. Apply migrations" -ForegroundColor Green
    Write-Host "5. Remove last migration" -ForegroundColor Yellow
    Write-Host "6. Generate SQL script" -ForegroundColor Cyan
    Write-Host "7. Reset database (DANGER!)" -ForegroundColor Red
    Write-Host "8. Open migrations folder" -ForegroundColor Gray
    Write-Host "9. Exit" -ForegroundColor Gray
    Write-Host ""
}

do {
    Show-Menu
    $choice = Read-Host "Select option"
    
    switch ($choice) {
        '1' { & ".\scripts\migrations\migrate.ps1" -Action status; Pause }
        '2' { & ".\scripts\migrations\migrate.ps1" -Action list; Pause }
        '3' { 
            $name = Read-Host "Enter migration name"
            & ".\scripts\migrations\migrate.ps1" -Action add -Name $name
            Pause
        }
        '4' { & ".\scripts\migrations\migrate.ps1" -Action update; Pause }
        '5' { & ".\scripts\migrations\migrate.ps1" -Action remove; Pause }
        '6' { 
            $from = Read-Host "From migration (empty for all)"
            $to = Read-Host "To migration (empty for latest)"
            & ".\scripts\migrations\migrate.ps1" -Action script -FromMigration $from -ToMigration $to
            Pause
        }
        '7' { 
            Write-Host "This will DELETE ALL DATA!" -ForegroundColor Red
            $confirm = Read-Host "Type 'RESET' to confirm"
            if ($confirm -eq 'RESET') {
                & ".\scripts\migrations\migrate.ps1" -Action reset -Force
            }
            Pause
        }
        '8' { 
            $path = "apps/test-service/Infrastructure/Infrastructure.EntityFramework/Migrations"
            if (Test-Path $path) {
                explorer $path
            }
            else {
                Write-Host "Folder not found: $path" -ForegroundColor Red
            }
            Pause
        }
        '9' { Write-Host "Goodbye!" -ForegroundColor Cyan }
        default { Write-Host "Invalid option" -ForegroundColor Red; Pause }
    }
} while ($choice -ne '9')