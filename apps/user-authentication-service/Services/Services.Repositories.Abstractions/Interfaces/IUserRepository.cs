using Domain.Entities.Entities;

namespace Services.Repositories.Abstractions.Interfaces;

/// <summary>
/// Интерфейс репозитория пользователей
/// Обеспечивает доступ к данным пользователей
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Получение пользователя по идентификатору
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Пользователь или null</returns>
    Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получение пользователя по email
    /// </summary>
    /// <param name="email">Email адрес</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Пользователь или null</returns>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получение пользователя по имени пользователя
    /// </summary>
    /// <param name="userName">Имя пользователя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Пользователь или null</returns>
    Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получение пользователя по номеру телефона
    /// </summary>
    /// <param name="phoneNumber">Номер телефона</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Пользователь или null</returns>
    Task<User?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получение пользователя по Google ID
    /// </summary>
    /// <param name="googleId">Google идентификатор</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Пользователь или null</returns>
    Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Создание нового пользователя
    /// </summary>
    /// <param name="user">Пользователь для создания</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    Task<User> CreateAsync(User user, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Обновление пользователя
    /// </summary>
    /// <param name="user">Пользователь для обновления</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Удаление пользователя (мягкое удаление)
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    Task DeleteAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Проверка существования пользователя по email
    /// </summary>
    /// <param name="email">Email адрес</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>True если пользователь существует</returns>
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Проверка существования пользователя по имени пользователя
    /// </summary>
    /// <param name="userName">Имя пользователя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>True если пользователь существует</returns>
    Task<bool> ExistsByUserNameAsync(string userName, CancellationToken cancellationToken = default);
}
