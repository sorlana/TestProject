/**
 * Интерфейс сервиса аутентификации
 */

import type { LoginCredentials, RegisterData, AuthResponse } from '../types/auth.types';

/**
 * Интерфейс для сервиса аутентификации
 * Определяет контракт для работы с Auth Service API
 */
export interface IAuthService {
  /**
   * Вход в систему по логину и паролю
   * @param credentials - Учетные данные пользователя
   * @returns Promise с ответом от Auth Service
   */
  login(credentials: LoginCredentials): Promise<AuthResponse>;

  /**
   * Регистрация нового пользователя
   * @param data - Данные для регистрации
   * @returns Promise с ответом от Auth Service
   */
  register(data: RegisterData): Promise<AuthResponse>;

  /**
   * Вход через Google OAuth
   * @param idToken - Google ID токен
   * @returns Promise с ответом от Auth Service
   */
  loginWithGoogle(idToken: string): Promise<AuthResponse>;

  /**
   * Обновление JWT токена с помощью refresh токена
   * @returns Promise с новыми токенами
   */
  refreshToken(): Promise<AuthResponse>;

  /**
   * Выход из системы
   * Отправляет запрос на сервер и очищает локальные токены
   * @returns Promise без возвращаемого значения
   */
  logout(): Promise<void>;

  /**
   * Выход из системы
   * Отправляет запрос на сервер и очищает локальные токены
   * @returns Promise без возвращаемого значения
   */
  logout(): Promise<void>;

  /**
   * Отправка кода подтверждения на email
   * @param email - Email адрес для отправки кода
   * @returns Promise без возвращаемого значения
   */
  sendEmailVerification(email: string): Promise<void>;

  /**
   * Проверка кода подтверждения email
   * @param email - Email адрес пользователя
   * @param code - Код подтверждения
   * @returns Promise без возвращаемого значения
   */
  verifyEmailCode(email: string, code: string): Promise<void>;

  /**
   * Регистрация нового пользователя с подтверждением email
   * @param data - Данные для регистрации
   * @param verificationCode - Код подтверждения email
   * @returns Promise с ответом от Auth Service
   */
  registerWithEmailVerification(
    data: RegisterData,
    verificationCode: string
  ): Promise<AuthResponse>;

}
