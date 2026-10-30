
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

    // Stok hangi depodan karşılanacak?
    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    // Sipariş edilen miktar
    public int Quantity { get; set; }

    // Sipariş anındaki ürün fiyatı
    public decimal UnitPrice { get; set; }
}
