using Domain.Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.EntityFramework.Configurations;

/// <summary>
/// Конфигурация Entity Framework для сущности PhoneVerificationCode
/// </summary>
public class PhoneVerificationCodeConfiguration : IEntityTypeConfiguration<PhoneVerificationCode>
{
    public void Configure(EntityTypeBuilder<PhoneVerificationCode> builder)
    {
        builder.ToTable("PhoneVerificationCodes");

        // Первичный ключ
        builder.HasKey(pvc => pvc.Id);

        // Настройка свойств
        builder.Property(pvc => pvc.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(pvc => pvc.Code)
            .IsRequired()
            .HasMaxLength(6);

        builder.Property(pvc => pvc.ExpiresAt)
            .IsRequired();

        builder.Property(pvc => pvc.IsUsed)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(pvc => pvc.CreatedAt)
            .IsRequired();

        builder.Property(pvc => pvc.AttemptCount)
            .IsRequired()
            .HasDefaultValue(0);

        // Настройка индексов
        builder.HasIndex(pvc => new { pvc.PhoneNumber, pvc.IsUsed, pvc.ExpiresAt })
            .HasDatabaseName("IX_PhoneVerificationCodes_PhoneNumber_IsUsed_ExpiresAt");
    }
}
