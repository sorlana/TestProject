/**
 * Компонент формы входа в систему
 */

import React, { useState, useCallback } from 'react';
import {
  FormGroup,
  InputGroup,
  Checkbox,
  Button,
  Callout,
  Intent
} from '@blueprintjs/core';
import { useAuth } from '../../../application/hooks/useAuth';
import { useErrorHandler } from '../../../application/hooks/useErrorHandler';
import { validateRequired, validatePassword } from '../../../infrastructure/utils/validation';
import type { LoginCredentials } from '../../../domain/types/auth.types';
import type { FieldValidationErrors } from '../../../infrastructure/utils/validationErrorExtractor';

/**
 * Интерфейс пропсов компонента LoginForm
 */
interface LoginFormProps {
  /** Callback, вызываемый при успешном входе */
  onSuccess?: () => void;
}

/**
 * Состояние формы входа
 */
interface LoginFormState {
  username: string;
  password: string;
  rememberMe: boolean;
}

/**
 * Состояние ошибок валидации (объединяет клиентские и серверные ошибки)
 */
interface ValidationErrors {
  username?: string;
  password?: string;
}

/**
 * Компонент формы входа в систему
 * 
 * Предоставляет интерфейс для ввода учетных данных пользователя,
 * выполняет клиентскую валидацию и отправляет данные на сервер.
 * Использует Blueprint UI компоненты для единообразного дизайна.
 */
export const LoginForm: React.FC<LoginFormProps> = ({ onSuccess }) => {
  // Состояние формы
  const [formData, setFormData] = useState<LoginFormState>({
    username: '',
    password: '',
    rememberMe: false
  });

  // Состояние ошибок валидации (клиентские + серверные)
  const [validationErrors, setValidationErrors] = useState<ValidationErrors>({});

  // Хук аутентификации
  const { login, isLoading, error, clearError } = useAuth();

  // Хук для обработки ошибок
  const { getFieldError, hasFieldError } = useErrorHandler();

  /**
   * Объединяет клиентские и серверные ошибки валидации
   * @param serverErrors - Ошибки валидации от сервера
   */
  const mergeValidationErrors = useCallback((serverErrors: FieldValidationErrors) => {
    const mergedErrors: ValidationErrors = { ...validationErrors };

    // Добавляем серверные ошибки
    if (hasFieldError(serverErrors, 'userName') || hasFieldError(serverErrors, 'username')) {
      mergedErrors.username = getFieldError(serverErrors, 'userName') || getFieldError(serverErrors, 'username');
    }
    
    if (hasFieldError(serverErrors, 'password')) {
      mergedErrors.password = getFieldError(serverErrors, 'password');
    }

    setValidationErrors(mergedErrors);
  }, [validationErrors, hasFieldError, getFieldError]);

  /**
   * Валидация формы
   * @returns true если форма валидна, false в противном случае
   */
  const validateForm = useCallback((): boolean => {
    const errors: ValidationErrors = {};

    // Валидация логина
    const usernameValidation = validateRequired(formData.username, 'Логин');
    if (!usernameValidation.isValid) {
      errors.username = usernameValidation.errorMessage;
    }

    // Валидация пароля
    const passwordValidation = validatePassword(formData.password);
    if (!passwordValidation.isValid) {
      errors.password = passwordValidation.errorMessage;
    }

    setValidationErrors(errors);
    return Object.keys(errors).length === 0;
  }, [formData]);

  /**
   * Обработчик изменения значений полей формы
   */
  const handleInputChange = useCallback((field: keyof LoginFormState) => {
    return (event: React.ChangeEvent<HTMLInputElement>) => {
      const value = field === 'rememberMe' ? event.target.checked : event.target.value;
      
      setFormData(prev => ({
        ...prev,
        [field]: value
      }));

      // Очищаем ошибку валидации для изменяемого поля
      if (validationErrors[field as keyof ValidationErrors]) {
        setValidationErrors(prev => ({
          ...prev,
          [field]: undefined
        }));
      }

      // Очищаем общую ошибку при изменении любого поля
      if (error) {
        clearError();
      }
    };
  }, [validationErrors, error, clearError]);

  /**
   * Обработчик отправки формы
   */
  const handleSubmit = useCallback(async (event: React.FormEvent) => {
    event.preventDefault();

    // Валидация формы
    if (!validateForm()) {
      return;
    }

    try {
      // Подготовка данных для отправки
      const credentials: LoginCredentials = {
        username: formData.username.trim(),
        password: formData.password,
        rememberMe: formData.rememberMe
      };

      // Отправка запроса на вход (возвращает ошибки валидации от сервера)
      const serverValidationErrors = await login(credentials);

      // Обрабатываем серверные ошибки валидации
      if (Object.keys(serverValidationErrors).length > 0) {
        mergeValidationErrors(serverValidationErrors);
        return;
      }

      // Вызов callback при успешном входе
      if (onSuccess) {
        onSuccess();
      }
    } catch (error) {
      // Ошибка уже обработана в useAuth hook и отображена через Toast
      console.error('Ошибка входа:', error);
    }
  }, [formData, validateForm, login, onSuccess, mergeValidationErrors]);

  /**
   * Проверка валидности формы для активации кнопки
   */
  const isFormValid = formData.username.trim().length > 0 && 
                     formData.password.length >= 8 && 
                     Object.keys(validationErrors).length === 0;

  return (
    <form onSubmit={handleSubmit} noValidate>
      {/* Отображение общей ошибки */}
      {error && (
        <Callout
          intent={Intent.DANGER}
          title="Ошибка входа"
          style={{ marginBottom: '16px' }}
        >
          {error}
        </Callout>
      )}

      {/* Поле логина */}
      <FormGroup
        label="Логин или Email"
        labelFor="username"
        intent={validationErrors.username ? Intent.DANGER : Intent.NONE}
        helperText={validationErrors.username}
      >
        <InputGroup
          id="username"
          type="text"
          placeholder="Введите логин или email"
          value={formData.username}
          onChange={handleInputChange('username')}
          intent={validationErrors.username ? Intent.DANGER : Intent.NONE}
          disabled={isLoading}
          autoComplete="username"
        />
      </FormGroup>

      {/* Поле пароля */}
      <FormGroup
        label="Пароль"
        labelFor="password"
        intent={validationErrors.password ? Intent.DANGER : Intent.NONE}
        helperText={validationErrors.password}
      >
        <InputGroup
          id="password"
          type="password"
          placeholder="Введите пароль"
          value={formData.password}
          onChange={handleInputChange('password')}
          intent={validationErrors.password ? Intent.DANGER : Intent.NONE}
          disabled={isLoading}
          autoComplete="current-password"
        />
      </FormGroup>

      {/* Чекбокс "Запомнить меня" */}
      <FormGroup>
        <Checkbox
          checked={formData.rememberMe}
          onChange={handleInputChange('rememberMe')}
          disabled={isLoading}
          label="Запомнить меня"
        />
      </FormGroup>

      {/* Кнопка отправки */}
      <Button
        type="submit"
        intent={Intent.PRIMARY}
        loading={isLoading}
        disabled={!isFormValid || isLoading}
        fill
        large
      >
        {isLoading ? 'Вход...' : 'Войти'}
      </Button>
    </form>
  );
};