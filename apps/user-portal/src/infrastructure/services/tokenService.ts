import type { ITokenService } from '../../domain/interfaces/ITokenService';

/**
 * Реализация сервиса управления JWT токенами
 * Обеспечивает сохранение токенов в localStorage или sessionStorage
 * в зависимости от флага "Запомнить меня"
 */
class TokenService implements ITokenService {
  private readonly ACCESS_TOKEN_KEY = 'access_token';
  private readonly REFRESH_TOKEN_KEY = 'refresh_token';
  private readonly REMEMBER_ME_KEY = 'remember_me';

  /**
   * Сохранение JWT токенов в хранилище
   * @param accessToken - JWT токен доступа
   * @param refreshToken - JWT токен обновления
   * @param rememberMe - Флаг сохранения в localStorage (true) или sessionStorage (false)
   */
  saveTokens(accessToken: string, refreshToken: string, rememberMe: boolean): void {
    const storage = rememberMe ? localStorage : sessionStorage;

    storage.setItem(this.ACCESS_TOKEN_KEY, accessToken);
    storage.setItem(this.REFRESH_TOKEN_KEY, refreshToken);
    storage.setItem(this.REMEMBER_ME_KEY, rememberMe.toString());
  }

  /**
   * Получение токена доступа из хранилища
   * @returns JWT токен доступа или null, если не найден
   */
  getAccessToken(): string | null {
    return this.getFromStorage(this.ACCESS_TOKEN_KEY);
  }

  /**
   * Получение токена обновления из хранилища
   * @returns JWT токен обновления или null, если не найден
   */
  getRefreshToken(): string | null {
    return this.getFromStorage(this.REFRESH_TOKEN_KEY);
  }

  /**
   * Очистка всех токенов из хранилища
   * Удаляет токены как из localStorage, так и из sessionStorage
   */
  clearTokens(): void {
    // Очищаем из localStorage
    localStorage.removeItem(this.ACCESS_TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    localStorage.removeItem(this.REMEMBER_ME_KEY);

    // Очищаем из sessionStorage
    sessionStorage.removeItem(this.ACCESS_TOKEN_KEY);
    sessionStorage.removeItem(this.REFRESH_TOKEN_KEY);
    sessionStorage.removeItem(this.REMEMBER_ME_KEY);
  }

  /**
   * Проверка, был ли установлен флаг "Запомнить меня"
   * @returns true, если токены сохранены в localStorage
   */
  isRememberMe(): boolean {
    const rememberMe = this.getFromStorage(this.REMEMBER_ME_KEY);
    return rememberMe === 'true';
  }

  /**
   * Получение значения из хранилища (проверяет и localStorage, и sessionStorage)
   * @param key - Ключ для поиска
   * @returns Значение или null, если не найдено
   */
  private getFromStorage(key: string): string | null {
    return localStorage.getItem(key) || sessionStorage.getItem(key);
  }
}

// Экспортируем singleton instance
export const tokenService = new TokenService();
