/**
 * Компонент Header - навигационная панель приложения
 */

import React from 'react';
import {
  Navbar,
  NavbarGroup,
  NavbarHeading,
  Button,
  Alignment,
  NavbarDivider,
  Text,
  Classes
} from '@blueprintjs/core';
import { useAuth } from '../../../application/hooks/useAuth';
import './Header.css';

/**
 * Компонент Header
 * 
 * Отображает навигационную панель с логотипом, информацией о пользователе
 * и кнопкой выхода. Адаптируется под различные размеры экрана.
 * 
 * Требования: 5.2, 5.3, 5.4, 5.5
 */
export const Header: React.FC = () => {
  const { user, logout } = useAuth();

  /**
   * Обработчик выхода из системы
   */
  const handleLogout = async (): Promise<void> => {
    try {
      await logout();
    } catch (error) {
      console.error('Ошибка при выходе из системы:', error);
    }
  };

  /**
   * Получить отображаемое имя пользователя
   */
  const getUserDisplayName = (): string => {
    if (!user) return '';
    
    // Приоритет: firstName lastName, затем userName
    if (user.firstName || user.lastName) {
      return [user.firstName, user.lastName].filter(Boolean).join(' ');
    }
    
    return user.userName;
  };

  return (
    <Navbar className="header-navbar">
      <NavbarGroup align={Alignment.LEFT}>
        <NavbarHeading className="header-logo">
          User Portal
        </NavbarHeading>
      </NavbarGroup>
      
      <NavbarGroup align={Alignment.RIGHT} className="header-user-section">
        {user && (
          <>
            <Text className="header-username" ellipsize>
              {getUserDisplayName()}
            </Text>
            <NavbarDivider />
            <Button
              className={Classes.MINIMAL}
              icon="log-out"
              text="Выйти"
              onClick={handleLogout}
            />
          </>
        )}
      </NavbarGroup>
    </Navbar>
  );
};