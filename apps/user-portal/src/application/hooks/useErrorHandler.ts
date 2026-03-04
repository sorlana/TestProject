import { useCallback } from 'react';
import { ErrorHandler, ErrorType, type ProcessedError } from '../../infrastructure/utils/errorHandler';
import { ValidationErrorExtractor, type FieldValidationErrors } from '../../infrastructure/utils/validationErrorExtractor';

/**
 * Результат обработки ошибки
 */
export interface ErrorHandlingResult {
  /** Обработанная ошибка */
  processedError: ProcessedError;
  /** Ошибки валидации полей (если есть) */
  fieldErrors: FieldValidationErrors;
  /** Основное сообщение об ошибке */
  message: string;
  /** Тип ошибки */
  type: ErrorType;
}

/**
 * Хук для обработки ошибок в компонентах
 * Предоставляет удобные методы для обработки различных типов ошибок
 */
export const useErrorHandler = () => {
  /**
   * Обрабатывает ошибку и возвращает структурированную информацию
   * @param error - Исходная ошибка
   * @param showToast - Показывать ли Toast уведомление (по умолчанию true)
   * @returns Результат обработки ошибки
   */
  const handleError = useCallback((error: unknown, showToast = true): ErrorHandlingResult => {
    const processedError = ErrorHandler.processError(error);
    const fieldErrors = ValidationErrorExtractor.extractFieldErrors(processedError);

    // Показываем Toast если требуется и это не ошибка валидации
    if (showToast && processedError.type !== ErrorType.VALIDATION) {
      ErrorHandler.showError(processedError);
    }

    return {
      processedError,
      fieldErrors,
      message: processedError.message,
      type: processedError.type
    };
  }, []);

  /**
   * Обрабатывает ошибку валидации формы
   * @param error - Исходная ошибка
   * @returns Ошибки валидации полей
   */
  const handleValidationError = useCallback((error: unknown): FieldValidationErrors => {
    const processedError = ErrorHandler.processError(error);
    return ValidationErrorExtractor.extractFieldErrors(processedError);
  }, []);

  /**
   * Получает ошибку для конкретного поля
   * @param fieldErrors - Ошибки валидации полей
   * @param fieldName - Имя поля
   * @returns Первая ошибка для поля или undefined
   */
  const getFieldError = useCallback((fieldErrors: FieldValidationErrors, fieldName: string): string | undefined => {
    return ValidationErrorExtractor.getFirstFieldError(fieldErrors, fieldName);
  }, []);

  /**
   * Проверяет, есть ли ошибка для поля
   * @param fieldErrors - Ошибки валидации полей
   * @param fieldName - Имя поля
   * @returns true, если есть ошибка для поля
   */
  const hasFieldError = useCallback((fieldErrors: FieldValidationErrors, fieldName: string): boolean => {
    return ValidationErrorExtractor.hasFieldError(fieldErrors, fieldName);
  }, []);

  /**
   * Обрабатывает сетевую ошибку
   * @param error - Исходная ошибка
   * @param customMessage - Кастомное сообщение (опционально)
   */
  const handleNetworkError = useCallback((error: unknown, customMessage?: string) => {
    const processedError = ErrorHandler.processError(error);
    
    if (processedError.type === ErrorType.NETWORK_ERROR) {
      ErrorHandler.showError({
        ...processedError,
        message: customMessage || processedError.message
      });
    } else {
      ErrorHandler.showError(processedError);
    }
  }, []);

  /**
   * Обрабатывает ошибку аутентификации
   * @param error - Исходная ошибка
   */
  const handleAuthError = useCallback((error: unknown) => {
    const processedError = ErrorHandler.processError(error);
    
    if (processedError.type === ErrorType.AUTHENTICATION) {
      // Для ошибок аутентификации показываем специальное сообщение
      ErrorHandler.showError({
        ...processedError,
        message: 'Сессия истекла. Необходимо войти в систему заново'
      });
    } else {
      ErrorHandler.showError(processedError);
    }
  }, []);

  /**
   * Показывает успешное уведомление
   * @param message - Сообщение об успехе
   */
  const showSuccess = useCallback((message: string) => {
    // Импортируем toast динамически, чтобы избежать циклических зависимостей
    import('../../infrastructure/utils/toast').then(({ toast }) => {
      toast.success(message);
    });
  }, []);

  /**
   * Показывает информационное уведомление
   * @param message - Информационное сообщение
   */
  const showInfo = useCallback((message: string) => {
    import('../../infrastructure/utils/toast').then(({ toast }) => {
      toast.info(message);
    });
  }, []);

  /**
   * Показывает предупреждение
   * @param message - Сообщение предупреждения
   */
  const showWarning = useCallback((message: string) => {
    import('../../infrastructure/utils/toast').then(({ toast }) => {
      toast.warning(message);
    });
  }, []);

  return {
    handleError,
    handleValidationError,
    getFieldError,
    hasFieldError,
    handleNetworkError,
    handleAuthError,
    showSuccess,
    showInfo,
    showWarning
  };
};