# API Документация

## Обзор

Микросервис аутентификации пользователей предоставляет REST API для регистрации, аутентификации и управления пользователями.

**Base URL:** `http://localhost:8081` (для локальной разработки)

**Формат данных:** JSON

**Аутентификация:** JWT Bearer Token (для защищенных endpoints)

## Swagger/OpenAPI

Интерактивная документация API доступна через Swagger UI:

**URL:** `http://localhost:8081/swagger`

### Экспорт OpenAPI спецификации

Для экспорта OpenAPI спецификации в JSON формате:

1. Запустите микросервис
2. Откройте в браузере: `http://localhost:8081/swagger/v1/swagger.json`
3. Сохраните содержимое в файл `openapi.json`

Альтернативно, используйте curl:

```bash
curl http://localhost:8081/swagger/v1/swagger.json > openapi.json
```

## Postman коллекция

Postman коллекция с примерами всех запросов доступна в файле `postman_collection.json`.

### Импорт в Postman

1. Откройте Postman
2. Нажмите "Import"
3. Выберите файл `postman_collection.json`
4. Коллекция будет импортирована со всеми запросами и переменными окружения

### Переменные коллекции

- `base_url` - базовый URL API (по умолчанию: `http://localhost:8081`)
- `access_token` - JWT токен (автоматически обновляется после входа)
- `refresh_token` - Refresh токен (автоматически обновляется после входа)
- `user_id` - ID пользователя (автоматически обновляется после входа)

## Группы endpoints

### 1. Аутентификация (`/api/auth`)

Endpoints для регистрации, входа и управления сессиями.

#### POST /api/auth/register

Регистрация нового пользователя.

**Тело запроса:**
```json
{
  "userName": "john_doe",
  "email": "john@example.com",
  "phoneNumber": "+79991234567",
  "password": "SecurePass123!",
  "confirmPassword": "SecurePass123!",
  "firstName": "Иван",
  "lastName": "Иванов",
  "middleName": "Петрович"
}
```

**Ответ (200 OK):**
```json
{
  "success": true,
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "a1b2c3d4e5f6...",
  "expiresAt": "2026-03-02T15:30:00Z",
  "user": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "userName": "john_doe",
    "email": "john@example.com",
    "phoneNumber": "+79991234567",
    "firstName": "Иван",
    "lastName": "Иванов"
  },
  "requiresPhoneVerification": true
}
```

**Ошибки:**
- `400 Bad Request` - ошибка валидации (email уже существует, слабый пароль и т.д.)

#### POST /api/auth/login

Вход в систему по логину и паролю.

**Тело запроса:**
```json
{
  "userName": "john_doe",
  "password": "SecurePass123!",
  "rememberMe": true
}
```

**Ответ (200 OK):**
```json
{
  "success": true,
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "a1b2c3d4e5f6...",
  "expiresAt": "2026-03-02T15:30:00Z",
  "user": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "userName": "john_doe",
    "email": "john@example.com"
  },
  "requiresPhoneVerification": false
}
```

**Ошибки:**
- `401 Unauthorized` - неверные учетные данные
- `400 Bad Request` - телефон не подтвержден

#### POST /api/auth/google

Вход через Google OAuth.

**Тело запроса:**
```json
{
  "idToken": "google-id-token-here"
}
```

**Ответ:** Аналогичен `/api/auth/login`

**Ошибки:**
- `401 Unauthorized` - неверный Google токен

#### POST /api/auth/refresh

Обновление JWT токена.

**Тело запроса:**
```json
{
  "refreshToken": "a1b2c3d4e5f6..."
}
```

**Ответ:** Аналогичен `/api/auth/login`

**Ошибки:**
- `401 Unauthorized` - истекший или отозванный refresh токен

#### POST /api/auth/logout

Выход из системы.

**Заголовки:**
```
Authorization: Bearer {access_token}
```

**Ответ (200 OK):**
```json
{
  "message": "Выход выполнен успешно"
}
```

**Ошибки:**
- `401 Unauthorized` - пользователь не авторизован

### 2. Подтверждение телефона (`/api/phone`)

Endpoints для подтверждения номера телефона через SMS.

#### POST /api/phone/send-code

Отправка кода подтверждения.

**Тело запроса:**
```json
{
  "phoneNumber": "+79991234567"
}
```

**Ответ (200 OK):**
```json
{
  "success": true,
  "message": "Код отправлен на номер +79991234567",
  "expiresAt": "2026-03-02T15:10:00Z"
}
```

**Ошибки:**
- `429 Too Many Requests` - превышен лимит отправок (3 в час)

#### POST /api/phone/verify

Проверка кода подтверждения.

**Тело запроса:**
```json
{
  "phoneNumber": "+79991234567",
  "code": "123456"
}
```

**Ответ (200 OK):**
```json
{
  "success": true,
  "message": "Телефон успешно подтвержден"
}
```

**Ошибки:**
- `400 Bad Request` - неверный или истекший код

#### POST /api/phone/resend-code

Повторная отправка кода.

**Тело запроса:** Аналогичен `/api/phone/send-code`

**Ответ:** Аналогичен `/api/phone/send-code`

### 3. Профиль пользователя (`/api/profile`)

Endpoints для управления профилем (требуют авторизации).

#### GET /api/profile

Получение данных профиля.

**Заголовки:**
```
Authorization: Bearer {access_token}
```

**Ответ (200 OK):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userName": "john_doe",
  "email": "john@example.com",
  "phoneNumber": "+79991234567",
  "phoneNumberConfirmed": true,
  "firstName": "Иван",
  "lastName": "Иванов",
  "middleName": "Петрович",
  "createdAt": "2026-03-01T10:00:00Z"
}
```

#### PUT /api/profile

Обновление профиля.

**Заголовки:**
```
Authorization: Bearer {access_token}
```

**Тело запроса:**
```json
{
  "email": "newemail@example.com",
  "firstName": "Иван",
  "lastName": "Петров",
  "middleName": "Сергеевич"
}
```

**Ответ (200 OK):**
```json
{
  "success": true,
  "message": "Профиль успешно обновлен"
}
```

**Ошибки:**
- `400 Bad Request` - email уже используется

#### PUT /api/profile/password

Смена пароля.

**Заголовки:**
```
Authorization: Bearer {access_token}
```

**Тело запроса:**
```json
{
  "currentPassword": "SecurePass123!",
  "newPassword": "NewSecurePass456!",
  "confirmNewPassword": "NewSecurePass456!"
}
```

**Ответ (200 OK):**
```json
{
  "success": true,
  "message": "Пароль успешно изменен"
}
```

**Ошибки:**
- `400 Bad Request` - неверный текущий пароль или новый пароль не соответствует требованиям

#### DELETE /api/profile

Удаление аккаунта.

**Заголовки:**
```
Authorization: Bearer {access_token}
```

**Ответ (200 OK):**
```json
{
  "success": true,
  "message": "Аккаунт успешно удален"
}
```

### 4. Пароли (`/api/password`)

Endpoints для работы с паролями.

#### GET /api/password/generate

Генерация надежного пароля.

**Ответ (200 OK):**
```json
{
  "password": "Kp9#mL2$xR5@nQ8!"
}
```

### 5. Подписки (`/api/subscriptions`)

Endpoints для управления подписками через nopCommerce.

#### GET /api/subscriptions

Получение подписок пользователя.

**Заголовки:**
```
Authorization: Bearer {access_token}
```

**Ответ (200 OK):**
```json
[
  {
    "id": 1,
    "planName": "Премиум",
    "startDate": "2026-03-01T00:00:00Z",
    "endDate": "2026-04-01T00:00:00Z",
    "isActive": true,
    "autoRenew": true
  }
]
```

#### GET /api/subscriptions/plans

Получение доступных планов.

**Ответ (200 OK):**
```json
[
  {
    "id": 1,
    "name": "Базовый",
    "description": "Доступ к базовым курсам",
    "price": 990.00,
    "durationDays": 30
  },
  {
    "id": 2,
    "name": "Премиум",
    "description": "Доступ ко всем курсам",
    "price": 1990.00,
    "durationDays": 30
  }
]
```

#### POST /api/subscriptions

Оформление подписки.

**Заголовки:**
```
Authorization: Bearer {access_token}
```

**Тело запроса:**
```json
{
  "planId": 2
}
```

**Ответ (200 OK):**
```json
{
  "success": true,
  "subscriptionId": 1,
  "message": "Подписка успешно оформлена"
}
```

#### DELETE /api/subscriptions/{id}

Отмена подписки.

**Заголовки:**
```
Authorization: Bearer {access_token}
```

**Параметры пути:**
- `id` - ID подписки

**Ответ (200 OK):**
```json
{
  "success": true,
  "message": "Подписка успешно отменена"
}
```

### 6. Мониторинг (`/health`)

Endpoints для проверки состояния сервиса.

#### GET /health

Полная проверка состояния.

**Ответ (200 OK):**
```json
{
  "status": "Healthy",
  "checks": [
    {
      "name": "postgresql",
      "status": "Healthy",
      "description": null,
      "duration": 15.5,
      "exception": null,
      "data": {}
    },
    {
      "name": "redis",
      "status": "Healthy",
      "description": null,
      "duration": 8.2,
      "exception": null,
      "data": {}
    },
    {
      "name": "rabbitmq",
      "status": "Healthy",
      "description": null,
      "duration": 12.3,
      "exception": null,
      "data": {}
    }
  ],
  "totalDuration": 36.0
}
```

**Ошибки:**
- `503 Service Unavailable` - одна или несколько зависимостей недоступны

#### GET /health/ready

Проверка готовности (для Kubernetes readiness probe).

**Ответ (200 OK):**
```json
{
  "status": "Healthy",
  "checks": [
    {
      "name": "postgresql",
      "status": "Healthy"
    },
    {
      "name": "redis",
      "status": "Healthy"
    },
    {
      "name": "rabbitmq",
      "status": "Healthy"
    }
  ]
}
```

#### GET /health/live

Проверка работоспособности (для Kubernetes liveness probe).

**Ответ (200 OK):**
```json
{
  "status": "Healthy"
}
```

## Коды ответов HTTP

- `200 OK` - запрос выполнен успешно
- `400 Bad Request` - ошибка валидации входных данных
- `401 Unauthorized` - требуется аутентификация или неверные учетные данные
- `403 Forbidden` - недостаточно прав для выполнения операции
- `404 Not Found` - запрашиваемый ресурс не найден
- `429 Too Many Requests` - превышен лимит запросов (rate limiting)
- `500 Internal Server Error` - внутренняя ошибка сервера
- `503 Service Unavailable` - сервис временно недоступен

## Обработка ошибок

Все ошибки возвращаются в едином формате:

```json
{
  "errors": [
    "Описание ошибки на русском языке"
  ]
}
```

Или для ошибок валидации:

```json
{
  "errors": {
    "Email": ["Email обязателен", "Некорректный формат email"],
    "Password": ["Пароль должен содержать минимум 8 символов"]
  }
}
```

## Аутентификация

Для доступа к защищенным endpoints необходимо передавать JWT токен в заголовке:

```
Authorization: Bearer {access_token}
```

### Получение токена

1. Зарегистрируйтесь через `/api/auth/register` или войдите через `/api/auth/login`
2. Сохраните `accessToken` из ответа
3. Используйте токен в заголовке `Authorization` для последующих запросов

### Обновление токена

Когда access токен истекает (через 15 минут):

1. Используйте refresh токен для получения нового access токена через `/api/auth/refresh`
2. Обновите сохраненный access токен

### Время жизни токенов

- **Access Token:** 15 минут
- **Refresh Token:** 7 дней (30 дней с флагом `rememberMe`)

## Rate Limiting

Для защиты от брутфорса применяются следующие ограничения:

### По пользователю
- **Лимит:** 5 неудачных попыток входа за 15 минут
- **Блокировка:** 15 минут

### По IP адресу
- **Лимит:** 20 неудачных попыток входа за час
- **Блокировка:** 1 час

### Подтверждение телефона
- **Лимит:** 3 отправки кода в час на номер
- **Ошибка:** `429 Too Many Requests`

## Интеграция с внешними сервисами

### Google OAuth

Для входа через Google необходимо:

1. Получить Google ID токен на клиенте
2. Отправить токен на `/api/auth/google`
3. Сервис валидирует токен через Google API
4. Возвращает JWT токены

### SMS провайдер

Для подтверждения телефона используется внешний SMS провайдер (Twilio, SMS.ru и т.д.).

### nopCommerce

Для управления подписками используется внешний nopCommerce API:

- **Retry политика:** 3 попытки с экспоненциальной задержкой
- **Таймаут:** 30 секунд на запрос
- **Кеширование:** Планы подписок кешируются в Redis на 1 час

## Примеры использования

### Полный flow регистрации и входа

1. **Регистрация:**
```bash
POST /api/auth/register
```

2. **Подтверждение телефона:**
```bash
POST /api/phone/send-code
POST /api/phone/verify
```

3. **Вход:**
```bash
POST /api/auth/login
```

4. **Работа с профилем:**
```bash
GET /api/profile
PUT /api/profile
```

5. **Обновление токена:**
```bash
POST /api/auth/refresh
```

6. **Выход:**
```bash
POST /api/auth/logout
```

## Дополнительные ресурсы

- **Swagger UI:** `http://localhost:8081/swagger`
- **OpenAPI спецификация:** `http://localhost:8081/swagger/v1/swagger.json`
- **Postman коллекция:** `postman_collection.json`
- **README:** `README.md`
