using System;
using System.Diagnostics;
using AutoMapper;
using MassTransit;
using MassTransit.RabbitMqTransport;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebApi.Mapping;
using WebApi.Settings;
using Infrastructure.EntityFramework;

namespace WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            InstallAutomapper(services);

            // Добавляем DatabaseContext для PostgreSQL
            services.AddDbContext<DatabaseContext>(options =>
            {
                options.UseNpgsql(
                    Configuration.GetConnectionString("DefaultConnection"),
                    npgsqlOptions =>
                    {
                        // ВАЖНО: Указываем сборку, где будут храниться миграции
                        // Это должен быть проект Infrastructure.EntityFramework, а не WebApi
                        npgsqlOptions.MigrationsAssembly(typeof(Infrastructure.EntityFramework.DatabaseContext).Assembly.FullName);
                        npgsqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorCodesToAdd: null);
                    });

                // Включаем логгирование SQL-запросов только в Development
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }
            });

            services.AddServices(Configuration);
            services.AddControllers();

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen();

            services.AddMassTransit(x => {
                x.UsingRabbitMq((context, cfg) =>
                {
                    ConfigureRmq(cfg, Configuration);
                });
            });

            // Добавляем Health Checks
            services.AddHealthChecks()
                .AddDbContextCheck<DatabaseContext>(
                    name: "database-context-check",
                    tags: new[] { "db_ef_healthcheck" });

            services.AddCors();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            app.UseHealthChecks("/health");
            app.UseHealthChecks("/db_ef_healthcheck", new HealthCheckOptions
            {
                Predicate = healthCheck => healthCheck.Tags.Contains("db_ef_healthcheck")
            });

            app.UseRouting();

            app.UseAuthorization();

            if (!env.IsProduction())
            {
                // Enable middleware to serve generated Swagger as a JSON endpoint.
                app.UseSwagger();

                // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
                // specifying the Swagger JSON endpoint.

                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                    c.RoutePrefix = string.Empty;
                });
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private static IServiceCollection InstallAutomapper(IServiceCollection services)
        {
            // Для AutoMapper 16 нужно использовать такую перегрузку:
            services.AddAutoMapper(cfg => { },
                typeof(CourseMappingsProfile).Assembly,
                typeof(Services.Implementations.Mapping.CourseMappingsProfile).Assembly);

            return services;
        }

        /// <summary>
        /// Конфигурирование RMQ.
        /// </summary>
        /// <param name="configurator"> Конфигуратор RMQ. </param>
        /// <param name="configuration"> Конфигурация приложения. </param>
        private static void ConfigureRmq(IRabbitMqBusFactoryConfigurator configurator, IConfiguration configuration)
        {
            var rmqSettings = configuration.Get<ApplicationSettings>().RmqSettings;
            configurator.Host(rmqSettings.Host,
                rmqSettings.VHost,
                h =>
                {
                    h.Username(rmqSettings.Login);
                    h.Password(rmqSettings.Password);
                });
        }
    }
}