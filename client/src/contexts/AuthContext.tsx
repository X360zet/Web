// src/contexts/AuthContext.tsx
import { createContext, useContext, useState, useEffect, ReactNode, useCallback } from 'react';
import { api, MeResponse } from '../api/client';

export interface User {
  id: string;
  displayName: string;
  email: string;
}

export type Role = 'User' | 'Operator' | 'Admin';

interface AuthContextType {
  user: User | null;
  role: Role | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean; // 🔹 Новый флаг для индикации загрузки
  login: (data: { accessToken: string; user: User; role: Role }) => void;
  register: (data: { email: string; password: string; displayName: string }) => Promise<{ accessToken: string; user: User; role: Role }>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider');
  }
  return context;
};

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider = ({ children }: AuthProviderProps) => {
  const [user, setUser] = useState<User | null>(null);
  const [role, setRole] = useState<Role | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true); // 🔹 По умолчанию true

  const isAuthenticated = !!token;

  // 🔹 Инициализация: восстановление сессии
  useEffect(() => {
    const initializeAuth = async () => {
      const storedToken = localStorage.getItem('token');
      const storedUser = localStorage.getItem('user');
      const storedRole = localStorage.getItem('role');

      // 1. Если есть данные в localStorage — восстанавливаем их МГНОВЕННО
      // Это предотвращает "мигание" экрана входа при обновлении страницы
      if (storedToken && storedUser && storedRole) {
        try {
          setUser(JSON.parse(storedUser));
          setRole(storedRole as Role);
          setToken(storedToken);
        } catch (e) {
          console.error('Failed to parse stored auth data', e);
          // Если данные битые — чистим
          localStorage.removeItem('token');
          localStorage.removeItem('user');
          localStorage.removeItem('role');
        }
      }

      // 2. Если есть токен, проверяем его валидность на сервере (фоновая проверка)
      if (storedToken) {
        try {
          const response = await api<MeResponse>('/api/auth/me', {
            headers: { Authorization: `Bearer ${storedToken}` }
          });
          
          // Если сервер вернул данные — обновляем состояние (на случай, если пользователь сменил имя)
          setUser({ 
            id: response.id, 
            displayName: response.displayName, 
            email: response.email 
          });
          setRole(response.role as Role);
          setToken(storedToken); // Подтверждаем токен
          
          } catch (error) {
           // 🔹 Логируем для отладки
           console.warn('Token validation failed:', error);
  
          localStorage.removeItem('token');
          localStorage.removeItem('user');
          localStorage.removeItem('role');
          setUser(null);
          setRole(null);
          setToken(null);
          }
      }
      
      // 3. Загрузка завершена в любом случае
      setIsLoading(false);
    };

    initializeAuth();
  }, []);

  const login = useCallback((data: { accessToken: string; user: User; role: Role }) => {
    setToken(data.accessToken);
    setUser(data.user);
    setRole(data.role);
    
    // 🔹 Сохраняем в localStorage для персистентности
    localStorage.setItem('token', data.accessToken);
    localStorage.setItem('user', JSON.stringify(data.user));
    localStorage.setItem('role', data.role); // Сохраняем только валидные строки
  }, []);

  const register = useCallback(async (data: { email: string; password: string; displayName: string }) => {
    const response = await api<{ accessToken: string; user: User; role: Role }>('/api/auth/register', {
      method: 'POST',
      body: JSON.stringify(data),
    });
    return response;
  }, []);

  const logout = useCallback(() => {
    setToken(null);
    setUser(null);
    setRole(null);
    setIsLoading(false);
    
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    localStorage.removeItem('role');
  }, []);

  return (
    <AuthContext.Provider value={{ 
      user, 
      role, 
      token, 
      isAuthenticated, 
      isLoading, // 🔹 Экспортируем флаг загрузки
      login, 
      register, 
      logout 
    }}>
      {children}
    </AuthContext.Provider>
  );
};