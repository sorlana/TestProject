using Domain.Entities.Entities;
using Infrastructure.Identity.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Identity;

/// <summary>
/// Установщик сервисов Identity для регистрации в DI контейнере
/// </summary>
/// <remarks>
/// Этот класс предоставляет метод расширения для настройки Identity.
/// Фактическая регистрация Identity должна выполняться в WebApi проекте
/// с использованием метода AddIdentity или AddIdentityCore.
/// </remarks>
public static class IdentityInstaller
{
    /// <summary>
    /// Настраивает параметры Identity согласно требованиям безопасности
    /// </summary>
    /// <param name="options">Параметры Identity для настройки</param>
    /// <remarks>
    /// Настраивает:
    /// - Требования к паролю (минимум 8 символов, заглавная буква, цифра)
    /// - Блокировку учетной записи (5 попыток за 15 минут, блокировка на 15 минут)
    /// - Хеширование паролей (PBKDF2 используется по умолчанию)
    /// - Параметры пользователя и входа
    /// 
    /// Пример использования в WebApi проекте:
    /// <code>
    /// services.AddIdentity&lt;User, IdentityRole&lt;Guid&gt;&gt;(IdentityInstaller.ConfigureIdentity)
    ///     .AddEntityFrameworkStores&lt;DatabaseContext&gt;()
    ///     .AddDefaultTokenProviders();
    /// </code>
    /// </remarks>
    public static void ConfigureIdentity(IdentityOptions options)
    {
        IdentityConfiguration.ConfigureIdentityOptions(options);
    }
}


