# User Portal

User Portal - это фронтенд SPA (Single Page Application) на базе React 18+ и TypeScript, предоставляющий пользовательский интерфейс для авторизации, регистрации и доступа к личному кабинету. Приложение интегрируется с существующим Auth Service через HTTP API и использует JWT токены для аутентификации.

## Технологический стек

- **React 19** - библиотека для создания пользовательских интерфейсов
- **TypeScript** - типизированный JavaScript для повышения надежности кода
- **Blueprint UI** - библиотека UI компонентов от Palantir
- **React Router v7** - маршрутизация для SPA
- **Zustand** - легковесное управление состоянием
- **Axios** - HTTP клиент с interceptors
- **Vite** - быстрый инструмент сборки и разработки
- **Vitest** - фреймворк для тестирования
- **fast-check** - библиотека для property-based тестирования

## Архитектура проекта

User Portal следует принципам **чистой архитектуры (Clean Architecture)** с четким разделением на слои:

```
src/
├── presentation/        # Presentation Layer - UI компоненты
│   ├── components/
│   │   ├── auth/       # Компоненты авторизации
│   │   ├── layout/     # Компоненты layout
│   │   └── common/     # Общие компоненты
│   ├── pages/          # Страницы приложения
│   └── styles/         # Стили и CSS
├── application/         # Application Layer - бизнес-логика
│   ├── hooks/          # Custom React hooks
│   ├── store/          # Zustand store
│   └── use-cases/      # Use cases (опционально)
├── domain/              # Domain Layer - типы и интерфейсы
│   ├── entities/       # Доменные сущности
│   ├── interfaces/     # Интерфейсы сервисов
│   └── types/          # TypeScript типы
├── infrastructure/      # Infrastructure Layer - внешние интеграции
│   ├── services/       # Реализация сервисов
│   └── utils/          # Утилиты
└── test/               # Тестирование
    └── properties/     # Property-based тесты
```

### Принципы архитектуры

1. **Разделение ответственности** - каждый слой имеет четко определенную роль
2. **Независимость от фреймворков** - бизнес-логика не зависит от React
3. **Тестируемость** - каждый слой можно тестировать независимо
4. **Правило зависимостей** - зависимости направлены внутрь

### Ключевые компоненты

- **Auth Service** - интеграция с Auth Service API
- **Token Service** - управление JWT токенами
- **Protected Routes** - защита маршрутов от неавторизованных пользователей
- **Error Boundary** - обработка ошибок React
- **Axios Interceptors** - автоматическое добавление токенов и обновление

## Локальная разработка

### Предварительные требования

- Node.js 18+ 
- npm или yarn

### Установка зависимостей

```bash
cd apps/user-portal
npm install
```

### Запуск в режиме разработки

```bash
npm run dev
```

Приложение будет доступно по адресу `http://localhost:5173`

### Другие команды разработки

```bash
# Форматирование кода
npm run format

# Проверка линтером
npm run lint

# Предварительный просмотр production сборки
npm run preview
```

## Сборка проекта

### Production сборка

```bash
npm run build
```

Собранные файлы будут находиться в директории `dist/`

### Проверка сборки

```bash
npm run preview
```

## Тестирование

Проект использует двойной подход к тестированию:

- **Unit тесты** - проверяют конкретные примеры и edge cases
- **Property-based тесты** - проверяют универсальные свойства корректности

### Запуск всех тестов

```bash
npm test
```

### Запуск тестов в watch режиме

```bash
npm run test:watch
```

### Запуск тестов с UI

```bash
npm run test:ui
```

### Структура тестов

- `src/test/setup.ts` - настройка тестового окружения
- `src/test/properties/` - property-based тесты с fast-check
- Компонентные тесты располагаются рядом с компонентами (`.test.tsx`)

## Развертывание

### Docker

Проект включает Dockerfile для контейнеризации:

```bash
# Сборка Docker образа
docker build -t user-portal .

# Запуск контейнера
docker run -p 80:80 user-portal
```

### Kubernetes

Kubernetes манифесты находятся в директории `k8s/`:

- `deployment.yaml` - развертывание приложения
- `service.yaml` - сервис для доступа к приложению  
- `ingress.yaml` - настройка входящего трафика

```bash
# Применение манифестов
kubectl apply -f k8s/
```

### Nginx конфигурация

Приложение использует Nginx для:
- Раздачи статических файлов
- Проксирования API запросов на Auth Service
- Обработки SPA маршрутизации (fallback на index.html)

Конфигурация находится в `nginx/nginx.conf`

## Переменные окружения

Приложение поддерживает следующие переменные окружения:

### Режим разработки (.env.development)

```env
VITE_API_BASE_URL=http://localhost:8080/api
VITE_GOOGLE_CLIENT_ID=your-google-client-id
```

### Режим production (.env.production)

```env
VITE_API_BASE_URL=/api
VITE_GOOGLE_CLIENT_ID=your-production-google-client-id
```

### Описание переменных

- `VITE_API_BASE_URL` - базовый URL для API запросов
- `VITE_GOOGLE_CLIENT_ID` - Client ID для Google OAuth

## Конфигурация

### TypeScript

- `tsconfig.json` - основная конфигурация TypeScript
- `tsconfig.app.json` - конфигурация для приложения
- `tsconfig.node.json` - конфигурация для Node.js скриптов

### Vite

- `vite.config.ts` - конфигурация сборщика Vite

### ESLint и Prettier

- `eslint.config.js` - правила линтера
- `.prettierrc` - настройки форматирования кода

## Маршруты приложения

- `/app/login` - страница авторизации и регистрации
- `/app/dashboard` - личный кабинет (защищенный маршрут)
- `/app/payment` - страница оплаты (защищенный маршрут)
- `/app` - редирект на dashboard или login в зависимости от авторизации

## Интеграция с Auth Service

Приложение интегрируется с Auth Service через следующие endpoints:

- `POST /api/auth/login` - вход в систему
- `POST /api/auth/register` - регистрация
- `POST /api/auth/google` - вход через Google OAuth
- `POST /api/auth/refresh` - обновление JWT токена
- `POST /api/auth/logout` - выход из системы

## Безопасность

- JWT токены хранятся в localStorage (при "Запомнить меня") или sessionStorage
- Автоматическое обновление истекших токенов
- Защита маршрутов от неавторизованных пользователей
- Использование HTTPS в production
- Защита от XSS атак через React

## Адаптивный дизайн

Приложение поддерживает адаптивный дизайн с breakpoints:

- **Mobile**: < 768px
- **Tablet**: 768px - 1024px  
- **Desktop**: > 1024px

## Поддержка и разработка

### Структура коммитов

Рекомендуется использовать conventional commits:

```
feat: добавить новую функциональность
fix: исправить ошибку
docs: обновить документацию
style: изменения стилей
refactor: рефакторинг кода
test: добавить или обновить тесты
```

### Отладка

Для отладки в режиме разработки доступны:

- React Developer Tools
- Redux DevTools (для Zustand)
- Консоль браузера с подробными логами

## Лицензия

Проект разработан для внутреннего использования.