/**
 * Компонент кнопки авторизации через Yandex ID
 */

import React, { useEffect, useCallback, useState } from 'react';
import { Button, Intent, Callout } from '@blueprintjs/core';
import { useAuth } from '../../../application/hooks/useAuth';

/**
 * Интерфейс пропсов компонента YandexAuthButton
 */
interface YandexAuthButtonProps {
  /** Callback, вызываемый при успешной авторизации через Yandex */
  onSuccess?: () => void;
  /** Текст кнопки (по умолчанию "Войти через Yandex") */
  buttonText?: string;
  /** Размер кнопки */
  large?: boolean;
  /** Заполнить всю ширину контейнера */
  fill?: boolean;
}

/**
 * Интерфейс Yandex ID SDK
 */
declare global {
  interface Window {
    YaAuthSuggest?: {
      init: (config: YandexIdConfiguration) => Promise<void>;
    };
  }
}

/**
 * Конфигурация Yandex ID
 */
interface YandexIdConfiguration {
  client_id: string;
  response_type: string;
  redirect_uri: string;
}

/**
 * Yandex Client ID (в реальном проекте должен быть в переменных окружения)
 */
const YANDEX_CLIENT_ID = import.meta.env.VITE_YANDEX_CLIENT_ID || 'your_yandex_client_id';

/**
 * Проверка, является ли Client ID placeholder'ом
 */
const isPlaceholderClientId = (clientId: string): boolean => {
  return clientId === 'your_yandex_client_id' || 
         clientId.includes('your_yandex_client_id');
};

/**
 * Компонент кнопки авторизации через Yandex
 */
export const YandexAuthButton: React.FC<YandexAuthButtonProps> = ({
  onSuccess,
  buttonText = 'Войти через Yandex',
  large = false,
  fill = false
}) => {
  // Состояние загрузки Yandex SDK
  const [isYandexLoaded, setIsYandexLoaded] = useState(false);
  const [yandexError, setYandexError] = useState<string | null>(null);

  // Хук аутентификации
  const { loginWithYandex, isLoading } = useAuth();

  /**
   * Загрузка Yandex ID SDK
   */
  const loadYandexScript = useCallback(() => {
    console.log('🔄 Начинаем загрузку Yandex ID SDK...');
    
    // Проверяем, не загружена ли уже библиотека
    if (window.YaAuthSuggest) {
      console.log('✅ Yandex ID SDK уже загружен');
      setIsYandexLoaded(true);
      return;
    }

    // Проверяем, не загружается ли уже скрипт
    if (document.querySelector('script[src*="yastatic.net"]')) {
      console.log('⏳ Yandex ID SDK уже загружается...');
      return;
    }

    console.log('📥 Загружаем скрипт Yandex ID SDK...');
    const script = document.createElement('script');
    script.src = 'https://yastatic.net/s3/passport-sdk/autofill/v1/sdk-suggest-with-polyfills-latest.js';
    script.async = true;
    script.defer = true;
    
    script.onload = () => {
      console.log('✅ Yandex ID SDK успешно загружен');
      setIsYandexLoaded(true);
      setYandexError(null);
    };
    
    script.onerror = () => {
      console.error('❌ Не удалось загрузить Yandex ID SDK');
      setYandexError('Не удалось загрузить Yandex ID SDK');
    };

    document.head.appendChild(script);
  }, []);

  /**
   * Обработчик клика по кнопке Yandex
   */
  const handleYandexLogin = useCallback(() => {
    console.log('🖱️ Клик по кнопке Yandex авторизации');
    
    // Проверяем, не является ли Client ID placeholder'ом
    if (isPlaceholderClientId(YANDEX_CLIENT_ID)) {
      console.error('❌ Yandex Client ID не настроен (используется placeholder)');
      setYandexError('Yandex авторизация не настроена. Обратитесь к администратору для настройки Yandex Client ID.');
      return;
    }

    console.log('🚀 Запускаем Yandex авторизацию...');
    try {
      // Простая авторизация через redirect
      const redirectUri = `${window.location.origin}/auth/yandex/callback`;
      const authUrl = `https://oauth.yandex.ru/authorize?response_type=code&client_id=${YANDEX_CLIENT_ID}&redirect_uri=${encodeURIComponent(redirectUri)}&scope=login:email+login:info`;
      
      console.log('🔗 Перенаправляем на Yandex OAuth:', authUrl);
      window.location.href = authUrl;
    } catch (error) {
      console.error('❌ Ошибка запуска Yandex авторизации:', error);
      setYandexError('Ошибка запуска Yandex авторизации');
    }
  }, []);

  // Загружаем Yandex SDK при монтировании компонента
  useEffect(() => {
    // Логируем переменные окружения для отладки
    console.log('🔧 Переменные окружения Yandex:');
    console.log('VITE_YANDEX_CLIENT_ID:', import.meta.env.VITE_YANDEX_CLIENT_ID);
    
    loadYandexScript();
  }, [loadYandexScript]);

  return (
    <div>
      {/* Отображение ошибки Yandex */}
      {yandexError && (
        <Callout intent={Intent.DANGER} style={{ marginBottom: '16px' }}>
          {yandexError}
        </Callout>
      )}

      {/* Кнопка Yandex авторизации */}
      <Button
        intent={Intent.NONE}
        large={large}
        fill={fill}
        loading={isLoading}
        disabled={isLoading || !!yandexError}
        onClick={handleYandexLogin}
        style={{
          backgroundColor: '#ffcc00',
          color: '#000',
          border: '1px solid #ffcc00',
          fontWeight: 500
        }}
      >
        <span style={{ marginRight: '8px' }}>🟡</span>
        {buttonText}
      </Button>
    </div>
  );
};

export default YandexAuthButton;