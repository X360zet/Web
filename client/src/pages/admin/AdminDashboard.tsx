import { Card, Row, Col, Button } from 'antd';
import { useNavigate } from 'react-router-dom';
import { FolderOutlined, TeamOutlined } from '@ant-design/icons';
import { useAuth } from '../../contexts/AuthContext';

export default function AdminDashboard() {
  const navigate = useNavigate();
  const { user } = useAuth(); 

  return (
    <div>
      <h1>Панель администратора {user?.displayName}</h1>
      
      <Row gutter={[16, 16]}>
        <Col xs={24} md={12}>
          <Card 
            title={<FolderOutlined />} 
            hoverable
            onClick={() => navigate('/admin/categories')}
            style={{ cursor: 'pointer', height: '100%' }}
          >
            <h3>Управление категориями</h3>
            <p>Создание, редактирование и активация категорий расходов</p>
            <Button type="primary">Открыть</Button>
          </Card>
        </Col>
        
        <Col xs={24} md={12}>
          <Card 
            title={<TeamOutlined />} 
            hoverable
            onClick={() => navigate('/admin/users')}
            style={{ cursor: 'pointer', height: '100%' }}
          >
            <h3>Управление пользователями</h3>
            <p>Просмотр списка пользователей и изменение их ролей</p>
            <Button type="primary">Открыть</Button>
          </Card>
        </Col>
      </Row>
    </div>
  );
}