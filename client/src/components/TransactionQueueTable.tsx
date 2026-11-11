import { Table, Button, Space, Tag, Tooltip, message } from 'antd';
import { assignTransaction, changeTransactionStatus, rejectTransaction, Transaction } from '../api/transactions';
import { useQueryClient } from '@tanstack/react-query';

type Props = {
  transactions: Transaction[];
  mode: 'pending' | 'assigned' | 'resolved';
  loading?: boolean;
};

export default function TransactionQueueTable({ transactions, mode, loading }: Props) {
  const qc = useQueryClient();

  const handleAssign = async (id: number) => {
    try {
      await assignTransaction(id);
      message.success('Расход взят на проверку');
      qc.invalidateQueries({ queryKey: ['transactions'] });
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Ошибка назначения';
      message.error(msg);
    }
  };

  const handleStatus = async (id: number) => {
    const status = prompt('Введите статус (InProgress, Approved):');
    if (!status) return;
    try {
      await changeTransactionStatus(id, status);
      message.success('Статус изменён');
      qc.invalidateQueries({ queryKey: ['transactions'] });
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Ошибка смены статуса';
      message.error(msg);
    }
  };

  const handleReject = async (id: number) => {
    const reason = prompt('Причина отклонения:');
    if (!reason) return;
    try {
      await rejectTransaction(id, reason);
      message.success('Расход отклонён');
      qc.invalidateQueries({ queryKey: ['transactions'] });
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Ошибка отклонения';
      message.error(msg);
    }
  };

  const columns = [
    { title: 'Название', dataIndex: 'title', key: 'title' },
    { title: 'Сумма', key: 'amount', render: (_: unknown, r: Transaction) => <strong>{r.amount} ₽</strong> },
    { title: 'Статус', key: 'status', render: (_: unknown, r: Transaction) => <Tag>{r.status}</Tag> },
    { title: 'Категория', dataIndex: 'categoryName', key: 'categoryName' },
    {
      title: 'Действия',
      key: 'actions',
      render: (_: unknown, r: Transaction) => (
        <Space>
          {mode === 'pending' && (
            <Button size="small" type="primary" onClick={() => handleAssign(r.id)}>👤 Взять на проверку</Button>
          )}
          {mode === 'assigned' && (
            <>
              <Button size="small" onClick={() => handleStatus(r.id)}>🔄 Изменить статус</Button>
              <Button size="small" danger onClick={() => handleReject(r.id)}>❌ Отклонить</Button>
            </>
          )}
          {mode === 'resolved' && r.status === 'Rejected' && r.description && (
            <Tooltip title={r.description}><Button size="small">💬 Причина</Button></Tooltip>
          )}
        </Space>
      )
    }
  ];

  return <Table dataSource={transactions} columns={columns} rowKey="id" loading={loading} pagination={false} />;
}