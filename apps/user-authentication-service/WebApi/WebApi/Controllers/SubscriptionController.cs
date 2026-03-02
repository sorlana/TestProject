using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Abstractions.Interfaces;
using Services.Contracts.DTOs;
using System.ComponentModel.DataAnnotations;

namespace WebApi.Controllers;

/// <summary>
/// Контроллер управления подписками
/// Обеспечивает работу с подписками пользователей через интеграцию с nopCommerce
/// </summary>
[ApiController]
[Route("api/subscriptions")]
[Produces("application/json")]
public class SubscriptionController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ILogger<SubscriptionController> _logger;

    public SubscriptionController(
        ISubscriptionService subscriptionService,
        ILogger<SubscriptionController> logger)
    {
        _subscriptionService = subscriptionService;
        _logger = logger;
    }

    /// <summary>
    /// Получение подписок текущего пользователя
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Список подписок пользователя</returns>
    /// <response code="200">Подписки получены успешно</response>
    /// <response code="401">Пользователь не авторизован</response>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<UserSubscriptionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserSubscriptions(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Пользователь не авторизован" });
        }

        var subscriptions = await _subscriptionService.GetUserSubscriptionsAsync(userId.Value, cancellationToken);

        return Ok(subscriptions);
    }

    /// <summary>
    /// Получение доступных планов подписок
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Список доступных планов</returns>
    /// <response code="200">Планы получены успешно</response>
    [HttpGet("plans")]
    [ProducesResponseType(typeof(IEnumerable<SubscriptionPlanDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailablePlans(CancellationToken cancellationToken)
    {
        var plans = await _subscriptionService.GetAvailablePlansAsync(cancellationToken);

        return Ok(plans);
    }

    /// <summary>
    /// Оформление подписки
    /// </summary>
    /// <param name="request">Идентификатор плана подписки</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат оформления подписки</returns>
    /// <response code="200">Подписка оформлена успешно</response>
    /// <response code="400">Ошибка при оформлении подписки</response>
    /// <response code="401">Пользователь не авторизован</response>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Subscribe(
        [FromBody] SubscribeRequestModel request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Пользователь не авторизован" });
        }

        var result = await _subscriptionService.SubscribeAsync(userId.Value, request.PlanId, cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning("Не удалось оформить подписку для пользователя {UserId}", userId.Value);
            return BadRequest(new { errors = result.Errors, message = result.Message });
        }

        _logger.LogInformation("Пользователь {UserId} успешно оформил подписку на план {PlanId}", userId.Value, request.PlanId);
        return Ok(new
        {
            message = result.Message ?? "Подписка успешно оформлена",
            subscription = result.Subscription
        });
    }

    /// <summary>
    /// Отмена подписки
    /// </summary>
    /// <param name="id">Идентификатор подписки</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат отмены подписки</returns>
    /// <response code="200">Подписка отменена успешно</response>
    /// <response code="400">Ошибка при отмене подписки</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="404">Подписка не найдена</response>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelSubscription(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Пользователь не авторизован" });
        }

        var result = await _subscriptionService.CancelSubscriptionAsync(userId.Value, id, cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning("Не удалось отменить подписку {SubscriptionId} для пользователя {UserId}", id, userId.Value);
            
            // Проверяем, является ли ошибка "не найдено"
            if (result.Errors?.Any(e => e.Contains("не найдена") || e.Contains("not found")) == true)
            {
                return NotFound(new { errors = result.Errors, message = result.Message });
            }
            
            return BadRequest(new { errors = result.Errors, message = result.Message });
        }

        _logger.LogInformation("Подписка {SubscriptionId} пользователя {UserId} успешно отменена", id, userId.Value);
        return Ok(new { message = result.Message ?? "Подписка успешно отменена" });
    }

    /// <summary>
    /// Получение идентификатора текущего пользователя из claims
    /// </summary>
    /// <returns>Идентификатор пользователя или null</returns>
    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("UserId") ?? User.FindFirst("sub") ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }

        return null;
    }
}

/// <summary>
/// Модель запроса оформления подписки с валидацией
/// </summary>
public class SubscribeRequestModel
{
    /// <summary>
    /// Идентификатор плана подписки
    /// </summary>
    [Required(ErrorMessage = "Идентификатор плана обязателен")]
    [Range(1, int.MaxValue, ErrorMessage = "Идентификатор плана должен быть положительным числом")]
    public int PlanId { get; set; }
}
