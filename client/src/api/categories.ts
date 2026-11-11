import { api } from './client';

export interface Category {
  id: number;
  name: string;
  isActive: boolean;
}

export interface CreateCategoryDto {
  name: string;
}

export const getCategories = (includeInactive = false) => 
  api<Category[]>(`/api/categories?includeInactive=${includeInactive}`);

export const createCategory = (data: CreateCategoryDto) => 
  api<Category>('/api/categories', { 
    method: 'POST', 
    body: JSON.stringify(data) 
  }
);

export const updateCategory = (id: number, data: CreateCategoryDto) => 
  api<Category>(`/api/categories/${id}`, { 
    method: 'PUT', 
    body: JSON.stringify(data) 
  }
);

export const toggleCategory = (id: number, isActive: boolean) => 
  api<Category>(`/api/categories/${id}/active`, { 
    method: 'PATCH', 
    body: JSON.stringify({ isActive })
  }
);
export const deleteCategory = (id: number) => 
  api(`/api/categories/${id}`, { 
    method: 'DELETE' 
  }
);