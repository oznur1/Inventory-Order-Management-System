//Ürünlerin depolardaki stok miktarları.




namespace backend.Entities;

// Bir ürünün belirli bir depodaki stok durumunu tutar.
public class InventoryBalance
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    // Depoda fiziksel olarak bulunan toplam miktar.
    public int QuantityOnHand { get; set; }

    // Siparişler için ayrılmış miktar.
    public int ReservedQuantity { get; set; } = 0;

    // Siparişlere ayrılabilecek kullanılabilir miktar.
    public int AvailableQuantity => QuantityOnHand - ReservedQuantity;

    public int ReorderLevel { get; set; }

    // SQL Server tarafından otomatik güncellenen eşzamanlılık alanı.
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
