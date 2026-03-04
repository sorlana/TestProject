# Система обработки ошибок User Portal

Эта документация описывает новую систему обработки ошибок, реализованную в User Portal для соответствия требованиям 8.1-8.5.

## Обзор

Система обработки ошибок состоит из нескольких компонентов:

1. **ErrorHandler** - Основной класс для обработки и классификации ошибок
2. **Toast Manager** - Система уведомлений на основе Blueprint UI
3. **ValidationErrorExtractor** - Утилита для работы с ошибками валидации
4. **useErrorHandler** - React hook для удобного использования в компонентах

## Компоненты системы

### ErrorHandler

Основной класс для обработки ошибок API. Автоматически определяет тип ошибки и предоставляет структурированную информацию.

```typescript
import { ErrorHandler } from '../infrastructure/utils/errorHandler';

try {
  await someApiCall();
} catch (error) {
  const processedError = ErrorHandler.processError(error);
  console.log(processedError.type); // 'validation', 'authentication', etc.
  console.log(processedError.message); // Человекочитаемое сообщение
  console.log(processedError.fieldErrors); // Ошибки валидации полей (если есть)
}
```

#### Типы ошибок

- `VALIDATION` (400) - Ошибки валидации данных
- `AUTHENTICATION` (401) - Ошибки аутентификации
- `AUTHORIZATION` (403) - Недостаточно прав
- `NOT_FOUND` (404) - Ресурс не найден
- `SERVER_ERROR` (500+) - Серверные ошибки
- `NETWORK_ERROR` - Сетевые ошибки
- `UNKNOWN` - Неизвестные ошибки

### Toast Manager

Система уведомлений для отображения ошибок пользователю.

```typescript
import { toast } from '../infrastructure/utils/toast';

// Различные типы уведомлений
toast.success('Операция выполнена успешно');
toast.error('Произошла ошибка');
toast.warning('Предупреждение');
toast.info('Информационное сообщение');

// Кастомная конфигурация
toast.show({
  message: 'Сервис недоступен',
  intent: 'danger',
  timeout: 7000,
  action: {
    text: 'Повторить',
    onClick: () => window.location.reload()
  }
});
```

### ValidationErrorExtractor

Утилита для работы с ошибками валидации от API.

```typescript
import { ValidationErrorExtractor } from '../infrastructure/utils/validationErrorExtractor';

const processedError = ErrorHandler.processError(error);
const fieldErrors = ValidationErrorExtractor.extractFieldErrors(processedError);

// Получение ошибки для конкретного поля
const usernameError = ValidationErrorExtractor.getFirstFieldError(fieldErrors, 'username');

// Проверка наличия ошибки
const hasError = ValidationErrorExtractor.hasFieldError(fieldErrors, 'email');
```

### useErrorHandler Hook

React hook для удобного использования системы обработки ошибок в компонентах.

```typescript
import { useErrorHandler } from '../application/hooks/useErrorHandler';

const MyComponent = () => {
  const { 
    handleError, 
    handleValidationError, 
    getFieldError, 
    hasFieldError,
    showSuccess 
  } = useErrorHandler();

  const handleSubmit = async () => {
    try {
      await submitForm();
      showSuccess('Форма отправлена успешно');
    } catch (error) {
      // Автоматическая обработка и отображение ошибки
      const result = handleError(error);
      
      // Для ошибок валидации получаем ошибки полей
      if (result.type === 'validation') {
        const fieldErrors = result.fieldErrors;
        // Обновляем состояние формы с ошибками
      }
    }
  };
};
```

## Интеграция с формами

### Пример использования в LoginForm

```typescript
const LoginForm = () => {
  const { login } = useAuth();
  const { getFieldError, hasFieldError } = useErrorHandler();
  const [validationErrors, setValidationErrors] = useState({});

  const handleSubmit = async (formData) => {
    try {
      // login теперь возвращает ошибки валидации от сервера
      const serverErrors = await login(formData);
      
      if (Object.keys(serverErrors).length > 0) {
        // Объединяем серверные ошибки с клиентскими
        const mergedErrors = {
          username: getFieldError(serverErrors, 'userName'),
          password: getFieldError(serverErrors, 'password')
        };
        setValidationErrors(mergedErrors);
      }
    } catch (error) {
      // Неожиданные ошибки уже обработаны и показаны через Toast
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <FormGroup
        helperText={validationErrors.username}
        intent={validationErrors.username ? 'danger' : 'none'}
      >
        <InputGroup />
      </FormGroup>
    </form>
  );
};
```

## Обработка различных типов ошибок

### Ошибки валидации (400)

- Извлекаются детальные ошибки для каждого поля
- Отображаются под соответствующими полями формы
- Общее сообщение показывается через Callout в форме
- Toast не показывается (чтобы не дублировать информацию)

### Ошибки аутентификации (401)

- Автоматическое обновление токена через interceptor
- При неудаче - очистка токенов и редирект на /login
- Toast с сообщением "Сессия истекла"

### Сетевые ошибки

- Toast с сообщением "Сервис временно недоступен"
- Кнопка "Повторить" для перезагрузки страницы

### Серверные ошибки (500)

- Toast с общим сообщением об ошибке сервера
- Детали логируются в консоль (только в dev режиме)

## Конфигурация

### Настройка Toast

Toast уведомления настраиваются в `toast.ts`:

- Позиция: `TOP_RIGHT`
- Максимум уведомлений: 5
- Автоматическое закрытие по Escape
- Таймауты по умолчанию:
  - Успех: 4 секунды
  - Ошибка: 5 секунд
  - Предупреждение: 4 секунды
  - Информация: 4 секунды

### Настройка Axios Interceptor

Response interceptor в `api.ts` автоматически:

- Обрабатывает 401 ошибки
- Обновляет токены
- Управляет очередью запросов
- Показывает уведомления об ошибках аутентификации

## Лучшие практики

1. **Используйте useErrorHandler hook** в компонентах вместо прямого обращения к ErrorHandler
2. **Не показывайте Toast для ошибок валидации** - они должны отображаться в формах
3. **Объединяйте клиентские и серверные ошибки валидации** для лучшего UX
4. **Логируйте детали ошибок** в консоль для отладки
5. **Предоставляйте действия восстановления** (кнопки "Повторить") для сетевых ошибок

## Требования

Система обработки ошибок реализует следующие требования:

- **8.1**: Отображение ошибок от API через Toast и Callout
- **8.2**: Обработка сетевых ошибок с сообщением "Сервис временно недоступен"
- **8.3**: Извлечение и отображение ошибок валидации для полей форм
- **8.4**: Обработка ошибок аутентификации с очисткой токенов и редиректом
- **8.5**: Обработка неожиданных ошибок с общими сообщениями