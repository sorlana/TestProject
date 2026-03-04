/**
 * Компонент кнопки авторизации через Google
 */

import React, { useEffect, useCallback, useState } from 'react';
import { Button, Intent, Callout } from '@blueprintjs/core';
import { useAuth } from '../../../application/hooks/useAuth';

/**
 * Интерфейс пропсов компонента GoogleAuthButton
 */
interface GoogleAuthButtonProps {
  /** Callback, вызываемый при успешной авторизации через Google */
  onSuccess?: () => void;
  /** Текст кнопки (по умолчанию "Войти через Google") */
  buttonText?: string;
  /** Размер кнопки */
  large?: boolean;
  /** Заполнить всю ширину контейнера */
  fill?: boolean;
}

/**
 * Интерфейс Google Identity Services
 */
declare global {
  interface Window {
    google?: {
      accounts: {
        id: {
          initialize: (config: GoogleIdConfiguration) => void;
          renderButton: (element: HTMLElement, config: GoogleButtonConfiguration) => void;
          prompt: () => void;
        };
      };
    };
  }
}

/**
 * Конфигурация Google Identity Services
 */
interface GoogleIdConfiguration {
  client_id: string;
  callback: (response: GoogleCredentialResponse) => void;
  auto_select?: boolean;
  cancel_on_tap_outside?: boolean;
}

/**
 * Конфигурация кнопки Google
 */
interface GoogleButtonConfiguration {
  theme?: 'outline' | 'filled_blue' | 'filled_black';
  size?: 'large' | 'medium' | 'small';
  text?: 'signin_with' | 'signup_with' | 'continue_with' | 'signin';
  shape?: 'rectangular' | 'pill' | 'circle' | 'square';
  logo_alignment?: 'left' | 'center';
  width?: string;
  locale?: string;
}

/**
 * Ответ от Google с учетными данными
 */
interface GoogleCredentialResponse {
  credential: string;
  select_by?: string;
}

/**
 * Google Client ID (в реальном проекте должен быть в переменных окружения)
 */
const GOOGLE_CLIENT_ID = import.meta.env.VITE_GOOGLE_CLIENT_ID || 'your-google-client-id.apps.googleusercontent.com';

/**
 * Проверка, является ли Client ID placeholder'ом
 */
const isPlaceholderClientId = (clientId: string): boolean => {
  return clientId === 'your-google-client-id.apps.googleusercontent.com' || 
         clientId === 'your_google_client_id' ||
         clientId.includes('your-google-client-id') ||
         clientId.includes('your_google_client_id');
};

/**
 * Компонент кнопки авторизации через Google
 * 
 * Интегрируется с Google Identity Services (GIS) для выполнения OAuth авторизации.
 * Автоматически загружает Google GIS библиотеку и инициализирует авторизацию.
 * При успешной авторизации получает ID токен и отправляет его на сервер.
 */
export const GoogleAuthButton: React.FC<GoogleAuthButtonProps> = ({
  onSuccess,
  buttonText = 'Войти через Google',
  large = false,
  fill = false
}) => {
  // Состояние загрузки Google GIS
  const [isGoogleLoaded, setIsGoogleLoaded] = useState(false);
  const [googleError, setGoogleError] = useState<string | null>(null);

  // Хук аутентификации
  const { loginWithGoogle, isLoading } = useAuth();

  /**
   * Обработчик ответа от Google
   */
  const handleGoogleResponse = useCallback(async (response: GoogleCredentialResponse) => {
    try {
      setGoogleError(null);
      
      // Отправляем ID токен на сервер
      await loginWithGoogle(response.credential);
      
      // Вызываем callback при успешной авторизации
      if (onSuccess) {
        onSuccess();
      }
    } catch (error) {
      console.error('Ошибка авторизации через Google:', error);
      setGoogleError('Ошибка авторизации через Google');
    }
  }, [loginWithGoogle, onSuccess]);

  /**
   * Загрузка Google Identity Services библиотеки
   */
  const loadGoogleScript = useCallback(() => {
    console.log('🔄 Начинаем загрузку Google Identity Services...');
    
    // Проверяем, не загружена ли уже библиотека
    if (window.google?.accounts?.id) {
      console.log('✅ Google Identity Services уже загружен');
      setIsGoogleLoaded(true);
      return;
    }

    // Проверяем, не загружается ли уже скрипт
    if (document.querySelector('script[src*="accounts.google.com"]')) {
      console.log('⏳ Google Identity Services уже загружается...');
      return;
    }

    console.log('📥 Загружаем скрипт Google Identity Services...');
    const script = document.createElement('script');
    script.src = 'https://accounts.google.com/gsi/client';
    script.async = true;
    script.defer = true;
    
    script.onload = () => {
      console.log('✅ Google Identity Services успешно загружен');
      setIsGoogleLoaded(true);
      setGoogleError(null);
    };
    
    script.onerror = () => {
      console.error('❌ Не удалось загрузить Google Identity Services');
      setGoogleError('Не удалось загрузить Google Identity Services');
    };

    document.head.appendChild(script);
  }, []);

  /**
   * Инициализация Google Identity Services
   */
  const initializeGoogle = useCallback(() => {
    if (!window.google?.accounts?.id || !isGoogleLoaded) {
      console.log('⏳ Ожидаем загрузки Google Identity Services...');
      return;
    }

    console.log('🔧 Инициализируем Google Identity Services...');
    console.log('Client ID:', GOOGLE_CLIENT_ID);

    // Проверяем, не является ли Client ID placeholder'ом
    if (isPlaceholderClientId(GOOGLE_CLIENT_ID)) {
      console.error('❌ Google Client ID не настроен (используется placeholder)');
      setGoogleError('Google авторизация не настроена. Обратитесь к администратору для настройки Google Client ID.');
      return;
    }

    try {
      window.google.accounts.id.initialize({
        client_id: GOOGLE_CLIENT_ID,
        callback: handleGoogleResponse,
        auto_select: false,
        cancel_on_tap_outside: true
      });
      console.log('✅ Google Identity Services успешно инициализирован');
    } catch (error) {
      console.error('❌ Ошибка инициализации Google Identity Services:', error);
      setGoogleError('Ошибка инициализации Google авторизации');
    }
  }, [isGoogleLoaded, handleGoogleResponse]);

  /**
   * Обработчик клика по кнопке
   */
  const handleGoogleLogin = useCallback(() => {
    console.log('🖱️ Клик по кнопке Google авторизации');
    
    // Проверяем, не является ли Client ID placeholder'ом
    if (isPlaceholderClientId(GOOGLE_CLIENT_ID)) {
      console.error('❌ Google Client ID не настроен (используется placeholder)');
      setGoogleError('Google авторизация не настроена. Обратитесь к администратору для настройки Google Client ID.');
      return;
    }
    
    if (!window.google?.accounts?.id) {
      console.error('❌ Google Identity Services не загружен');
      setGoogleError('Google Identity Services не загружен');
      return;
    }

    console.log('🚀 Запускаем Google авторизацию...');
    try {
      // Простой вызов prompt - самый надежный способ
      window.google.accounts.id.prompt();
    } catch (error) {
      console.error('❌ Ошибка запуска Google авторизации:', error);
      setGoogleError('Ошибка запуска Google авторизации');
    }
  }, []);

  // Загружаем Google GIS при монтировании компонента
  useEffect(() => {
    // Логируем переменные окружения для отладки
    console.log('🔧 Переменные окружения:');
    console.log('VITE_GOOGLE_CLIENT_ID:', import.meta.env.VITE_GOOGLE_CLIENT_ID);
    console.log('VITE_API_BASE_URL:', import.meta.env.VITE_API_BASE_URL);
    console.log('VITE_NODE_ENV:', import.meta.env.VITE_NODE_ENV);
    
    // eslint-disable-next-line react-hooks/set-state-in-effect
    loadGoogleScript();
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

  // Инициализируем Google GIS когда библиотека загружена
  useEffect(() => {
    if (isGoogleLoaded) {
      // eslint-disable-next-line react-hooks/set-state-in-effect
      initializeGoogle();
    }
  }, [isGoogleLoaded]); // eslint-disable-line react-hooks/exhaustive-deps

  return (
    <div>
      {/* Отображение ошибки Google */}
      {googleError && (
        <Callout
          intent={Intent.WARNING}
          title="Проблема с Google авторизацией"
          style={{ marginBottom: '16px' }}
        >
          {googleError}
          {isPlaceholderClientId(GOOGLE_CLIENT_ID) && (
            <div style={{ marginTop: '8px', fontSize: '14px' }}>
              <strong>Инструкция по настройке:</strong>
              <ol style={{ marginTop: '4px', paddingLeft: '20px' }}>
                <li>Перейдите в <a href="https://console.cloud.google.com/" target="_blank" rel="noopener noreferrer">Google Cloud Console</a></li>
                <li>Создайте OAuth Client ID для веб-приложения</li>
                <li>Добавьте <code>http://localhost:5173</code> в authorized redirect URIs</li>
                <li>Скопируйте Client ID в файл <code>.env</code></li>
              </ol>
            </div>
          )}
        </Callout>
      )}

      {/* Кнопка авторизации через Google */}
      <Button
        intent={Intent.NONE}
        loading={isLoading}
        disabled={!isGoogleLoaded || isLoading || isPlaceholderClientId(GOOGLE_CLIENT_ID)}
        onClick={handleGoogleLogin}
        fill={fill}
        large={large}
        style={{
          backgroundColor: isPlaceholderClientId(GOOGLE_CLIENT_ID) ? '#ccc' : '#4285f4',
          color: 'white',
          border: 'none'
        }}
      >
        {isLoading ? 'Авторизация...' : buttonText}
      </Button>

      {/* Информация о статусе загрузки */}
      {!isGoogleLoaded && !googleError && (
        <div style={{ fontSize: '12px', color: '#666', marginTop: '8px' }}>
          Загрузка Google авторизации...
        </div>
      )}
    </div>
  );
};