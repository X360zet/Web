import { Result } from 'antd';
export default function PageError({ title = 'Ошибка', desc = 'Не удалось загрузить данные' }: { title?: string; desc?: string }) {
  return <Result status="error" title={title} subTitle={desc} />;
}