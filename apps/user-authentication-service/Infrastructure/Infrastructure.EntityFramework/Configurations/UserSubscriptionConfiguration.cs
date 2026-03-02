using Domain.Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.EntityFramework.Configurations;

/// <summary>
/// Конфигурация Entity Framework для сущности UserSubscription
/// </summary>
public class UserSubscriptionConfiguration : IEntityTypeConfiguration<UserSubscription>
{
    public void Configure(EntityTypeBuilder<UserSubscription> builder)
    {
        builder.ToTable("UserSubscriptions");

        // Первичный ключ
        builder.HasKey(us => us.Id);

        // Настройка свойств
        builder.Property(us => us.UserId)
            .IsRequired();

        builder.Property(us => us.SubscriptionPlanId)
            .IsRequired();

        builder.Property(us => us.PlanName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(us => us.StartDate)
            .IsRequired();

        builder.Property(us => us.EndDate)
            .IsRequired();

        builder.Property(us => us.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(us => us.AutoRenew)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(us => us.CreatedAt)
            .IsRequired();

        builder.Property(us => us.NopCommerceOrderId)
            .HasMaxLength(100);

        // Настройка индексов
        builder.HasIndex(us => new { us.UserId, us.IsActive })
            .HasDatabaseName("IX_UserSubscriptions_UserId_IsActive");

        builder.HasIndex(us => us.NopCommerceOrderId)
            .HasDatabaseName("IX_UserSubscriptions_NopCommerceOrderId")
            .HasFilter("\"NopCommerceOrderId\" IS NOT NULL");

        // Связь с User настроена в UserConfiguration
    }
}
