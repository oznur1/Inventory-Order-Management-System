// Ürün bilgileri,fiyat,SKU.


namespace backend.Entities;

public class Product
{
    public int Id { get; set; }

    public string SKU { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal UnitPrice { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Bu ürünün farklı depolardaki stok bakiyeleri
    public ICollection<InventoryBalance> InventoryBalances { get; set; }
        = new List<InventoryBalance>();
}
