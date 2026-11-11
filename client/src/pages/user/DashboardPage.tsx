// client/src/pages/user/DashboardPage.tsx
import { useState, useMemo } from 'react';
import { Card, Button, Table, Tag, Space, InputNumber, Popconfirm, Typography, Progress, Divider, message } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { getTransactions, requestDeleteTransaction, Transaction } from '../../api/transactions';
import { useAuth } from '../../contexts/AuthContext';
import TransactionModal from '../../components/TransactionModal';
import dayjs from 'dayjs';
import 'dayjs/locale/ru';

dayjs.locale('ru');

const { Title, Text } = Typography;

export default function DashboardPage() {
  const { user } = useAuth();
  const queryClient = useQueryClient();
  const [modalOpen, setModalOpen] = useState(false);
  const [editingTx, setEditingTx] = useState<Transaction | null>(null);
  
  const [budgetLimit, setBudgetLimit] = useState<number>(() => {
    const saved = localStorage.getItem(`budgetLimit_${user?.id}`);
    return saved ? Number(saved) : 36000;
  });

  const currentMonth = dayjs().startOf('month').toISOString();
  const nextMonth = dayjs().endOf('month').toISOString();

  const { data, isLoading } = useQuery({
    queryKey: ['transactions', currentMonth],
    queryFn: () => getTransactions({ dateFrom: currentMonth, dateTo: nextMonth, pageSize: 50 }),
  });

  const deleteMut = useMutation({
    mutationFn: (id: number) => requestDeleteTransaction(id),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['transactions'] });
      message.success(data?.message || 'Запрос на удаление отправлен оператору');
    },
    onError: (err: Error) => message.error(err.message || 'Ошибка отправки запроса'),
  });

  const transactions = data?.items || [];
  const expenses = transactions.filter((t: Transaction) => t.type === 'Expense');
  const totalSpent = expenses.reduce((sum: number, t: Transaction) => sum + t.amount, 0);
  const remaining = budgetLimit - totalSpent;
  const usagePercent = Math.min(100, Math.max(0, (totalSpent / budgetLimit) * 100));

  const categoryStats = useMemo(() => {
    const stats: Record<string, number> = {};
    expenses.forEach((t: Transaction) => {
      stats[t.categoryName] = (stats[t.categoryName] || 0) + t.amount;
    });
    return Object.entries(stats).map(([name, amount]) => ({ name, amount }));
  }, [expenses]);

  const recentTransactions = transactions.slice(0, 5);

  const handleBudgetChange = (val: number | null) => {
    if (val && val > 0) {
      setBudgetLimit(val);
      localStorage.setItem(`budgetLimit_${user?.id}`, String(val));
    }
  };

  const columns = [
    { title: 'Название', dataIndex: 'title', key: 'title' },
    { title: 'Категория', dataIndex: 'categoryName', key: 'categoryName' },
    { 
      title: 'Сумма', 
      key: 'amount',
      render: (_: unknown, r: Transaction) => (
        <span style={{ color: r.type === 'Expense' ? '#ff4d4f' : '#52c41a', fontWeight: 600 }}>
          {r.type === 'Expense' ? '-' : '+'}{r.amount.toLocaleString('ru-RU')} ₽
        </span>
      )
    },
    { title: 'Дата', dataIndex: 'transactionDate', key: 'date', render: (d: string) => dayjs(d).format('DD.MM') },
    {
      title: 'Действия',
      key: 'actions',
      render: (_: unknown, r: Transaction) => (
        <Space>
          <Button size="small" icon={<EditOutlined />} onClick={() => { setEditingTx(r); setModalOpen(true); }} />
          <Popconfirm 
            title="Запросить удаление?" 
            description="Оператор должен одобрить удаление этой транзакции."
            onConfirm={() => deleteMut.mutate(r.id)}
          >
            <Button size="small" icon={<DeleteOutlined />} danger />
          </Popconfirm>
        </Space>
      )
    }
  ];

  return (
    <div style={{ maxWidth: 1200, margin: '0 auto', padding: 24 }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 24 }}>
        <div>
          <Title level={3} style={{ margin: 0 }}>Привет, {user?.displayName}!</Title>
          <Text type="secondary">Ваш финансовый помощник</Text>
        </div>
        <Button type="primary" size="large" icon={<PlusOutlined />} onClick={() => { setEditingTx(null); setModalOpen(true); }}>
          Добавить операцию
        </Button>
      </div>

      <Space direction="vertical" size="large" style={{ width: '100%' }}>
        <Card>
          <Space size="large" style={{ width: '100%', justifyContent: 'space-between', flexWrap: 'wrap' }}>
            <div style={{ textAlign: 'center' }}>
              <Text type="secondary">Потрачено</Text>
              <div style={{ fontSize: 24, fontWeight: 700, color: '#ff4d4f' }}>↓ {totalSpent.toLocaleString('ru-RU')} ₽</div>
            </div>
            <div style={{ textAlign: 'center' }}>
              <Text type="secondary">Лимит</Text>
              <div style={{ display: 'flex', alignItems: 'center', gap: 8, justifyContent: 'center' }}>
                <InputNumber 
                  value={budgetLimit} 
                  onChange={handleBudgetChange} 
                  size="small" 
                  style={{ width: 100, fontWeight: 700 }} 
                  prefix="₽"
                />
              </div>
            </div>
            <div style={{ textAlign: 'center' }}>
              <Text type="secondary">Осталось</Text>
              <div style={{ fontSize: 24, fontWeight: 700, color: remaining >= 0 ? '#52c41a' : '#ff4d4f' }}>
                {remaining >= 0 ? '↑' : '↓'} {Math.abs(remaining).toLocaleString('ru-RU')} ₽
              </div>
            </div>
          </Space>
          <Divider />
          <Progress percent={Math.round(usagePercent)} status={usagePercent > 90 ? 'exception' as const : 'active' as const} />
          <Text type={remaining >= 0 ? 'success' : 'danger'}>
            {remaining >= 0 ? `Вы укладываетесь в бюджет. Осталось ${remaining.toLocaleString('ru-RU')} ₽` : 'Бюджет превышен!'}
          </Text>
        </Card>

        {categoryStats.length > 0 && (
          <Card title="📂 Расходы по категориям">
            <Space direction="vertical" style={{ width: '100%' }} size="middle">
              {categoryStats.map((cat: {name: string; amount: number}) => {
                const percent = Math.round((cat.amount / totalSpent) * 100);
                return (
                  <div key={cat.name}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 4 }}>
                      <Tag color="blue">{cat.name}</Tag>
                      <Text strong>{cat.amount.toLocaleString('ru-RU')} ₽ ({percent}%)</Text>
                    </div>
                    <Progress percent={percent} size="small" showInfo={false} />
                  </div>
                );
              })}
            </Space>
          </Card>
        )}

        <Card title="📜 Последние операции">
          <Table 
            dataSource={recentTransactions} 
            columns={columns} 
            rowKey="id" 
            loading={isLoading}
            pagination={false}
            locale={{ emptyText: 'Нет операций за этот месяц' }}
          />
        </Card>
      </Space>

      <TransactionModal 
        open={modalOpen} 
        onClose={() => { setModalOpen(false); setEditingTx(null); }} 
        initialData={editingTx} 
      />
    </div>
  );
}