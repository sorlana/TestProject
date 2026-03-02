namespace Services.Contracts.Enums;

/// <summary>
/// Уровень надежности пароля
/// </summary>
public enum PasswordStrength
{
    /// <summary>
    /// Очень слабый пароль
    /// </summary>
    VeryWeak = 0,
    
    /// <summary>
    /// Слабый пароль
    /// </summary>
    Weak = 1,
    
    /// <summary>
    /// Средний пароль
    /// </summary>
    Medium = 2,
    
    /// <summary>
    /// Сильный пароль
    /// </summary>
    Strong = 3,
    
    /// <summary>
    /// Очень сильный пароль
    /// </summary>
    VeryStrong = 4
}
