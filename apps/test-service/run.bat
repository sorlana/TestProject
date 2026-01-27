@echo off
chcp 1251 > nul
echo Запуск веб-проекта...
dotnet run --project "WebApi\WebApi.csproj"
pause