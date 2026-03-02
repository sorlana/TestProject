# Интеграция Identity в WebApi проект

Этот документ описывает, как интегрировать настроенный Identity в WebApi проект.

## Шаг 1: Добавить ссылку на проект

В файле `WebApi.csproj` добавьте ссылку на Infrastructure.Identity:

```xml
<ItemGroup>
  <ProjectReference Include="..\Infrastructure\Infrastructure.Identity\Infrastructure.Identity.csproj" />
  <ProjectReference Include="..\Infrastructure\Infrastructure.EntityFramework\Infrastructure.EntityFramework.csproj" />
</ItemGroup>
```

## Шаг 2: Зарегистрировать Identity в Program.cs

```csharp
using Infrastructure.Identity;
using Infrastructure.EntityFramework;
using Domain.Entities.Entities;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Регистрация DbContext
builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Регистрация Identity с настройками безопасности
builder.Services.AddIdentity<User, IdentityRole<Guid>>(IdentityInstaller.ConfigureIdentity)
    .AddEntityFrameworkStores<DatabaseContext>()
    .AddDefaultTokenProviders();

// Остальная конфигурация...
```

## Шаг 3: Добавить Authentication и Authorization middleware

```csharp
var app = builder.Build();

// Middleware для аутентификации и авторизации
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
```

## Проверка конфигурации

После регистрации Identity будут доступны следующие сервисы:

- `UserManager<User>` - управление пользователями
- `SignInManager<User>` - управление входом
- `RoleManager<IdentityRole<Guid>>` - управление ролями

### Пример использования в контроллере

```csharp
using Microsoft.AspNetCore.Identity;
using Domain.Entities.Entities;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public AuthController(
        UserManager<User> userManager,
        SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // Методы контроллера...
}
```

## Настройки безопасности

После интеграции автоматически применяются следующие настройки:

✅ Требования к паролю: минимум 8 символов, заглавная буква, цифра  
✅ Блокировка: 5 попыток за 15 минут, блокировка на 15 минут  
✅ Хеширование паролей: PBKDF2 (по умолчанию)  
✅ Уникальность email  
✅ Требование подтверждения телефона  

## Дополнительная настройка (опционально)

Если требуется изменить настройки Identity, можно использовать отдельные методы конфигурации:

```csharp
builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
{
    // Применить стандартную конфигурацию
    IdentityInstaller.ConfigureIdentity(options);
    
    // Переопределить отдельные параметры
    options.Password.RequiredLength = 10; // Увеличить минимальную длину пароля
    options.Lockout.MaxFailedAccessAttempts = 3; // Уменьшить количество попыток
})
.AddEntityFrameworkStores<DatabaseContext>()
.AddDefaultTokenProviders();
```
