// client/src/pages/user/NewTransactionPage.tsx
import { Card, Form, Input, InputNumber, Select, Button, message } from 'antd';
import { useNavigate } from 'react-router-dom';
import { CreateTransactionDto } from '../../api/transactions';  // ← Импортируйте тип

export default function NewTransactionPage() {
  const navigate = useNavigate();
  const [form] = Form.useForm(); //ВОПРОС !!!

  // 🔹 Замените any на правильный тип
  const onFinish = (values: CreateTransactionDto) => {
    console.log(values);
    message.success('Транзакция создана (заглушка)');
    navigate('/transactions');
  };

  return (
    <Card title="Новая транзакция">
      <Form form={form} onFinish={onFinish} layout="vertical">
        <Form.Item name="title" label="Название" rules={[{ required: true }]}>
          <Input />
        </Form.Item>
        <Form.Item name="amount" label="Сумма" rules={[{ required: true }]}>
          <InputNumber style={{ width: '100%' }} />
        </Form.Item>
        <Form.Item name="type" label="Тип" rules={[{ required: true }]}>
          <Select>
            <Select.Option value="Expense">Расход</Select.Option>
            <Select.Option value="Income">Доход</Select.Option>
          </Select>
        </Form.Item>
        <Form.Item>
          <Button type="primary" htmlType="submit">Создать</Button>
          <Button style={{ marginLeft: 8 }} onClick={() => navigate('/transactions')}>Отмена</Button>
        </Form.Item>
      </Form>
    </Card>
  );
}