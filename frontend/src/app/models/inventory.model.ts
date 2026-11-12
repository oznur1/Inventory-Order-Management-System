export interface Inventory {
  id: number;
  productId: number;
  productName: string;
  warehouseId: number;
  warehouseName: string;
  quantityOnHand: number;
  reservedQuantity: number;
  availableQuantity: number;
  reorderLevel: number;
  updatedAt: string;
}

export interface CreateInventory {
  productId: number;
  warehouseId: number;
  quantityOnHand: number;
  reorderLevel: number;
}

export interface LowStockAlert {
  inventoryId: number;
  productId: number;
  productName: string;
  warehouseId: number;
  warehouseName: string;
  quantityOnHand: number;
  reservedQuantity: number;
  availableQuantity: number;
  reorderLevel: number;
  stockDeficit: number;
  updatedAt: string;
}

export interface InventoryReport {
  totalInventoryRecords: number;
  totalProducts: number;
  totalWarehouses: number;
  totalQuantityOnHand: number;
  totalReservedQuantity: number;
  totalAvailableQuantity: number;
  lowStockCount: number;
  generatedAt: string;
}