import { useQuery } from '@tanstack/react-query';
import { Card, Typography } from 'antd';
import { getTransactions, TransactionsQuery } from '../../api/transactions';
import TransactionQueueTable from '../../components/TransactionQueueTable';

const { Title } = Typography;

export default function QueueAssignedPage() {
  const query: TransactionsQuery = { assignedToMe: true };
  
  const { data, isLoading } = useQuery({
    queryKey: ['transactions', 'assigned'],
    queryFn: () => getTransactions(query),
  });

  return (
    <Card>
      <Title level={4}>🔧 Мои расходы на проверке</Title>
      <TransactionQueueTable transactions={data?.items || []} mode="assigned" loading={isLoading} />
    </Card>
  );
}