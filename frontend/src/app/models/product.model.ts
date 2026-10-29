export interface Product {
  id: number;
  sku: string;
  name: string;
  description: string | null;
  unitPrice: number;
  isActive: boolean;
  createdAt: string;
}

export interface CreateProduct {
  sku: string;
  name: string;
  description: string | null;
  unitPrice: number;
}

export interface UpdateProduct {
  sku: string;
  name: string;
  description: string | null;
  unitPrice: number;
  isActive: boolean;
}