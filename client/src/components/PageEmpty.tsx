import { Empty } from 'antd';
export default function PageEmpty({ desc = 'Нет данных' }: { desc?: string }) { return <Empty description={desc} />; }