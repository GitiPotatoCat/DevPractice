// brand.model.ts
export interface Brand {
  id: number;
  name: string;
  country: string;
}

export interface BrandCreate {
  name: string;
  country: string;
}