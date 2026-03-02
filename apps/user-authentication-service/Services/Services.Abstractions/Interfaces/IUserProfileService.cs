using Services.Contracts.DTOs;
using Services.Contracts.Results;

namespace Services.Abstractions.Interfaces;

/// <summary>
/// Интерфейс сервиса управления профилем пользователя
/// Обеспечивает просмотр, обновление и удаление профиля
/// </summary>
public interface IUserProfileService
{
    /// <summary>
    /// Получение профиля пользователя
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Данные профиля пользователя</returns>
    Task<UserProfileDto> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Обновление профиля пользователя
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="request">Данные для обновления</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат обновления профиля</returns>
    Task<UpdateProfileResult> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Смена пароля пользователя
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="request">Данные для смены пароля</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат смены пароля</returns>
    Task<ChangePasswordResult> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Удаление аккаунта пользователя (мягкое удаление)
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат удаления аккаунта</returns>
    Task<DeleteAccountResult> DeleteAccountAsync(Guid userId, CancellationToken cancellationToken = default);
}
