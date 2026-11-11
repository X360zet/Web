import { api } from './client';

export interface User {
  id: string;
  displayName: string;
  email: string;
  role: string;
}

export const getUsers = () => 
  api<User[]>('/api/users');

export const updateUserRole = (userId: string, role: string) => 
  api(`/api/users/${userId}/role`, { 
    method: 'PUT', 
    body: JSON.stringify({ role }) 
  });