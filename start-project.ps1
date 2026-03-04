# Скрипт для запуска всего проекта TestProject

Write-Host "Запуск проекта TestProject..." -ForegroundColor Green

# Запуск всех контейнеров
docker-compose up -d

Write-Host ""
Write-Host "Проект запущен!" -ForegroundColor Green
Write-Host ""
Write-Host "Доступные сервисы:" -ForegroundColor Cyan
Write-Host "  - User Portal:              http://localhost:3001" -ForegroundColor White
Write-Host "  - User Auth Service:        http://localhost:8081" -ForegroundColor White
Write-Host "  - User Auth Swagger:        http://localhost:8081/swagger" -ForegroundColor White
Write-Host "  - Test Service:             http://localhost:5000" -ForegroundColor White
Write-Host "  - Test Service Swagger:     http://localhost:5000/swagger" -ForegroundColor White
Write-Host "  - Landing CMS:              http://localhost:3000" -ForegroundColor White
Write-Host "  - PostgreSQL:               localhost:5432" -ForegroundColor White
Write-Host "  - Redis:                    localhost:6379" -ForegroundColor White
Write-Host ""
Write-Host "Для остановки проекта используйте: docker-compose down" -ForegroundColor Yellow
Write-Host "Для просмотра логов: docker-compose logs -f [service-name]" -ForegroundColor Yellow
