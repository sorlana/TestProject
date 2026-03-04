import { AxiosError } from 'axios';
import type { ApiError } from '../../domain/types/api.types';
import { showToast } from './toast';

/**
 * Типы ошибок для обработки
 */
export const ErrorType = {
  VALIDATION: 'validation',
  AUTHENTICATION: 'authentication',
  AUTHORIZATION: 'authorization',
  NOT_FOUND: 'not_found',
  SERVER_ERROR: 'server_error',
  NETWORK_ERROR: 'network_error',
  UNKNOWN: 'unknown'
} as const;

export type ErrorType = typeof ErrorType[keyof typeof ErrorType];

/**
 * Структура обработанной ошибки
 */
export interface ProcessedError {
  type: ErrorType;
  message: string;
  fieldErrors?: Record<string, string[]>;
  statusCode?: number;
}

/**
 * Обработчик ошибок API
 * Анализирует ошибку и возвращает структурированную информацию для отображения
 */
export class ErrorHandler {
  /**
   * Обрабатывает ошибку и возвращает структурированную информацию
   * @param error - Ошибка от Axios или другого источника
   * @returns Обработанная ошибка с типом и сообщением
   */
  static processError(error: unknown): ProcessedError {
    // Если это AxiosError
    if (error instanceof AxiosError) {
      return this.processAxiosError(error);
    }

    // Если это обычная ошибка JavaScript
    if (error instanceof Error) {
      return {
        type: ErrorType.UNKNOWN,
        message: error.message || 'Произошла неизвестная ошибка'
      };
    }

    // Если это строка
    if (typeof error === 'string') {
      return {
        type: ErrorType.UNKNOWN,
        message: error
      };
    }

    // Неизвестный тип ошибки
    return {
      type: ErrorType.UNKNOWN,
      message: 'Произошла неизвестная ошибка'
    };
  }

  /**
   * Обрабатывает AxiosError
   * @param error - Ошибка от Axios
   * @returns Обработанная ошибка
   */
  private static processAxiosError(error: AxiosError): ProcessedError {
    const statusCode = error.response?.status;
    const responseData = error.response?.data as ApiError | undefined;

    // Определяем тип ошибки по статус коду
    switch (statusCode) {
      case 400:
        return {
          type: ErrorType.VALIDATION,
          message: responseData?.message || 'Ошибка валидации данных',
          fieldErrors: responseData?.errors,
          statusCode
        };

      case 401:
        return {
          type: ErrorType.AUTHENTICATION,
          message: responseData?.message || 'Ошибка аутентификации',
          statusCode
        };

      case 403:
        return {
          type: ErrorType.AUTHORIZATION,
          message: responseData?.message || 'Недостаточно прав для выполнения операции',
          statusCode
        };

      case 404:
        return {
          type: ErrorType.NOT_FOUND,
          message: responseData?.message || 'Запрашиваемый ресурс не найден',
          statusCode
        };

      case 500:
      case 502:
      case 503:
      case 504:
        return {
          type: ErrorType.SERVER_ERROR,
          message: 'Внутренняя ошибка сервера. Попробуйте позже',
          statusCode
        };

      default:
        // Если нет ответа от сервера (сетевая ошибка)
        if (!error.response) {
          return {
            type: ErrorType.NETWORK_ERROR,
            message: 'Сервис временно недоступен. Проверьте подключение к интернету'
          };
        }

        return {
          type: ErrorType.UNKNOWN,
          message: responseData?.message || 'Произошла неизвестная ошибка',
          statusCode
        };
    }
  }

  /**
   * Отображает ошибку пользователю через Toast
   * @param error - Обработанная ошибка
   */
  static showError(error: ProcessedError): void {
    switch (error.type) {
      case ErrorType.VALIDATION:
        // Для ошибок валидации показываем основное сообщение
        // Детальные ошибки полей должны обрабатываться в компонентах форм
        showToast({
          message: error.message,
          intent: 'warning',
          timeout: 5000
        });
        break;

      case ErrorType.AUTHENTICATION:
        showToast({
          message: 'Сессия истекла. Необходимо войти в систему заново',
          intent: 'danger',
          timeout: 5000
        });
        break;

      case ErrorType.AUTHORIZATION:
        showToast({
          message: error.message,
          intent: 'danger',
          timeout: 5000
        });
        break;

      case ErrorType.NOT_FOUND:
        showToast({
          message: error.message,
          intent: 'warning',
          timeout: 5000
        });
        break;

      case ErrorType.SERVER_ERROR:
        showToast({
          message: error.message,
          intent: 'danger',
          timeout: 7000
        });
        break;

      case ErrorType.NETWORK_ERROR:
        showToast({
          message: error.message,
          intent: 'danger',
          timeout: 7000,
          action: {
            text: 'Повторить',
            onClick: () => window.location.reload()
          }
        });
        break;

      default:
        showToast({
          message: error.message,
          intent: 'danger',
          timeout: 5000
        });
        break;
    }
  }

  /**
   * Обрабатывает и отображает ошибку одним вызовом
   * @param error - Исходная ошибка
   * @returns Обработанная ошибка для дополнительной обработки
   */
  static handleAndShowError(error: unknown): ProcessedError {
    const processedError = this.processError(error);
    this.showError(processedError);
    return processedError;
  }
}