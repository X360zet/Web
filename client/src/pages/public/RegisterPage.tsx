// src/pages/public/RegisterPage.tsx
import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { Form, Input, Button, Card, Alert, Typography, Divider } from 'antd';
import { MailOutlined, LockOutlined, UserOutlined } from '@ant-design/icons';
import { useAuth } from '../../contexts/AuthContext';
import { api, RegisterResponse, MeResponse } from '../../api/client';

const { Title, Text } = Typography;

export default function RegisterPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const onFinish = async (values: { 
    email: string; 
    password: string; 
    displayName: string;
    confirm?: string;
  }) => {
    setLoading(true);
    setError(null);
    
    try {
      // 🔹 1. Регистрируемся
      const response = await api<RegisterResponse>('/api/auth/register', {
        method: 'POST',
        body: JSON.stringify({
          email: values.email,
          password: values.password,
          displayName: values.displayName,
        }),
      });

      // 🔹 2. Подготовка данных для входа
      let user = response.user ?? null;
      let role: 'User' | 'Operator' | 'Admin' | null = response.user?.role ?? null;

      // Если бэкенд вернул только токен (без user), запрашиваем профиль отдельно
      if (!user) {
        localStorage.setItem('token', response.accessToken);
        
        const me = await api<MeResponse>('/api/auth/me', {
          headers: { Authorization: `Bearer ${response.accessToken}` },
        });
        
        user = {
          id: me.id,
          displayName: me.displayName,
          email: me.email,
          role: me.role,
        };
        role = me.role;
      }

      // 🔹 3. Выполняем вход в приложение
      if (user && role) {
        login({
          accessToken: response.accessToken,
          user,
          role,
        });
        // Редирект по роли (согласно Лабе 3/5)
        if (role === 'User') navigate('/tickets');
        else if (role === 'Operator') navigate('/queue/new');
        else navigate('/admin/categories');
      } else {
        throw new Error('Не удалось получить данные пользователя');
      }

    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Ошибка регистрации';
      setError(message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{
      minHeight: '100vh',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
      padding: 20,
    }}>
      {/* 🔹 Исправлено: bodyStyle → styles.body */}
      <Card 
        style={{ width: '100%', maxWidth: 420, borderRadius: 12 }} 
        styles={{ body: { padding: 32 } }}
      >
        <div style={{ textAlign: 'center', marginBottom: 24 }}>
          <Title level={2} style={{ margin: 0 }}>🏠 Домашние финансы</Title>
          <Text type="secondary">Создайте аккаунт</Text>
        </div>

        {error && <Alert message={error} type="error" showIcon style={{ marginBottom: 16 }} />}

        <Form onFinish={onFinish} layout="vertical" size="large">
          <Form.Item
            name="displayName"
            rules={[{ required: true, message: 'Введите ваше имя' }]}
          >
            <Input prefix={<UserOutlined />} placeholder="Ваше имя" />
          </Form.Item>

          {/* 🔹 Исправлено: убрана type: 'email' */}
          <Form.Item
            name="email"
            rules={[{ required: true, message: 'Введите email' }]}
          >
            <Input prefix={<MailOutlined />} placeholder="Email" />
          </Form.Item>

          <Form.Item
            name="password"
            rules={[{ required: true, message: 'Введите пароль' }]}
          >
            <Input.Password prefix={<LockOutlined />} placeholder="Пароль" />
          </Form.Item>

          <Form.Item
            name="confirm"
            dependencies={['password']}
            rules={[
              { required: true, message: 'Подтвердите пароль' },
              ({ getFieldValue }) => ({
                validator(_: unknown, value: string) {
                  if (!value || getFieldValue('password') === value) {
                    return Promise.resolve();
                  }
                  return Promise.reject(new Error('Пароли не совпадают'));
                },
              }),
            ]}
          >
            <Input.Password prefix={<LockOutlined />} placeholder="Подтвердите пароль" />
          </Form.Item>

          <Form.Item>
            <Button type="primary" htmlType="submit" loading={loading} block size="large">
              Зарегистрироваться
            </Button>
          </Form.Item>
        </Form>

        <Divider style={{ margin: '16px 0' }}>или</Divider>

        <div style={{ textAlign: 'center' }}>
          <Text type="secondary">Уже есть аккаунт? </Text>
          <Link to="/login">Войти</Link>
        </div>
      </Card>
    </div>
  );
}