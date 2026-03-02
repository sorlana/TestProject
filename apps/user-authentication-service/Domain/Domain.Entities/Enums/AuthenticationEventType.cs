namespace Domain.Entities.Enums;

/// <summary>
/// Типы событий аутентификации для логирования
/// </summary>
public enum AuthenticationEventType
{
    /// <summary>
    /// Успешный вход в систему
    /// </summary>
    Login,
    
    /// <summary>
    /// Выход из системы
    /// </summary>
    Logout,
    
    /// <summary>
    /// Неудачная попытка входа
    /// </summary>
    FailedLogin,
    
    /// <summary>
    /// Регистрация нового пользователя
    /// </summary>
    Registration,
    
    /// <summary>
    /// Смена пароля
    /// </summary>
    PasswordChange,
    
    /// <summary>
    /// Обновление токена
    /// </summary>
    TokenRefresh,
    
    /// <summary>
    /// Подтверждение телефона
    /// </summary>
    PhoneVerification,
    
    /// <summary>
    /// Вход через Google OAuth
    /// </summary>
    GoogleLogin,
    
    /// <summary>
    /// Подозрительная активность
    /// </summary>
    SuspiciousActivity
}
