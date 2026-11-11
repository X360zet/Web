// client/src/pages/user/EditTransactionPage.tsx
import { Card, Button, } from 'antd';  // ← Добавьте Button
import { useNavigate } from 'react-router-dom';

export default function EditTransactionPage() {
  const navigate = useNavigate();
  
  return (
    <Card title="Редактирование транзакции">
      <p>Страница редактирования (заглушка)</p>
      <Button onClick={() => navigate('/transactions')}>Назад</Button>
    </Card>
  );
}