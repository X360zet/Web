import { Result, Button } from 'antd';
import { useNavigate } from 'react-router-dom';
export default function PageNotFound() {
  const nav = useNavigate();
  return <Result status="404" title="Страница не найдена" extra={<Button type="primary" onClick={() => nav('/')}>На главную</Button>} />;
}