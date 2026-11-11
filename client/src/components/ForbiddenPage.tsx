import { Result, Button } from 'antd';
import { useNavigate } from 'react-router-dom';
export default function ForbiddenPage() {
  const nav = useNavigate();
  return <Result status="403" title="Доступ запрещен" extra={<Button type="primary" onClick={() => nav(-1)}>Назад</Button>} />;
}