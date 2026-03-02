using Infrastructure.ExternalServices.Clients;
using Infrastructure.ExternalServices.Configuration;
using Infrastructure.ExternalServices.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Services.Abstractions.Interfaces;

namespace Infrastructure.ExternalServices.DependencyInjection;

/// <summary>
/// Класс для регистрации внешних сервисов в DI контейнере
/// </summary>
public static class ExternalServicesInstaller
{
    /// <summary>
    /// Регистрация внешних сервисов
    /// </summary>
    public static IServiceCollection AddExternalServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Регистрация настроек
        services.Configure<GoogleOAuthSettings>(configuration.GetSection("GoogleOAuth"));
        services.Configure<SmsSettings>(configuration.GetSection("Sms"));
        services.Configure<NopCommerceSettings>(configuration.GetSection("NopCommerce"));
        services.Configure<RabbitMqSettings>(configuration.GetSection(RabbitMqSettings.SectionName));

        // Регистрация Google Auth Service
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();

        // Регистрация SMS Service с HttpClient
        services.AddHttpClient<ISmsService, SmsService>()
            .SetHandlerLifetime(TimeSpan.FromMinutes(5))
            .AddPolicyHandler(GetRetryPolicy());

        // Регистрация NopCommerce Client с HttpClient и retry политикой
        services.AddHttpClient<INopCommerceClient, NopCommerceClient>()
            .SetHandlerLifetime(TimeSpan.FromMinutes(5))
            .AddPolicyHandler(GetRetryPolicy());

        // Регистрация Subscription Service
        // Примечание: SubscriptionService находится в Services.Implementations
        // и будет зарегистрирован там
        // services.AddScoped<ISubscriptionService, SubscriptionService>();

        // Регистрация RabbitMQ Event Publisher
        services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();

        return services;
    }

    /// <summary>
    /// Политика повторных попыток для HTTP клиентов
    /// </summary>
    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }
}
