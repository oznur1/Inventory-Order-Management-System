
namespace backend.Entities;

public class StockReservation
{
    public int Id { get; set; }

    // Rezervasyon yapılan ürün
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    // Ürünün ayrıldığı depo
    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    // Ayrılan stok miktarı
    public int Quantity { get; set; }

    // Rezervasyon bir siparişe bağlı olabilir.
    public int? OrderId { get; set; }
    public Order? Order { get; set; }

    // Active, Released, Expired, Fulfilled
    public string Status { get; set; } = "Active";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Rezervasyonun sona ereceği zaman; isteğe bağlıdır.
    public DateTime? ExpiresAt { get; set; }
}
