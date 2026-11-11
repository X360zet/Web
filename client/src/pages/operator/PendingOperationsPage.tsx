// client/src/pages/operator/PendingOperationsPage.tsx
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Card, Table, Button, Space, Tag, Modal, Input, message, Typography, Empty } from 'antd';
import { getPendingOperations, approveOperation, rejectOperation, TransactionOperation } from '../../api/operations';
import { CheckOutlined, CloseOutlined, UserOutlined, FileAddOutlined, DeleteOutlined } from '@ant-design/icons';

const { Title, Text } = Typography;

export default function PendingOperationsPage() {
  const qc = useQueryClient();
  
  const { data, isLoading } = useQuery({
    queryKey: ['operations', 'pending'],
    queryFn: () => getPendingOperations(1, 100),
  });

  const approveMut = useMutation({
    mutationFn: (id: number) => approveOperation(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['operations'] });
      qc.invalidateQueries({ queryKey: ['transactions'] });
      message.success('Операция одобрена!');
    },
    onError: (err: Error) => message.error(err.message || 'Ошибка одобрения'),
  });

  const rejectMut = useMutation({
    mutationFn: ({ id, reason }: { id: number; reason: string }) => rejectOperation(id, reason),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['operations'] });
      message.success('Операция отклонена');
    },
    onError: (err: Error) => message.error(err.message || 'Ошибка отклонения'),
  });

  const handleReject = (record: TransactionOperation) => {
    Modal.confirm({
      title: `Отклонить: ${record.operationType === 'Create' ? 'Создание' : 'Удаление'}`,
      content: (
        <div>
          <Text>Пользователь: {record.userName}</Text><br />
          <Text>Сумма: {record.amount} ₽</Text><br /><br />
          <Input.TextArea id="rejectReason" placeholder="Причина отклонения" rows={3} />
        </div>
      ),
      onOk: () => {
        const reason = (document.getElementById('rejectReason') as HTMLTextAreaElement)?.value;
        if (!reason) { message.error('Укажите причину'); return false; }
        rejectMut.mutate({ id: record.id, reason });
      },
    });
  };

  const columns = [
    { 
      title: 'Пользователь', 
      key: 'userName',
      render: (_: unknown, record: TransactionOperation) => (
        <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
          <UserOutlined />
          <span>{record.userName}</span>
        </div>
      )
    },
    { 
      title: 'Транзакция', 
      key: 'transactionTitle',
      render: (_: unknown, record: TransactionOperation) => (
        <strong>{record.transactionTitle}</strong>
      )
    },
    { 
      title: 'Сумма', 
      key: 'amount',
      render: (_: unknown, record: TransactionOperation) => (
        <strong>{record.amount.toLocaleString('ru-RU')} ₽</strong>
      )
    },
    { 
      title: 'Тип операции', 
      key: 'operationType',
      render: (_: unknown, record: TransactionOperation) => {
        if (record.operationType === 'Create') {
          return <Tag color="green" icon={<FileAddOutlined />}>➕ Создание</Tag>;
        }
        if (record.operationType === 'Delete') {
          return <Tag color="red" icon={<DeleteOutlined />}>🗑️ Удаление</Tag>;
        }
        return <Tag>{record.operationType}</Tag>;
      }
    },
    {
      title: 'Действия',
      key: 'actions',
      width: 250,
      render: (_: unknown, record: TransactionOperation) => (
        <Space direction="vertical" style={{ width: '100%' }}>
          <Button 
            size="small" 
            type="primary" 
            icon={<CheckOutlined />}
            onClick={() => {
              Modal.confirm({
                title: 'Одобрить операцию?',
                content: `Пользователь: ${record.userName}\nТип: ${record.operationType}`,
                onOk: () => approveMut.mutate(record.id),
              });
            }}
            loading={approveMut.isPending}
          >
            Одобрить
          </Button>
          <Button 
            size="small" 
            danger 
            icon={<CloseOutlined />}
            onClick={() => handleReject(record)}
            loading={rejectMut.isPending}
          >
            Отклонить
          </Button>
        </Space>
      )
    }
  ];

  return (
    <Card>
      <Title level={4}>📋 Ожидающие операции пользователей</Title>
      <Text type="secondary" style={{ display: 'block', marginBottom: 16 }}>
        Здесь отображаются запросы на создание или удаление транзакций.
      </Text>
      
      {isLoading ? (
        <div style={{ textAlign: 'center', padding: 40 }}>Загрузка...</div>
      ) : data?.items && data.items.length > 0 ? (
        <Table 
          dataSource={data.items} 
          columns={columns} 
          rowKey="id" 
          pagination={{ pageSize: 20 }}
        />
      ) : (
        <Empty description="Нет ожидающих операций" />
      )}
    </Card>
  );
}