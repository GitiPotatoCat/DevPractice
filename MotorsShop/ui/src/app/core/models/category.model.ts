// category.model.ts
export interface Category {
  id: number;
  name: string;
  description?: string | null;
}

export interface CategoryCreate {
  name: string;
  description?: string | null;
}