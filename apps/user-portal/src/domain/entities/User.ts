/**
 * Доменная сущность пользователя
 */

/**
 * Интерфейс пользователя системы
 */
export interface User {
  /** Уникальный идентификатор пользователя */
  id: string;
  /** Логин пользователя */
  userName: string;
  /** Email адрес пользователя */
  email: string;
  /** Номер телефона пользователя */
  phoneNumber: string;
  /** Имя пользователя */
  firstName?: string;
  /** Фамилия пользователя */
  lastName?: string;
  /** Отчество пользователя */
  middleName?: string;
  /** Подтвержден ли email адрес */
  emailConfirmed: boolean;
  /** Подтвержден ли номер телефона */
  phoneNumberConfirmed: boolean;
}
