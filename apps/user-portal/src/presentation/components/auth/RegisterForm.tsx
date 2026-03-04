/**
 * Компонент формы регистрации нового пользователя
 */

import React, { useState, useCallback } from 'react';
import {
  FormGroup,
  InputGroup,
  Button,
  Callout,
  Intent,
  RadioGroup,
  Radio
} from '@blueprintjs/core';
import { useAuth } from '../../../application/hooks/useAuth';
import {
  validateRequired,
  validateEmail,
  validatePassword,
  validatePasswordMatch,
  validatePhone,
  validateUsername
} from '../../../infrastructure/utils/validation';
import type { RegisterData } from '../../../domain/types/auth.types';

/**
 * Способ подтверждения регистрации
 */
type VerificationMethod = 'email' | 'phone';

/**
 * Этап процесса регистрации
 */
type RegistrationStep = 'input' | 'verification';

/**
 * Интерфейс пропсов компонента RegisterForm
 */
interface RegisterFormProps {
  /** Callback, вызываемый при успешной регистрации */
  onSuccess?: () => void;
}

/**
 * Состояние формы регистрации
 */
interface RegisterFormState {
  username: string;
  email: string;
  phone: string;
  password: string;
  confirmPassword: string;
  firstName: string;
  lastName: string;
  middleName: string;
  verificationMethod: VerificationMethod;
  verificationCode: string;
  registrationStep: RegistrationStep;
}

/**
 * Состояние ошибок валидации
 */
interface ValidationErrors {
  username?: string;
  email?: string;
  phone?: string;
  password?: string;
  confirmPassword?: string;
  firstName?: string;
  lastName?: string;
  middleName?: string;
}

/**
 * Компонент формы регистрации нового пользователя
 * 
 * Предоставляет интерфейс для ввода данных нового пользователя,
 * выполняет клиентскую валидацию всех полей и отправляет данные на сервер.
 * Использует Blueprint UI компоненты для единообразного дизайна.
 */
export const RegisterForm: React.FC<RegisterFormProps> = ({ onSuccess }) => {
  // Состояние формы
  const [formData, setFormData] = useState<RegisterFormState>({
    username: '',
    email: '',
    phone: '',
    password: '',
    confirmPassword: '',
    firstName: '',
    lastName: '',
    middleName: '',
    verificationMethod: 'email',
    verificationCode: '',
    registrationStep: 'input'
  });

  // Состояние ошибок валидации
  const [validationErrors, setValidationErrors] = useState<ValidationErrors>({});

  // Состояние ошибки отправки кода
  const [verificationError, setVerificationError] = useState<string | null>(null);

  // Хук аутентификации
  const { isLoading, error, clearError, sendEmailVerification, verifyEmailCode, registerWithEmailVerification } = useAuth();

  /**
   * Валидация формы
   * @returns true если форма валидна, false в противном случае
   */
  const validateForm = useCallback((): boolean => {
    const errors: ValidationErrors = {};

    // Валидация логина
    const usernameValidation = validateUsername(formData.username);
    if (!usernameValidation.isValid) {
      errors.username = usernameValidation.errorMessage;
    }

    // Валидация email (только если выбран способ подтверждения email)
    if (formData.verificationMethod === 'email') {
      const emailValidation = validateEmail(formData.email);
      if (!emailValidation.isValid) {
        errors.email = emailValidation.errorMessage;
      }
    }

    // Валидация телефона (только если выбран способ подтверждения phone)
    if (formData.verificationMethod === 'phone') {
      const phoneValidation = validatePhone(formData.phone);
      if (!phoneValidation.isValid) {
        errors.phone = phoneValidation.errorMessage;
      }
    }

    // Валидация пароля
    const passwordValidation = validatePassword(formData.password);
    if (!passwordValidation.isValid) {
      errors.password = passwordValidation.errorMessage;
    }

    // Валидация совпадения паролей
    const passwordMatchValidation = validatePasswordMatch(formData.password, formData.confirmPassword);
    if (!passwordMatchValidation.isValid) {
      errors.confirmPassword = passwordMatchValidation.errorMessage;
    }

    // Валидация имени (опционально, но если заполнено - должно быть валидным)
    if (formData.firstName.trim().length > 0) {
      const firstNameValidation = validateRequired(formData.firstName, 'Имя');
      if (!firstNameValidation.isValid) {
        errors.firstName = firstNameValidation.errorMessage;
      }
    }

    // Валидация фамилии (опционально, но если заполнено - должно быть валидным)
    if (formData.lastName.trim().length > 0) {
      const lastNameValidation = validateRequired(formData.lastName, 'Фамилия');
      if (!lastNameValidation.isValid) {
        errors.lastName = lastNameValidation.errorMessage;
      }
    }

    setValidationErrors(errors);
    return Object.keys(errors).length === 0;
  }, [formData]);

  /**
   * Обработчик изменения значений полей формы
   */
  const handleInputChange = useCallback((field: keyof RegisterFormState) => {
    return (event: React.ChangeEvent<HTMLInputElement>) => {
      const value = event.target.value;
      
      setFormData(prev => ({
        ...prev,
        [field]: value
      }));

      // Очищаем ошибку валидации для изменяемого поля (только для полей, которые есть в ValidationErrors)
      if (field in validationErrors && validationErrors[field as keyof ValidationErrors]) {
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
   * Обработчик изменения способа подтверждения
   */
  const handleVerificationMethodChange = useCallback((event: React.FormEvent<HTMLInputElement>) => {
    const value = event.currentTarget.value as VerificationMethod;
    
    setFormData(prev => ({
      ...prev,
      verificationMethod: value,
      // Очищаем поле email при выборе phone
      email: value === 'phone' ? '' : prev.email,
      // Очищаем поле phone при выборе email
      phone: value === 'email' ? '' : prev.phone
    }));

    // Очищаем ошибки валидации для email и phone
    setValidationErrors(prev => ({
      ...prev,
      email: undefined,
      phone: undefined
    }));
  }, []);

  /**
   * Обработчик отправки кода подтверждения
   */
  const handleSendVerificationCode = useCallback(async () => {
    // Валидация формы
    if (!validateForm()) {
      return;
    }

    try {
      setVerificationError(null);

      // Для email: отправляем код на email
      if (formData.verificationMethod === 'email') {
        await sendEmailVerification(formData.email.trim());
      } else {
        // Для phone: используем существующую логику SMS
        // TODO: Реализовать отправку SMS кода
        // Пока просто переключаем на этап verification
        console.log('Отправка SMS кода на:', formData.phone);
      }

      // При успехе: переключаем на этап ввода кода
      setFormData(prev => ({
        ...prev,
        registrationStep: 'verification'
      }));
    } catch (error) {
      // Ошибка уже обработана в useAuth hook (показан Toast)
      // Дополнительно показываем Callout в форме
      setVerificationError(
        formData.verificationMethod === 'email'
          ? 'Не удалось отправить код подтверждения на email. Попробуйте позже.'
          : 'Не удалось отправить SMS с кодом. Попробуйте позже.'
      );
    }
  }, [formData, validateForm, sendEmailVerification]);

  /**
   * Обработчик проверки кода подтверждения
   */
  const handleVerifyCode = useCallback(async () => {
    try {
      setVerificationError(null);

      // Проверяем, что код введен
      if (!formData.verificationCode || formData.verificationCode.length !== 6) {
        setVerificationError('Введите 6-значный код подтверждения');
        return;
      }

      // Для email: проверяем код и регистрируем
      if (formData.verificationMethod === 'email') {
        // Сначала проверяем код
        await verifyEmailCode(formData.email.trim(), formData.verificationCode);

        // Подготовка данных для регистрации
        const registerData: RegisterData = {
          username: formData.username.trim(),
          email: formData.email.trim(),
          phone: formData.phone.trim(),
          password: formData.password,
          confirmPassword: formData.confirmPassword,
          firstName: formData.firstName.trim() || undefined,
          lastName: formData.lastName.trim() || undefined,
          middleName: formData.middleName.trim() || undefined
        };

        // Регистрируем пользователя с подтверждением email
        await registerWithEmailVerification(registerData, formData.verificationCode);
      } else {
        // Для phone: используем существующую логику
        // TODO: Реализовать проверку SMS кода и регистрацию
        console.log('Проверка SMS кода:', formData.verificationCode);
      }

      // При успехе: вызываем callback
      if (onSuccess) {
        onSuccess();
      }
    } catch (error) {
      // Ошибка уже обработана в useAuth hook (показан Toast)
      // Дополнительно показываем Callout в форме
      setVerificationError('Неверный код подтверждения. Проверьте код и попробуйте снова.');
    }
  }, [formData, verifyEmailCode, registerWithEmailVerification, onSuccess]);

  /**
   * Обработчик повторной отправки кода
   */
  const handleResendCode = useCallback(async () => {
    try {
      setVerificationError(null);

      // Для email: отправляем код на email
      if (formData.verificationMethod === 'email') {
        await sendEmailVerification(formData.email.trim());
      } else {
        // Для phone: используем существующую логику SMS
        // TODO: Реализовать отправку SMS кода
        console.log('Повторная отправка SMS кода на:', formData.phone);
      }

      // Показываем уведомление об успешной отправке
      // Toast уже показан в useAuth hook
    } catch (error) {
      // Ошибка уже обработана в useAuth hook (показан Toast)
      setVerificationError(
        formData.verificationMethod === 'email'
          ? 'Не удалось отправить код повторно. Попробуйте позже.'
          : 'Не удалось отправить SMS повторно. Попробуйте позже.'
      );
    }
  }, [formData, sendEmailVerification]);

  /**
   * Обработчик отправки формы
   */
  const handleSubmit = useCallback(async (event: React.FormEvent) => {
    event.preventDefault();

    // Если мы на этапе ввода данных - отправляем код
    if (formData.registrationStep === 'input') {
      await handleSendVerificationCode();
      return;
    }

    // Если мы на этапе проверки кода - проверяем код и регистрируем
    if (formData.registrationStep === 'verification') {
      await handleVerifyCode();
      return;
    }
  }, [formData.registrationStep, handleSendVerificationCode, handleVerifyCode]);

  /**
   * Проверка валидности формы для активации кнопки
   */
  const isFormValid = formData.registrationStep === 'input'
    ? // На этапе ввода данных проверяем основные поля
      formData.username.trim().length >= 3 && 
      ((formData.verificationMethod === 'email' && formData.email.trim().length > 0) ||
       (formData.verificationMethod === 'phone' && formData.phone.trim().length > 0)) &&
      formData.password.length >= 8 && 
      formData.confirmPassword.length >= 8 &&
      formData.password === formData.confirmPassword &&
      Object.keys(validationErrors).length === 0
    : // На этапе verification проверяем наличие кода
      formData.verificationCode.length === 6;

  return (
    <form onSubmit={handleSubmit} noValidate>
      {/* Отображение общей ошибки */}
      {error && (
        <Callout
          intent={Intent.DANGER}
          title="Ошибка регистрации"
          style={{ marginBottom: '16px' }}
        >
          {error}
        </Callout>
      )}

      {/* Отображение ошибки отправки кода */}
      {verificationError && (
        <Callout
          intent={Intent.DANGER}
          title="Ошибка отправки кода"
          style={{ marginBottom: '16px' }}
        >
          {verificationError}
        </Callout>
      )}

      {/* Выбор способа подтверждения */}
      <RadioGroup
        label="Способ подтверждения"
        onChange={handleVerificationMethodChange}
        selectedValue={formData.verificationMethod}
        inline
        style={{ marginBottom: '16px' }}
      >
        <Radio label="Подтверждение по email" value="email" />
        <Radio label="Подтверждение по телефону" value="phone" />
      </RadioGroup>

      {/* Поле логина */}
      <FormGroup
        label="Логин *"
        labelFor="username"
        intent={validationErrors.username ? Intent.DANGER : Intent.NONE}
        helperText={validationErrors.username}
      >
        <InputGroup
          id="username"
          type="text"
          placeholder="Введите логин"
          value={formData.username}
          onChange={handleInputChange('username')}
          intent={validationErrors.username ? Intent.DANGER : Intent.NONE}
          disabled={isLoading}
          autoComplete="username"
        />
      </FormGroup>

      {/* Поле email */}
      {formData.verificationMethod === 'email' && (
        <FormGroup
          label="Email *"
          labelFor="email"
          intent={validationErrors.email ? Intent.DANGER : Intent.NONE}
          helperText={validationErrors.email}
        >
          <InputGroup
            id="email"
            type="email"
            placeholder="Введите email адрес"
            value={formData.email}
            onChange={handleInputChange('email')}
            intent={validationErrors.email ? Intent.DANGER : Intent.NONE}
            disabled={isLoading}
            autoComplete="email"
          />
        </FormGroup>
      )}

      {/* Поле телефона */}
      {formData.verificationMethod === 'phone' && (
        <FormGroup
          label="Телефон *"
          labelFor="phone"
          intent={validationErrors.phone ? Intent.DANGER : Intent.NONE}
          helperText={validationErrors.phone}
        >
          <InputGroup
            id="phone"
            type="tel"
            placeholder="Введите номер телефона (+7XXXXXXXXXX)"
            value={formData.phone}
            onChange={handleInputChange('phone')}
            intent={validationErrors.phone ? Intent.DANGER : Intent.NONE}
            disabled={isLoading}
            autoComplete="tel"
          />
        </FormGroup>
      )}

      {/* Поле пароля */}
      <FormGroup
        label="Пароль *"
        labelFor="password"
        intent={validationErrors.password ? Intent.DANGER : Intent.NONE}
        helperText={validationErrors.password}
      >
        <InputGroup
          id="password"
          type="password"
          placeholder="Введите пароль (минимум 8 символов)"
          value={formData.password}
          onChange={handleInputChange('password')}
          intent={validationErrors.password ? Intent.DANGER : Intent.NONE}
          disabled={isLoading}
          autoComplete="new-password"
        />
      </FormGroup>

      {/* Поле подтверждения пароля */}
      <FormGroup
        label="Подтверждение пароля *"
        labelFor="confirmPassword"
        intent={validationErrors.confirmPassword ? Intent.DANGER : Intent.NONE}
        helperText={validationErrors.confirmPassword}
      >
        <InputGroup
          id="confirmPassword"
          type="password"
          placeholder="Повторите пароль"
          value={formData.confirmPassword}
          onChange={handleInputChange('confirmPassword')}
          intent={validationErrors.confirmPassword ? Intent.DANGER : Intent.NONE}
          disabled={isLoading}
          autoComplete="new-password"
        />
      </FormGroup>

      {/* Поле имени */}
      <FormGroup
        label="Имя"
        labelFor="firstName"
        intent={validationErrors.firstName ? Intent.DANGER : Intent.NONE}
        helperText={validationErrors.firstName}
      >
        <InputGroup
          id="firstName"
          type="text"
          placeholder="Введите имя (опционально)"
          value={formData.firstName}
          onChange={handleInputChange('firstName')}
          intent={validationErrors.firstName ? Intent.DANGER : Intent.NONE}
          disabled={isLoading}
          autoComplete="given-name"
        />
      </FormGroup>

      {/* Поле фамилии */}
      <FormGroup
        label="Фамилия"
        labelFor="lastName"
        intent={validationErrors.lastName ? Intent.DANGER : Intent.NONE}
        helperText={validationErrors.lastName}
      >
        <InputGroup
          id="lastName"
          type="text"
          placeholder="Введите фамилию (опционально)"
          value={formData.lastName}
          onChange={handleInputChange('lastName')}
          intent={validationErrors.lastName ? Intent.DANGER : Intent.NONE}
          disabled={isLoading}
          autoComplete="family-name"
        />
      </FormGroup>

      {/* Поле отчества */}
      <FormGroup
        label="Отчество"
        labelFor="middleName"
        intent={validationErrors.middleName ? Intent.DANGER : Intent.NONE}
        helperText={validationErrors.middleName}
      >
        <InputGroup
          id="middleName"
          type="text"
          placeholder="Введите отчество (опционально)"
          value={formData.middleName}
          onChange={handleInputChange('middleName')}
          intent={validationErrors.middleName ? Intent.DANGER : Intent.NONE}
          disabled={isLoading}
          autoComplete="additional-name"
        />
      </FormGroup>

      {/* Поле кода подтверждения (показывается после отправки кода) */}
      {formData.registrationStep === 'verification' && (
        <Callout
          intent={Intent.PRIMARY}
          title="Код подтверждения отправлен"
          style={{ marginBottom: '16px' }}
        >
          {formData.verificationMethod === 'email'
            ? `Код подтверждения отправлен на ${formData.email}. Проверьте почту и введите код ниже.`
            : `Код подтверждения отправлен на ${formData.phone}. Проверьте SMS и введите код ниже.`}
        </Callout>
      )}

      {formData.registrationStep === 'verification' && (
        <FormGroup
          label="Код подтверждения *"
          labelFor="verificationCode"
          helperText="Введите 6-значный код из письма или SMS"
        >
          <InputGroup
            id="verificationCode"
            type="text"
            placeholder="Введите код подтверждения"
            value={formData.verificationCode}
            onChange={handleInputChange('verificationCode')}
            disabled={isLoading}
            maxLength={6}
            autoComplete="one-time-code"
          />
        </FormGroup>
      )}

      {/* Кнопка повторной отправки кода */}
      {formData.registrationStep === 'verification' && (
        <Button
          onClick={handleResendCode}
          intent={Intent.NONE}
          minimal
          disabled={isLoading}
          style={{ marginBottom: '16px' }}
        >
          Отправить код повторно
        </Button>
      )}

      {/* Кнопка отправки */}
      <Button
        type="submit"
        intent={Intent.PRIMARY}
        loading={isLoading}
        disabled={!isFormValid || isLoading}
        fill
        large
      >
        {isLoading 
          ? 'Отправка...' 
          : formData.registrationStep === 'input' 
            ? 'Отправить код подтверждения' 
            : 'Зарегистрироваться'}
      </Button>
    </form>
  );
};