
namespace backend.DTOs.Inventory;

public class LowStockAlertDto
{
    public int InventoryId { get; set; }

    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;

    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;

    public int QuantityOnHand { get; set; }
    public int ReservedQuantity { get; set; }
    public int AvailableQuantity { get; set; }

    public int ReorderLevel { get; set; }
    public int StockDeficit { get; set; }

    public DateTime UpdatedAt { get; set; }
}
