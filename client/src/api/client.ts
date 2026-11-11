// src/api/client.ts
const API_BASE = import.meta.env.VITE_API_URL || '';

export interface LoginResponse {
  accessToken: string;
  user: {
    id: string;
    displayName: string;
    email: string;
    role: 'User' | 'Operator' | 'Admin';
  };
}

// User может не быть в ответе регистрации (согласно Лабе 3)
export interface RegisterResponse {
  accessToken: string;
  user?: {
    id: string;
    displayName: string;
    email: string;
    role: 'User' | 'Operator' | 'Admin';
  };
}

export interface MeResponse {
  id: string;
  email: string;
  displayName: string;
  role: 'User' | 'Operator' | 'Admin';
}

export const api = async <T = unknown>(
  path: string, 
  options: RequestInit = {}
): Promise<T> => {
  const token = localStorage.getItem('token');
  const headers: HeadersInit = {
    'Content-Type': 'application/json',
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
  };

  try {
    const res = await fetch(`${API_BASE}${path}`, { ...options, headers });
    
    if (!res.ok) {
      let errData = {};
      // Пробуем прочитать JSON, но если это HTML (например, 404), игнорируем ошибку парсинга
      try {
        errData = await res.json();
      } catch { /* ignore */ }
      
      const message = 
        (errData as { detail?: string }).detail || 
        (errData as { title?: string }).title || 
        `HTTP ${res.status}: ${res.statusText}`;
      throw new Error(message);
    }

    // Если ответ пустой (204 No Content), возвращаем undefined
    if (res.status === 204) return undefined as T;

    return res.json() as Promise<T>;
  } catch (error: unknown) {
    // Если ошибка сети (CORS, сервер выключен)
    if (error instanceof TypeError && error.message === 'Failed to fetch') {
      throw new Error('Не удалось подключиться к серверу. Проверьте, запущен ли бэкенд (dotnet run).');
    }
    throw error;
  }
};