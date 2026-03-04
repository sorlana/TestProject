/**
 * Компонент ErrorBoundary - обработчик ошибок React
 */

import { Component, type ErrorInfo, type ReactNode } from 'react';
import { Card, Button, Intent, Icon } from '@blueprintjs/core';
import './ErrorBoundary.css';

/**
 * Пропсы для ErrorBoundary
 */
interface ErrorBoundaryProps {
  /** Дочерние компоненты */
  children: ReactNode;
  /** Кастомный fallback компонент */
  fallback?: ReactNode;
}

/**
 * Состояние ErrorBoundary
 */
interface ErrorBoundaryState {
  /** Флаг наличия ошибки */
  hasError: boolean;
  /** Объект ошибки */
  error?: Error;
  /** Информация об ошибке */
  errorInfo?: ErrorInfo;
}

/**
 * Компонент ErrorBoundary
 * 
 * React Error Boundary для перехвата ошибок рендеринга.
 * Отображает fallback UI при ошибке и логирует ошибки в консоль в dev режиме.
 * 
 * Требования: 8.5
 */
export class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState> {
  constructor(props: ErrorBoundaryProps) {
    super(props);
    
    this.state = {
      hasError: false
    };
  }

  /**
   * Статический метод для обновления состояния при ошибке
   */
  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    return {
      hasError: true,
      error
    };
  }

  /**
   * Метод для обработки ошибки и логирования
   */
  componentDidCatch(error: Error, errorInfo: ErrorInfo): void {
    // Логируем ошибку в консоль в dev режиме
    if (import.meta.env.DEV) {
      console.error('ErrorBoundary перехватил ошибку:', error);
      console.error('Информация об ошибке:', errorInfo);
    }

    // Сохраняем информацию об ошибке в состоянии
    this.setState({
      error,
      errorInfo
    });

    // В продакшене можно отправить ошибку в систему мониторинга
    // например, Sentry, LogRocket и т.д.
    if (import.meta.env.PROD) {
      // Здесь можно добавить отправку ошибки в систему мониторинга
      console.error('Произошла ошибка в приложении:', error.message);
    }
  }

  /**
   * Обработчик сброса ошибки
   */
  handleReset = (): void => {
    this.setState({
      hasError: false,
      error: undefined,
      errorInfo: undefined
    });
  };

  /**
   * Обработчик перезагрузки страницы
   */
  handleReload = (): void => {
    window.location.reload();
  };

  render(): ReactNode {
    const { hasError, error } = this.state;
    const { children, fallback } = this.props;

    if (hasError) {
      // Если передан кастомный fallback, используем его
      if (fallback) {
        return fallback;
      }

      // Стандартный fallback UI
      return (
        <div className="error-boundary-container">
          <Card className="error-boundary-card" elevation={2}>
            <div className="error-boundary-content">
              <Icon 
                icon="error" 
                size={48} 
                intent={Intent.DANGER}
                className="error-boundary-icon"
              />
              
              <h2 className="error-boundary-title">
                Что-то пошло не так
              </h2>
              
              <p className="error-boundary-message">
                Произошла неожиданная ошибка в приложении. 
                Мы уже работаем над её исправлением.
              </p>

              {import.meta.env.DEV && error && (
                <details className="error-boundary-details">
                  <summary>Детали ошибки (только в режиме разработки)</summary>
                  <pre className="error-boundary-stack">
                    {error.message}
                    {error.stack}
                  </pre>
                </details>
              )}

              <div className="error-boundary-actions">
                <Button
                  intent={Intent.PRIMARY}
                  onClick={this.handleReset}
                  text="Попробовать снова"
                />
                <Button
                  onClick={this.handleReload}
                  text="Перезагрузить страницу"
                />
              </div>
            </div>
          </Card>
        </div>
      );
    }

    return children;
  }
}