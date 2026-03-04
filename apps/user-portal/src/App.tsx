/**
 * Главный компонент приложения User Portal
 */

import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { ErrorBoundary } from './presentation/components/common/ErrorBoundary';
import { ProtectedRoute } from './presentation/components/layout/ProtectedRoute';
import { LoginPage, DashboardPage, PaymentPage } from './presentation/pages';
import { YandexCallbackPage } from './presentation/pages/YandexCallbackPage';
import { useAuthStore } from './application/store/authStore';
import { tokenService } from './infrastructure/services/tokenService';

/**
 * Компонент для редиректа с /app
 * Перенаправляет на /app/dashboard если авторизован, иначе на /app/login
 */
const AppRedirect: React.FC = () => {
  const isAuthenticated = useAuthStore(state => state.isAuthenticated);
  const hasToken = tokenService.getAccessToken() !== null;
  
  // Если есть токен или пользователь авторизован - редирект на dashboard
  if (hasToken || isAuthenticated) {
    return <Navigate to="/app/dashboard" replace />;
  }
  
  // Иначе редирект на страницу входа
  return <Navigate to="/app/login" replace />;
};

/**
 * Главный компонент приложения
 * 
 * Настраивает React Router с маршрутами:
 * - /app/login - страница авторизации/регистрации
 * - /app/dashboard - защищенная страница личного кабинета
 * - /app/payment - защищенная страница оплаты
 * - /app - редирект на dashboard (если авторизован) или login
 * 
 * Все приложение обернуто в ErrorBoundary для обработки ошибок.
 * Защищенные маршруты используют ProtectedRoute компонент.
 * 
 * Требования: 6.5, 6.6, 7.1, 7.2, 7.3, 7.4
 */
function App() {
  return (
    <ErrorBoundary>
      <BrowserRouter>
        <Routes>
          {/* Публичные маршруты */}
          <Route path="/app/login" element={<LoginPage />} />
          <Route path="/auth/yandex/callback" element={<YandexCallbackPage />} />
          
          {/* Защищенные маршруты */}
          <Route path="/app" element={<ProtectedRoute />}>
            <Route path="dashboard" element={<DashboardPage />} />
            <Route path="payment" element={<PaymentPage />} />
            {/* Редирект с /app на соответствующую страницу */}
            <Route index element={<AppRedirect />} />
          </Route>
          
          {/* Редирект с корня на /app */}
          <Route path="/" element={<Navigate to="/app" replace />} />
          
          {/* Fallback для несуществующих маршрутов */}
          <Route path="*" element={<Navigate to="/app" replace />} />
        </Routes>
      </BrowserRouter>
    </ErrorBoundary>
  );
}

export default App;