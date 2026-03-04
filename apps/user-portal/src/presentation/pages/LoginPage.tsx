/**
 * Страница авторизации и регистрации
 */

import React, { useState, useCallback, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Card,
  Tabs,
  Tab,
  Divider,
  H3
} from '@blueprintjs/core';
import { LoginForm } from '../components/auth/LoginForm';
import { RegisterForm } from '../components/auth/RegisterForm';
import { GoogleAuthButton } from '../components/auth/GoogleAuthButton';
import { YandexAuthButton } from '../components/auth/YandexAuthButton';
import { useAuth } from '../../application/hooks/useAuth';
import './LoginPage.css';

/**
 * Типы табов на странице авторизации
 */
type AuthTabId = 'login' | 'register';

/**
 * Страница авторизации и регистрации
 * 
 * Предоставляет интерфейс для входа в систему и регистрации новых пользователей.
 * Использует Blueprint Tabs для переключения между формами входа и регистрации.
 * Включает возможность авторизации через Google на обоих табах.
 * При успешной авторизации выполняет редирект на dashboard.
 */
export const LoginPage: React.FC = () => {
  // Состояние активного таба
  const [activeTab, setActiveTab] = useState<AuthTabId>('login');

  // Хуки для навигации и аутентификации
  const navigate = useNavigate();
  const { isAuthenticated } = useAuth();

  /**
   * Редирект на dashboard если пользователь уже авторизован
   */
  useEffect(() => {
    if (isAuthenticated) {
      navigate('/app/dashboard', { replace: true });
    }
  }, [isAuthenticated, navigate]);

  /**
   * Обработчик успешной авторизации/регистрации
   */
  const handleAuthSuccess = useCallback(() => {
    // Редирект на dashboard
    navigate('/app/dashboard', { replace: true });
  }, [navigate]);

  /**
   * Обработчик изменения активного таба
   */
  const handleTabChange = useCallback((newTabId: AuthTabId) => {
    setActiveTab(newTabId);
  }, []);

  return (
    <div className="login-page" style={{
      minHeight: '100vh',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      backgroundColor: '#f5f8fa',
      padding: '20px'
    }}>
      <Card
        className="login-card"
        style={{
          width: '100%',
          maxWidth: '480px',
          padding: '32px'
        }}
        elevation={2}
      >
        {/* Заголовок страницы */}
        <div className="login-title" style={{ textAlign: 'center', marginBottom: '24px' }}>
          <H3 style={{ margin: 0, color: '#394b59' }}>
            Добро пожаловать в User Portal
          </H3>
        </div>

        {/* Табы для переключения между входом и регистрацией */}
        <Tabs
          id="auth-tabs"
          selectedTabId={activeTab}
          onChange={handleTabChange}
          large
        >
          {/* Таб входа */}
          <Tab
            id="login"
            title="Вход"
            panel={
              <div style={{ paddingTop: '20px' }}>
                {/* Форма входа */}
                <LoginForm onSuccess={handleAuthSuccess} />
                
                {/* Разделитель */}
                <Divider style={{ margin: '24px 0' }} />
                
                {/* Кнопки социальных сетей */}
                <div style={{ display: 'flex', gap: '12px', flexDirection: 'column' }}>
                  {/* Кнопка Google авторизации */}
                  <GoogleAuthButton
                    onSuccess={handleAuthSuccess}
                    buttonText="Войти через Google"
                    fill
                    large
                  />
                  
                  {/* Кнопка Yandex авторизации */}
                  <YandexAuthButton
                    onSuccess={handleAuthSuccess}
                    buttonText="Войти через Yandex"
                    fill
                    large
                  />
                </div>
              </div>
            }
          />

          {/* Таб регистрации */}
          <Tab
            id="register"
            title="Регистрация"
            panel={
              <div style={{ paddingTop: '20px' }}>
                {/* Форма регистрации */}
                <RegisterForm onSuccess={handleAuthSuccess} />
              </div>
            }
          />
        </Tabs>

        {/* Дополнительная информация */}
        <div className="auth-switch" style={{
          textAlign: 'center',
          marginTop: '24px',
          fontSize: '14px',
          color: '#5c7080'
        }}>
          {activeTab === 'login' ? (
            <span>
              Нет аккаунта?{' '}
              <button
                type="button"
                onClick={() => handleTabChange('register')}
                style={{
                  background: 'none',
                  border: 'none',
                  color: '#137cbd',
                  cursor: 'pointer',
                  textDecoration: 'underline'
                }}
              >
                Зарегистрируйтесь
              </button>
            </span>
          ) : (
            <span>
              Уже есть аккаунт?{' '}
              <button
                type="button"
                onClick={() => handleTabChange('login')}
                style={{
                  background: 'none',
                  border: 'none',
                  color: '#137cbd',
                  cursor: 'pointer',
                  textDecoration: 'underline'
                }}
              >
                Войдите
              </button>
            </span>
          )}
        </div>
      </Card>
    </div>
  );
};