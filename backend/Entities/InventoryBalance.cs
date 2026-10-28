//Ürünlerin depolardaki stok miktarları.



namespace backend.Entities;

public class InventoryBalance
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public Product Product { get; set; } = null!;

    public int WarehouseId { get; set; }

    public Warehouse Warehouse { get; set; } = null!;

    public int QuantityOnHand { get; set; }

    public int ReorderLevel { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}