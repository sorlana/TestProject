using Domain.Entities.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Identity.Configuration;

/// <summary>
/// Конфигурация параметров ASP.NET Core Identity для микросервиса аутентификации
/// </summary>
public static class IdentityConfiguration
{
    /// <summary>
    /// Настраивает параметры Identity
    /// Этот метод должен вызываться после AddIdentity в WebApi проекте
    /// </summary>
    /// <param name="options">Параметры Identity</param>
    public static void ConfigureIdentityOptions(IdentityOptions options)
    {
        // Настройка требований к паролю
        ConfigurePasswordOptions(options.Password);
        
        // Настройка блокировки учетной записи (lockout)
        ConfigureLockoutOptions(options.Lockout);
        
        // Настройка параметров пользователя
        ConfigureUserOptions(options.User);
        
        // Настройка параметров входа
        ConfigureSignInOptions(options.SignIn);
    }
    
    /// <summary>
    /// Настраивает требования к паролю
    /// Требования: минимум 8 символов, заглавная буква, цифра
    /// </summary>
    public static void ConfigurePasswordOptions(PasswordOptions options)
    {
        options.RequireDigit = true;                    // Требуется цифра
        options.RequireLowercase = true;                // Требуется строчная буква
        options.RequireUppercase = true;                // Требуется заглавная буква
        options.RequireNonAlphanumeric = false;         // Спецсимволы не обязательны
        options.RequiredLength = 8;                     // Минимальная длина 8 символов
        options.RequiredUniqueChars = 1;                // Минимум 1 уникальный символ
    }
    
    /// <summary>
    /// Настраивает параметры блокировки учетной записи
    /// Требования: 5 попыток за 15 минут, блокировка на 15 минут
    /// </summary>
    public static void ConfigureLockoutOptions(LockoutOptions options)
    {
        options.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);  // Длительность блокировки: 15 минут
        options.MaxFailedAccessAttempts = 5;                        // Максимум неудачных попыток: 5
        options.AllowedForNewUsers = true;                          // Включить lockout для новых пользователей
    }
    
    /// <summary>
    /// Настраивает параметры пользователя
    /// </summary>
    public static void ConfigureUserOptions(UserOptions options)
    {
        options.RequireUniqueEmail = true;              // Email должен быть уникальным
        options.AllowedUserNameCharacters =             // Разрешенные символы в имени пользователя
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+абвгдеёжзийклмнопрстуфхцчшщъыьэюя";
    }
    
    /// <summary>
    /// Настраивает параметры входа
    /// </summary>
    public static void ConfigureSignInOptions(SignInOptions options)
    {
        options.RequireConfirmedEmail = false;          // Email подтверждение не требуется
        options.RequireConfirmedPhoneNumber = true;     // Требуется подтверждение телефона
        options.RequireConfirmedAccount = false;        // Подтверждение аккаунта не требуется
    }
}
