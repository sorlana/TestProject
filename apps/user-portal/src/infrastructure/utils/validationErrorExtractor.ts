import { ErrorType, type ProcessedError } from './errorHandler';

/**
 * Структура ошибок валидации для полей формы
 */
export interface FieldValidationErrors {
  [fieldName: string]: string[];
}

/**
 * Утилита для извлечения ошибок валидации из ответа API
 */
export class ValidationErrorExtractor {
  /**
   * Извлекает ошибки валидации из обработанной ошибки
   * @param processedError - Обработанная ошибка от ErrorHandler
   * @returns Объект с ошибками валидации по полям
   */
  static extractFieldErrors(processedError: ProcessedError): FieldValidationErrors {
    if (processedError.type !== ErrorType.VALIDATION || !processedError.fieldErrors) {
      return {};
    }

    return processedError.fieldErrors;
  }

  /**
   * Получает первую ошибку для конкретного поля
   * @param fieldErrors - Ошибки валидации полей
   * @param fieldName - Имя поля
   * @returns Первая ошибка для поля или undefined
   */
  static getFirstFieldError(fieldErrors: FieldValidationErrors, fieldName: string): string | undefined {
    const errors = fieldErrors[fieldName];
    return errors && errors.length > 0 ? errors[0] : undefined;
  }

  /**
   * Получает все ошибки для конкретного поля
   * @param fieldErrors - Ошибки валидации полей
   * @param fieldName - Имя поля
   * @returns Массив ошибок для поля
   */
  static getFieldErrors(fieldErrors: FieldValidationErrors, fieldName: string): string[] {
    return fieldErrors[fieldName] || [];
  }

  /**
   * Проверяет, есть ли ошибки для конкретного поля
   * @param fieldErrors - Ошибки валидации полей
   * @param fieldName - Имя поля
   * @returns true, если есть ошибки для поля
   */
  static hasFieldError(fieldErrors: FieldValidationErrors, fieldName: string): boolean {
    const errors = fieldErrors[fieldName];
    return errors && errors.length > 0;
  }

  /**
   * Получает общее количество ошибок валидации
   * @param fieldErrors - Ошибки валидации полей
   * @returns Общее количество ошибок
   */
  static getTotalErrorCount(fieldErrors: FieldValidationErrors): number {
    return Object.values(fieldErrors).reduce((total, errors) => total + errors.length, 0);
  }

  /**
   * Преобразует ошибки валидации в плоский массив строк
   * @param fieldErrors - Ошибки валидации полей
   * @returns Массив всех сообщений об ошибках
   */
  static flattenErrors(fieldErrors: FieldValidationErrors): string[] {
    const allErrors: string[] = [];
    
    Object.entries(fieldErrors).forEach(([fieldName, errors]) => {
      errors.forEach(error => {
        allErrors.push(`${fieldName}: ${error}`);
      });
    });

    return allErrors;
  }

  /**
   * Создает сводное сообщение об ошибках валидации
   * @param fieldErrors - Ошибки валидации полей
   * @param maxErrors - Максимальное количество ошибок для отображения
   * @returns Сводное сообщение
   */
  static createSummaryMessage(fieldErrors: FieldValidationErrors, maxErrors = 3): string {
    const allErrors = this.flattenErrors(fieldErrors);
    
    if (allErrors.length === 0) {
      return '';
    }

    if (allErrors.length <= maxErrors) {
      return allErrors.join('; ');
    }

    const displayedErrors = allErrors.slice(0, maxErrors);
    const remainingCount = allErrors.length - maxErrors;
    
    return `${displayedErrors.join('; ')} и еще ${remainingCount} ошибок`;
  }
}