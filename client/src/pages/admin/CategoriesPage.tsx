import { useState } from 'react';
import { Card, Table, Button, Modal, Form, Input, message, Popconfirm } from 'antd';
import { EditOutlined, DeleteOutlined } from '@ant-design/icons';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { getCategories, createCategory, updateCategory, deleteCategory, Category, CreateCategoryDto } from '../../api/categories';
import { useAuth } from '../../contexts/AuthContext';

export default function CategoriesPage() {
  const { role } = useAuth();
  const queryClient = useQueryClient();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [form] = Form.useForm();

  const { data: categories, isLoading } = useQuery({
    queryKey: ['categories'],
    queryFn: () => getCategories(role === 'Admin'),
  });

  const createMut = useMutation({
    mutationFn: (data: CreateCategoryDto) => createCategory(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categories'] });
      message.success('Категория создана');
      setIsModalOpen(false);
      form.resetFields();
    },
    onError: (err: Error) => message.error(err.message || 'Ошибка создания'),
  });

  const updateMut = useMutation({
    mutationFn: ({ id, data }: { id: number; data: CreateCategoryDto }) => 
      updateCategory(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categories'] });
      message.success('Категория обновлена');
      setIsModalOpen(false);
      form.resetFields();
    },
    onError: (err: Error) => message.error(err.message || 'Ошибка обновления'),
  });

  const deleteMut = useMutation({
    mutationFn: (id: number) => deleteCategory(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categories'] });
      message.success('Категория удалена');
    },
    onError: (err: Error) => message.error(err.message || 'Ошибка удаления'),
  });

  const handleSave = async (values: CreateCategoryDto) => {
    if (editingId) {
      updateMut.mutate({ id: editingId, data: values });
    } else {
      createMut.mutate(values);
    }
  };

  const handleEdit = (record: Category) => {
    form.setFieldsValue({ name: record.name });
    setEditingId(record.id);
    setIsModalOpen(true);
  };

  const handleCreate = () => {
    form.resetFields();
    setEditingId(null);
    setIsModalOpen(true);
  };

  const handleDelete = (id: number) => {
    deleteMut.mutate(id);
  };

  return (
    <Card 
      title="📂 Управление категориями" 
      extra={role === 'Admin' && (
        <Button type="primary" onClick={handleCreate}>
          Добавить категорию
        </Button>
      )}
    >
      <Table 
        dataSource={categories ?? []} 
        rowKey="id" 
        loading={isLoading}
        columns={[
          { 
            title: 'Название', 
            dataIndex: 'name',
            key: 'name'
          },
          { 
            title: 'Действия', 
            key: 'actions',
            width: 250,
            render: (_: unknown, record: Category) => role === 'Admin' && (
              <>
                <Button 
                  type="link" 
                  icon={<EditOutlined />}
                  onClick={() => handleEdit(record)}
                  style={{ marginRight: 8 }}
                >
                  Редактировать
                </Button>
                
                <Popconfirm
                  title="Удалить категорию"
                  description="Вы уверены, что хотите удалить эту категорию?"
                  onConfirm={() => handleDelete(record.id)}
                  okText="Удалить"
                  cancelText="Отмена"
                  okButtonProps={{ danger: true }}
                >
                  <Button 
                    type="link" 
                    icon={<DeleteOutlined />}
                    danger
                  >
                    Удалить
                  </Button>
                </Popconfirm>
              </>
            )
          }
        ]} 
      />
      
      <Modal 
        open={isModalOpen} 
        onCancel={() => {
          setIsModalOpen(false);
          form.resetFields();
        }} 
        onOk={() => form.submit()} 
        title={editingId ? 'Редактировать категорию' : 'Создать категорию'}
        confirmLoading={createMut.isPending || updateMut.isPending}
      >
        <Form 
          form={form} 
          onFinish={handleSave} 
          layout="vertical"
        >
          <Form.Item 
            name="name" 
            rules={[{ required: true, message: 'Введите название' }]} 
            label="Название категории"
          >
            <Input disabled={createMut.isPending || updateMut.isPending} />
          </Form.Item>
        </Form>
      </Modal>
    </Card>
  );
}