import { Layout, Menu, Typography, Avatar, Dropdown, Button } from 'antd';
import { Link, Outlet, useLocation, useNavigate } from 'react-router-dom';
import { 
  HomeOutlined, 
  PlusCircleOutlined, 
  TeamOutlined, 
  LogoutOutlined,
  UserOutlined,
  FolderOutlined,
  DashboardOutlined,
  CheckCircleOutlined // 🔹 Добавили иконку
} from '@ant-design/icons';
import { useAuth } from '../contexts/AuthContext';

const { Header, Sider, Content } = Layout;
const { Title } = Typography;

export default function AppLayout() {
  const { user, role, logout } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const getMenuItems = () => {
    if (role === 'User') return [
      { key: '/dashboard', icon: <HomeOutlined />, label: <Link to="/dashboard">Главная</Link> },
      { key: '/transactions', icon: <PlusCircleOutlined />, label: <Link to="/transactions">Транзакции</Link> },
    ];
    
    if (role === 'Operator') return [
      { 
        key: '/operator/operations', 
        icon: <CheckCircleOutlined />, 
        label: <Link to="/operator/operations">📋 Ожидающие операции</Link> 
      },
    ];
    
    if (role === 'Admin') return [
      { key: '/admin', icon: <DashboardOutlined />, label: <Link to="/admin">Панель администратора</Link> },
      { key: '/admin/categories', icon: <FolderOutlined />, label: <Link to="/admin/categories">Категории</Link> },
      { key: '/admin/users', icon: <TeamOutlined />, label: <Link to="/admin/users">Пользователи</Link> },
    ];
    
    return [];
  };

  const userMenu = {
    items: [
      { key: 'profile', label: 'Профиль', icon: <UserOutlined /> },
      { key: 'logout', label: 'Выйти', icon: <LogoutOutlined />, onClick: handleLogout },
    ],
  };

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Sider 
        theme="light" 
        width={220} 
        style={{ borderRight: '1px solid #f0f0f0', position: 'fixed', height: '100vh', left: 0, top: 0 }}
      >
        <div style={{ padding: '20px 16px', borderBottom: '1px solid #f0f0f0' }}>
          <Title level={4} style={{ margin: 0, display: 'flex', alignItems: 'center', gap: 8 }}>
            <span style={{ fontSize: '1.5em' }}>🏠</span>
            Финансы
          </Title>
        </div>
        <Menu
          mode="inline"
          selectedKeys={[location.pathname]}
          items={getMenuItems()}
          style={{ borderRight: 0 }}
        />
      </Sider>

      <Layout style={{ marginLeft: 220 }}>
        <Header style={{ 
          background: '#fff', 
          padding: '0 24px', 
          display: 'flex', 
          alignItems: 'center', 
          justifyContent: 'flex-end',
          borderBottom: '1px solid #f0f0f0',
          position: 'sticky',
          top: 0,
          zIndex: 10,
        }}>
          <Dropdown menu={userMenu} placement="bottomRight">
            <Button type="text" style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
              <Avatar icon={<UserOutlined />} style={{ background: '#667eea' }} />
              <span>{user?.displayName}</span>
              <span style={{ color: '#999', fontSize: '0.9em' }}>• {role}</span>
            </Button>
          </Dropdown>
        </Header>

        <Content style={{ padding: 24, background: '#f8f9fa', minHeight: 'calc(100vh - 64px)' }}>
          <Outlet />
        </Content>
      </Layout>
    </Layout>
  );
}