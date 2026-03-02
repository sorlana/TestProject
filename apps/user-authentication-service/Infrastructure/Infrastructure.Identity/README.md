# Infrastructure.Identity

Проект содержит конфигурацию ASP.NET Core Identity для микросервиса аутентификации пользователей.

## Описание

Этот проект предоставляет настройку параметров ASP.NET Core Identity с требованиями безопасности согласно спецификации. Проект содержит только конфигурацию, фактическая регистрация Identity выполняется в WebApi проекте.

## Конфигурация

### Требования к паролю

- Минимальная длина: 8 символов
- Обязательно наличие заглавной буквы
- Обязательно наличие строчной буквы
- Обязательно наличие цифры
- Специальные символы не обязательны
- Минимум 1 уникальный символ

### Блокировка учетной записи (Lockout)

- Максимальное количество неудачных попыток входа: 5
- Период отслеживания попыток: 15 минут
- Длительность блокировки: 15 минут
- Блокировка включена для всех пользователей, включая новых

### Хеширование паролей

- Используется алгоритм PBKDF2 (по умолчанию в ASP.NET Core Identity)
- Пароли хранятся только в виде хеша
- Автоматическое добавление соли (salt) для каждого пароля
- Дополнительная настройка не требуется

### Параметры пользователя

- Email должен быть уникальным
- Требуется подтверждение номера телефона
- Email подтверждение не требуется
- Разрешенные символы в имени пользователя: латиница, кириллица, цифры и специальные символы (.-_@+)

### Параметры входа

- Требуется подтверждение номера телефона (RequireConfirmedPhoneNumber = true)
- Email подтверждение не требуется (RequireConfirmedEmail = false)
- Подтверждение аккаунта не требуется (RequireConfirmedAccount = false)

## Использование

### Регистрация в DI контейнере (WebApi проект)

```csharp
// В Program.cs
using Infrastructure.Identity;
using Domain.Entities.Entities;
using Microsoft.AspNetCore.Identity;

// Регистрация Identity с настройками безопасности
services.AddIdentity<User, IdentityRole<Guid>>(IdentityInstaller.ConfigureIdentity)
    .AddEntityFrameworkStores<DatabaseContext>()
    .AddDefaultTokenProviders();
```

### Альтернативный способ с явной конфигурацией

```csharp
using Infrastructure.Identity.Configuration;

services.AddIdentity<User, IdentityRole<Guid>>(options =>
{
    IdentityConfiguration.ConfigureIdentityOptions(options);
})
.AddEntityFrameworkStores<DatabaseContext>()
.AddDefaultTokenProviders();
```

### Использование отдельных методов конфигурации

```csharp
using Infrastructure.Identity.Configuration;

services.AddIdentity<User, IdentityRole<Guid>>(options =>
{
    // Настройка только паролей
    IdentityConfiguration.ConfigurePasswordOptions(options.Password);
    
    // Настройка только lockout
    IdentityConfiguration.ConfigureLockoutOptions(options.Lockout);
    
    // Настройка только пользователей
    IdentityConfiguration.ConfigureUserOptions(options.User);
    
    // Настройка только входа
    IdentityConfiguration.ConfigureSignInOptions(options.SignIn);
})
.AddEntityFrameworkStores<DatabaseContext>()
.AddDefaultTokenProviders();
```

## Зависимости

- Microsoft.AspNetCore.Identity.EntityFrameworkCore 8.0.11
- Microsoft.Extensions.DependencyInjection.Abstractions 8.0.2
- Microsoft.Extensions.Identity.Core 8.0.11
- Domain.Entities (ссылка на проект)

## Связанные требования

- Требование 1.4: Валидация пароля (минимум 8 символов, заглавная буква, цифра)
- Требование 12.1: Хеширование паролей с PBKDF2
- Требование 12.2: Хранение только хеша пароля
- Требование 12.3: Сравнение хешей при аутентификации
- Требование 13.1: Защита от брутфорса (блокировка после 5 попыток за 15 минут на 15 минут)

## Примечания

- PBKDF2 используется по умолчанию в ASP.NET Core Identity для хеширования паролей
- Дополнительная настройка алгоритма хеширования не требуется
- Все параметры безопасности соответствуют требованиям спецификации
- Конфигурация применяется автоматически при регистрации Identity в DI контейнере
