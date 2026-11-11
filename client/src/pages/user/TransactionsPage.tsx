// client/src/pages/user/TransactionsPage.tsx
import { useState } from 'react';
import { Card, Table, Button, Input, Select, Space, Popconfirm, message } from 'antd';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { getTransactions, deleteTransaction, Transaction, TransactionType, TransactionStatus } from '../../api/transactions';
import { useAuth } from '../../contexts/AuthContext';
import { useNavigate } from 'react-router-dom';
import { EditOutlined, DeleteOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';

export default function TransactionsPage() {
  const { role } = useAuth();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  
  const [filters, setFilters] = useState<{
    type: TransactionType | undefined;
    status: TransactionStatus | undefined;
    search: string;
  }>({ 
    type: undefined, 
    status: undefined, 
    search: '' 
  });

  const { data, isLoading } = useQuery({
    queryKey: ['transactions-all', filters],
    queryFn: () => getTransactions({
      type: filters.type,
      status: filters.status,
      search: filters.search || undefined,
      page: 1,
      pageSize: 50
    }),
  });

  const deleteMut = useMutation({
    mutationFn: (id: number) => deleteTransaction(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transactions-all'] });
      message.success('Запись удалена');
    },
  });
  const columns = [
    { title: 'Название', dataIndex: 'title', key: 'title', render: (t: string, r: Transaction) => <a onClick={() => navigate(`/transactions/${r.id}/edit`)}>{t}</a> },
    { title: 'Категория', dataIndex: 'categoryName', key: 'categoryName' },
    { title: 'Сумма', key: 'amount', render: (_: unknown, r: Transaction) => <span style={{ color: r.type === 'Expense' ? '#ff4d4f' : '#52c41a', fontWeight: 600 }}>{r.type === 'Expense' ? '-' : '+'}{r.amount.toLocaleString('ru-RU')} ₽</span> },
    { title: 'Дата', dataIndex: 'transactionDate', key: 'date', render: (d?: string) => d ? dayjs(d).format('DD.MM.YYYY') : '-' },
    { title: 'Действия', key: 'actions', render: (_: unknown, r: Transaction) => (
      <Space>
        <Button size="small" icon={<EditOutlined />} onClick={() => navigate(`/transactions/${r.id}/edit`)} />
        <Popconfirm title="Удалить?" onConfirm={() => deleteMut.mutate(r.id)}>
          <Button size="small" icon={<DeleteOutlined />} danger />
        </Popconfirm>
      </Space>
    )}
  ];

  return (
    <Card title="💰 Все финансовые записи">
      <Space style={{ marginBottom: 16, flexWrap: 'wrap' }}>
        <Input 
          placeholder="Поиск..." 
          value={filters.search} 
          onChange={e => setFilters(f => ({ ...f, search: e.target.value }))} 
          style={{ width: 200 }} 
        />
        <Select 
          placeholder="Тип" 
          allowClear 
          style={{ width: 120 }} 
          onChange={val => setFilters(f => ({ ...f, type: val as TransactionType }))} 
          options={[{ value: 'Expense', label: 'Расход' }, { value: 'Income', label: 'Доход' }]} 
        />
        {role === 'User' && <Button type="primary" onClick={() => navigate('/transactions/new')}>+ Добавить</Button>}
      </Space>
      <Table dataSource={data?.items} columns={columns} rowKey="id" loading={isLoading} pagination={{ pageSize: 10 }} />
    </Card>
  );
}