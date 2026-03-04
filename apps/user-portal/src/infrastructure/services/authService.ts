import type { IAuthService } from '../../domain/interfaces/IAuthService';
import type { LoginCredentials, RegisterData, AuthResponse } from '../../domain/types/auth.types';
import { tokenService } from './tokenService';
import { useAuthStore } from '../../application/store/authStore';
import { ErrorHandler, ErrorType } from '../utils/errorHandler';
import { toast } from '../utils/toast';
import api from './api';

/**
 * Реализация сервиса аутентификации
 * Обеспечивает взаимодействие с Auth Service API и управление состоянием аутентификации
 */
class AuthService implements IAuthService {
  /**
   * Вход в систему по логину и паролю
   * @param credentials - Учетные данные пользователя
   * @returns Promise с ответом от Auth Service
   */
  async login(credentials: LoginCredentials): Promise<AuthResponse> {
    try {
      useAuthStore.getState().setLoading(true);
      useAuthStore.getState().setError(null);

      const response = await api.post<AuthResponse>('/auth/login', {
        userName: credentials.username,
        password: credentials.password,
        rememberMe: credentials.rememberMe,
      });

      const authResponse = response.data;

      // Сохраняем токены
      tokenService.saveTokens(
        authResponse.accessToken,
        authResponse.refreshToken,
        credentials.rememberMe
      );

      // Обновляем состояние аутентификации
      useAuthStore.getState().setUser(authResponse.user);
      useAuthStore.getState().setAuthenticated(true);

      // Показываем уведомление об успешном входе
      toast.success('Добро пожаловать!');

      return authResponse;
    } catch (error: unknown) {
      // Обрабатываем ошибку через ErrorHandler
      const processedError = ErrorHandler.processError(error);
      
      // Устанавливаем ошибку в store для отображения в форме
      useAuthStore.getState().setError(processedError.message);

      // Для ошибок валидации не показываем Toast (ошибки отображаются в форме)
      if (processedError.type !== ErrorType.VALIDATION) {
        ErrorHandler.showError(processedError);
      }

      throw error;
    } finally {
      useAuthStore.getState().setLoading(false);
    }
  }

  /**
   * Регистрация нового пользователя
   * @param data - Данные для регистрации
   * @returns Promise с ответом от Auth Service
   */
  async register(data: RegisterData): Promise<AuthResponse> {
    try {
      useAuthStore.getState().setLoading(true);
      useAuthStore.getState().setError(null);

      const response = await api.post<AuthResponse>('/auth/register', {
        userName: data.username,
        email: data.email,
        phoneNumber: data.phone,
        password: data.password,
        confirmPassword: data.confirmPassword,
        firstName: data.firstName,
        lastName: data.lastName,
        middleName: data.middleName,
      });

      const authResponse = response.data;

      // Сохраняем токены (при регистрации не используем rememberMe)
      tokenService.saveTokens(authResponse.accessToken, authResponse.refreshToken, false);

      // Обновляем состояние аутентификации
      useAuthStore.getState().setUser(authResponse.user);
      useAuthStore.getState().setAuthenticated(true);

      // Показываем уведомление об успешной регистрации
      toast.success('Регистрация прошла успешно! Добро пожаловать!');

      return authResponse;
    } catch (error: unknown) {
      // Обрабатываем ошибку через ErrorHandler
      const processedError = ErrorHandler.processError(error);
      
      // Устанавливаем ошибку в store для отображения в форме
      useAuthStore.getState().setError(processedError.message);

      // Для ошибок валидации не показываем Toast (ошибки отображаются в форме)
      if (processedError.type !== ErrorType.VALIDATION) {
        ErrorHandler.showError(processedError);
      }

      throw error;
    } finally {
      useAuthStore.getState().setLoading(false);
    }
  }

  /**
   * Вход через Google OAuth
   * @param idToken - Google ID токен
   * @returns Promise с ответом от Auth Service
   */
  async loginWithGoogle(idToken: string): Promise<AuthResponse> {
    try {
      useAuthStore.getState().setLoading(true);
      useAuthStore.getState().setError(null);

      const response = await api.post<AuthResponse>('/auth/google', {
        idToken,
      });

      const authResponse = response.data;

      // Сохраняем токены (при Google OAuth не используем rememberMe)
      tokenService.saveTokens(authResponse.accessToken, authResponse.refreshToken, false);

      // Обновляем состояние аутентификации
      useAuthStore.getState().setUser(authResponse.user);
      useAuthStore.getState().setAuthenticated(true);

      // Показываем уведомление об успешном входе
      toast.success('Добро пожаловать!');

      return authResponse;
    } catch (error: unknown) {
      // Обрабатываем ошибку через ErrorHandler
      const processedError = ErrorHandler.processError(error);
      
      // Устанавливаем ошибку в store
      useAuthStore.getState().setError(processedError.message);

      // Показываем Toast для всех типов ошибок Google OAuth
      ErrorHandler.showError(processedError);

      throw error;
    } finally {
      useAuthStore.getState().setLoading(false);
    }
  }

  /**
   * Авторизация через Yandex OAuth
   * @param code - Yandex authorization code
   * @returns Promise с ответом от Auth Service
   */
  async loginWithYandex(code: string): Promise<AuthResponse> {
    try {
      useAuthStore.getState().setLoading(true);
      useAuthStore.getState().setError(null);

      const response = await api.post<AuthResponse>('/auth/yandex', {
        code,
      });

      const authResponse = response.data;

      // Сохраняем токены (при Yandex OAuth не используем rememberMe)
      tokenService.saveTokens(authResponse.accessToken, authResponse.refreshToken, false);

      // Обновляем состояние аутентификации
      useAuthStore.getState().setUser(authResponse.user);
      useAuthStore.getState().setAuthenticated(true);

      // Показываем уведомление об успешном входе
      toast.success('Добро пожаловать через Yandex!');

      return authResponse;
    } catch (error: unknown) {
      // Обрабатываем ошибку через ErrorHandler
      const processedError = ErrorHandler.processError(error);
      
      // Устанавливаем ошибку в store
      useAuthStore.getState().setError(processedError.message);

      // Показываем Toast для всех типов ошибок Yandex OAuth
      ErrorHandler.showError(processedError);

      throw error;
    } finally {
      useAuthStore.getState().setLoading(false);
    }
  }

  /**
   * Обновление JWT токена с помощью refresh токена
   * @returns Promise с новыми токенами
   */
  async refreshToken(): Promise<AuthResponse> {
    const refreshToken = tokenService.getRefreshToken();

    if (!refreshToken) {
      throw new Error('Отсутствует refresh токен');
    }

    try {
      const response = await api.post<AuthResponse>('/auth/refresh', {
        refreshToken,
      });

      const authResponse = response.data;

      // Сохраняем новые токены с тем же флагом rememberMe
      tokenService.saveTokens(
        authResponse.accessToken,
        authResponse.refreshToken,
        tokenService.isRememberMe()
      );

      return authResponse;
    } catch (error: unknown) {
      // Обрабатываем ошибку обновления токена
      const processedError = ErrorHandler.processError(error);
      
      // Для ошибок обновления токена очищаем токены и не показываем Toast
      // (это обрабатывается в interceptor)
      if (processedError.type === ErrorType.AUTHENTICATION) {
        tokenService.clearTokens();
        useAuthStore.getState().setAuthenticated(false);
        useAuthStore.getState().setUser(null);
      }

      throw error;
    }
  }

  /**
   * Выход из системы
   * Отправляет запрос на сервер и очищает локальные токены
   * @returns Promise без возвращаемого значения
   */
  async logout(): Promise<void> {
    try {
      // Отправляем запрос на сервер для завершения сессии
      await api.post('/auth/logout');
      
      // Показываем уведомление об успешном выходе
      toast.info('Вы успешно вышли из системы');
    } catch (error) {
      // Обрабатываем ошибки при выходе
      const processedError = ErrorHandler.processError(error);
      
      // Логируем ошибку, но не показываем пользователю (выход всё равно произойдет)
      console.warn('Ошибка при выходе из системы:', processedError.message);
    } finally {
      // Очищаем токены из хранилища
      tokenService.clearTokens();

      // Сбрасываем состояние аутентификации
      useAuthStore.getState().setAuthenticated(false);
      useAuthStore.getState().setUser(null);
      useAuthStore.getState().setError(null);
    }
  }

  /**
   * Отправка кода подтверждения на email
   * @param email - Email адрес для отправки кода
   * @returns Promise без возвращаемого значения
   */
  async sendEmailVerification(email: string): Promise<void> {
    try {
      useAuthStore.getState().setLoading(true);
      useAuthStore.getState().setError(null);

      await api.post('/auth/send-email-verification', {
        email,
      });

      // Показываем уведомление об успешной отправке
      toast.success('Код подтверждения отправлен на ваш email');
    } catch (error: unknown) {
      // Обрабатываем ошибку через ErrorHandler
      const processedError = ErrorHandler.processError(error);

      // Устанавливаем ошибку в store
      useAuthStore.getState().setError(processedError.message);

      // Показываем Toast с ошибкой
      ErrorHandler.showError(processedError);

      throw error;
    } finally {
      useAuthStore.getState().setLoading(false);
    }
  }

  /**
   * Проверка кода подтверждения email
   * @param email - Email адрес пользователя
   * @param code - Код подтверждения
   * @returns Promise без возвращаемого значения
   */
  async verifyEmailCode(email: string, code: string): Promise<void> {
    try {
      useAuthStore.getState().setLoading(true);
      useAuthStore.getState().setError(null);

      await api.post('/auth/verify-email-code', {
        email,
        code,
      });

      // Показываем уведомление об успешной проверке
      toast.success('Код подтверждения верный');
    } catch (error: unknown) {
      // Обрабатываем ошибку через ErrorHandler
      const processedError = ErrorHandler.processError(error);

      // Устанавливаем ошибку в store
      useAuthStore.getState().setError(processedError.message);

      // Показываем Toast с ошибкой
      ErrorHandler.showError(processedError);

      throw error;
    } finally {
      useAuthStore.getState().setLoading(false);
    }
  }

  /**
   * Регистрация нового пользователя с подтверждением email
   * @param data - Данные для регистрации
   * @param verificationCode - Код подтверждения email
   * @returns Promise с ответом от Auth Service
   */
  async registerWithEmailVerification(
    data: RegisterData,
    verificationCode: string
  ): Promise<AuthResponse> {
    try {
      useAuthStore.getState().setLoading(true);
      useAuthStore.getState().setError(null);

      const response = await api.post<AuthResponse>('/auth/register-with-email', {
        userName: data.username,
        email: data.email,
        phoneNumber: data.phone,
        password: data.password,
        confirmPassword: data.confirmPassword,
        firstName: data.firstName,
        lastName: data.lastName,
        middleName: data.middleName,
        verificationCode,
      });

      const authResponse = response.data;

      // Сохраняем токены (при регистрации не используем rememberMe)
      tokenService.saveTokens(authResponse.accessToken, authResponse.refreshToken, false);

      // Обновляем состояние аутентификации
      useAuthStore.getState().setUser(authResponse.user);
      useAuthStore.getState().setAuthenticated(true);

      // Показываем уведомление об успешной регистрации
      toast.success('Регистрация прошла успешно! Добро пожаловать!');

      return authResponse;
    } catch (error: unknown) {
      // Обрабатываем ошибку через ErrorHandler
      const processedError = ErrorHandler.processError(error);

      // Устанавливаем ошибку в store для отображения в форме
      useAuthStore.getState().setError(processedError.message);

      // Для ошибок валидации не показываем Toast (ошибки отображаются в форме)
      if (processedError.type !== ErrorType.VALIDATION) {
        ErrorHandler.showError(processedError);
      }

      throw error;
    } finally {
      useAuthStore.getState().setLoading(false);
    }
  }



}

// Экспортируем singleton instance
export const authService = new AuthService();
