/**
 * Интерфейс сервиса управления токенами
 */

/**
 * Интерфейс для сервиса управления JWT токенами
 * Определяет контракт для работы с токенами в localStorage/sessionStorage
 */
export interface ITokenService {
  /**
   * Сохранение JWT токенов в хранилище
   * @param accessToken - JWT токен доступа
   * @param refreshToken - JWT токен обновления
   * @param rememberMe - Флаг сохранения в localStorage (true) или sessionStorage (false)
   */
  saveTokens(accessToken: string, refreshToken: string, rememberMe: boolean): void;

  /**
   * Получение токена доступа из хранилища
   * @returns JWT токен доступа или null, если не найден
   */
  getAccessToken(): string | null;

  /**
   * Получение токена обновления из хранилища
   * @returns JWT токен обновления или null, если не найден
   */
  getRefreshToken(): string | null;

  /**
   * Очистка всех токенов из хранилища
   * Удаляет токены как из localStorage, так и из sessionStorage
   */
  clearTokens(): void;

  /**
   * Проверка, был ли установлен флаг "Запомнить меня"
   * @returns true, если токены сохранены в localStorage
   */
  isRememberMe(): boolean;
}
