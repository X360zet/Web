import React from 'react';
import { Modal, Form, Input, InputNumber, Select, DatePicker, message } from 'antd';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { createTransaction, updateTransaction, CreateTransactionDto, UpdateTransactionDto, Transaction } from '../api/transactions';
import { getCategories, Category } from '../api/categories';
import dayjs from 'dayjs';
import 'dayjs/locale/ru';

dayjs.locale('ru');

interface TransactionFormValues {
  title: string;
  amount: number;
  type: string;
  categoryId: number;
  transactionDate?: dayjs.Dayjs;
  description?: string;
}

interface TransactionModalProps {
  open: boolean;
  onClose: () => void;
  initialData?: Transaction | null;
}

export default function TransactionModal({ open, onClose, initialData }: TransactionModalProps) {
  const [form] = Form.useForm();
  const queryClient = useQueryClient();

  // 🔹 ИСПРАВЛЕНО: правильный синтаксис - data: categories
const query = useQuery({
  queryKey: ['categories'],
  queryFn: () => getCategories(false),
  enabled: open,
});

const categories = query.data;

 const createMut = useMutation({
  mutationFn: async (payload: CreateTransactionDto) => {
    return await createTransaction(payload);
  },
  onSuccess: () => {
    // 🔹 ИСПРАВЛЕНО: инвалидируем ВСЕ запросы с transactions
    queryClient.invalidateQueries({ queryKey: ['transactions'] });
    queryClient.invalidateQueries({ queryKey: ['transactions-all'] });
message.success('Транзакция создана и отправлена на одобрение оператору');
    form.resetFields();
    onClose();
  },
  onError: (err: Error) => message.error(err.message || 'Ошибка сохранения'),
});

const updateMut = useMutation({
  mutationFn: async ({ id, payload }: { id: number; payload: UpdateTransactionDto }) => {
    return await updateTransaction(id, payload);
  },
  onSuccess: () => {
    queryClient.invalidateQueries({ queryKey: ['transactions'] });
    queryClient.invalidateQueries({ queryKey: ['transactions-all'] });
    message.success('Транзакция обновлена');
    form.resetFields();
    onClose();
  },
  onError: (err: Error) => message.error(err.message || 'Ошибка обновления'),
});

const handleFinish = (values: TransactionFormValues) => {
  console.log('Form values:', values);  // 🔹 Для отладки
  
  const payload: CreateTransactionDto = {
    title: values.title,
    amount: values.amount,
    type: values.type === 'Расход' ? 'Expense' : 'Income',  // 🔹 Конвертация
    categoryId: Number(values.categoryId),  // 🔹 Преобразуем в число
    transactionDate: values.transactionDate?.toISOString(),
  };

  console.log('Sending payload:', payload);  // 🔹 Для отладки

  if (initialData) {
    updateMut.mutate({ id: initialData.id, payload: payload as UpdateTransactionDto });
  } else {
    createMut.mutate(payload);
  }
};

  const handleCancel = () => {
    form.resetFields();
    onClose();
  };

  React.useEffect(() => {
    if (open && initialData) {
      form.setFieldsValue({
        ...initialData,
        type: initialData.type === 'Expense' ? 'Расход' : 'Доход',
        transactionDate: dayjs(initialData.transactionDate),
      });
    } else if (open) {
      form.resetFields();
      form.setFieldsValue({ 
        type: 'Расход',
        transactionDate: dayjs() 
      });
    }
  }, [open, initialData, form]);

  return (
    <Modal
      title={initialData ? 'Редактировать транзакцию' : 'Новая транзакция'}
      open={open}
      onCancel={handleCancel}
      onOk={() => form.submit()}
      confirmLoading={createMut.isPending || updateMut.isPending}
      destroyOnHidden
    >
      <Form 
        form={form} 
        onFinish={handleFinish} 
        layout="vertical" 
        initialValues={{ type: 'Расход', transactionDate: dayjs() }}
      >
        <Form.Item name="title" label="Название" rules={[{ required: true, message: 'Введите название' }]}>
          <Input placeholder="Например: Продукты" />
        </Form.Item>

        <Form.Item name="amount" label="Сумма (₽)" rules={[{ required: true, message: 'Введите сумму' }]}>
          <InputNumber style={{ width: '100%' }} min={0.01} step={0.01} placeholder="0.00" />
        </Form.Item>

        <Form.Item name="type" label="Тип операции" rules={[{ required: true }]}>
          <Select>
            <Select.Option value="Расход">Расход</Select.Option>
            <Select.Option value="Доход">Доход</Select.Option>
          </Select>
        </Form.Item>

        <Form.Item name="categoryId" label="Категория" rules={[{ required: true, message: 'Выберите категорию' }]}>
          <Select placeholder="Выберите категорию" loading={!categories}>
            {categories?.map((cat: Category) => (
              <Select.Option key={cat.id} value={cat.id}>{cat.name}</Select.Option>
            ))}
          </Select>
        </Form.Item>

        <Form.Item name="transactionDate" label="Дата операции" rules={[{ required: true }]}>
          <DatePicker style={{ width: '100%' }} format="DD.MM.YYYY" />
        </Form.Item>
      </Form>
    </Modal>
  );
}