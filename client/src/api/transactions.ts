import { api } from './client';

export type TransactionType = 'Expense' | 'Income';
export type TransactionStatus = 'Pending' | 'Approved' | 'Rejected';

export interface Transaction {
  id: number;
  title: string;
  description?: string;
  amount: number;
  type: TransactionType;
  status: TransactionStatus;
  categoryId: number;
  categoryName: string;
  user: { id: string; displayName: string; email: string };
  approver?: { id: string; displayName: string; email: string };
  createdAt: string;
  transactionDate?: string;
  approvedAt?: string;
}

export interface CreateTransactionDto {
  title: string;
  description?: string;
  amount: number;
  type: TransactionType;
  categoryId: number;
  transactionDate?: string;
}

export interface UpdateTransactionDto {
  title?: string;
  description?: string;
  amount?: number;
  categoryId?: number;
  transactionDate?: string;
}

export interface TransactionsQuery {
  type?: TransactionType;
  status?: TransactionStatus;
  categoryId?: number;
  assignedToMe?: boolean; 
  unassignedOnly?: boolean;
  minAmount?: number;
  maxAmount?: number;
  dateFrom?: string;
  dateTo?: string;
  search?: string;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  descending?: boolean;
}

export interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  totalAmount?: number;
}
export const requestDeleteTransaction = (id: number) => 
  api<{ message: string; operationId: number; status: string }>(`/api/transactions/${id}`, { 
    method: 'DELETE' 
  });

// 🔹 Замените buildQueryString на эту версию:
const buildQueryString = (params: Record<string, string | number | boolean | undefined | null>): string => {
  const filtered = Object.entries(params)
    .filter(([, value]) => value !== undefined && value !== null && value !== '')  // ← Убрали '_'
    .map(([key, value]) => `${encodeURIComponent(key)}=${encodeURIComponent(String(value))}`)
    .join('&');
  
  return filtered ? `?${filtered}` : '';
};

export const getTransactions = (query: TransactionsQuery = {}) => {
  const queryString = buildQueryString({
    type: query.type,
    status: query.status,
    categoryId: query.categoryId,
    minAmount: query.minAmount,
    maxAmount: query.maxAmount,
    dateFrom: query.dateFrom,
    dateTo: query.dateTo,
    search: query.search,
    page: query.page || 1,
    pageSize: query.pageSize || 20,
    sortBy: query.sortBy || 'TransactionDate',
    descending: query.descending ?? false,  // 🔹 Явное значение вместо undefined
  });
  
  return api<PagedResponse<Transaction>>(`/api/transactions${queryString}`);
};

export const getTransactionById = (id: number) => 
  api<Transaction>(`/api/transactions/${id}`);

export const createTransaction = (payload: CreateTransactionDto) => 
  api<Transaction>('/api/transactions', { 
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload) 
  });

export const updateTransaction = (id: number, payload: UpdateTransactionDto) => 
  api<Transaction>(`/api/transactions/${id}`, { 
    method: 'PUT', 
    body: JSON.stringify(payload) 
  });

export const deleteTransaction = (id: number) => 
  api(`/api/transactions/${id}`, { 
    method: 'DELETE' 
  });

export const approveTransaction = (id: number) => 
  api<Transaction>(`/api/transactions/${id}/approve`, { 
    method: 'POST' 
  });

export const rejectTransaction = (id: number, comment?: string) => 
  api<Transaction>(`/api/transactions/${id}/reject`, { 
    method: 'POST',
    body: JSON.stringify({ comment }) 
  });
  export const assignTransaction = (id: number) => 
  api<Transaction>(`/api/transactions/${id}/assign`, { method: 'POST' });

export const changeTransactionStatus = (id: number, status: string) => 
  api<Transaction>(`/api/transactions/${id}/status`, { 
    method: 'POST', 
    body: JSON.stringify({ status }) 
  });
