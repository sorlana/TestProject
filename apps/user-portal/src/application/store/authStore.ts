/**
 * Zustand store для управления состоянием аутентификации
 */

import { create } from 'zustand';
import type { User } from '../../domain/entities/User';

/**
 * Интерфейс состояния аутентификации
 */
export interface AuthState {
  /** Флаг авторизации пользователя */
  isAuthenticated: boolean;
  /** Данные текущего пользователя */
  user: User | null;
  /** Флаг состояния загрузки */
  isLoading: boolean;
  /** Сообщение об ошибке */
  error: string | null;
  
  /** Установить статус авторизации */
  setAuthenticated: (value: boolean) => void;
  /** Установить данные пользователя */
  setUser: (user: User | null) => void;
  /** Установить состояние загрузки */
  setLoading: (value: boolean) => void;
  /** Установить ошибку */
  setError: (error: string | null) => void;
  /** Сбросить состояние к начальным значениям */
  reset: () => void;
}

/**
 * Zustand store для аутентификации
 * 
 * Управляет глобальным состоянием аутентификации пользователя,
 * включая статус авторизации, данные пользователя, состояние загрузки и ошибки.
 */
export const useAuthStore = create<AuthState>((set) => ({
  // Начальное состояние
  isAuthenticated: false,
  user: null,
  isLoading: false,
  error: null,
  
  // Методы для обновления состояния
  setAuthenticated: (value: boolean) => set({ isAuthenticated: value }),
  
  setUser: (user: User | null) => set({ user }),
  
  setLoading: (value: boolean) => set({ isLoading: value }),
  
  setError: (error: string | null) => set({ error }),
  
  reset: () => set({
    isAuthenticated: false,
    user: null,
    isLoading: false,
    error: null
  })
}));