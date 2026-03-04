/**
 * Custom React hook для управления аутентификацией
 */

import { useCallback } from 'react';
import { useAuthStore } from '../store/authStore';
import type { AuthState } from '../store/authStore';
import { authService } from '../../infrastructure/services/authService';
import { useErrorHandler } from './useErrorHandler';
import type { LoginCredentials, RegisterData } from '../../domain/types/auth.types';
import type { FieldValidationErrors } from '../../infrastructure/utils/validationErrorExtractor';

/**
 * Интерфейс возвращаемого значения useAuth hook
 */
/**
 * Интерфейс возвращаемого значения useAuth hook
 */
interface UseAuthReturn {
  /** Флаг авторизации пользователя */
  isAuthenticated: boolean;
  /** Данные текущего пользователя */
  user: AuthState['user'];
  /** Флаг состояния загрузки */
  isLoading: boolean;
  /** Сообщение об ошибке */
  error: string | null;

  /** Функция входа в систему */
  login: (credentials: LoginCredentials) => Promise<FieldValidationErrors>;
  /** Функция регистрации */
  register: (data: RegisterData) => Promise<FieldValidationErrors>;
  /** Функция входа через Google */
  loginWithGoogle: (idToken: string) => Promise<void>;
  /** Функция входа через Yandex */
  loginWithYandex: (code: string) => Promise<void>;
  /** Функция выхода из системы */
  logout: () => Promise<void>;
  /** Функция очистки ошибок */
  clearError: () => void;

  /** Функция отправки кода подтверждения на email */
  sendEmailVerification: (email: string) => Promise<void>;
  /** Функция проверки кода подтверждения email */
  verifyEmailCode: (email: string, code: string) => Promise<void>;
  /** Функция регистрации с подтверждением email */
  registerWithEmailVerification: (data: RegisterData, verificationCode: string) => Promise<FieldValidationErrors>;
}


/**
 * Custom hook для управления аутентификацией
 * 
 * Предоставляет удобный интерфейс для работы с аутентификацией,
 * объединяя состояние из authStore и методы из authService.
 * Обрабатывает состояния загрузки и ошибок автоматически.
 * 
 * @returns Объект с состоянием аутентификации и методами для работы с ней
 */
export const useAuth = (): UseAuthReturn => {
  // Получаем состояние из Zustand store
  const {
    isAuthenticated,
    user,
    isLoading,
    error,
    setError
  } = useAuthStore();

  // Используем хук для обработки ошибок
  const { handleValidationError, handleError } = useErrorHandler();

  /**
   * Вход в систему по логину и паролю
   * @param credentials - Учетные данные
   * @returns Promise с ошибками валидации полей (если есть)
   */
  const login = useCallback(async (credentials: LoginCredentials): Promise<FieldValidationErrors> => {
    try {
      await authService.login(credentials);
      return {}; // Нет ошибок валидации
    } catch (error) {
      // Обрабатываем ошибку и возвращаем ошибки валидации для формы
      return handleValidationError(error);
    }
  }, [handleValidationError]);

  /**
   * Регистрация нового пользователя
   * @param data - Данные для регистрации
   * @returns Promise с ошибками валидации полей (если есть)
   */
  const register = useCallback(async (data: RegisterData): Promise<FieldValidationErrors> => {
    try {
      await authService.register(data);
      return {}; // Нет ошибок валидации
    } catch (error) {
      // Обрабатываем ошибку и возвращаем ошибки валидации для формы
      return handleValidationError(error);
    }
  }, [handleValidationError]);

  /**
   * Вход через Google OAuth
   * @param idToken - Google ID токен
   */
  const loginWithGoogle = useCallback(async (idToken: string): Promise<void> => {
    try {
      await authService.loginWithGoogle(idToken);
    } catch (error) {
      // Для Google OAuth показываем все ошибки через Toast
      handleError(error, true);
      throw error;
    }
  }, [handleError]);

  /**
   * Вход через Yandex OAuth
   * @param code - Yandex authorization code
   */
  const loginWithYandex = useCallback(async (code: string): Promise<void> => {
    try {
      await authService.loginWithYandex(code);
    } catch (error) {
      // Для Yandex OAuth показываем все ошибки через Toast
      handleError(error, true);
      throw error;
    }
  }, [handleError]);

  /**
   * Выход из системы
   */
  const logout = useCallback(async (): Promise<void> => {
    try {
      await authService.logout();
    } catch (error) {
      // Ошибка уже обработана в authService
      console.warn('Ошибка при выходе из системы:', error);
    }
  }, []);

  /**
   * Очистка ошибок
   */
  const clearError = useCallback((): void => {
    setError(null);
  }, [setError]);

  /**
   * Отправка кода подтверждения на email
   * @param email - Email адрес для отправки кода
   */
  const sendEmailVerification = useCallback(async (email: string): Promise<void> => {
    try {
      await authService.sendEmailVerification(email);
    } catch (error) {
      // Ошибка уже обработана в authService (показан Toast)
      handleError(error, false);
      throw error;
    }
  }, [handleError]);

  /**
   * Проверка кода подтверждения email
   * @param email - Email адрес
   * @param code - Код подтверждения
   */
  const verifyEmailCode = useCallback(async (email: string, code: string): Promise<void> => {
    try {
      await authService.verifyEmailCode(email, code);
    } catch (error) {
      // Ошибка уже обработана в authService (показан Toast)
      handleError(error, false);
      throw error;
    }
  }, [handleError]);

  /**
   * Регистрация с подтверждением email
   * @param data - Данные для регистрации
   * @param verificationCode - Код подтверждения
   * @returns Promise с ошибками валидации полей (если есть)
   */
  const registerWithEmailVerification = useCallback(async (
    data: RegisterData,
    verificationCode: string
  ): Promise<FieldValidationErrors> => {
    try {
      await authService.registerWithEmailVerification(data, verificationCode);
      return {}; // Нет ошибок валидации
    } catch (error) {
      // Обрабатываем ошибку и возвращаем ошибки валидации для формы
      return handleValidationError(error);
    }
  }, [handleValidationError]);

  return {
    // Состояние
    isAuthenticated,
    user,
    isLoading,
    error,
    
    // Методы
    login,
    register,
    loginWithGoogle,
    loginWithYandex,
    logout,
    clearError,
    sendEmailVerification,
    verifyEmailCode,
    registerWithEmailVerification
  };
};