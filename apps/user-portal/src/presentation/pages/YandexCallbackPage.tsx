/**
 * Страница обработки callback от Yandex OAuth
 */

import React, { useEffect, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { Spinner, Callout, Intent } from '@blueprintjs/core';
import { useAuth } from '../../application/hooks/useAuth';

/**
 * Страница обработки callback от Yandex OAuth
 * 
 * Обрабатывает authorization code от Yandex и выполняет авторизацию пользователя.
 * При успехе перенаправляет на dashboard, при ошибке показывает сообщение.
 */
export const YandexCallbackPage: React.FC = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const { loginWithYandex } = useAuth();
  
  const [status, setStatus] = useState<'loading' | 'success' | 'error'>('loading');
  const [errorMessage, setErrorMessage] = useState<string>('');

  useEffect(() => {
    const handleYandexCallback = async () => {
      try {
        // Получаем authorization code из URL параметров
        const code = searchParams.get('code');
        const error = searchParams.get('error');
        const errorDescription = searchParams.get('error_description');

        // Проверяем на ошибки от Yandex
        if (error) {
          console.error('❌ Ошибка от Yandex OAuth:', error, errorDescription);
          setErrorMessage(errorDescription || `Ошибка авторизации: ${error}`);
          setStatus('error');
          return;
        }

        // Проверяем наличие authorization code
        if (!code) {
          console.error('❌ Отсутствует authorization code в URL');
          setErrorMessage('Отсутствует код авторизации от Yandex');
          setStatus('error');
          return;
        }

        console.log('🔄 Обрабатываем Yandex authorization code:', code);

        // Выполняем авторизацию через наш сервис
        await loginWithYandex(code);

        console.log('✅ Успешная авторизация через Yandex');
        setStatus('success');

        // Перенаправляем на dashboard через 1 секунду
        setTimeout(() => {
          navigate('/app/dashboard', { replace: true });
        }, 1000);

      } catch (error) {
        console.error('❌ Ошибка при обработке Yandex callback:', error);
        setErrorMessage('Ошибка при авторизации через Yandex');
        setStatus('error');
      }
    };

    handleYandexCallback();
  }, [searchParams, loginWithYandex, navigate]);

  /**
   * Обработчик возврата на страницу входа
   */
  const handleBackToLogin = () => {
    navigate('/auth/login', { replace: true });
  };

  return (
    <div style={{ 
      display: 'flex', 
      justifyContent: 'center', 
      alignItems: 'center', 
      minHeight: '100vh',
      padding: '20px'
    }}>
      <div style={{ maxWidth: '400px', width: '100%' }}>
        {status === 'loading' && (
          <div style={{ textAlign: 'center' }}>
            <Spinner size={50} />
            <h3 style={{ marginTop: '20px' }}>Авторизация через Yandex...</h3>
            <p>Пожалуйста, подождите</p>
          </div>
        )}

        {status === 'success' && (
          <Callout intent={Intent.SUCCESS}>
            <h4>✅ Успешная авторизация!</h4>
            <p>Перенаправляем вас в личный кабинет...</p>
          </Callout>
        )}

        {status === 'error' && (
          <div>
            <Callout intent={Intent.DANGER} style={{ marginBottom: '20px' }}>
              <h4>❌ Ошибка авторизации</h4>
              <p>{errorMessage}</p>
            </Callout>
            
            <button 
              onClick={handleBackToLogin}
              style={{
                width: '100%',
                padding: '12px',
                backgroundColor: '#137cbd',
                color: 'white',
                border: 'none',
                borderRadius: '4px',
                cursor: 'pointer',
                fontSize: '14px'
              }}
            >
              Вернуться к входу
            </button>
          </div>
        )}
      </div>
    </div>
  );
};

export default YandexCallbackPage;