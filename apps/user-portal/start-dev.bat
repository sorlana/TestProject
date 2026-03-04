@echo off
echo Запуск User Portal в режиме разработки...
echo.

REM Проверяем наличие .env файла
if not exist .env (
    echo ОШИБКА: Файл .env не найден!
    echo Создайте файл .env на основе .env.example
    pause
    exit /b 1
)

REM Показываем текущие переменные окружения
echo Переменные окружения:
type .env
echo.

REM Устанавливаем зависимости если нужно
if not exist node_modules (
    echo Установка зависимостей...
    npm install
    echo.
)

REM Запускаем приложение
echo Запуск приложения на http://localhost:3001
echo Нажмите Ctrl+C для остановки
echo.
npm run dev