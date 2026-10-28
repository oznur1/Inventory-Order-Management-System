
namespace backend.Entities;

public class Order
{
    public int Id { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string? CustomerEmail { get; set; }

    public string? ShippingAddress { get; set; }

    // Pending, Confirmed, Shipped, Delivered, Cancelled
    public string Status { get; set; } = "Pending";

    // Siparişin toplam tutarı
    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Bir sipariş birden fazla ürün içerebilir.
    public ICollection<OrderItem> Items { get; set; }
        = new List<OrderItem>();

    // Siparişe bağlı stok rezervasyonları
    public ICollection<StockReservation> StockReservations { get; set; }
        = new List<StockReservation>();
}
