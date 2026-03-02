using Domain.Entities.Entities;
using Infrastructure.ExternalServices.Clients;
using Infrastructure.ExternalServices.Configuration;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Services.Abstractions.Interfaces;
using Services.Contracts.DTOs;
using Services.Contracts.Events;
using Services.Contracts.Results;
using Services.Repositories.Abstractions.Interfaces;
using System.Text.Json;

namespace Services.Implementations.Services;

/// <summary>
/// Сервис управления подписками
/// Обеспечивает интеграцию с внешним nopCommerce API и локальное кеширование
/// </summary>
public class SubscriptionService : ISubscriptionService
{
    private readonly INopCommerceClient _nopCommerceClient;
    private readonly IUserSubscriptionRepository _subscriptionRepository;
    private readonly IDistributedCache _cache;
    private readonly NopCommerceSettings _settings;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<SubscriptionService> _logger;
    
    private const string CacheKeyPrefix = "subscription_plans";

    public SubscriptionService(
        INopCommerceClient nopCommerceClient,
        IUserSubscriptionRepository subscriptionRepository,
        IDistributedCache cache,
        IOptions<NopCommerceSettings> settings,
        IEventPublisher eventPublisher,
        ILogger<SubscriptionService> logger)
    {
        _nopCommerceClient = nopCommerceClient;
        _subscriptionRepository = subscriptionRepository;
        _cache = cache;
        _settings = settings.Value;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    /// <summary>
    /// Получение подписок пользователя
    /// </summary>
    public async Task<IEnumerable<UserSubscriptionDto>> GetUserSubscriptionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Получение подписок пользователя {UserId}", userId);

            // Попытка получить данные из внешнего nopCommerce
            IEnumerable<UserSubscriptionDto> subscriptions;
            
            try
            {
                subscriptions = await _nopCommerceClient.GetUserSubscriptionsAsync(userId, cancellationToken);
                
                // Синхронизация с локальной БД
                await SynchronizeSubscriptionsAsync(userId, subscriptions, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Не удалось получить подписки из nopCommerce, используем локальные данные");
                
                // Fallback на локальные данные при недоступности nopCommerce
                var localSubscriptions = await _subscriptionRepository.GetByUserIdAsync(userId, cancellationToken);
                subscriptions = localSubscriptions.Select(MapToDto);
            }

            _logger.LogInformation("Получено {Count} подписок для пользователя {UserId}", 
                subscriptions.Count(), userId);

            return subscriptions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении подписок пользователя {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Получение доступных планов подписок с кешированием
    /// </summary>
    public async Task<IEnumerable<SubscriptionPlanDto>> GetAvailablePlansAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Получение доступных планов подписок");

            // Попытка получить из кеша
            var cachedPlans = await _cache.GetStringAsync(CacheKeyPrefix, cancellationToken);
            if (!string.IsNullOrEmpty(cachedPlans))
            {
                _logger.LogDebug("Планы подписок получены из кеша");
                var plans = JsonSerializer.Deserialize<List<SubscriptionPlanDto>>(cachedPlans);
                if (plans != null)
                {
                    return plans;
                }
            }

            // Получение из nopCommerce
            var plansFromApi = await _nopCommerceClient.GetSubscriptionPlansAsync(cancellationToken);
            var plansList = plansFromApi.ToList();

            // Сохранение в кеш
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_settings.CacheTtlMinutes)
            };

            var serializedPlans = JsonSerializer.Serialize(plansList);
            await _cache.SetStringAsync(CacheKeyPrefix, serializedPlans, cacheOptions, cancellationToken);

            _logger.LogInformation("Получено {Count} планов подписок", plansList.Count);

            return plansList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении планов подписок");
            throw;
        }
    }

    /// <summary>
    /// Оформление подписки
    /// </summary>
    public async Task<SubscribeResult> SubscribeAsync(Guid userId, int planId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Оформление подписки для пользователя {UserId}, план {PlanId}", userId, planId);

            // Проверка наличия активной подписки
            var hasActiveSubscription = await _subscriptionRepository.HasActiveSubscriptionAsync(userId, cancellationToken);
            if (hasActiveSubscription)
            {
                _logger.LogWarning("У пользователя {UserId} уже есть активная подписка", userId);
                return new SubscribeResult
                {
                    Success = false,
                    Message = "У вас уже есть активная подписка",
                    Errors = new[] { "У пользователя уже есть активная подписка" }
                };
            }

            // Создание подписки через nopCommerce
            CreateSubscriptionResponse nopCommerceResponse;
            try
            {
                nopCommerceResponse = await _nopCommerceClient.CreateSubscriptionAsync(userId, planId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании подписки в nopCommerce");
                return new SubscribeResult
                {
                    Success = false,
                    Message = "Не удалось оформить подписку. Попробуйте позже.",
                    Errors = new[] { "Сервис подписок временно недоступен" }
                };
            }

            // Получение информации о плане
            var plans = await GetAvailablePlansAsync(cancellationToken);
            var plan = plans.FirstOrDefault(p => p.Id == planId);

            // Создание записи в локальной БД
            var subscription = new UserSubscription
            {
                UserId = userId,
                SubscriptionPlanId = planId,
                PlanName = plan?.Name ?? "Неизвестный план",
                StartDate = nopCommerceResponse.StartDate,
                EndDate = nopCommerceResponse.EndDate,
                IsActive = true,
                AutoRenew = false,
                CreatedAt = DateTime.UtcNow,
                NopCommerceOrderId = nopCommerceResponse.OrderId
            };

            var createdSubscription = await _subscriptionRepository.CreateAsync(subscription, cancellationToken);

            _logger.LogInformation("Подписка успешно создана: {SubscriptionId}", createdSubscription.Id);

            // Публикация события SubscriptionCreated
            try
            {
                await _eventPublisher.PublishAsync(new SubscriptionCreatedEvent
                {
                    UserId = userId,
                    SubscriptionId = createdSubscription.Id,
                    SubscriptionPlanId = planId,
                    PlanName = subscription.PlanName,
                    StartDate = subscription.StartDate,
                    EndDate = subscription.EndDate,
                    CreatedAt = DateTime.UtcNow,
                    NopCommerceOrderId = subscription.NopCommerceOrderId
                }, cancellationToken);

                _logger.LogInformation("Событие SubscriptionCreated опубликовано для подписки {SubscriptionId}", createdSubscription.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при публикации события SubscriptionCreated для подписки {SubscriptionId}", createdSubscription.Id);
            }

            return new SubscribeResult
            {
                Success = true,
                Message = "Подписка успешно оформлена",
                Subscription = MapToDto(createdSubscription)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при оформлении подписки для пользователя {UserId}", userId);
            return new SubscribeResult
            {
                Success = false,
                Message = "Произошла ошибка при оформлении подписки",
                Errors = new[] { ex.Message }
            };
        }
    }

    /// <summary>
    /// Отмена подписки
    /// </summary>
    public async Task<CancelSubscriptionResult> CancelSubscriptionAsync(Guid userId, int subscriptionId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Отмена подписки {SubscriptionId} для пользователя {UserId}", subscriptionId, userId);

            // Получение подписки
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId, cancellationToken);
            if (subscription == null)
            {
                _logger.LogWarning("Подписка {SubscriptionId} не найдена", subscriptionId);
                return new CancelSubscriptionResult
                {
                    Success = false,
                    Message = "Подписка не найдена",
                    Errors = new[] { "Подписка не найдена" }
                };
            }

            // Проверка принадлежности подписки пользователю
            if (subscription.UserId != userId)
            {
                _logger.LogWarning("Попытка отменить чужую подписку: {SubscriptionId}, пользователь {UserId}", subscriptionId, userId);
                return new CancelSubscriptionResult
                {
                    Success = false,
                    Message = "Доступ запрещен",
                    Errors = new[] { "У вас нет прав на отмену этой подписки" }
                };
            }

            // Обновление статуса в nopCommerce
            try
            {
                await _nopCommerceClient.UpdateSubscriptionStatusAsync(subscriptionId, "Cancelled", cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Не удалось обновить статус подписки в nopCommerce, обновляем только локально");
            }

            // Обновление локальной записи
            subscription.IsActive = false;
            subscription.CancelledAt = DateTime.UtcNow;
            subscription.AutoRenew = false;

            await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);

            _logger.LogInformation("Подписка {SubscriptionId} успешно отменена", subscriptionId);

            return new CancelSubscriptionResult
            {
                Success = true,
                Message = "Подписка успешно отменена"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отмене подписки {SubscriptionId}", subscriptionId);
            return new CancelSubscriptionResult
            {
                Success = false,
                Message = "Произошла ошибка при отмене подписки",
                Errors = new[] { ex.Message }
            };
        }
    }

    /// <summary>
    /// Проверка наличия активной подписки у пользователя
    /// </summary>
    public async Task<bool> HasActiveSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Проверка наличия активной подписки для пользователя {UserId}", userId);

            var hasActive = await _subscriptionRepository.HasActiveSubscriptionAsync(userId, cancellationToken);

            _logger.LogDebug("Пользователь {UserId} {Status} активную подписку", 
                userId, hasActive ? "имеет" : "не имеет");

            return hasActive;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при проверке активной подписки для пользователя {UserId}", userId);
            return false;
        }
    }

    /// <summary>
    /// Синхронизация подписок между nopCommerce и локальной БД
    /// </summary>
    private async Task SynchronizeSubscriptionsAsync(
        Guid userId, 
        IEnumerable<UserSubscriptionDto> nopCommerceSubscriptions, 
        CancellationToken cancellationToken)
    {
        try
        {
            var localSubscriptions = await _subscriptionRepository.GetByUserIdAsync(userId, cancellationToken);
            var localDict = localSubscriptions.ToDictionary(s => s.Id);

            foreach (var nopSub in nopCommerceSubscriptions)
            {
                if (localDict.TryGetValue(nopSub.Id, out var localSub))
                {
                    // Обновление существующей подписки
                    if (localSub.IsActive != nopSub.IsActive || 
                        localSub.EndDate != nopSub.EndDate)
                    {
                        localSub.IsActive = nopSub.IsActive;
                        localSub.EndDate = nopSub.EndDate;
                        localSub.CancelledAt = nopSub.CancelledAt;
                        
                        await _subscriptionRepository.UpdateAsync(localSub, cancellationToken);
                        _logger.LogDebug("Синхронизирована подписка {SubscriptionId}", localSub.Id);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ошибка при синхронизации подписок для пользователя {UserId}", userId);
        }
    }

    /// <summary>
    /// Маппинг сущности подписки в DTO
    /// </summary>
    private static UserSubscriptionDto MapToDto(UserSubscription subscription)
    {
        return new UserSubscriptionDto
        {
            Id = subscription.Id,
            UserId = subscription.UserId,
            SubscriptionPlanId = subscription.SubscriptionPlanId,
            PlanName = subscription.PlanName,
            StartDate = subscription.StartDate,
            EndDate = subscription.EndDate,
            IsActive = subscription.IsActive,
            AutoRenew = subscription.AutoRenew,
            CreatedAt = subscription.CreatedAt,
            CancelledAt = subscription.CancelledAt
        };
    }
}
