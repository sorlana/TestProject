/**
 * Страница личного кабинета пользователя
 */

import React from 'react';
import { Card, H2, Text } from '@blueprintjs/core';
import { Header } from '../components/layout/Header';
import { useAuthStore } from '../../application/store/authStore';
import './DashboardPage.css';

/**
 * Компонент DashboardPage
 * 
 * Отображает личный кабинет пользователя с приветственным сообщением.
 * Включает Header компонент и основную область для будущего функционала.
 * 
 * Требования: 5.1
 */
export const DashboardPage: React.FC = () => {
  const user = useAuthStore(state => state.user);

  /**
   * Получить приветственное сообщение с именем пользователя
   */
  const getWelcomeMessage = (): string => {
    if (!user) return 'Добро пожаловать!';
    
    // Приоритет: firstName, затем userName
    const displayName = user.firstName || user.userName;
    return `Добро пожаловать, ${displayName}!`;
  };

  return (
    <div className="dashboard-page">
      <Header />
      
      <div className="dashboard-content" style={{ padding: '20px' }}>
        <Card className="dashboard-card" elevation={1} style={{ maxWidth: '800px', margin: '0 auto' }}>
          <div className="dashboard-welcome">
            <H2>{getWelcomeMessage()}</H2>
          </div>
          <Text className="dashboard-description">
            Это ваш личный кабинет. Здесь будет размещен основной функционал платформы.
          </Text>
          
          {/* Основная область - заглушка для будущего функционала */}
          <div 
            className="dashboard-main-area" 
            style={{ 
              marginTop: '30px', 
              padding: '40px', 
              backgroundColor: '#f5f8fa', 
              borderRadius: '6px',
              textAlign: 'center'
            }}
          >
            <Text style={{ color: '#5c7080' }}>
              Основной функционал будет добавлен в следующих версиях
            </Text>
          </div>
        </Card>
      </div>
    </div>
  );
};