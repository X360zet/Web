import { Card, Empty, Button } from 'antd';

export default function QueuePage() {
  return (
    <Card title="📋 Очередь проверок" style={{ borderRadius: 12, maxWidth: 800, margin: '0 auto' }}>
      <Empty
        image={Empty.PRESENTED_IMAGE_SIMPLE}
        description="На этой странице будут отображаться заявки на проверку транзакций"
      >
        <Button type="primary">Обновить</Button>
      </Empty>
    </Card>
  );
}