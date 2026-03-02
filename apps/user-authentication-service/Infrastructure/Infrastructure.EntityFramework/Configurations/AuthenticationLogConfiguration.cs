using Domain.Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.EntityFramework.Configurations;

/// <summary>
/// Конфигурация Entity Framework для сущности AuthenticationLog
/// </summary>
public class AuthenticationLogConfiguration : IEntityTypeConfiguration<AuthenticationLog>
{
    public void Configure(EntityTypeBuilder<AuthenticationLog> builder)
    {
        builder.ToTable("AuthenticationLogs");

        // Первичный ключ
        builder.HasKey(al => al.Id);

        // Настройка свойств
        builder.Property(al => al.UserName)
            .HasMaxLength(256);

        builder.Property(al => al.EventType)
            .IsRequired();

        builder.Property(al => al.Success)
            .IsRequired();

        builder.Property(al => al.FailureReason)
            .HasMaxLength(500);

        builder.Property(al => al.IpAddress)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(al => al.UserAgent)
            .HasMaxLength(500);

        builder.Property(al => al.Timestamp)
            .IsRequired();

        builder.Property(al => al.LogLevel)
            .IsRequired();

        // Настройка индексов
        builder.HasIndex(al => new { al.UserId, al.Timestamp })
            .HasDatabaseName("IX_AuthenticationLogs_UserId_Timestamp");

        builder.HasIndex(al => new { al.IpAddress, al.Timestamp })
            .HasDatabaseName("IX_AuthenticationLogs_IpAddress_Timestamp");

        builder.HasIndex(al => new { al.EventType, al.Timestamp })
            .HasDatabaseName("IX_AuthenticationLogs_EventType_Timestamp");

        // Связь с User настроена в UserConfiguration
    }
}
