# Скрипт для остановки проекта TestProject

Write-Host "Остановка проекта TestProject..." -ForegroundColor Yellow

# Остановка всех контейнеров
docker-compose stop

Write-Host ""
Write-Host "Проект остановлен!" -ForegroundColor Green
Write-Host ""
Write-Host "Контейнеры остановлены, но не удалены." -ForegroundColor Cyan
Write-Host "Для полного удаления используйте: docker-compose down" -ForegroundColor Yellow
Write-Host "Для повторного запуска используйте: .\start-project.ps1" -ForegroundColor Yellow
