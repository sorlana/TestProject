/**
 * Типы для работы с API
 */

/**
 * Структура ошибки API
 */
export interface ApiError {
  /** Основное сообщение об ошибке */
  message: string;
  /** Детализированные ошибки валидации по полям */
  errors?: Record<string, string[]>;
  /** HTTP статус код ошибки */
  statusCode: number;
}
