using Domain.Entities.Entities;
using Google.Apis.Auth;
using Infrastructure.ExternalServices.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Services.Abstractions.Interfaces;
using Services.Contracts.DTOs;

namespace Infrastructure.ExternalServices.Services;

/// <summary>
/// Сервис аутентификации через Google OAuth
/// </summary>
public class GoogleAuthService : IGoogleAuthService
{
    private readonly GoogleOAuthSettings _settings;
    private readonly UserManager<User> _userManager;
    private readonly IPhoneVerificationService _phoneVerificationService;
    private readonly ILogger<GoogleAuthService> _logger;

    public GoogleAuthService(
        IOptions<GoogleOAuthSettings> settings,
        UserManager<User> userManager,
        IPhoneVerificationService phoneVerificationService,
        ILogger<GoogleAuthService> logger)
    {
        _settings = settings.Value;
        _userManager = userManager;
        _phoneVerificationService = phoneVerificationService;
        _logger = logger;
    }

    /// <summary>
    /// Валидация Google ID токена и получение информации о пользователе
    /// </summary>
    public async Task<GoogleUserInfo> ValidateGoogleTokenAsync(string idToken, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Начало валидации Google ID токена");

            // Валидация токена через Google API
            var validationSettings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _settings.ClientId }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, validationSettings);

            _logger.LogInformation("Google токен успешно валидирован для пользователя {Email}", payload.Email);

            // Преобразование в DTO
            return new GoogleUserInfo
            {
                GoogleId = payload.Subject,
                Email = payload.Email,
                FirstName = payload.GivenName,
                LastName = payload.FamilyName,
                EmailVerified = payload.EmailVerified
            };
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogWarning(ex, "Невалидный Google ID токен");
            throw new UnauthorizedAccessException("Невалидный Google токен", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при валидации Google токена");
            throw;
        }
    }

    /// <summary>
    /// Получение существующего пользователя или создание нового из данных Google
    /// </summary>
    public async Task<User> GetOrCreateUserFromGoogleAsync(GoogleUserInfo googleInfo, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Поиск или создание пользователя для Google ID {GoogleId}", googleInfo.GoogleId);

            // Поиск пользователя по Google ID
            var existingUserByGoogleId = await _userManager.FindByLoginAsync("Google", googleInfo.GoogleId);
            if (existingUserByGoogleId != null)
            {
                _logger.LogInformation("Найден пользователь по Google ID: {UserId}", existingUserByGoogleId.Id);
                return existingUserByGoogleId;
            }

            // Поиск пользователя по email
            var existingUserByEmail = await _userManager.FindByEmailAsync(googleInfo.Email);
            if (existingUserByEmail != null)
            {
                _logger.LogInformation("Найден пользователь по email, связывание с Google аккаунтом: {UserId}", existingUserByEmail.Id);

                // Связывание Google аккаунта с существующим пользователем
                var loginInfo = new UserLoginInfo("Google", googleInfo.GoogleId, "Google");
                var addLoginResult = await _userManager.AddLoginAsync(existingUserByEmail, loginInfo);

                if (!addLoginResult.Succeeded)
                {
                    _logger.LogError("Ошибка при связывании Google аккаунта: {Errors}", 
                        string.Join(", ", addLoginResult.Errors.Select(e => e.Description)));
                    throw new InvalidOperationException("Не удалось связать Google аккаунт с существующим пользователем");
                }

                // Обновление Google ID
                existingUserByEmail.GoogleId = googleInfo.GoogleId;
                
                // Подтверждение email если он подтвержден в Google
                if (googleInfo.EmailVerified && !existingUserByEmail.EmailConfirmed)
                {
                    existingUserByEmail.EmailConfirmed = true;
                }

                await _userManager.UpdateAsync(existingUserByEmail);

                return existingUserByEmail;
            }

            // Создание нового пользователя
            _logger.LogInformation("Создание нового пользователя из Google данных для email {Email}", googleInfo.Email);

            var newUser = new User
            {
                UserName = googleInfo.Email.Split('@')[0], // Используем часть email до @ как username
                Email = googleInfo.Email,
                EmailConfirmed = googleInfo.EmailVerified,
                FirstName = googleInfo.FirstName,
                LastName = googleInfo.LastName,
                GoogleId = googleInfo.GoogleId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true,
                Deleted = false,
                PhoneNumberConfirmed = false // Требуется подтверждение телефона
            };

            // Создание пользователя без пароля (вход только через Google)
            var createResult = await _userManager.CreateAsync(newUser);
            if (!createResult.Succeeded)
            {
                _logger.LogError("Ошибка при создании пользователя: {Errors}", 
                    string.Join(", ", createResult.Errors.Select(e => e.Description)));
                throw new InvalidOperationException($"Не удалось создать пользователя: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
            }

            // Добавление внешнего логина
            var newUserLoginInfo = new UserLoginInfo("Google", googleInfo.GoogleId, "Google");
            var newUserAddLoginResult = await _userManager.AddLoginAsync(newUser, newUserLoginInfo);
            if (!newUserAddLoginResult.Succeeded)
            {
                _logger.LogError("Ошибка при добавлении Google логина: {Errors}", 
                    string.Join(", ", newUserAddLoginResult.Errors.Select(e => e.Description)));
                
                // Откат создания пользователя
                await _userManager.DeleteAsync(newUser);
                throw new InvalidOperationException("Не удалось добавить Google логин");
            }

            _logger.LogInformation("Новый пользователь успешно создан: {UserId}", newUser.Id);

            return newUser;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении или создании пользователя из Google данных");
            throw;
        }
    }
}
