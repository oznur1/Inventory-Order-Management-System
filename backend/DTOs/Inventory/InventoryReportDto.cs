
namespace backend.DTOs.Inventory;

public class InventoryReportDto
{
    public int TotalInventoryRecords { get; set; }

    public int TotalProducts { get; set; }

    public int TotalWarehouses { get; set; }

    public int TotalQuantityOnHand { get; set; }

    public int TotalReservedQuantity { get; set; }

    public int TotalAvailableQuantity { get; set; }

    public int LowStockCount { get; set; }

    public DateTime GeneratedAt { get; set; }
}
