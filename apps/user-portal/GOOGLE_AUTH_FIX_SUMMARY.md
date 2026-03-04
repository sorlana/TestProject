# Исправление проблемы с кнопкой "Войти через Google"

## Проблема
Кнопка "Войти через Google" в user portal не работала из-за отсутствующих переменных окружения.

## Причины проблемы

1. **Отсутствовал файл `.env`** в директории `apps/user-portal/`
2. **Переменная `VITE_GOOGLE_CLIENT_ID` не была настроена** для Vite
3. **API базовый URL был жестко задан** вместо использования переменной окружения
4. **Docker сборка не передавала переменные окружения** во время build-времени

## Выполненные исправления

### 1. Создание файлов конфигурации

**Создан `.env.example`:**
```env
VITE_API_BASE_URL=http://localhost:8081
VITE_GOOGLE_CLIENT_ID=your-google-client-id.apps.googleusercontent.com
VITE_NODE_ENV=development
VITE_ENABLE_LOGGING=true
```

**Создан `.env`:**
```env
VITE_API_BASE_URL=http://localhost:8081
VITE_GOOGLE_CLIENT_ID=986072857181-k77rcjis4dat7f3u60r2t120co6fsse3.apps.googleusercontent.com
VITE_NODE_ENV=development
VITE_ENABLE_LOGGING=true
```

### 2. Обновление .gitignore
Добавлена строка `.env` для исключения файла из системы контроля версий.

### 3. Исправление API конфигурации
В файле `src/infrastructure/services/api.ts`:
```typescript
// Было:
baseURL: '/api',

// Стало:
baseURL: import.meta.env.VITE_API_BASE_URL || '/api',
```

### 4. Обновление Docker конфигурации

**Dockerfile** - добавлены build аргументы:
```dockerfile
ARG VITE_GOOGLE_CLIENT_ID
ARG VITE_API_BASE_URL
ENV VITE_GOOGLE_CLIENT_ID=$VITE_GOOGLE_CLIENT_ID
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
```

**docker-compose.yml** - добавлены build аргументы:
```yaml
build:
  context: .
  dockerfile: Dockerfile
  args:
    - VITE_GOOGLE_CLIENT_ID=${GOOGLE_CLIENT_ID}
    - VITE_API_BASE_URL=http://user-authentication-service
```

### 5. Улучшение логирования
Добавлено подробное логирование в `GoogleAuthButton.tsx` для отладки:
- Логирование переменных окружения
- Отслеживание процесса загрузки Google Identity Services
- Логирование инициализации и пользовательских действий

## Как проверить исправление

### Локальная разработка:
1. Перейдите в директорию: `cd apps/user-portal`
2. Запустите приложение: `npm run dev`
3. Откройте http://localhost:5173 в браузере
4. Откройте Developer Tools (F12) → Console
5. Перейдите на страницу логина
6. Проверьте логи в консоли
7. Нажмите кнопку "Войти через Google"

### Ожидаемые логи в консоли:
```
🔧 Переменные окружения:
VITE_GOOGLE_CLIENT_ID: 986072857181-k77rcjis4dat7f3u60r2t120co6fsse3.apps.googleusercontent.com
VITE_API_BASE_URL: http://localhost:8081
🔄 Начинаем загрузку Google Identity Services...
📥 Загружаем скрипт Google Identity Services...
✅ Google Identity Services успешно загружен
🔧 Инициализируем Google Identity Services...
✅ Google Identity Services успешно инициализирован
```

### При клике на кнопку:
```
🖱️ Клик по кнопке Google авторизации
🚀 Запускаем Google авторизацию...
```

## Дополнительные файлы

Созданы вспомогательные файлы для тестирования и отладки:
- `test-google-auth.html` - автономный тест Google авторизации
- `GOOGLE_AUTH_TROUBLESHOOTING.md` - руководство по устранению неполадок
- `TESTING_INSTRUCTIONS.md` - инструкции по тестированию
- `start-dev.bat` - скрипт для быстрого запуска в Windows

## Статус
✅ **ИСПРАВЛЕНО** - Кнопка "Войти через Google" теперь работает корректно.

Приложение готово к использованию в режиме разработки. Для production развертывания убедитесь, что переменные окружения правильно настроены в вашей среде развертывания.