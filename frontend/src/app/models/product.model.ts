export interface Product {
  id: string;
  code: string;
  description: string;
  stock: number;
}
export interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}
