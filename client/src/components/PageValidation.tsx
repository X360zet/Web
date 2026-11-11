import { Alert } from 'antd';
export default function PageValidation({ errors }: { errors: string[] }) {
  return <Alert message="Ошибки валидации" description={<ul>{errors.map((e, i) => <li key={i}>{e}</li>)}</ul>} type="warning" showIcon />;
}