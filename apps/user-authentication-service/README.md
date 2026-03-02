# Микросервис аутентификации пользователей

Микросервис аутентификации пользователей предоставляет функциональность регистрации, входа и управления пользователями для системы онлайн-обучения.

## Архитектура

Микросервис построен на ASP.NET Core 8.0 с использованием принципов чистой архитектуры (Clean Architecture):

- **Domain Layer** - доменные сущности и бизнес-правила
- **Services Layer** - бизнес-логика и интерфейсы сервисов
- **Infrastructure Layer** - реализация технических деталей (БД, внешние интеграции)
- **WebApi Layer** - REST API контроллеры и middleware

## Основные возможности

- Регистрация пользователей по логину и паролю
- Аутентификация через логин/пароль
- Аутентификация через Google OAuth 2.0
- Подтверждение телефона через SMS
- Управление профилем пользователя
- Смена пароля
- Генерация надежных паролей
- Управление подписками через интеграцию с nopCommerce
- JWT токены с refresh механизмом
- Rate limiting для защиты от брутфорса
- Логирование операций аутентификации
- Health checks для мониторинга

## Технологический стек

- **Framework**: ASP.NET Core 8.0
- **База данных**: PostgreSQL
- **ORM**: Entity Framework Core
- **Аутентификация**: ASP.NET Core Identity, JWT Bearer
- **Кеширование**: Redis
- **Обмен сообщениями**: RabbitMQ
- **Логирование**: Serilog
- **Документация API**: Swagger/OpenAPI
- **Контейнеризация**: Docker
- **Оркестрация**: Kubernetes

## Внешние зависимости

- PostgreSQL - база данных
- Redis - кеширование и rate limiting
- RabbitMQ - обмен сообщениями между микросервисами
- Google OAuth API - аутентификация через Google
- SMS провайдер API - подтверждение телефона
- nopCommerce API - внешний сервис управления подписками (развернут отдельно)

## Структура проекта

```
apps/user-authentication-service/
├── Domain/
│   └── Domain.Entities/              # Доменные сущности
├── Services/
│   ├── Services.Abstractions/        # Интерфейсы бизнес-сервисов
│   ├── Services.Contracts/           # DTO контракты
│   ├── Services.Implementations/     # Реализация бизнес-логики
│   └── Services.Repositories.Abstractions/  # Интерфейсы репозиториев
├── Infrastructure/
│   ├── Infrastructure.EntityFramework/      # DbContext, конфигурации EF
│   ├── Infrastructure.Repositories.Implementations/  # Реализация репозиториев
│   ├── Infrastructure.Identity/      # Настройка ASP.NET Core Identity
│   ├── Infrastructure.ExternalServices/  # Интеграции (SMS, Google, nopCommerce)
│   └── Services.UnitTests/           # Модульные тесты
├── WebApi/                           # REST API, контроллеры, middleware
├── k8s/                              # Kubernetes манифесты
├── manifests/                        # ConfigMap и Secret
├── Dockerfile
└── README.md
```

## Локальный запуск

### Предварительные требования

- .NET 8.0 SDK
- Docker и Docker Compose
- PostgreSQL, Redis, RabbitMQ (через docker-compose)

### Запуск через docker-compose

1. Убедитесь, что в корневом `docker-compose.yml` добавлен сервис `user-authentication-service`

2. Запустите все сервисы:
```bash
docker-compose up -d
```

3. Микросервис будет доступен по адресу: `http://localhost:8081`

4. Swagger документация: `http://localhost:8081/swagger`

### Локальная разработка

1. Запустите зависимости (PostgreSQL, Redis, RabbitMQ):
```bash
docker-compose up -d postgres redis rabbitmq
```

2. Настройте строку подключения в `appsettings.Development.json`

3. Примените миграции базы данных:
```bash
cd Infrastructure/Infrastructure.EntityFramework
dotnet ef database update
```

4. Запустите приложение:
```bash
cd WebApi/WebApi
dotnet run
```

## Переменные окружения

### Обязательные

- `ConnectionStrings__DefaultConnection` - строка подключения к PostgreSQL
- `Jwt__SecretKey` - секретный ключ для JWT (минимум 32 символа)
- `Google__ClientId` - Client ID для Google OAuth
- `Google__ClientSecret` - Client Secret для Google OAuth
- `Sms__ApiKey` - API ключ SMS провайдера
- `NopCommerce__ApiKey` - API ключ для внешнего nopCommerce

### Опциональные

- `Redis__ConnectionString` - строка подключения к Redis (по умолчанию: localhost:6379)
- `RabbitMQ__Host` - хост RabbitMQ (по умолчанию: localhost)
- `RabbitMQ__Port` - порт RabbitMQ (по умолчанию: 5672)
- `Jwt__AccessTokenExpirationMinutes` - время жизни access токена (по умолчанию: 15)
- `NopCommerce__BaseUrl` - базовый URL внешнего nopCommerce API
- `NopCommerce__TimeoutSeconds` - таймаут запросов к nopCommerce (по умолчанию: 30)

## API Endpoints

### Аутентификация

- `POST /api/auth/register` - регистрация нового пользователя
- `POST /api/auth/login` - вход по логину и паролю
- `POST /api/auth/google` - вход через Google OAuth
- `POST /api/auth/refresh` - обновление JWT токена
- `POST /api/auth/logout` - выход из системы

### Подтверждение телефона

- `POST /api/phone/send-code` - отправка кода подтверждения
- `POST /api/phone/verify` - проверка кода подтверждения
- `POST /api/phone/resend-code` - повторная отправка кода

### Профиль пользователя

- `GET /api/profile` - получение профиля (требует авторизации)
- `PUT /api/profile` - обновление профиля (требует авторизации)
- `PUT /api/profile/password` - смена пароля (требует авторизации)
- `DELETE /api/profile` - удаление аккаунта (требует авторизации)

### Пароли

- `GET /api/password/generate` - генерация надежного пароля

### Подписки

- `GET /api/subscriptions` - получение подписок пользователя (требует авторизации)
- `GET /api/subscriptions/plans` - получение доступных планов подписок
- `POST /api/subscriptions` - оформление подписки (требует авторизации)
- `DELETE /api/subscriptions/{id}` - отмена подписки (требует авторизации)

### Мониторинг

- `GET /health` - проверка состояния сервиса
- `GET /health/ready` - проверка готовности к обработке запросов
- `GET /health/live` - проверка работоспособности процесса

## Примеры API запросов

### Регистрация пользователя

```bash
curl -X POST http://localhost:8081/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "john_doe",
    "email": "john@example.com",
    "phoneNumber": "+79991234567",
    "password": "SecurePass123!",
    "confirmPassword": "SecurePass123!",
    "firstName": "Иван",
    "lastName": "Иванов"
  }'
```

Ответ:
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

### Подтверждение телефона

```bash
# Отправка кода
curl -X POST http://localhost:8081/api/phone/send-code \
  -H "Content-Type: application/json" \
  -d '{
    "phoneNumber": "+79991234567"
  }'

# Проверка кода
curl -X POST http://localhost:8081/api/phone/verify \
  -H "Content-Type: application/json" \
  -d '{
    "phoneNumber": "+79991234567",
    "code": "123456"
  }'
```

### Вход в систему

```bash
curl -X POST http://localhost:8081/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "john_doe",
    "password": "SecurePass123!",
    "rememberMe": true
  }'
```

Ответ:
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

### Вход через Google

```bash
curl -X POST http://localhost:8081/api/auth/google \
  -H "Content-Type: application/json" \
  -d '{
    "idToken": "google-id-token-here"
  }'
```

### Обновление токена

```bash
curl -X POST http://localhost:8081/api/auth/refresh \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "a1b2c3d4e5f6..."
  }'
```

### Получение профиля

```bash
curl -X GET http://localhost:8081/api/profile \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

Ответ:
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userName": "john_doe",
  "email": "john@example.com",
  "phoneNumber": "+79991234567",
  "phoneNumberConfirmed": true,
  "firstName": "Иван",
  "lastName": "Иванов",
  "middleName": null,
  "createdAt": "2026-03-01T10:00:00Z"
}
```

### Обновление профиля

```bash
curl -X PUT http://localhost:8081/api/profile \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json" \
  -d '{
    "email": "newemail@example.com",
    "firstName": "Иван",
    "lastName": "Петров",
    "middleName": "Сергеевич"
  }'
```

### Смена пароля

```bash
curl -X PUT http://localhost:8081/api/profile/password \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json" \
  -d '{
    "currentPassword": "SecurePass123!",
    "newPassword": "NewSecurePass456!",
    "confirmNewPassword": "NewSecurePass456!"
  }'
```

### Генерация надежного пароля

```bash
curl -X GET http://localhost:8081/api/password/generate
```

Ответ:
```json
{
  "password": "Kp9#mL2$xR5@nQ8!"
}
```

### Получение подписок

```bash
curl -X GET http://localhost:8081/api/subscriptions \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

Ответ:
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

### Получение доступных планов

```bash
curl -X GET http://localhost:8081/api/subscriptions/plans
```

Ответ:
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

### Оформление подписки

```bash
curl -X POST http://localhost:8081/api/subscriptions \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json" \
  -d '{
    "planId": 2
  }'
```

### Выход из системы

```bash
curl -X POST http://localhost:8081/api/auth/logout \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

### Проверка состояния сервиса

```bash
# Полная проверка всех зависимостей
curl -X GET http://localhost:8081/health

# Проверка готовности (для Kubernetes readiness probe)
curl -X GET http://localhost:8081/health/ready

# Проверка работоспособности (для Kubernetes liveness probe)
curl -X GET http://localhost:8081/health/live
```

## Интеграция с nopCommerce

Микросервис интегрируется с внешним nopCommerce API для управления подписками пользователей. nopCommerce развернут как отдельный сервис.

### Конфигурация

```json
{
  "NopCommerce": {
    "BaseUrl": "https://shop.example.com/api",
    "ApiKey": "your-api-key",
    "TimeoutSeconds": 30,
    "RetryCount": 3
  }
}
```

### Retry политика

Для обеспечения устойчивости к временным сбоям используется retry политика:
- 3 попытки с экспоненциальной задержкой
- Таймаут 30 секунд для каждого запроса

## Развертывание в Kubernetes

1. Создайте ConfigMap и Secret:
```bash
kubectl apply -f manifests/configmap.yaml
kubectl apply -f manifests/secret.yaml
```

2. Разверните приложение:
```bash
kubectl apply -f k8s/deployment.yaml
kubectl apply -f k8s/service.yaml
kubectl apply -f k8s/hpa.yaml
```

3. Проверьте статус:
```bash
kubectl get pods -l app=user-authentication-service
kubectl get svc user-authentication-service
```

## Graceful Shutdown

Микросервис поддерживает корректное завершение работы при получении сигнала SIGTERM:

- При получении сигнала завершения (SIGTERM) сервис прекращает принимать новые запросы
- Текущие запросы обрабатываются в течение 30 секунд (настраиваемый таймаут)
- После завершения обработки запросов автоматически закрываются подключения к:
  - PostgreSQL (через DbContext)
  - Redis (через IDistributedCache)
  - RabbitMQ (через зарегистрированные сервисы)
- Все события логируются для отслеживания процесса завершения

Это обеспечивает безопасное развертывание в Kubernetes без потери обрабатываемых запросов при обновлении или перезапуске подов.

## Безопасность

- Пароли хешируются с использованием PBKDF2
- JWT токены подписываются секретным ключом
- Rate limiting для защиты от брутфорса (5 попыток за 15 минут)
- HTTPS обязателен в продакшене
- CORS настроен только для доверенных доменов
- Все чувствительные данные хранятся в Kubernetes Secrets

## Логирование

Используется Serilog для структурированного логирования:
- Логи записываются в консоль и файлы
- Все операции аутентификации логируются в БД
- Подозрительная активность логируется с повышенным уровнем

## Тестирование

Запуск unit тестов:
```bash
cd Infrastructure/Services.UnitTests
dotnet test
```

## Лицензия

Proprietary
