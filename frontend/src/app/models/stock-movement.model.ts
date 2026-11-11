
export interface StockMovement {
  id: number;
  productId: number;
  productName: string;
  warehouseId: number;
  warehouseName: string;
  movementType: 'Inbound' | 'Outbound';
  quantity: number;
  referenceNumber: string;
  notes: string;
  createdAt: string;
}

export interface StockMovementFilters {
  productId?: number;
  warehouseId?: number;
  movementType?: 'Inbound' | 'Outbound';
  from?: string;
  to?: string;
}

export interface StockMovementReport {
  totalMovements: number;
  totalInboundQuantity: number;
  totalOutboundQuantity: number;
  netStockChange: number;
  generatedAt: string;
}
