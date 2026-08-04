// customer.model.ts
export interface Customer {
  id: number;
  fullName: string;
  email: string;
  phone?: string | null;
  address?: string | null;
  applicationUserId?: string | null;
}

export interface CustomerCreate {
  fullName: string;
  email: string;
  phone?: string | null;
  address?: string | null;
}

export interface CustomerUpdate extends CustomerCreate {}

export interface CustomerPatch {
  fullName?: string | null;
  email?: string | null;
  phone?: string | null;
  address?: string | null;
}