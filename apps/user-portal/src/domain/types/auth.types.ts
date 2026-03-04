/**
 * Типы для аутентификации и авторизации
 */

import type { User } from '../entities/User';

/**
 * Данные для входа в систему
 */
export interface LoginCredentials {
  /** Логин или email пользователя */
  username: string;
  /** Пароль пользователя */
  password: string;
  /** Флаг "Запомнить меня" для сохранения токенов в localStorage */
  rememberMe: boolean;
}

/**
 * Данные для регистрации нового пользователя
 */
export interface RegisterData {
  /** Уникальный логин пользователя */
  username: string;
  /** Email адрес пользователя (опционально, зависит от способа подтверждения) */
  email?: string;
  /** Номер телефона пользователя (опционально, зависит от способа подтверждения) */
  phone?: string;
  /** Пароль пользователя */
  password: string;
  /** Подтверждение пароля */
  confirmPassword: string;
  /** Имя пользователя (опционально) */
  firstName?: string;
  /** Фамилия пользователя (опционально) */
  lastName?: string;
  /** Отчество пользователя (опционально) */
  middleName?: string;
  /** Способ подтверждения регистрации */
  verificationMethod: 'email' | 'phone';
}

/**
 * Ответ от Auth Service API при успешной аутентификации
 */
export interface AuthResponse {
  /** Флаг успешности операции */
  success: boolean;
  /** JWT токен доступа */
  accessToken: string;
  /** JWT токен обновления */
  refreshToken: string;
  /** Время истечения токена доступа */
  expiresAt: string;
  /** Данные пользователя */
  user: User;
  /** Массив ошибок (если есть) */
  errors?: string[];
  /** Требуется ли подтверждение телефона */
  requiresPhoneVerification?: boolean;
}

/**
 * Данные для Google OAuth аутентификации
 */
export interface GoogleAuthRequest {
  /** Google ID токен, полученный от Google Identity Services */
  idToken: string;
}

/**
 * Запрос на обновление JWT токена
 */
export interface RefreshTokenRequest {
  /** Refresh токен для получения нового access токена */
  refreshToken: string;
}
/**
 * Запрос на отправку кода подтверждения email
 */
export interface EmailVerificationRequest {
  /** Email адрес для отправки кода подтверждения */
  email: string;
}

/**
 * Запрос на проверку кода подтверждения email
 */
export interface EmailVerificationCodeRequest {
  /** Email адрес пользователя */
  email: string;
  /** Код подтверждения, полученный по email */
  code: string;
}

/**
 * Запрос на регистрацию с подтверждением email
 */
export interface RegisterWithEmailRequest extends RegisterData {
  /** Код подтверждения email */
  verificationCode: string;
}
