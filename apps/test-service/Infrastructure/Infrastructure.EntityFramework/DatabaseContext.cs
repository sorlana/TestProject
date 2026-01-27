using System;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.EntityFramework
{
    /// <summary>
    /// Контекст.
    /// </summary>
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        /// <summary>
        /// Курсы.
        /// </summary>
        public DbSet<Course> Courses { get; set; }

        /// <summary>
        /// Уроки.
        /// </summary>
        public DbSet<Lesson> Lessons { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Конфигурация для PostgreSQL
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                // Устанавливаем схему по умолчанию
                entity.SetSchema("public");

                foreach (var property in entity.GetProperties())
                {
                    if (property.ClrType == typeof(string))
                    {
                        // Для PostgreSQL лучше использовать text вместо nvarchar
                        property.SetColumnType("text");
                    }
                    else if (property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?))
                    {
                        // Для денежных значений в PostgreSQL
                        property.SetColumnType("decimal(18,2)");
                    }
                }
            }

            modelBuilder.Entity<Course>()
                .HasMany(u => u.Lessons)
                .WithOne(c => c.Course)
                .IsRequired();

            modelBuilder.Entity<Course>().Property(c => c.Name).HasMaxLength(100);
            modelBuilder.Entity<Lesson>().Property(c => c.Subject).HasMaxLength(100);

            // Если нужно создать индексы для PostgreSQL
            modelBuilder.Entity<Course>().HasIndex(c => c.Name);
            modelBuilder.Entity<Lesson>().HasIndex(l => l.Subject);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Удалите или измените эту строку, так как она может переопределять настройки из Startup
            // Если хотите сохранить логгирование, используйте условие для окружения
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
            }
        }
    }
}