# Устранение неполадок Google авторизации

## Проблема: Кнопка "Войти через Google" не работает

### Возможные причины и решения

#### 1. Отсутствуют переменные окружения

**Проверка:**
```bash
# Убедитесь, что файл .env существует
ls -la apps/user-portal/.env

# Проверьте содержимое
cat apps/user-portal/.env
```

**Решение:**
Создайте файл `.env` в директории `apps/user-portal/` со следующим содержимым:
```env
VITE_API_BASE_URL=http://localhost:8081
VITE_GOOGLE_CLIENT_ID=986072857181-k77rcjis4dat7f3u60r2t120co6fsse3.apps.googleusercontent.com
VITE_NODE_ENV=development
VITE_ENABLE_LOGGING=true
```

#### 2. Неправильный Google Client ID

**Проверка:**
1. Откройте [Google Cloud Console](https://console.cloud.google.com/)
2. Перейдите в раздел "APIs & Services" > "Credentials"
3. Проверьте, что Client ID соответствует значению в `.env`

**Решение:**
Обновите `VITE_GOOGLE_CLIENT_ID` в файле `.env` правильным значением.

#### 3. Неправильные настройки домена в Google Console

**Проверка:**
В Google Cloud Console убедитесь, что в настройках OAuth 2.0 Client ID добавлены:
- Authorized JavaScript origins: `http://localhost:3001`, `http://localhost:3000`
- Authorized redirect URIs: `http://localhost:3001`, `http://localhost:3000`

#### 4. Блокировка загрузки Google Identity Services

**Проверка:**
1. Откройте Developer Tools (F12)
2. Перейдите на вкладку Console
3. Ищите ошибки связанные с `accounts.google.com`

**Возможные ошибки:**
- CORS ошибки
- Блокировка рекламными блокировщиками
- Проблемы с сетью

**Решение:**
- Отключите блокировщики рекламы
- Проверьте настройки CORS
- Убедитесь в доступности интернета

#### 5. Проблемы с сертификатами SSL (для production)

**Проверка:**
Убедитесь, что сайт использует HTTPS в production окружении.

**Решение:**
Google Identity Services требует HTTPS для production. Для разработки можно использовать HTTP localhost.

### Тестирование

#### Быстрый тест
Откройте файл `test-google-auth.html` в браузере для проверки базовой функциональности Google Identity Services.

#### Проверка в приложении
1. Запустите приложение: `npm run dev`
2. Откройте Developer Tools (F12)
3. Перейдите на страницу логина
4. Проверьте консоль на наличие ошибок
5. Попробуйте нажать кнопку "Войти через Google"

### Логи для отладки

Добавьте в компонент `GoogleAuthButton.tsx` дополнительное логирование:

```typescript
console.log('Google Client ID:', import.meta.env.VITE_GOOGLE_CLIENT_ID);
console.log('API Base URL:', import.meta.env.VITE_API_BASE_URL);
console.log('Google loaded:', !!window.google?.accounts?.id);
```

### Частые ошибки

1. **"Google Identity Services не загружен"**
   - Проверьте интернет соединение
   - Отключите блокировщики рекламы
   - Проверьте настройки браузера

2. **"Invalid client ID"**
   - Проверьте правильность Client ID в `.env`
   - Убедитесь, что домен добавлен в Google Console

3. **"Redirect URI mismatch"**
   - Добавьте текущий URL в Authorized redirect URIs в Google Console

4. **CORS ошибки**
   - Проверьте настройки CORS на сервере
   - Убедитесь, что API доступен с фронтенда

### Контакты для поддержки

При возникновении проблем обратитесь к разработчику с:
1. Скриншотом ошибки из консоли браузера
2. Содержимым файла `.env` (без секретных данных)
3. Описанием шагов для воспроизведения проблемы