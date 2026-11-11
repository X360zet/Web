import { Card, Table, Select, message, Tag } from 'antd';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { getUsers, updateUserRole, User } from '../../api/users';

export default function UsersPage() {
  const queryClient = useQueryClient();
  
  const { data: users, isLoading } = useQuery({
    queryKey: ['users'],
    queryFn: getUsers,
  });

  const changeRoleMut = useMutation({
    mutationFn: ({ userId, role }: { userId: string; role: string }) => 
      updateUserRole(userId, role),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['users'] });
      message.success('Роль пользователя обновлена');
    },
  });

  return (
    <Card title="👥 Управление пользователями">
      <Table 
        dataSource={users} 
        rowKey="id" 
        loading={isLoading}
        columns={[
          { 
            title: 'Имя', 
            dataIndex: 'displayName',
            key: 'displayName'
          },
          { 
            title: 'Email', 
            dataIndex: 'email',
            key: 'email'
          },
          { 
            title: 'Роль', 
            dataIndex: 'role',
            key: 'role',
            render: (role: string, record: User) => (
              <Select 
                value={role} 
                style={{ width: 120 }} 
                onChange={(val) => 
                  changeRoleMut.mutate({ userId: record.id, role: val })
                } 
                options={[
                  { value: 'User', label: 'User' },
                  { value: 'Operator', label: 'Operator' },
                  { value: 'Admin', label: 'Admin' }
                ]} 
              />
            )
          },
          {
            title: 'Статус',
            key: 'status',
            render: (_: unknown, record: User) => (
              <Tag color={record.role === 'Admin' ? 'red' : record.role === 'Operator' ? 'blue' : 'green'}>
                {record.role}
              </Tag>
            )
          }
        ]} 
      />
    </Card>
  );
}