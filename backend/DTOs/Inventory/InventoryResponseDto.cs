
namespace backend.DTOs.Inventory;

public class InventoryResponseDto
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public int WarehouseId { get; set; }

    public string WarehouseName { get; set; } = string.Empty;

    public int QuantityOnHand { get; set; }

    public int ReorderLevel { get; set; }

    public DateTime UpdatedAt { get; set; }
}
