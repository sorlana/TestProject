using Services.Contracts.Enums;

namespace Services.Abstractions.Interfaces;

/// <summary>
/// Интерфейс сервиса работы с паролями
/// Обеспечивает генерацию, валидацию и оценку надежности паролей
/// </summary>
public interface IPasswordService
{
    /// <summary>
    /// Генерация надежного пароля
    /// </summary>
    /// <returns>Сгенерированный пароль</returns>
    string GenerateSecurePassword();
    
    /// <summary>
    /// Валидация пароля по правилам
    /// </summary>
    /// <param name="password">Пароль для проверки</param>
    /// <returns>True если пароль валиден, иначе False</returns>
    Task<bool> ValidatePasswordAsync(string password);
    
    /// <summary>
    /// Оценка надежности пароля
    /// </summary>
    /// <param name="password">Пароль для оценки</param>
    /// <returns>Уровень надежности пароля</returns>
    PasswordStrength EvaluatePasswordStrength(string password);
}
