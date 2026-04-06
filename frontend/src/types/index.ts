export interface Product {
  id: number;
  name: string;
  description: string;
  sku: string;
  price: number;
  quantity: number;
  categoryId: number;
  categoryName: string;
  supplierId: number | null;
  supplierName: string | null;
  minStockLevel: number;
  isLowStock: boolean;
  isActive: boolean;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateProduct {
  name: string;
  description: string;
  sku: string;
  price: number;
  quantity: number;
  categoryId: number;
  supplierId: number | null;
  minStockLevel: number;
}

export interface UpdateProduct {
  name: string;
  description: string;
  price: number;
  quantity: number;
  categoryId: number;
  supplierId: number | null;
  minStockLevel: number;
  isActive: boolean;
}

export interface ProductFilterParams {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
  categoryId?: number;
  supplierId?: number;
  minPrice?: number;
  maxPrice?: number;
  isLowStock?: boolean;
  isActive?: boolean;
  sortBy?: string;
  sortDescending?: boolean;
}

export interface Category {
  id: number;
  name: string;
  description: string;
  isActive: boolean;
  productCount: number;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateCategory {
  name: string;
  description: string;
}

export interface UpdateCategory {
  name: string;
  description: string;
  isActive: boolean;
}

export interface Supplier {
  id: number;
  name: string;
  contactName: string;
  email: string;
  phone: string;
  address: string;
  isActive: boolean;
  productCount: number;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateSupplier {
  name: string;
  contactName: string;
  email: string;
  phone: string;
  address: string;
}

export interface UpdateSupplier {
  name: string;
  contactName: string;
  email: string;
  phone: string;
  address: string;
  isActive: boolean;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface LoginCredentials {
  email: string;
  password: string;
}

export interface RegisterData {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

export interface AuthResponse {
  token: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
  expiration: string;
}

export interface User {
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
}

export interface ChangePasswordData {
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}
