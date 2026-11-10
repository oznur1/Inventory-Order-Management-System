
export interface StockMovementReport {
  totalMovements: number;
  totalInboundQuantity: number;
  totalOutboundQuantity: number;
  netStockChange: number;
  generatedAt: string;
}
