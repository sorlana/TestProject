# План реализации: User Portal

## Обзор

User Portal - это фронтенд SPA на React с TypeScript, следующий принципам чистой архитектуры. План реализации разбит на логические этапы: настройка проекта, реализация инфраструктурного слоя, доменного слоя, слоя приложения, слоя представления, и финальная интеграция с развертыванием.

## Задачи

- [ ] 1. Инициализация проекта и настройка инфраструктуры
  - Создать проект с Vite + React + TypeScript в директории apps/user-portal/
  - Установить зависимости: @blueprintjs/core, @blueprintjs/icons, react-router-dom, zustand, axios
  - Настроить TypeScript (tsconfig.json) с strict режимом
  - Настроить ESLint и Prettier для единого стиля кода
  - Создать структуру директорий согласно чистой архитектуре (presentation/, application/, domain/, infrastructure/)
  - _Requirements: 12.1, 12.5_

- [ ] 2. Реализация Domain Layer (доменный слой)
  - [ ] 2.1 Создать доменные типы и интерфейсы
    - Создать src/domain/types/auth.types.ts с типами LoginCredentials, RegisterData, AuthResponse, GoogleAuthRequest, RefreshTokenRequest
    - Создать src/domain/entities/User.ts с интерфейсом User
    - Создать src/domain/types/api.types.ts с типом ApiError
    - Создать src/domain/interfaces/IAuthService.ts с интерфейсом IAuthService
    - Создать src/domain/interfaces/ITokenService.ts с интерфейсом ITokenService
    - _Requirements: 1.1, 1.2, 1.3, 3.1, 3.2, 3.3, 4.1_

- [ ] 3. Реализация Infrastructure Layer (инфраструктурный слой)
  - [ ] 3.1 Реализовать Token Service
    - Создать src/infrastructure/services/tokenService.ts, реализующий ITokenService
    - Реализовать методы saveTokens, getAccessToken, getRefreshToken, clearTokens, isRememberMe
    - Использовать localStorage для rememberMe=true, sessionStorage для rememberMe=false
    - _Requirements: 4.1, 4.2_
  
  - [ ]* 3.2 Написать property тест для Token Service
    - **Property 10: Сохранение токенов в localStorage при "Запомнить меня"**
    - **Property 11: Сохранение токенов в sessionStorage без "Запомнить меня"**
    - **Validates: Requirements 3.4, 4.1, 4.2**
  
  - [ ] 3.3 Создать Axios instance с interceptors
    - Создать src/infrastructure/services/api.ts с настроенным Axios instance
    - Настроить baseURL на '/api', timeout 30 секунд
    - Реализовать request interceptor для автоматического добавления JWT токена в заголовок Authorization
    - Реализовать response interceptor для обработки 401 ошибок и автоматического обновления токена
    - Реализовать очередь запросов во время обновления токена
    - _Requirements: 3.5, 4.3, 4.4, 4.5_
  
  - [ ]* 3.4 Написать property тесты для Axios interceptors
    - **Property 12: Автоматическое добавление токена в заголовки**
    - **Property 13: Автоматическое обновление истекшего токена**
    - **Property 14: Очистка токенов при неудачном обновлении**
    - **Validates: Requirements 4.3, 3.5, 4.4, 4.5**
  
  - [ ] 3.4 Реализовать Auth Service
    - Создать src/infrastructure/services/authService.ts, реализующий IAuthService
    - Реализовать метод login: отправка POST /api/auth/login, сохранение токенов, обновление store
    - Реализовать метод register: отправка POST /api/auth/register, сохранение токенов, обновление store
    - Реализовать метод loginWithGoogle: отправка POST /api/auth/google, сохранение токенов
    - Реализовать метод refreshToken: отправка POST /api/auth/refresh с refresh токеном
    - Реализовать метод logout: отправка POST /api/auth/logout, очистка токенов, сброс store
    - _Requirements: 3.1, 3.2, 3.3, 3.5, 3.6, 5.5_
  
  - [ ]* 3.5 Написать property тесты для Auth Service
    - **Property 7: Отправка запроса на вход**
    - **Property 8: Отправка запроса на регистрацию**
    - **Property 9: Отправка запроса Google OAuth**
    - **Property 15: Выход из системы**
    - **Validates: Requirements 3.1, 3.2, 3.3, 3.6, 5.5**
  
  - [ ] 3.6 Создать утилиты валидации
    - Создать src/infrastructure/utils/validation.ts
    - Реализовать функцию validateEmail для проверки формата email
    - Реализовать функцию validatePassword для проверки минимальной длины 8 символов
    - Реализовать функцию validatePasswordMatch для проверки совпадения паролей
    - Реализовать функцию validatePhone для проверки формата телефона
    - _Requirements: 2.1, 2.2, 2.3_
  
  - [ ]* 3.7 Написать property тесты для валидации
    - **Property 2: Email валидация**
    - **Property 3: Валидация минимальной длины пароля**
    - **Property 4: Валидация совпадения паролей**
    - **Validates: Requirements 2.1, 2.2, 2.3**

- [ ] 4. Checkpoint - Проверка инфраструктурного слоя
  - Убедиться, что все тесты проходят, задать вопросы пользователю при необходимости

- [ ] 5. Реализация Application Layer (слой приложения)
  - [ ] 5.1 Создать Zustand store для аутентификации
    - Создать src/application/store/authStore.ts
    - Реализовать состояние: isAuthenticated, user, isLoading, error
    - Реализовать методы: setAuthenticated, setUser, setLoading, setError, reset
    - _Requirements: 4.1, 5.1_
  
  - [ ] 5.2 Создать custom hook useAuth
    - Создать src/application/hooks/useAuth.ts
    - Реализовать hook, который использует authStore и authService
    - Предоставить методы: login, register, loginWithGoogle, logout
    - Обрабатывать состояния загрузки и ошибок
    - _Requirements: 3.1, 3.2, 3.3, 3.6_

- [ ] 6. Реализация Presentation Layer - компоненты авторизации
  - [ ] 6.1 Создать LoginForm компонент
    - Создать src/presentation/components/auth/LoginForm.tsx
    - Использовать Blueprint UI компоненты: FormGroup, InputGroup, Checkbox, Button
    - Реализовать поля: username, password, rememberMe
    - Реализовать клиентскую валидацию с использованием validation utils
    - Отображать ошибки через Blueprint Callout
    - Вызывать useAuth().login при отправке формы
    - Активировать кнопку отправки только при валидной форме
    - _Requirements: 1.2, 2.1, 2.2, 2.4, 2.5, 3.1_
  
  - [ ]* 6.2 Написать unit тесты для LoginForm
    - Тест рендеринга всех полей формы
    - Тест деактивации кнопки при невалидной форме
    - Тест вызова onSubmit с корректными данными
    - _Requirements: 1.2, 2.5_
  
  - [ ] 6.3 Создать RegisterForm компонент
    - Создать src/presentation/components/auth/RegisterForm.tsx
    - Использовать Blueprint UI компоненты для полей: username, email, phone, password, confirmPassword, firstName, lastName, middleName
    - Реализовать клиентскую валидацию всех полей
    - Отображать ошибки валидации через Blueprint Callout
    - Вызывать useAuth().register при отправке формы
    - _Requirements: 1.3, 2.1, 2.2, 2.3, 2.4, 3.2_
  
  - [ ]* 6.4 Написать unit тесты для RegisterForm
    - Тест рендеринга всех полей формы
    - Тест валидации email формата
    - Тест валидации совпадения паролей
    - _Requirements: 1.3, 2.1, 2.3_
  
  - [ ] 6.5 Создать GoogleAuthButton компонент
    - Создать src/presentation/components/auth/GoogleAuthButton.tsx
    - Интегрировать Google Identity Services (GIS) библиотеку
    - Реализовать кнопку "Войти через Google" с Blueprint Button
    - Обрабатывать Google OAuth callback и получать ID токен
    - Вызывать useAuth().loginWithGoogle с полученным токеном
    - _Requirements: 1.4, 3.3_
  
  - [ ] 6.6 Создать LoginPage
    - Создать src/presentation/pages/LoginPage.tsx
    - Использовать Blueprint Tabs для переключения между "Вход" и "Регистрация"
    - Отображать LoginForm на табе "Вход"
    - Отображать RegisterForm на табе "Регистрация"
    - Отображать GoogleAuthButton на обоих табах
    - Реализовать редирект на /app/dashboard при успешной авторизации
    - _Requirements: 1.1, 1.5_
  
  - [ ]* 6.7 Написать property тест для успешной авторизации
    - **Property 1: Успешная авторизация приводит к редиректу на dashboard**
    - **Validates: Requirements 1.5**

- [ ] 7. Реализация Presentation Layer - layout компоненты
  - [ ] 7.1 Создать Header компонент
    - Создать src/presentation/components/layout/Header.tsx
    - Использовать Blueprint Navbar компонент
    - Отображать логотип/название платформы слева
    - Отображать логин пользователя как ссылку справа (получать из authStore)
    - Отображать кнопку "Выйти" справа
    - Вызывать useAuth().logout при клике на "Выйти"
    - Реализовать адаптивную навигацию для мобильных устройств
    - _Requirements: 5.2, 5.3, 5.4, 5.5_
  
  - [ ]* 7.2 Написать unit тесты для Header
    - Тест отображения логина пользователя
    - Тест вызова logout при клике на кнопку "Выйти"
    - _Requirements: 5.3, 5.5_
  
  - [ ] 7.3 Создать ProtectedRoute компонент
    - Создать src/presentation/components/layout/ProtectedRoute.tsx
    - Проверять наличие JWT токена через tokenService
    - Проверять состояние isAuthenticated из authStore
    - Выполнять редирект на /app/login если токен отсутствует или истек
    - Отображать Outlet для вложенных маршрутов если авторизован
    - _Requirements: 6.1, 6.2, 6.3, 6.4_
  
  - [ ]* 7.4 Написать property тесты для ProtectedRoute
    - **Property 16: Защита маршрутов от неавторизованных пользователей**
    - **Property 17: Доступ к защищенным маршрутам с валидным токеном**
    - **Validates: Requirements 6.1, 6.3, 6.4**
  
  - [ ] 7.5 Создать ErrorBoundary компонент
    - Создать src/presentation/components/common/ErrorBoundary.tsx
    - Реализовать React Error Boundary для перехвата ошибок рендеринга
    - Отображать fallback UI при ошибке
    - Логировать ошибки в консоль (dev режим)
    - _Requirements: 8.5_
  
  - [ ] 7.6 Создать LoadingSpinner компонент
    - Создать src/presentation/components/common/LoadingSpinner.tsx
    - Использовать Blueprint Spinner компонент
    - Отображать индикатор загрузки с центрированием
    - _Requirements: 5.1_

- [ ] 8. Реализация Presentation Layer - страницы
  - [ ] 8.1 Создать DashboardPage
    - Создать src/presentation/pages/DashboardPage.tsx
    - Отображать Header компонент
    - Отображать приветственное сообщение с именем пользователя
    - Пока оставить основную область пустой (заглушка для будущего функционала)
    - _Requirements: 5.1_
  
  - [ ] 8.2 Создать PaymentPage (заглушка)
    - Создать src/presentation/pages/PaymentPage.tsx
    - Отображать Header компонент
    - Отображать сообщение "Страница оплаты - в разработке"
    - _Requirements: 7.3_

- [ ] 9. Настройка маршрутизации и главного компонента
  - [ ] 9.1 Настроить React Router
    - Создать src/App.tsx с настройкой React Router
    - Настроить маршрут /app/login для LoginPage
    - Настроить защищенный маршрут /app/dashboard для DashboardPage (через ProtectedRoute)
    - Настроить защищенный маршрут /app/payment для PaymentPage (через ProtectedRoute)
    - Настроить редирект с /app на /app/dashboard если авторизован, иначе на /app/login
    - Обернуть приложение в ErrorBoundary
    - _Requirements: 6.5, 6.6, 7.1, 7.2, 7.3, 7.4_
  
  - [ ]* 9.2 Написать integration тесты для маршрутизации
    - Тест редиректа с /app на /app/login для неавторизованного пользователя
    - Тест редиректа с /app на /app/dashboard для авторизованного пользователя
    - Тест доступа к защищенным маршрутам
    - _Requirements: 6.5, 6.6_

- [ ] 10. Обработка ошибок
  - [ ] 10.1 Реализовать обработку ошибок API
    - Обновить authService для обработки различных типов ошибок
    - Обработка ошибок валидации (400): извлечение и отображение ошибок для полей
    - Обработка ошибок аутентификации (401): очистка токенов и редирект
    - Обработка сетевых ошибок: отображение сообщения "Сервис временно недоступен"
    - Обработка серверных ошибок (500): отображение общего сообщения
    - Использовать Blueprint Toast для отображения ошибок
    - _Requirements: 8.1, 8.2, 8.3, 8.4, 8.5_
  
  - [ ]* 10.2 Написать property тесты для обработки ошибок
    - **Property 18: Отображение ошибок от API**
    - **Property 19: Обработка сетевых ошибок**
    - **Property 20: Отображение ошибок валидации от API**
    - **Property 21: Обработка ошибок авторизации**
    - **Validates: Requirements 8.1, 8.2, 8.3, 8.4**

- [ ] 11. Checkpoint - Проверка функциональности
  - Убедиться, что все тесты проходят, задать вопросы пользователю при необходимости

- [ ] 12. Адаптивный дизайн
  - [ ] 12.1 Реализовать адаптивные стили
    - Создать src/index.css с глобальными стилями и media queries
    - Определить breakpoints: mobile (< 768px), tablet (768px - 1024px), desktop (> 1024px)
    - Реализовать mobile-first подход
    - Адаптировать LoginPage для мобильных устройств
    - Адаптировать DashboardPage для мобильных устройств
    - Адаптировать Header для мобильных устройств (сворачивающееся меню)
    - _Requirements: 9.1, 9.2, 9.3, 9.4_
  
  - [ ]* 12.2 Написать property тесты для адаптивности
    - **Property 22: Адаптивность для мобильных устройств**
    - **Property 23: Адаптивность для планшетов**
    - **Property 24: Адаптивность для десктопов**
    - **Property 25: Динамическая адаптация при изменении размера**
    - **Validates: Requirements 9.1, 9.2, 9.3, 9.4**

- [ ] 13. Настройка тестирования
  - [ ] 13.1 Настроить Vitest и React Testing Library
    - Установить зависимости: vitest, @testing-library/react, @testing-library/user-event, @testing-library/jest-dom
    - Создать vitest.config.ts с настройками для React
    - Создать src/test/setup.ts для настройки тестового окружения
    - _Requirements: Testing Strategy_
  
  - [ ] 13.2 Настроить fast-check для property-based тестирования
    - Установить зависимость: fast-check
    - Настроить минимум 100 итераций для каждого property теста
    - Создать примеры property тестов с тегами формата "Feature: user-portal, Property N: ..."
    - _Requirements: Testing Strategy_

- [ ] 14. Развертывание
  - [ ] 14.1 Создать Dockerfile
    - Создать Dockerfile с multi-stage build (builder + nginx)
    - Builder stage: установка зависимостей и сборка проекта с Vite
    - Production stage: копирование собранных файлов в nginx:alpine
    - _Requirements: 10.1_
  
  - [ ] 14.2 Создать Nginx конфигурацию
    - Создать nginx/nginx.conf
    - Настроить раздачу статических файлов из /usr/share/nginx/html
    - Настроить SPA routing (try_files для fallback на index.html)
    - Настроить проксирование /api/* на user-authentication-service:8080
    - Настроить gzip сжатие
    - Настроить кэширование статических файлов (js, css, изображения)
    - _Requirements: 10.2, 10.5_
  
  - [ ] 14.3 Создать Kubernetes манифесты
    - Создать k8s/deployment.yaml с 2 репликами, resource limits, health checks
    - Создать k8s/service.yaml с ClusterIP сервисом на порту 80
    - Создать k8s/ingress.yaml с путем /app/* и TLS конфигурацией
    - _Requirements: 10.3, 10.4_

- [ ] 15. Финальная интеграция и документация
  - [ ] 15.1 Создать README.md
    - Описать структуру проекта и чистую архитектуру
    - Добавить инструкции по локальной разработке (npm install, npm run dev)
    - Добавить инструкции по сборке (npm run build)
    - Добавить инструкции по запуску тестов (npm test)
    - Добавить инструкции по развертыванию (Docker, Kubernetes)
    - Описать переменные окружения и конфигурацию
    - _Requirements: 12.1_
  
  - [ ] 15.2 Финальное тестирование
    - Запустить все unit тесты
    - Запустить все property тесты
    - Проверить работу приложения локально
    - Проверить сборку Docker образа
    - _Requirements: Testing Strategy_

- [ ] 16. Checkpoint - Финальная проверка
  - Убедиться, что все тесты проходят, приложение собирается и работает корректно

## Примечания

- Задачи, помеченные `*`, являются опциональными и могут быть пропущены для более быстрого MVP
- Каждая задача ссылается на конкретные требования для отслеживаемости
- Checkpoints обеспечивают инкрементальную валидацию
- Property тесты валидируют универсальные свойства корректности
- Unit тесты валидируют конкретные примеры и edge cases
- Структура проекта следует принципам чистой архитектуры с четким разделением слоев
