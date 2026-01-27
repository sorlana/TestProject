using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Infrastructure.EntityFramework
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
    {
        public DatabaseContext CreateDbContext(string[] args)
        {
            // Добавляем отладочный вывод
            Console.WriteLine("=== DesignTimeDbContextFactory ===");
            Console.WriteLine($"Current Directory: {Directory.GetCurrentDirectory()}");

            // Пытаемся найти appsettings.json в разных местах
            var basePath1 = Directory.GetCurrentDirectory();
            var basePath2 = Path.Combine(Directory.GetCurrentDirectory(), "..", "WebApi");
            var basePath3 = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "WebApi");

            Console.WriteLine($"BasePath1 (Current): {basePath1}");
            Console.WriteLine($"BasePath2 (../WebApi): {basePath2}");
            Console.WriteLine($"BasePath3 (../../WebApi): {basePath3}");

            // Проверяем существование appsettings.json в этих путях
            Console.WriteLine($"appsettings.json exists in Current: {File.Exists(Path.Combine(basePath1, "appsettings.json"))}");
            Console.WriteLine($"appsettings.json exists in ../WebApi: {File.Exists(Path.Combine(basePath2, "appsettings.json"))}");
            Console.WriteLine($"appsettings.json exists in ../../WebApi: {File.Exists(Path.Combine(basePath3, "appsettings.json"))}");

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            Console.WriteLine($"ConnectionString from config: {connectionString}");

            // Если строка подключения пустая, используем запасной вариант
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = "Host=localhost;Port=5432;Database=myappdb;Username=myappuser;Password=postgres;";
                Console.WriteLine($"Using fallback connection string: {connectionString}");
            }

            Console.WriteLine("=== End DesignTimeDbContextFactory ===");

            var builder = new DbContextOptionsBuilder<DatabaseContext>();
            builder.UseNpgsql(connectionString,
                options => options.MigrationsAssembly(typeof(DatabaseContext).Assembly.FullName));

            return new DatabaseContext(builder.Options);
        }
    }
}