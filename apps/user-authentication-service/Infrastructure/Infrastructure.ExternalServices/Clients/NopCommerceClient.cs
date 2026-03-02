using Infrastructure.ExternalServices.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using Services.Contracts.DTOs;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Infrastructure.ExternalServices.Clients;

/// <summary>
/// HTTP клиент для интеграции с внешним nopCommerce API
/// Реализует retry политику и обработку ошибок
/// </summary>
public class NopCommerceClient : INopCommerceClient
{
    private readonly HttpClient _httpClient;
    private readonly NopCommerceSettings _settings;
    private readonly ILogger<NopCommerceClient> _logger;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

    public NopCommerceClient(
        HttpClient httpClient,
        IOptions<NopCommerceSettings> settings,
        ILogger<NopCommerceClient> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;

        // Настройка HttpClient
        _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
        _httpClient.DefaultRequestHeaders.Add("X-Api-Key", _settings.ApiKey);

        // Настройка retry политики с экспоненциальной задержкой
        _retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => 
                r.StatusCode == HttpStatusCode.RequestTimeout ||
                r.StatusCode == HttpStatusCode.ServiceUnavailable ||
                r.StatusCode == HttpStatusCode.GatewayTimeout ||
                (int)r.StatusCode >= 500)
            .Or<HttpRequestException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                _settings.RetryCount,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        "Повторная попытка {RetryCount} после {Delay}с. Причина: {Reason}",
                        retryCount,
                        timespan.TotalSeconds,
                        outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
                });
    }

    /// <summary>
    /// Получение доступных планов подписок
    /// </summary>
    public async Task<IEnumerable<SubscriptionPlanDto>> GetSubscriptionPlansAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Запрос планов подписок из nopCommerce");

            var response = await _retryPolicy.ExecuteAsync(async () =>
                await _httpClient.GetAsync("/api/subscriptions/plans", cancellationToken));

            await EnsureSuccessStatusCodeAsync(response);

            var plans = await response.Content.ReadFromJsonAsync<List<SubscriptionPlanDto>>(cancellationToken);

            _logger.LogInformation("Получено {Count} планов подписок", plans?.Count ?? 0);

            return plans ?? new List<SubscriptionPlanDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении планов подписок из nopCommerce");
            throw new InvalidOperationException("Не удалось получить планы подписок", ex);
        }
    }

    /// <summary>
    /// Получение подписок пользователя
    /// </summary>
    public async Task<IEnumerable<UserSubscriptionDto>> GetUserSubscriptionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Запрос подписок пользователя {UserId} из nopCommerce", userId);

            var response = await _retryPolicy.ExecuteAsync(async () =>
                await _httpClient.GetAsync($"/api/subscriptions/user/{userId}", cancellationToken));

            await EnsureSuccessStatusCodeAsync(response);

            var subscriptions = await response.Content.ReadFromJsonAsync<List<UserSubscriptionDto>>(cancellationToken);

            _logger.LogInformation("Получено {Count} подписок для пользователя {UserId}", 
                subscriptions?.Count ?? 0, userId);

            return subscriptions ?? new List<UserSubscriptionDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении подписок пользователя {UserId} из nopCommerce", userId);
            throw new InvalidOperationException($"Не удалось получить подписки пользователя {userId}", ex);
        }
    }

    /// <summary>
    /// Создание новой подписки
    /// </summary>
    public async Task<CreateSubscriptionResponse> CreateSubscriptionAsync(Guid userId, int planId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Создание подписки для пользователя {UserId}, план {PlanId}", userId, planId);

            var request = new
            {
                userId = userId,
                planId = planId
            };

            var response = await _retryPolicy.ExecuteAsync(async () =>
                await _httpClient.PostAsJsonAsync("/api/subscriptions", request, cancellationToken));

            await EnsureSuccessStatusCodeAsync(response);

            var result = await response.Content.ReadFromJsonAsync<CreateSubscriptionResponse>(cancellationToken);

            if (result == null)
            {
                throw new InvalidOperationException("Пустой ответ от nopCommerce при создании подписки");
            }

            _logger.LogInformation("Подписка успешно создана: OrderId={OrderId}, SubscriptionId={SubscriptionId}", 
                result.OrderId, result.SubscriptionId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании подписки для пользователя {UserId}", userId);
            throw new InvalidOperationException($"Не удалось создать подписку для пользователя {userId}", ex);
        }
    }

    /// <summary>
    /// Обновление статуса подписки
    /// </summary>
    public async Task<bool> UpdateSubscriptionStatusAsync(int subscriptionId, string status, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Обновление статуса подписки {SubscriptionId} на {Status}", subscriptionId, status);

            var request = new
            {
                status = status
            };

            var response = await _retryPolicy.ExecuteAsync(async () =>
                await _httpClient.PutAsJsonAsync($"/api/subscriptions/{subscriptionId}/status", request, cancellationToken));

            await EnsureSuccessStatusCodeAsync(response);

            _logger.LogInformation("Статус подписки {SubscriptionId} успешно обновлен", subscriptionId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении статуса подписки {SubscriptionId}", subscriptionId);
            return false;
        }
    }

    /// <summary>
    /// Проверка успешности HTTP ответа и логирование деталей ошибки
    /// </summary>
    private async Task EnsureSuccessStatusCodeAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            
            _logger.LogError(
                "Ошибка HTTP запроса к nopCommerce: {StatusCode} {ReasonPhrase}. Тело ответа: {Content}",
                (int)response.StatusCode,
                response.ReasonPhrase,
                content);

            throw new HttpRequestException(
                $"Ошибка запроса к nopCommerce: {response.StatusCode} {response.ReasonPhrase}");
        }
    }
}
