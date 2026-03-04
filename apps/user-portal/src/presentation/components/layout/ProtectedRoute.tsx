/**
 * Компонент ProtectedRoute - защищенный маршрут
 */

import React, { useEffect } from 'react';
import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuthStore } from '../../../application/store/authStore';
import { tokenService } from '../../../infrastructure/services/tokenService';

/**
 * Компонент ProtectedRoute
 * 
 * Защищает маршруты от неавторизованных пользователей.
 * Проверяет наличие JWT токена и состояние аутентификации.
 * Выполняет редирект на /app/login если пользователь не авторизован.
 * 
 * Требования: 6.1, 6.2, 6.3, 6.4
 */
export const ProtectedRoute: React.FC = () => {
  const location = useLocation();
  const { isAuthenticated, setAuthenticated } = useAuthStore();
  
  // Проверяем наличие токена в хранилище
  const hasValidToken = tokenService.getAccessToken() !== null;

  useEffect(() => {
    // Синхронизируем состояние аутентификации с наличием токена
    if (hasValidToken && !isAuthenticated) {
      setAuthenticated(true);
    } else if (!hasValidToken && isAuthenticated) {
      setAuthenticated(false);
    }
  }, [hasValidToken, isAuthenticated, setAuthenticated]);

  // Если нет токена или пользователь не авторизован - редирект на логин
  if (!hasValidToken || !isAuthenticated) {
    return (
      <Navigate 
        to="/app/login" 
        state={{ from: location }} 
        replace 
      />
    );
  }

  // Если авторизован - отображаем защищенный контент
  return <Outlet />;
};