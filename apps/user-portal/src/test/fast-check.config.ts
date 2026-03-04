// Конфигурация для fast-check property-based тестов
import * as fc from 'fast-check'

// Глобальные настройки для всех property тестов
export const defaultPropertyConfig = {
  numRuns: 100, // Минимум 100 итераций для каждого property теста
  timeout: 5000, // Таймаут для каждого теста
  verbose: true, // Подробный вывод при ошибках
}

// Кастомные генераторы для доменных объектов
export const generators = {
  // Генератор для валидных email адресов
  validEmail: () => fc.emailAddress(),
  
  // Генератор для невалидных email адресов
  invalidEmail: () => fc.oneof(
    fc.string().filter(s => !s.includes('@')),
    fc.string({ maxLength: 2 }),
    fc.constant(''),
    fc.constant('@'),
    fc.constant('test@'),
    fc.constant('@domain.com')
  ),
  
  // Генератор для паролей разной длины
  shortPassword: () => fc.string({ maxLength: 7 }),
  validPassword: () => fc.string({ minLength: 8, maxLength: 50 }),
  
  // Генератор для JWT токенов (имитация)
  jwtToken: () => fc.string({ minLength: 20, maxLength: 200 })
    .filter(s => !s.includes(' ')) // JWT не содержат пробелов
    .map(s => `eyJ${s}.${s}.${s}`), // Имитация структуры JWT
  
  // Генератор для учетных данных пользователя
  loginCredentials: () => fc.record({
    username: fc.string({ minLength: 3, maxLength: 50 }),
    password: fc.string({ minLength: 8, maxLength: 50 }),
    rememberMe: fc.boolean()
  }),
  
  // Генератор для данных регистрации
  registerData: () => fc.record({
    username: fc.string({ minLength: 3, maxLength: 50 }),
    email: fc.emailAddress(),
    phone: fc.string({ minLength: 10, maxLength: 15 }).map(s => '+7' + s),
    password: fc.string({ minLength: 8, maxLength: 50 }),
    confirmPassword: fc.string({ minLength: 8, maxLength: 50 }),
    firstName: fc.option(fc.string({ minLength: 1, maxLength: 50 })),
    lastName: fc.option(fc.string({ minLength: 1, maxLength: 50 })),
    middleName: fc.option(fc.string({ minLength: 1, maxLength: 50 }))
  }),
  
  // Генератор для размеров экрана
  mobileWidth: () => fc.integer({ min: 320, max: 767 }),
  tabletWidth: () => fc.integer({ min: 768, max: 1024 }),
  desktopWidth: () => fc.integer({ min: 1025, max: 3840 }),
  anyWidth: () => fc.integer({ min: 320, max: 3840 })
}

// Утилитарные функции для property тестов
export const propertyHelpers = {
  // Проверка формата email
  isValidEmailFormat: (email: string): boolean => {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
    return emailRegex.test(email)
  },
  
  // Проверка минимальной длины пароля
  isValidPasswordLength: (password: string): boolean => {
    return password.length >= 8
  },
  
  // Проверка совпадения паролей
  passwordsMatch: (password1: string, password2: string): boolean => {
    return password1 === password2
  },
  
  // Определение типа устройства по ширине
  getDeviceType: (width: number): 'mobile' | 'tablet' | 'desktop' => {
    if (width < 768) return 'mobile'
    if (width <= 1024) return 'tablet'
    return 'desktop'
  }
}