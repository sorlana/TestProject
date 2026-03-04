# 🔐 Настройка Google OAuth для User Portal

## Проблема: Ошибка 401: invalid_client

Если вы видите ошибку **"401: invalid_client"** при попытке входа через Google, это означает, что Google Client ID не настроен или настроен неправильно.

## Решение: Настройка Google Client ID

### Шаг 1: Создание OAuth credentials в Google Cloud Console

1. **Перейдите в Google Cloud Console:**
   - Откройте [Google Cloud Console](https://console.cloud.google.com/)
   - Войдите в свой Google аккаунт

2. **Создайте или выберите проект:**
   - Если у вас нет проекта, создайте новый
   - Выберите существующий проект из списка

3. **Включите Google Identity API:**
   - Перейдите в **"APIs & Services"** → **"Library"**
   - Найдите **"Google Identity Services API"**
   - Нажмите **"Enable"**

4. **Создайте OAuth Client ID:**
   - Перейдите в **"APIs & Services"** → **"Credentials"**
   - Нажмите **"Create Credentials"** → **"OAuth client ID"**
   - Выберите тип приложения: **"Web application"**

5. **Настройте Authorized redirect URIs:**
   ```
   http://localhost:5173
   http://localhost:3001
   https://yourdomain.com (для production)
   ```

6. **Скопируйте Client ID:**
   - После создания скопируйте **Client ID**
   - Он будет выглядеть примерно так: `123456789-abcdefg.apps.googleusercontent.com`

### Шаг 2: Обновление переменных окружения

1. **Откройте файл `.env`** в папке `apps/user-portal/`

2. **Замените placeholder на реальный Client ID:**
   ```env
   # Было:
   VITE_GOOGLE_CLIENT_ID=your_google_client_id
   
   # Стало:
   VITE_GOOGLE_CLIENT_ID=123456789-abcdefg.apps.googleusercontent.com
   ```

3. **Сохраните файл**

### Шаг 3: Перезапуск приложения

1. **Остановите сервер разработки** (Ctrl+C)

2. **Запустите заново:**
   ```bash
   npm run dev
   ```

3. **Откройте приложение:** http://localhost:5173/

### Шаг 4: Проверка работы

1. Перейдите на вкладку **"Вход"**
2. Нажмите кнопку **"Войти через Google"**
3. Должно открыться окно авторизации Google
4. После успешной авторизации вы будете перенаправлены в приложение

## Безопасность

⚠️ **Важные моменты:**

- **Client ID** - публичное значение, его можно хранить в `.env`
- **Client Secret** - секретное значение, используется только на backend
- Никогда не коммитьте реальные значения в git
- Используйте разные credentials для development и production

## Устранение проблем

### Ошибка "The OAuth client was not found"
- Проверьте правильность Client ID
- Убедитесь, что проект в Google Cloud Console активен

### Ошибка "redirect_uri_mismatch"
- Добавьте текущий URL в Authorized redirect URIs
- Проверьте точное соответствие URL (включая протокол и порт)

### Кнопка Google неактивна
- Проверьте, что Client ID не является placeholder'ом
- Откройте консоль браузера для просмотра ошибок

## Поддержка

Если у вас остались вопросы:
1. Проверьте консоль браузера на наличие ошибок
2. Убедитесь, что все шаги выполнены правильно
3. Обратитесь к документации Google OAuth 2.0