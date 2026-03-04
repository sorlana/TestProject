/**
 * Утилиты для валидации пользовательского ввода
 */

/**
 * Результат валидации
 */
export interface ValidationResult {
  /** Флаг валидности */
  isValid: boolean;
  /** Сообщение об ошибке (если есть) */
  errorMessage?: string;
}

/**
 * Валидация email адреса
 * Проверяет соответствие стандартному формату email
 * @param email - Email адрес для проверки
 * @returns Результат валидации
 */
export function validateEmail(email: string): ValidationResult {
  if (!email || email.trim().length === 0) {
    return {
      isValid: false,
      errorMessage: 'Email адрес обязателен',
    };
  }

  // Регулярное выражение для проверки формата email
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

  if (!emailRegex.test(email.trim())) {
    return {
      isValid: false,
      errorMessage: 'Некорректный формат email адреса',
    };
  }

  return { isValid: true };
}

/**
 * Валидация пароля
 * Проверяет минимальную длину 8 символов
 * @param password - Пароль для проверки
 * @returns Результат валидации
 */
export function validatePassword(password: string): ValidationResult {
  if (!password || password.length === 0) {
    return {
      isValid: false,
      errorMessage: 'Пароль обязателен',
    };
  }

  if (password.length < 8) {
    return {
      isValid: false,
      errorMessage: 'Пароль должен содержать минимум 8 символов',
    };
  }

  return { isValid: true };
}

/**
 * Валидация совпадения паролей
 * Проверяет, что пароль и подтверждение пароля идентичны
 * @param password - Основной пароль
 * @param confirmPassword - Подтверждение пароля
 * @returns Результат валидации
 */
export function validatePasswordMatch(password: string, confirmPassword: string): ValidationResult {
  if (!confirmPassword || confirmPassword.length === 0) {
    return {
      isValid: false,
      errorMessage: 'Подтверждение пароля обязательно',
    };
  }

  if (password !== confirmPassword) {
    return {
      isValid: false,
      errorMessage: 'Пароли не совпадают',
    };
  }

  return { isValid: true };
}

/**
 * Валидация номера телефона
 * Проверяет соответствие базовому формату телефона
 * @param phone - Номер телефона для проверки
 * @returns Результат валидации
 */
export function validatePhone(phone: string): ValidationResult {
  if (!phone || phone.trim().length === 0) {
    return {
      isValid: false,
      errorMessage: 'Номер телефона обязателен',
    };
  }

  // Убираем все символы кроме цифр и знака +
  const cleanPhone = phone.replace(/[^\d+]/g, '');

  // Проверяем базовый формат: должен начинаться с + и содержать от 10 до 15 цифр
  const phoneRegex = /^\+\d{10,14}$/;

  if (!phoneRegex.test(cleanPhone)) {
    return {
      isValid: false,
      errorMessage: 'Некорректный формат номера телефона. Используйте формат +7XXXXXXXXXX',
    };
  }

  return { isValid: true };
}

/**
 * Валидация обязательного поля
 * Проверяет, что поле не пустое
 * @param value - Значение для проверки
 * @param fieldName - Название поля для сообщения об ошибке
 * @returns Результат валидации
 */
export function validateRequired(value: string, fieldName: string): ValidationResult {
  if (!value || value.trim().length === 0) {
    return {
      isValid: false,
      errorMessage: `${fieldName} обязательно для заполнения`,
    };
  }

  return { isValid: true };
}

/**
 * Валидация логина
 * Проверяет минимальную длину и допустимые символы
 * @param username - Логин для проверки
 * @returns Результат валидации
 */
export function validateUsername(username: string): ValidationResult {
  if (!username || username.trim().length === 0) {
    return {
      isValid: false,
      errorMessage: 'Логин обязателен',
    };
  }

  if (username.trim().length < 3) {
    return {
      isValid: false,
      errorMessage: 'Логин должен содержать минимум 3 символа',
    };
  }

  // Проверяем, что логин содержит только допустимые символы
  const usernameRegex = /^[a-zA-Z0-9_.-]+$/;

  if (!usernameRegex.test(username.trim())) {
    return {
      isValid: false,
      errorMessage: 'Логин может содержать только буквы, цифры, точки, дефисы и подчеркивания',
    };
  }

  return { isValid: true };
}
