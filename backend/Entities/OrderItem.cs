
namespace backend.Entities;

public class OrderItem
{
    public int Id { get; set; }

    // Hangi siparişe ait?
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    // Hangi ürün sipariş edildi?
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    // Sipariş edilen ürün miktarı
    public int Quantity { get; set; }

    // Sipariş verildiği andaki ürün fiyatı
    public decimal UnitPrice { get; set; }
}
