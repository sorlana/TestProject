using Domain.Entities.Entities;
using Domain.Entities.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Services.Abstractions.Interfaces;
using Services.Contracts.DTOs;
using Services.Contracts.Events;
using Services.Contracts.Results;
using Services.Repositories.Abstractions.Interfaces;

namespace Services.Implementations.Implementations;

/// <summary>
/// Реализация сервиса управления профилем пользователя
/// Обеспечивает просмотр, обновление и удаление профиля
/// </summary>
public class UserProfileService : IUserProfileService
{
    private readonly UserManager<User> _userManager;
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IPhoneVerificationService _phoneVerificationService;
    private readonly IAuthenticationLogRepository _authenticationLogRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<UserProfileService> _logger;

    public UserProfileService(
        UserManager<User> userManager,
        IUserRepository userRepository,
        ITokenService tokenService,
        IPhoneVerificationService phoneVerificationService,
        IAuthenticationLogRepository authenticationLogRepository,
        IEventPublisher eventPublisher,
        ILogger<UserProfileService> logger)
    {
        _userManager = userManager;
        _userRepository = userRepository;
        _tokenService = tokenService;
        _phoneVerificationService = phoneVerificationService;
        _authenticationLogRepository = authenticationLogRepository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    /// <summary>
    /// Получение профиля пользователя
    /// </summary>
    public async Task<UserProfileDto> GetProfileAsync(
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user == null)
        {
            throw new InvalidOperationException("Пользователь не найден.");
        }

        return new UserProfileDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            FirstName = user.FirstName,
            LastName = user.LastName,
            MiddleName = user.MiddleName,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            HasGoogleAccount = !string.IsNullOrEmpty(user.GoogleId)
        };
    }

    /// <summary>
    /// Обновление профиля пользователя
    /// </summary>
    public async Task<UpdateProfileResult> UpdateProfileAsync(
        Guid userId, 
        UpdateProfileRequest request, 
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user == null)
        {
            return new UpdateProfileResult
            {
                Success = false,
                Errors = new[] { "Пользователь не найден." }
            };
        }

        bool requiresEmailVerification = false;
        bool requiresPhoneVerification = false;

        // Обновление email
        if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != user.Email)
        {
            // Проверка уникальности email
            var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (existingUser != null && existingUser.Id != userId)
            {
                return new UpdateProfileResult
                {
                    Success = false,
                    Errors = new[] { "Пользователь с таким email уже существует." }
                };
            }

            user.Email = request.Email;
            user.EmailConfirmed = false; // Помечаем email как неподтвержденный
            requiresEmailVerification = true;
        }

        // Обновление телефона
        if (!string.IsNullOrWhiteSpace(request.PhoneNumber) && request.PhoneNumber != user.PhoneNumber)
        {
            // Проверка уникальности телефона
            var existingUser = await _userRepository.GetByPhoneNumberAsync(request.PhoneNumber, cancellationToken);
            if (existingUser != null && existingUser.Id != userId)
            {
                return new UpdateProfileResult
                {
                    Success = false,
                    Errors = new[] { "Пользователь с таким номером телефона уже существует." }
                };
            }

            user.PhoneNumber = request.PhoneNumber;
            user.PhoneNumberConfirmed = false; // Помечаем телефон как неподтвержденный
            requiresPhoneVerification = true;

            // Инициирование подтверждения через SMS
            await _phoneVerificationService.SendVerificationCodeAsync(request.PhoneNumber, cancellationToken);
        }

        // Обновление имени, фамилии, отчества
        if (!string.IsNullOrWhiteSpace(request.FirstName))
        {
            user.FirstName = request.FirstName;
        }

        if (!string.IsNullOrWhiteSpace(request.LastName))
        {
            user.LastName = request.LastName;
        }

        if (!string.IsNullOrWhiteSpace(request.MiddleName))
        {
            user.MiddleName = request.MiddleName;
        }

        user.UpdatedAt = DateTime.UtcNow;

        // Сохранение изменений
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return new UpdateProfileResult
            {
                Success = false,
                Errors = result.Errors.Select(e => e.Description)
            };
        }

        return new UpdateProfileResult
        {
            Success = true,
            RequiresEmailVerification = requiresEmailVerification,
            RequiresPhoneVerification = requiresPhoneVerification,
            Profile = new UserProfileDto
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                FirstName = user.FirstName,
                LastName = user.LastName,
                MiddleName = user.MiddleName,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                HasGoogleAccount = !string.IsNullOrEmpty(user.GoogleId)
            }
        };
    }

    /// <summary>
    /// Смена пароля пользователя
    /// </summary>
    public async Task<ChangePasswordResult> ChangePasswordAsync(
        Guid userId, 
        ChangePasswordRequest request, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Попытка смены пароля для пользователя (ID: {UserId})", userId);

        // Валидация данных
        if (request.NewPassword != request.ConfirmNewPassword)
        {
            _logger.LogWarning("Смена пароля отклонена: новые пароли не совпадают для пользователя (ID: {UserId})", userId);
            return new ChangePasswordResult
            {
                Success = false,
                Errors = new[] { "Новые пароли не совпадают." }
            };
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Смена пароля отклонена: пользователь (ID: {UserId}) не найден", userId);
            return new ChangePasswordResult
            {
                Success = false,
                Errors = new[] { "Пользователь не найден." }
            };
        }

        // Проверка текущего пароля
        var passwordValid = await _userManager.CheckPasswordAsync(user, request.CurrentPassword);

        if (!passwordValid)
        {
            _logger.LogWarning("Смена пароля отклонена: неверный текущий пароль для пользователя {UserName} (ID: {UserId})", 
                user.UserName, userId);
            return new ChangePasswordResult
            {
                Success = false,
                Errors = new[] { "Текущий пароль неверен." }
            };
        }

        // Смена пароля
        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        if (!result.Succeeded)
        {
            _logger.LogError("Ошибка при смене пароля для пользователя {UserName} (ID: {UserId}): {Errors}", 
                user.UserName, userId, string.Join(", ", result.Errors.Select(e => e.Description)));
            return new ChangePasswordResult
            {
                Success = false,
                Errors = result.Errors.Select(e => e.Description)
            };
        }

        // Отзыв всех refresh токенов при смене пароля
        await _tokenService.RevokeAllUserTokensAsync(userId, cancellationToken);

        _logger.LogInformation("Пароль успешно изменен для пользователя {UserName} (ID: {UserId}). Все активные сессии завершены.", 
            user.UserName, userId);

        // Логирование смены пароля
        await _authenticationLogRepository.CreateAsync(new AuthenticationLog
        {
            UserId = userId,
            UserName = user.UserName,
            EventType = AuthenticationEventType.PasswordChange,
            Success = true,
            IpAddress = string.Empty,
            Timestamp = DateTime.UtcNow,
            LogLevel = Domain.Entities.Enums.LogLevel.Information
        }, cancellationToken);

        return new ChangePasswordResult
        {
            Success = true,
            Message = "Пароль успешно изменен. Все активные сессии завершены."
        };
    }

    /// <summary>
    /// Удаление аккаунта пользователя (мягкое удаление)
    /// </summary>
    public async Task<DeleteAccountResult> DeleteAccountAsync(
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user == null)
        {
            return new DeleteAccountResult
            {
                Success = false,
                Errors = new[] { "Пользователь не найден." }
            };
        }

        // Мягкое удаление (soft delete)
        user.Deleted = true;
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);

        // Отзыв всех refresh токенов при удалении аккаунта
        await _tokenService.RevokeAllUserTokensAsync(userId, cancellationToken);

        // Логирование удаления аккаунта
        await _authenticationLogRepository.CreateAsync(new AuthenticationLog
        {
            UserId = userId,
            UserName = user.UserName,
            EventType = AuthenticationEventType.SuspiciousActivity,
            Success = true,
            FailureReason = "Аккаунт удален пользователем",
            IpAddress = string.Empty,
            Timestamp = DateTime.UtcNow,
            LogLevel = Domain.Entities.Enums.LogLevel.Information
        }, cancellationToken);

        // Публикация события UserDeleted
        try
        {
            await _eventPublisher.PublishAsync(new UserDeletedEvent
            {
                UserId = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                DeletedAt = DateTime.UtcNow,
                Reason = "Удаление по запросу пользователя"
            }, cancellationToken);

            _logger.LogInformation("Событие UserDeleted опубликовано для пользователя {UserId}", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при публикации события UserDeleted для пользователя {UserId}", user.Id);
        }

        return new DeleteAccountResult
        {
            Success = true,
            Message = "Аккаунт успешно удален."
        };
    }
}
