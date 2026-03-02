using Domain.Entities.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.EntityFramework;

/// <summary>
/// Контекст базы данных для микросервиса аутентификации
/// Наследуется от IdentityDbContext для интеграции с ASP.NET Core Identity
/// </summary>
public class DatabaseContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
    }

    /// <summary>
    /// Токены обновления
    /// </summary>
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    /// <summary>
    /// Коды подтверждения телефона
    /// </summary>
    public DbSet<PhoneVerificationCode> PhoneVerificationCodes { get; set; }

    /// <summary>
    /// Подписки пользователей
    /// </summary>
    public DbSet<UserSubscription> UserSubscriptions { get; set; }

    /// <summary>
    /// Логи аутентификации
    /// </summary>
    public DbSet<AuthenticationLog> AuthenticationLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Применяем все конфигурации из текущей сборки
        builder.ApplyConfigurationsFromAssembly(typeof(DatabaseContext).Assembly);
    }
}
