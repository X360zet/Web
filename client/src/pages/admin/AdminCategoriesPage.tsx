import { Card, Table, Button } from 'antd';
export default function AdminCategoriesPage() {
  return (
    <Card title="Управление категориями">
      <Table dataSource={[{ key: '1', name: 'Еда', budget: 20000 }, { key: '2', name: 'ЖКХ', budget: 15000 }]} columns={[
        { title: 'Название', dataIndex: 'name' },
        { title: 'Общий лимит', dataIndex: 'budget' },
        { title: 'Действия', render: () => <Button type="link">Ред.</Button> }
      ]} />
    </Card>
  );
}