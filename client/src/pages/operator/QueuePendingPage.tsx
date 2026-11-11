import { useQuery } from '@tanstack/react-query';
import { Card, Typography } from 'antd';
import { getTransactions, TransactionsQuery } from '../../api/transactions';
import TransactionQueueTable from '../../components/TransactionQueueTable';

const { Title } = Typography;

export default function QueuePendingPage() {
  const query: TransactionsQuery = { unassignedOnly: true, status: 'Pending' };
  
  const { data, isLoading } = useQuery({
    queryKey: ['transactions', 'pending'],
    queryFn: () => getTransactions(query),
  });

  return (
    <Card>
      <Title level={4}>📥 Новые расходы (Ожидают проверки)</Title>
      <TransactionQueueTable transactions={data?.items || []} mode="pending" loading={isLoading} />
    </Card>
  );
}