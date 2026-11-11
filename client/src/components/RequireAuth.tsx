// src/components/RequireAuth.tsx
import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

export default function RequireAuth() {
  const { isAuthenticated } = useAuth();
  const location = useLocation();

  // 🔹 Если нет токена — редирект на логин
  if (!isAuthenticated) {
    // Проверяем, есть ли токен в localStorage (инициализация ещё идёт)
    const token = localStorage.getItem('token');
    if (token) {
      // Показываем загрузку, пока init() не завершится
      return (
        <div style={{ padding: 40, textAlign: 'center', color: '#666' }}>
          Проверка сессии...
        </div>
      );
    }
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  // 🔹 Рендерим вложенные маршруты через Outlet
  return <Outlet />;
}