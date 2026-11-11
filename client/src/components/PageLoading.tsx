import { Spin } from 'antd';
export default function PageLoading() { return <div style={{ padding: 40, textAlign: 'center' }}><Spin tip="Загрузка..." /></div>; }