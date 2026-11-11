import { useQuery } from '@tanstack/react-query';
import { Card, Typography } from 'antd';
import { getTransactions, TransactionsQuery } from '../../api/transactions';
import TransactionQueueTable from '../../components/TransactionQueueTable';

const { Title } = Typography;

export default function QueueResolvedPage() {
  const query: TransactionsQuery = { assignedToMe: true, status: 'Approved' };  // 🔹 Используйте 'Approved' вместо 'Resolved'
  
  const { data, isLoading } = useQuery({
    queryKey: ['transactions', 'resolved'],
    queryFn: () => getTransactions(query),
  });

  return (
    <Card>
      <Title level={4}>✅ Одобрённые расходы</Title>
      <TransactionQueueTable transactions={data?.items || []} mode="resolved" loading={isLoading} />
    </Card>
  );
}