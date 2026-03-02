using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Domain.Entities.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Services.Abstractions.Interfaces;
using Services.Implementations.Settings;
using Services.Repositories.Abstractions.Interfaces;

namespace Services.Implementations.Implementations;

/// <summary>
/// Реализация сервиса работы с токенами
/// Обеспечивает генерацию, валидацию и отзыв JWT и refresh токенов
/// </summary>
public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public TokenService(
        IOptions<JwtSettings> jwtSettings,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _jwtSettings = jwtSettings.Value;
        _refreshTokenRepository = refreshTokenRepository;
    }

    /// <summary>
    /// Генерация JWT токена для пользователя
    /// </summary>
    public string GenerateJwtToken(User user, IList<string> roles)
    {
        // Создаем claims для токена
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Добавляем роли в claims
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        // Создаем ключ подписи
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Создаем токен
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        // Возвращаем строковое представление токена
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Генерация refresh токена
    /// </summary>
    public async Task<RefreshToken> GenerateRefreshTokenAsync(
        Guid userId, 
        bool rememberMe, 
        CancellationToken cancellationToken = default)
    {
        // Генерируем криптографически случайную строку для токена
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        var tokenValue = Convert.ToBase64String(randomBytes);

        // Определяем время жизни токена в зависимости от флага "Запомнить меня"
        var expirationDays = rememberMe 
            ? _jwtSettings.RefreshTokenExpirationDaysRememberMe 
            : _jwtSettings.RefreshTokenExpirationDays;

        // Создаем объект refresh токена
        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = tokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(expirationDays),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        // Сохраняем токен в базе данных
        await _refreshTokenRepository.CreateAsync(refreshToken, cancellationToken);

        return refreshToken;
    }

    /// <summary>
    /// Валидация refresh токена
    /// </summary>
    public async Task<bool> ValidateRefreshTokenAsync(
        string token, 
        CancellationToken cancellationToken = default)
    {
        // Получаем токен из базы данных
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(token, cancellationToken);

        // Проверяем существование токена
        if (refreshToken == null)
        {
            return false;
        }

        // Проверяем, не отозван ли токен
        if (refreshToken.IsRevoked)
        {
            return false;
        }

        // Проверяем срок действия токена
        if (refreshToken.ExpiresAt < DateTime.UtcNow)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Отзыв refresh токена
    /// </summary>
    public async Task RevokeRefreshTokenAsync(
        string token, 
        CancellationToken cancellationToken = default)
    {
        // Отзываем токен через репозиторий
        await _refreshTokenRepository.RevokeAsync(token, null, cancellationToken);
    }

    /// <summary>
    /// Отзыв всех refresh токенов пользователя
    /// </summary>
    public async Task RevokeAllUserTokensAsync(
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        // Отзываем все токены пользователя через репозиторий
        await _refreshTokenRepository.RevokeAllUserTokensAsync(userId, null, cancellationToken);
    }
}
