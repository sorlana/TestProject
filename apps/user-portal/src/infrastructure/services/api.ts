import axios, { AxiosError, type AxiosRequestConfig } from 'axios';
import { tokenService } from './tokenService';
import { ErrorHandler, ErrorType } from '../utils/errorHandler';
import { useAuthStore } from '../../application/store/authStore';

/**
 * Настроенный Axios instance для работы с API
 * Включает interceptors для автоматического добавления токенов и обработки ошибок
 */
const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || '/api',
  timeout: 30000, // 30 секунд
  headers: {
    'Content-Type': 'application/json',
  },
});

/**
 * Request interceptor - автоматическое добавление JWT токена в заголовки
 */
api.interceptors.request.use(
  (config) => {
    const token = tokenService.getAccessToken();

    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

/**
 * Response interceptor - обработка 401 ошибок и автоматическое обновление токена
 */
let isRefreshing = false;
let failedQueue: Array<{
  resolve: (value?: unknown) => void;
  reject: (reason?: unknown) => void;
}> = [];

/**
 * Обработка очереди запросов после обновления токена
 * @param error - Ошибка, если обновление не удалось
 * @param token - Новый токен, если обновление успешно
 */
const processQueue = (error: Error | null, token: string | null = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve(token);
    }
  });

  failedQueue = [];
};

api.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as AxiosRequestConfig & { _retry?: boolean };

    // Если получили 401 ошибку и это не повторный запрос
    if (error.response?.status === 401 && !originalRequest._retry) {
      if (isRefreshing) {
        // Если уже идет процесс обновления токена, добавляем запрос в очередь
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        })
          .then(() => {
            return api(originalRequest);
          })
          .catch((err) => {
            return Promise.reject(err);
          });
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
        // Пытаемся обновить токен
        const refreshToken = tokenService.getRefreshToken();

        if (!refreshToken) {
          throw new Error('Отсутствует refresh токен');
        }

        // Отправляем запрос на обновление токена
        const response = await axios.post(`${import.meta.env.VITE_API_BASE_URL || '/api'}/auth/refresh`, {
          refreshToken,
        });

        // Сохраняем новые токены
        const { accessToken, refreshToken: newRefreshToken } = response.data;
        tokenService.saveTokens(accessToken, newRefreshToken, tokenService.isRememberMe());

        // Обрабатываем очередь запросов
        processQueue(null, accessToken);

        // Повторяем исходный запрос с новым токеном
        return api(originalRequest);
      } catch (refreshError) {
        // Если обновление токена не удалось
        processQueue(refreshError as Error);

        // Обрабатываем ошибку через ErrorHandler
        const processedError = ErrorHandler.processError(refreshError);
        
        // Очищаем токены и состояние
        tokenService.clearTokens();
        useAuthStore.getState().setAuthenticated(false);
        useAuthStore.getState().setUser(null);

        // Показываем уведомление о необходимости повторного входа
        if (processedError.type === ErrorType.AUTHENTICATION) {
          ErrorHandler.showError({
            type: ErrorType.AUTHENTICATION,
            message: 'Сессия истекла. Необходимо войти в систему заново'
          });
        }

        // Перенаправляем на страницу входа только если мы не на ней
        if (window.location.pathname !== '/app/login') {
          window.location.href = '/app/login';
        }

        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    }

    return Promise.reject(error);
  }
);

export default api;
