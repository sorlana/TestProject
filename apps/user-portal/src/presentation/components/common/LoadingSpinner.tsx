/**
 * Компонент LoadingSpinner - индикатор загрузки
 */

import React from 'react';
import { Spinner, SpinnerSize, Intent } from '@blueprintjs/core';
import './LoadingSpinner.css';

/**
 * Пропсы для LoadingSpinner
 */
interface LoadingSpinnerProps {
  /** Размер спиннера */
  size?: SpinnerSize;
  /** Цветовая схема */
  intent?: Intent;
  /** Текст сообщения */
  message?: string;
  /** Показывать ли overlay на всю страницу */
  overlay?: boolean;
  /** Дополнительные CSS классы */
  className?: string;
}

/**
 * Компонент LoadingSpinner
 * 
 * Отображает индикатор загрузки с центрированием.
 * Может использоваться как inline компонент или как overlay на всю страницу.
 * 
 * Требования: 5.1
 */
export const LoadingSpinner: React.FC<LoadingSpinnerProps> = ({
  size = SpinnerSize.STANDARD,
  intent = Intent.PRIMARY,
  message,
  overlay = false,
  className = ''
}) => {
  const containerClass = `loading-spinner-container ${
    overlay ? 'loading-spinner-overlay' : 'loading-spinner-inline'
  } ${className}`.trim();

  return (
    <div className={containerClass}>
      <div className="loading-spinner-content">
        <Spinner
          size={size}
          intent={intent}
          className="loading-spinner"
        />
        {message && (
          <div className="loading-spinner-message">
            {message}
          </div>
        )}
      </div>
    </div>
  );
};