# Infrastructure.ExternalServices

Проект содержит реализацию интеграций с внешними сервисами для микросервиса аутентификации пользователей.

## Компоненты

### 1. GoogleAuthService
Сервис аутентификации через Google OAuth 2.0.

**Функциональность:**
- Валидация Google ID токенов
- Получение информации о пользователе из Google
- Создание новых пользователей из Google данных
- Связывание Google аккаунтов с существующими пользователями

**Конфигурация (appsettings.json):**
```json
{
  "GoogleOAuth": {
    "ClientId": "your-client-id.apps.googleusercontent.com",
    "ClientSecret": "your-client-secret"
  }
}
```

### 2. SmsService
Сервис отправки SMS через провайдера SMS.ru.

**Функциональность:**
- Отправка SMS сообщений
- Обработка ошибок отправки
- Логирование отправленных сообщений
- Маскирование номеров телефонов в логах

**Конфигурация (appsettings.json):**
```json
{
  "Sms": {
    "ApiKey": "your-sms-api-key",
    "ApiUrl": "https://sms.ru",
    "SenderName": "YourApp",
    "TimeoutSeconds": 30
  }
}
```

### 3. NopCommerceClient
HTTP клиент для интеграции с внешним nopCommerce API.

**Функциональность:**
- Получение планов подписок
- Получение подписок пользователя
- Создание новых подписок
- Обновление статуса подписок
- Retry политика с экспоненциальной задержкой
- Обработка таймаутов и ошибок HTTP

**Конфигурация (appsettings.json):**
```json
{
  "NopCommerce": {
    "BaseUrl": "https://shop.example.com/api",
    "ApiKey": "your-nopcommerce-api-key",
    "TimeoutSeconds": 30,
    "RetryCount": 3,
    "CacheTtlMinutes": 60
  }
}
```

## Регистрация в DI контейнере

```csharp
services.AddExternalServices(configuration);
```

Этот метод регистрирует все внешние сервисы с необходимыми зависимостями:
- GoogleAuthService
- SmsService с HttpClient
- NopCommerceClient с HttpClient и retry политикой
- Настройки из конфигурации

## Зависимости

- **Google.Apis.Auth** - для валидации Google токенов
- **Polly** - для retry политики HTTP запросов
- **Microsoft.Extensions.Caching.StackExchangeRedis** - для кеширования
- **Microsoft.Extensions.Http.Polly** - для интеграции Polly с HttpClient

## Обработка ошибок

Все сервисы логируют ошибки и возвращают понятные сообщения:
- GoogleAuthService выбрасывает UnauthorizedAccessException при невалидном токене
- SmsService возвращает false при ошибке отправки
- NopCommerceClient выбрасывает InvalidOperationException при ошибках API

## Retry политика

NopCommerceClient и SmsService используют retry политику:
- 3 попытки с экспоненциальной задержкой (2^n секунд)
- Повтор при HTTP 500, 503, 504, таймаутах
- Логирование каждой попытки

## Безопасность

- API ключи передаются через заголовки HTTP
- Номера телефонов маскируются в логах
- Чувствительные данные должны храниться в переменных окружения или секретах
