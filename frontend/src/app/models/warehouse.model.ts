export interface Warehouse {
  id: number;
  code: string;
  name: string;
  address: string | null;
  isActive: boolean;
  createdAt: string;
}

export interface CreateWarehouse {
  code: string;
  name: string;
  address: string | null;
}

export interface UpdateWarehouse {
  code: string;
  name: string;
  address: string | null;
  isActive: boolean;
}