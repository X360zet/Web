import { useState } from 'react';
import { Form, Input, InputNumber, Select, Button, Card, Typography, Space, message } from 'antd';
import { useNavigate } from 'react-router-dom';

const { Option } = Select;

interface FormValues {
  date: string;
  description: string;
  category: string;
  amount: number;
  type: 'income' | 'expense';
}

const categories = [
  'Food',
  'Transport',
  'Utilities',
  'Entertainment',
  'Shopping',
  'Healthcare',
  'Education',
  'Income',
  'Other',
];

export default function NewTransactionPage() {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (values: FormValues) => {
    setLoading(true);
    try {
      // TODO: Implement API call to create transaction
      console.log('Transaction created:', values);
      message.success('Transaction added successfully!');
      navigate('/transactions');
    } catch (error) {
      message.error('Failed to add transaction. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <Typography.Title level={3}>Add New Transaction</Typography.Title>
      <Card style={{ maxWidth: 600 }}>
        <Form layout="vertical" onFinish={handleSubmit} autoComplete="off">
          <Form.Item
            label="Type"
            name="type"
            rules={[{ required: true, message: 'Please select transaction type' }]}
            initialValue="expense"
          >
            <Select>
              <Option value="expense">Expense</Option>
              <Option value="income">Income</Option>
            </Select>
          </Form.Item>

          <Form.Item
            label="Date"
            name="date"
            rules={[{ required: true, message: 'Please enter date' }]}
          >
            <Input type="date" />
          </Form.Item>

          <Form.Item
            label="Description"
            name="description"
            rules={[{ required: true, message: 'Please enter description' }]}
          >
            <Input placeholder="e.g., Grocery shopping" />
          </Form.Item>

          <Form.Item
            label="Category"
            name="category"
            rules={[{ required: true, message: 'Please select category' }]}
          >
            <Select placeholder="Select a category">
              {categories.map((cat) => (
                <Option key={cat} value={cat}>{cat}</Option>
              ))}
            </Select>
          </Form.Item>

          <Form.Item
            label="Amount"
            name="amount"
            rules={[
              { required: true, message: 'Please enter amount' },
              { type: 'number', min: 0.01, message: 'Amount must be greater than 0' },
            ]}
          >
            <InputNumber
              style={{ width: '100%' }}
              placeholder="0.00"
              min={0.01}
              step={0.01}
              precision={2}
              addonBefore="$"
            />
          </Form.Item>

          <Form.Item>
            <Space>
              <Button type="primary" htmlType="submit" loading={loading}>
                Add Transaction
              </Button>
              <Button onClick={() => navigate('/transactions')}>
                Cancel
              </Button>
            </Space>
          </Form.Item>
        </Form>
      </Card>
    </div>
  );
}
