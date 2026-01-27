@echo off
chcp 1251 > nul
setlocal enabledelayedexpansion

:: Получаем путь к текущей папке
set "current_dir=%~dp0"
cd /d "%current_dir%"

:: Полностью очищаем или создаем новый файл context.txt
> context.txt echo.

:: Создаем структуру папок без заголовочных строк
echo Структура папки: >> context.txt
echo ================= >> context.txt

:: Сохраняем полную структуру во временный файл
tree /F > temp_full.txt 2>&1

:: Пропускаем первые 3 строки (заголовок тома) и сохраняем остальное
more +3 temp_full.txt > temp_tree.txt 2>nul || (
    copy /y temp_full.txt temp_tree.txt >nul
)

:: Фильтруем временный файл, исключая игнорируемые файлы
findstr /v /i /c:"context.bat" /c:"context.txt" /c:".git" /c:"node_modules" temp_tree.txt >> context.txt

:: Удаляем временные файлы
del temp_full.txt >nul 2>&1
del temp_tree.txt >nul 2>&1

echo. >> context.txt
echo ================= >> context.txt
echo. >> context.txt

:: Обрабатываем все файлы в папке и подпапках
for /r %%f in (*.*) do (
    set "file_name=%%~nxf"
    set "file_ext=%%~xf"
    
    :: Приводим расширение к нижнему регистру для сравнения
    set "file_ext_lower=!file_ext!"
    if defined file_ext_lower (
        set "file_ext_lower=!file_ext_lower:.=!"
        set "file_ext_lower=!file_ext_lower:,=!"
        set "file_ext_lower=!file_ext_lower: =!"
        set "file_ext_lower=!file_ext_lower:~0,4!"
    )
    
    :: Проверяем, нужно ли игнорировать файл
    set "ignore_file=0"
    
    :: Игнорируем служебные файлы
    if /i "!file_name!"=="context.bat" set "ignore_file=1"
    if /i "!file_name!"=="context.txt" set "ignore_file=1"
    
    :: Игнорируем графические файлы
    if /i "!file_ext_lower!"=="jpg" set "ignore_file=1"
    if /i "!file_ext_lower!"=="jpeg" set "ignore_file=1"
    if /i "!file_ext_lower!"=="png" set "ignore_file=1"
    if /i "!file_ext_lower!"=="svg" set "ignore_file=1"
    if /i "!file_ext_lower!"=="gif" set "ignore_file=1"
    if /i "!file_ext_lower!"=="bmp" set "ignore_file=1"
    if /i "!file_ext_lower!"=="webp" set "ignore_file=1"
    if /i "!file_ext_lower!"=="ico" set "ignore_file=1"
    
    :: Игнорируем бинарные и системные файлы
    if /i "!file_ext_lower!"=="exe" set "ignore_file=1"
    if /i "!file_ext_lower!"=="dll" set "ignore_file=1"
    if /i "!file_ext_lower!"=="zip" set "ignore_file=1"
    if /i "!file_ext_lower!"=="rar" set "ignore_file=1"
    if /i "!file_ext_lower!"=="7z" set "ignore_file=1"
    if /i "!file_ext_lower!"=="pdf" set "ignore_file=1"
    if /i "!file_ext_lower!"=="doc" set "ignore_file=1"
    if /i "!file_ext_lower!"=="docx" set "ignore_file=1"
    if /i "!file_ext_lower!"=="xls" set "ignore_file=1"
    if /i "!file_ext_lower!"=="xlsx" set "ignore_file=1"
    if /i "!file_ext_lower!"=="class" set "ignore_file=1"
    if /i "!file_ext_lower!"=="jar" set "ignore_file=1"
    
    :: Если файл нужно игнорировать, пропускаем его
    if "!ignore_file!"=="1" (
        echo Игнорируем файл: %%f
    ) else (
        echo Обработка файла: %%f
        
        :: Получаем относительный путь от текущей папки
        set "relative_path=%%f"
        set "relative_path=!relative_path:%current_dir%=!"
        
        :: Убираем возможный начальный обратный слэш
        if "!relative_path:~0,1!"=="\" set "relative_path=!relative_path:~1!"
        
        :: Записываем заголовок файла
        echo /!relative_path!: >> context.txt
        
        :: Читаем содержимое файла с ГАРАНТИРОВАННОЙ правильной кодировкой
        powershell -Command ^
        "$filePath = [System.IO.Path]::GetFullPath('%%f'); " ^
        "$encodings = @( " ^
        "    [System.Text.Encoding]::UTF8, " ^
        "    [System.Text.Encoding]::GetEncoding(1251), " ^
        "    [System.Text.Encoding]::Default " ^
        "); " ^
        "$content = $null; " ^
        "foreach ($enc in $encodings) { " ^
        "    try { " ^
        "        $content = [System.IO.File]::ReadAllText($filePath, $enc); " ^
        "        if ($content -and $content.Length -gt 0) { break; } " ^
        "    } catch { } " ^
        "} " ^
        "if ($content -eq $null) { " ^
        "    try { " ^
        "        $bytes = [System.IO.File]::ReadAllBytes($filePath); " ^
        "        $content = [System.Text.Encoding]::UTF8.GetString($bytes); " ^
        "    } catch { " ^
        "        $content = '[Не удалось определить кодировку файла]'; " ^
        "    } " ^
        "} " ^
        "[System.Console]::OutputEncoding = [System.Text.Encoding]::GetEncoding(1251); " ^
        "[System.Console]::Write($content);" >> context.txt 2>nul
        
        :: Добавляем разделитель после каждого файла
        echo. >> context.txt
        echo. >> context.txt
    )
)

echo.
echo Файл context.txt успешно создан и заполнен!
echo Путь к файлу: %current_dir%context.txt
echo.
echo Игнорируемые файлы:
echo   - context.bat, context.txt
echo   - Графические файлы: jpg, jpeg, png, svg, gif, bmp, webp, ico
echo   - Бинарные файлы: exe, dll, zip, rar, 7z, pdf
echo   - Офисные файлы: doc, docx, xls, xlsx
echo   - Системные файлы: class, jar

endlocal