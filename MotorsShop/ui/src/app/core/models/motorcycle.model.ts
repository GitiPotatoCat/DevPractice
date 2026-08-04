// motorcycle.model.ts
export interface Motorcycle {
  id: number;
  name: string;
  description?: string | null;
  price: number;
  year: number;
  stock: number;
  engineCc: number;
  brandName: string;
  categoryName: string;
}

export interface MotorcycleCreate {
  name: string;
  description?: string | null;
  price: number;
  year: number;
  stock: number;
  engineCc: number;
  brandId: number;
  categoryId: number;
}

export interface MotorcycleQuery {
  page?: number;
  pageSize?: number;
  brandId?: number;
  categoryId?: number;
  minPrice?: number;
  maxPrice?: number;
  search?: string;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}