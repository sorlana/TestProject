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
/// Реализация сервиса аутентификации
/// Обеспечивает регистрацию, вход и выход пользователей
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IPhoneVerificationService _phoneVerificationService;
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IAuthenticationLogRepository _authenticationLogRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        UserManager<User> userManager,
        ITokenService tokenService,
        IPhoneVerificationService phoneVerificationService,
        IGoogleAuthService googleAuthService,
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IAuthenticationLogRepository authenticationLogRepository,
        IEventPublisher eventPublisher,
        ILogger<AuthenticationService> logger)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _phoneVerificationService = phoneVerificationService;
        _googleAuthService = googleAuthService;
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _authenticationLogRepository = authenticationLogRepository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    /// <summary>
    /// Регистрация нового пользователя
    /// </summary>
    public async Task<AuthenticationResult> RegisterAsync(
        RegisterRequest request, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Попытка регистрации пользователя с логином {UserName} и email {Email}", 
            request.UserName, request.Email);

        // Валидация данных
        if (request.Password != request.ConfirmPassword)
        {
            _logger.LogWarning("Регистрация отклонена: пароли не совпадают для пользователя {UserName}", request.UserName);
            return new AuthenticationResult
            {
                Success = false,
                Errors = new[] { "Пароли не совпадают" }
            };
        }

        // Проверка уникальности логина
        var existingUserByUsername = await _userRepository.GetByUserNameAsync(request.UserName, cancellationToken);
        if (existingUserByUsername != null)
        {
            _logger.LogWarning("Регистрация отклонена: логин {UserName} уже существует", request.UserName);
            return new AuthenticationResult
            {
                Success = false,
                Errors = new[] { "Пользователь с таким логином уже существует" }
            };
        }

        // Проверка уникальности email
        var existingUserByEmail = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUserByEmail != null)
        {
            _logger.LogWarning("Регистрация отклонена: email {Email} уже существует", request.Email);
            return new AuthenticationResult
            {
                Success = false,
                Errors = new[] { "Пользователь с таким email уже существует" }
            };
        }

        // Проверка уникальности телефона
        var existingUserByPhone = await _userRepository.GetByPhoneNumberAsync(request.PhoneNumber, cancellationToken);
        if (existingUserByPhone != null)
        {
            _logger.LogWarning("Регистрация отклонена: телефон {PhoneNumber} уже существует", request.PhoneNumber);
            return new AuthenticationResult
            {
                Success = false,
                Errors = new[] { "Пользователь с таким номером телефона уже существует" }
            };
        }

        // Создание пользователя
        var user = new User
        {
            UserName = request.UserName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            Deleted = false
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Ошибка при создании пользователя {UserName}: {Errors}", request.UserName, errors);
            return new AuthenticationResult
            {
                Success = false,
                Errors = result.Errors.Select(e => e.Description)
            };
        }

        _logger.LogInformation("Пользователь {UserName} успешно зарегистрирован с ID {UserId}", user.UserName, user.Id);

        // Отправка кода подтверждения телефона
        try
        {
            await _phoneVerificationService.SendVerificationCodeAsync(user.PhoneNumber, cancellationToken);
            _logger.LogInformation("Код подтверждения отправлен на номер {PhoneNumber}", user.PhoneNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отправке кода подтверждения на номер {PhoneNumber}", user.PhoneNumber);
        }

        // Логирование события регистрации
        await _authenticationLogRepository.CreateAsync(new AuthenticationLog
        {
            UserId = user.Id,
            UserName = user.UserName,
            EventType = AuthenticationEventType.Registration,
            Success = true,
            IpAddress = "Unknown",
            Timestamp = DateTime.UtcNow,
            LogLevel = Domain.Entities.Enums.LogLevel.Information
        }, cancellationToken);

        // Публикация события UserRegistered
        try
        {
            await _eventPublisher.PublishAsync(new UserRegisteredEvent
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                RegisteredAt = DateTime.UtcNow,
                RegistrationMethod = "Password"
            }, cancellationToken);

            _logger.LogInformation("Событие UserRegistered опубликовано для пользователя {UserId}", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при публикации события UserRegistered для пользователя {UserId}", user.Id);
        }

        return new AuthenticationResult
        {
            Success = true,
            RequiresPhoneVerification = true,
            User = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FirstName = user.FirstName,
                LastName = user.LastName,
                MiddleName = user.MiddleName
            }
        };
    }

    /// <summary>
    /// Вход пользователя по логину и паролю
    /// </summary>
    public async Task<AuthenticationResult> LoginAsync(
        LoginRequest request, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Попытка входа пользователя {UserName}", request.UserName);

        var user = await _userRepository.GetByUserNameAsync(request.UserName, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("Вход отклонен: пользователь {UserName} не найден", request.UserName);
            await LogFailedLoginAsync(null, request.UserName, "Неверный логин или пароль", cancellationToken);
            return new AuthenticationResult
            {
                Success = false,
                Errors = new[] { "Неверный логин или пароль" }
            };
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Вход отклонен: учетная запись {UserName} неактивна", request.UserName);
            await LogFailedLoginAsync(user.Id, request.UserName, "Учетная запись неактивна", cancellationToken);
            return new AuthenticationResult
            {
                Success = false,
                Errors = new[] { "Учетная запись неактивна" }
            };
        }

        if (user.Deleted)
        {
            _logger.LogWarning("Вход отклонен: учетная запись {UserName} удалена", request.UserName);
            await LogFailedLoginAsync(user.Id, request.UserName, "Учетная запись удалена", cancellationToken);
            return new AuthenticationResult
            {
                Success = false,
                Errors = new[] { "Учетная запись не найдена" }
            };
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            _logger.LogWarning("Вход отклонен: неверный пароль для пользователя {UserName}", request.UserName);
            await LogFailedLoginAsync(user.Id, request.UserName, "Неверный пароль", cancellationToken);
            return new AuthenticationResult
            {
                Success = false,
                Errors = new[] { "Неверный логин или пароль" }
            };
        }

        if (!user.PhoneNumberConfirmed)
        {
            _logger.LogWarning("Вход отклонен: телефон не подтвержден для пользователя {UserName}", request.UserName);
            await LogFailedLoginAsync(user.Id, request.UserName, "Телефон не подтвержден", cancellationToken);
            return new AuthenticationResult
            {
                Success = false,
                RequiresPhoneVerification = true,
                Errors = new[] { "Необходимо подтвердить номер телефона" }
            };
        }

        var accessToken = _tokenService.GenerateJwtToken(user, await _userManager.GetRolesAsync(user));
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id, request.RememberMe, cancellationToken);

        _logger.LogInformation("Пользователь {UserName} успешно вошел в систему", request.UserName);

        await _authenticationLogRepository.CreateAsync(new AuthenticationLog
        {
            UserId = user.Id,
            UserName = user.UserName,
            EventType = AuthenticationEventType.Login,
            Success = true,
            IpAddress = "Unknown",
            Timestamp = DateTime.UtcNow,
            LogLevel = Domain.Entities.Enums.LogLevel.Information
        }, cancellationToken);

        // Публикация события UserLoggedIn
        try
        {
            await _eventPublisher.PublishAsync(new UserLoggedInEvent
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                LoggedInAt = DateTime.UtcNow,
                IpAddress = "Unknown",
                LoginMethod = "Password"
            }, cancellationToken);

            _logger.LogInformation("Событие UserLoggedIn опубликовано для пользователя {UserId}", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при публикации события UserLoggedIn для пользователя {UserId}", user.Id);
        }

        return new AuthenticationResult
        {
            Success = true,
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = refreshToken.ExpiresAt,
            User = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FirstName = user.FirstName,
                LastName = user.LastName,
                MiddleName = user.MiddleName
            }
        };
    }

    /// <summary>
    /// Вход через Google OAuth
    /// </summary>
    public async Task<AuthenticationResult> LoginWithGoogleAsync(
        GoogleAuthRequest request, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Попытка входа через Google");

        var googleUserInfo = await _googleAuthService.ValidateGoogleTokenAsync(request.IdToken, cancellationToken);
        if (googleUserInfo == null)
        {
            _logger.LogWarning("Вход через Google отклонен: невалидный токен");
            return new AuthenticationResult
            {
                Success = false,
                Errors = new[] { "Невалидный Google токен" }
            };
        }

        var user = await _googleAuthService.GetOrCreateUserFromGoogleAsync(googleUserInfo, cancellationToken);
        
        if (!user.IsActive)
        {
            _logger.LogWarning("Вход отклонен: учетная запись {UserName} неактивна", user.UserName);
            return new AuthenticationResult
            {
                Success = false,
                Errors = new[] { "Учетная запись неактивна" }
            };
        }

        if (user.Deleted)
        {
            _logger.LogWarning("Вход отклонен: учетная запись {UserName} удалена", user.UserName);
            return new AuthenticationResult
            {
                Success = false,
                Errors = new[] { "Учетная запись не найдена" }
            };
        }

        var requiresPhoneVerification = !user.PhoneNumberConfirmed;

        var accessToken = _tokenService.GenerateJwtToken(user, await _userManager.GetRolesAsync(user));
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id, false, cancellationToken);

        _logger.LogInformation("Пользователь {UserName} успешно вошел через Google", user.UserName);

        await _authenticationLogRepository.CreateAsync(new AuthenticationLog
        {
            UserId = user.Id,
            UserName = user.UserName,
            EventType = AuthenticationEventType.GoogleLogin,
            Success = true,
            IpAddress = "Unknown",
            Timestamp = DateTime.UtcNow,
            LogLevel = Domain.Entities.Enums.LogLevel.Information
        }, cancellationToken);

        // Публикация события UserLoggedIn
        try
        {
            await _eventPublisher.PublishAsync(new UserLoggedInEvent
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                LoggedInAt = DateTime.UtcNow,
                IpAddress = "Unknown",
                LoginMethod = "Google"
            }, cancellationToken);

            _logger.LogInformation("Событие UserLoggedIn опубликовано для пользователя {UserId}", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при публикации события UserLoggedIn для пользователя {UserId}", user.Id);
        }

        return new AuthenticationResult
        {
            Success = true,
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = refreshToken.ExpiresAt,
            RequiresPhoneVerification = requiresPhoneVerification,
            User = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FirstName = user.FirstName,
                LastName = user.LastName,
                MiddleName = user.MiddleName
            }
        };
    }

    /// <summary>
    /// Обновление токенов через refresh token
    /// </summary>
    public async Task<AuthenticationResult> RefreshTokenAsync(
        string refreshToken, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Попытка обновления токенов");

        var isValid = await _tokenService.ValidateRefreshTokenAsync(refreshToken, cancellationToken);
        if (!isValid)
        {
            _logger.LogWarning("Обновление токенов отклонено: невалидный refresh token");
            return new AuthenticationResult
            {
                Success = false,
                Errors = new[] { "Невалидный или истекший refresh token" }
            };
        }

        var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken, cancellationToken);
        if (token == null)
        {
            _logger.LogWarning("Обновление токенов отклонено: refresh token не найден");
            return new AuthenticationResult
            {
                Success = false,
                Errors = new[] { "Refresh token не найден" }
            };
        }

        var user = await _userRepository.GetByIdAsync(token.UserId, cancellationToken);
        if (user == null || !user.IsActive || user.Deleted)
        {
            _logger.LogWarning("Обновление токенов отклонено: пользователь не найден или неактивен");
            return new AuthenticationResult
            {
                Success = false,
                Errors = new[] { "Пользователь не найден" }
            };
        }

        var accessToken = _tokenService.GenerateJwtToken(user, await _userManager.GetRolesAsync(user));
        var newRefreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id, false, cancellationToken);

        await _tokenService.RevokeRefreshTokenAsync(refreshToken, cancellationToken);

        _logger.LogInformation("Токены успешно обновлены для пользователя {UserId}", user.Id);

        return new AuthenticationResult
        {
            Success = true,
            AccessToken = accessToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresAt = newRefreshToken.ExpiresAt,
            User = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FirstName = user.FirstName,
                LastName = user.LastName,
                MiddleName = user.MiddleName
            }
        };
    }

    /// <summary>
    /// Выход из системы
    /// </summary>
    public async Task LogoutAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Попытка выхода пользователя {UserId}", userId);

        await _tokenService.RevokeAllUserTokensAsync(userId, cancellationToken);

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user != null)
        {
            await _authenticationLogRepository.CreateAsync(new AuthenticationLog
            {
                UserId = user.Id,
                UserName = user.UserName,
                EventType = AuthenticationEventType.Logout,
                Success = true,
                IpAddress = "Unknown",
                Timestamp = DateTime.UtcNow,
                LogLevel = Domain.Entities.Enums.LogLevel.Information
            }, cancellationToken);
        }

        _logger.LogInformation("Пользователь {UserId} успешно вышел из системы", userId);
    }

    /// <summary>
    /// Логирование неудачной попытки входа
    /// </summary>
    private async Task LogFailedLoginAsync(
        Guid? userId, 
        string userName, 
        string reason, 
        CancellationToken cancellationToken)
    {
        await _authenticationLogRepository.CreateAsync(new AuthenticationLog
        {
            UserId = userId,
            UserName = userName,
            EventType = AuthenticationEventType.FailedLogin,
            Success = false,
            FailureReason = reason,
            IpAddress = "Unknown",
            Timestamp = DateTime.UtcNow,
            LogLevel = Domain.Entities.Enums.LogLevel.Warning
        }, cancellationToken);
    }
}
