import { Card, Empty, Button } from 'antd';

export default function AdminPage() {
  return (
    <Card title="⚙️ Панель администратора" style={{ borderRadius: 12, maxWidth: 800, margin: '0 auto' }}>
      <Empty
        image={Empty.PRESENTED_IMAGE_SIMPLE}
        description="Здесь будет управление категориями, пользователями и настройками системы"
      >
        <Button type="primary">Настройки</Button>
      </Empty>
    </Card>
  );
}