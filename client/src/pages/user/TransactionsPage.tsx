import { Table, Tag, Button, Space, Typography } from 'antd';
import { PlusOutlined } from '@ant-design/icons';
import { Link } from 'react-router-dom';

interface Transaction {
  id: string;
  date: string;
  description: string;
  category: string;
  amount: number;
  type: 'income' | 'expense';
}

const mockData: Transaction[] = [
  { id: '1', date: '2025-01-15', description: 'Salary', category: 'Income', amount: 5000, type: 'income' },
  { id: '2', date: '2025-01-16', description: 'Groceries', category: 'Food', amount: 150, type: 'expense' },
  { id: '3', date: '2025-01-17', description: 'Electric bill', category: 'Utilities', amount: 80, type: 'expense' },
  { id: '4', date: '2025-01-18', description: 'Freelance work', category: 'Income', amount: 800, type: 'income' },
];

export default function TransactionsPage() {
  const columns = [
    {
      title: 'Date',
      dataIndex: 'date',
      key: 'date',
      sorter: (a: Transaction, b: Transaction) => a.date.localeCompare(b.date),
    },
    {
      title: 'Description',
      dataIndex: 'description',
      key: 'description',
    },
    {
      title: 'Category',
      dataIndex: 'category',
      key: 'category',
    },
    {
      title: 'Amount',
      dataIndex: 'amount',
      key: 'amount',
      render: (amount: number, record: Transaction) => (
        <span style={{ color: record.type === 'income' ? '#52c41a' : '#ff4d4f', fontWeight: 'bold' }}>
          {record.type === 'income' ? '+' : '-'}${amount.toFixed(2)}
        </span>
      ),
      sorter: (a: Transaction, b: Transaction) => a.amount - b.amount,
    },
    {
      title: 'Type',
      key: 'type',
      dataIndex: 'type',
      render: (type: 'income' | 'expense') => (
        <Tag color={type === 'income' ? 'green' : 'red'}>{type.toUpperCase()}</Tag>
      ),
    },
  ];

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 24 }}>
        <Typography.Title level={3}>Transactions</Typography.Title>
        <Button type="primary" icon={<PlusOutlined />}>
          <Link to="/transactions/new">Add Transaction</Link>
        </Button>
      </div>
      <Table columns={columns} dataSource={mockData} rowKey="id" />
    </div>
  );
}
