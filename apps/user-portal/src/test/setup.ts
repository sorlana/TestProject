// Настройка тестового окружения для Vitest и React Testing Library
import '@testing-library/jest-dom'

// Настройка глобальных переменных для тестов
Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: (query: string) => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: () => {},
    removeListener: () => {},
    addEventListener: () => {},
    removeEventListener: () => {},
    dispatchEvent: () => {},
  }),
})

// Мок для localStorage и sessionStorage
const localStorageMock = {
  getItem: () => null,
  setItem: () => {},
  removeItem: () => {},
  clear: () => {},
  length: 0,
  key: () => null,
}

Object.defineProperty(window, 'localStorage', {
  value: localStorageMock,
})

Object.defineProperty(window, 'sessionStorage', {
  value: localStorageMock,
})

// Мок для Google Identity Services (для тестов Google OAuth)
Object.defineProperty(window, 'google', {
  value: {
    accounts: {
      id: {
        initialize: () => {},
        renderButton: () => {},
        prompt: () => {},
      },
    },
  },
})