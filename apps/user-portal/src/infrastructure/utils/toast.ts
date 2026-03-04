import { Intent, Position, OverlayToaster, type ToastProps } from '@blueprintjs/core';

/**
 * Конфигурация для Toast уведомления
 */
export interface ToastConfig {
  /** Сообщение для отображения */
  message: string;
  /** Тип уведомления (цветовая схема) */
  intent?: Intent;
  /** Время отображения в миллисекундах (0 = бесконечно) */
  timeout?: number;
  /** Дополнительное действие */
  action?: {
    text: string;
    onClick: () => void;
  };
}

/**
 * Singleton Toaster для отображения уведомлений
 */
class ToastManager {
  private toaster: { show: (config: ToastProps) => void; clear: () => void } | null = null;

  /**
   * Инициализирует Toaster если он еще не создан
   */
  private async initializeToaster(): Promise<void> {
    if (!this.toaster) {
      this.toaster = await OverlayToaster.create({
        position: Position.TOP_RIGHT,
        maxToasts: 5,
        canEscapeKeyClear: true
      });
    }
  }

  /**
   * Отображает Toast уведомление
   * @param config - Конфигурация уведомления
   */
  async show(config: ToastConfig): Promise<void> {
    await this.initializeToaster();

    if (!this.toaster) {
      console.error('Не удалось инициализировать Toaster');
      return;
    }

    this.toaster.show({
      message: config.message,
      intent: config.intent || 'none',
      timeout: config.timeout || 4000,
      action: config.action ? {
        text: config.action.text,
        onClick: config.action.onClick
      } : undefined
    });
  }

  /**
   * Отображает уведомление об успехе
   * @param message - Сообщение
   * @param timeout - Время отображения
   */
  success(message: string, timeout = 4000): void {
    this.show({
      message,
      intent: 'success',
      timeout
    }).catch(console.error);
  }

  /**
   * Отображает уведомление об ошибке
   * @param message - Сообщение
   * @param timeout - Время отображения
   */
  error(message: string, timeout = 5000): void {
    this.show({
      message,
      intent: 'danger',
      timeout
    }).catch(console.error);
  }

  /**
   * Отображает предупреждение
   * @param message - Сообщение
   * @param timeout - Время отображения
   */
  warning(message: string, timeout = 4000): void {
    this.show({
      message,
      intent: 'warning',
      timeout
    }).catch(console.error);
  }

  /**
   * Отображает информационное уведомление
   * @param message - Сообщение
   * @param timeout - Время отображения
   */
  info(message: string, timeout = 4000): void {
    this.show({
      message,
      intent: 'primary',
      timeout
    }).catch(console.error);
  }

  /**
   * Очищает все активные уведомления
   */
  clear(): void {
    if (this.toaster) {
      this.toaster.clear();
    }
  }
}

// Создаем singleton instance
const toastManager = new ToastManager();

/**
 * Функция для отображения Toast уведомления
 * @param config - Конфигурация уведомления
 */
export const showToast = (config: ToastConfig): void => {
  toastManager.show(config).catch(console.error);
};

/**
 * Экспортируем методы для удобства использования
 */
export const toast = {
  show: showToast,
  success: toastManager.success.bind(toastManager),
  error: toastManager.error.bind(toastManager),
  warning: toastManager.warning.bind(toastManager),
  info: toastManager.info.bind(toastManager),
  clear: toastManager.clear.bind(toastManager)
};