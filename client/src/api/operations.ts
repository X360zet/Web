import { api } from './client';

export interface TransactionOperation {
  id: number;
  transactionId: number;
  transactionTitle: string;
  amount: number;
  operationType: 'Create' | 'Delete';
  status: 'Pending' | 'Approved' | 'Rejected';
  userName: string;
  rejectionReason?: string;
  createdAt: string;
  processedAt?: string;
}

export interface PagedOperations {
  items: TransactionOperation[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export const getPendingOperations = (page = 1, pageSize = 10) =>
  api<PagedOperations>(`/api/operations/pending?page=${page}&pageSize=${pageSize}`);

export const approveOperation = (id: number, comment?: string) =>
  api<TransactionOperation>(`/api/operations/${id}/approve`, {
    method: 'POST',
    body: JSON.stringify(comment ? { comment } : null),
  });

export const rejectOperation = (id: number, reason: string) =>
  api<TransactionOperation>(`/api/operations/${id}/reject`, {
    method: 'POST',
    body: JSON.stringify(reason),
  });