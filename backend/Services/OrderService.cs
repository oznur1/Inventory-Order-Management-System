using backend.Data;
using backend.DTOs;
using backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class OrderService
{
    private readonly AppDbContext _context;

    public OrderService(AppDbContext context)
    {
        _context = context;
    }

    // Yeni sipariş oluşturur ve sipariş için stok rezervasyonu yapar.
    public async Task<OrderResponseDto> CreateAsync(CreateOrderDto dto)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            // 1. Sipariş kalemlerini kontrol et.
            if (dto.Items == null || dto.Items.Count == 0)
            {
                throw new InvalidOperationException(
                    "Sipariş en az bir ürün içermelidir.");
            }

            // Aynı ürünün aynı depodan iki kez eklenmesini engelle.
            var duplicateItem = dto.Items
                .GroupBy(i => new { i.ProductId, i.WarehouseId })
                .Any(g => g.Count() > 1);

            if (duplicateItem)
            {
                throw new InvalidOperationException(
                    "Aynı ürün aynı depodan bir siparişte yalnızca bir kez eklenebilir.");
            }

            // 2. İstek içerisindeki ürünleri veritabanından getir.
            var productIds = dto.Items
                .Select(i => i.ProductId)
                .Distinct()
                .ToList();

            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            if (products.Count != productIds.Count)
            {
                throw new InvalidOperationException(
                    "Siparişte bulunmayan bir ürün mevcut.");
            }

            // Pasif ürünlerin sipariş edilmesini engelle.
            foreach (var product in products.Values)
            {
                if (!product.IsActive)
                {
                    throw new InvalidOperationException(
                        $"'{product.Name}' ürünü aktif olmadığı için sipariş edilemez.");
                }

                if (product.UnitPrice < 0)
                {
                    throw new InvalidOperationException(
                        $"'{product.Name}' ürününün fiyatı geçersiz.");
                }
            }

            // 3. Kullanılacak depoları getir.
            var warehouseIds = dto.Items
                .Select(i => i.WarehouseId)
                .Distinct()
                .ToList();

            var warehouses = await _context.Warehouses
                .Where(w => warehouseIds.Contains(w.Id))
                .ToDictionaryAsync(w => w.Id);

            if (warehouses.Count != warehouseIds.Count)
            {
                throw new InvalidOperationException(
                    "Siparişte bulunmayan bir depo mevcut.");
            }

            foreach (var warehouse in warehouses.Values)
            {
                if (!warehouse.IsActive)
                {
                    throw new InvalidOperationException(
                        $"'{warehouse.Name}' deposu aktif değil.");
                }
            }

            // 4. İlgili ürün-depo stok kayıtlarını getir.
            var inventoryBalances = await _context.InventoryBalances
                .Where(i =>
                    productIds.Contains(i.ProductId) &&
                    warehouseIds.Contains(i.WarehouseId))
                .ToListAsync();

            var inventoryDictionary = inventoryBalances
                .ToDictionary(
                    i => (i.ProductId, i.WarehouseId));

            // 5. Sipariş nesnesini oluştur.
            var order = new Order
            {
                CustomerName = dto.CustomerName.Trim(),
                CustomerEmail = dto.CustomerEmail?.Trim(),
                ShippingAddress = dto.ShippingAddress?.Trim(),
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            decimal totalAmount = 0;

            // 6. Her sipariş kalemi için stok kontrolü ve rezervasyon.
            foreach (var item in dto.Items)
            {
                var product = products[item.ProductId];

                var inventoryKey = (
                    item.ProductId,
                    item.WarehouseId
                );

                if (!inventoryDictionary.TryGetValue(
                    inventoryKey, out var inventory))
                {
                    throw new InvalidOperationException(
                        $"'{product.Name}' ürünü seçilen depoda bulunamadı.");
                }

                // Rezerve edilebilir stok miktarı.
                var availableQuantity =
                    inventory.QuantityOnHand -
                    inventory.ReservedQuantity;

                if (availableQuantity < item.Quantity)
                {
                    throw new InvalidOperationException(
                        $"'{product.Name}' ürünü için yetersiz stok. " +
                        $"Mevcut kullanılabilir stok: {availableQuantity}, " +
                        $"istenen miktar: {item.Quantity}.");
                }

                // Ürün fiyatını istemciden değil veritabanından al.
                var unitPrice = product.UnitPrice;

                var lineTotal = unitPrice * item.Quantity;

                totalAmount += lineTotal;

                // Sipariş kalemini oluştur.
                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    WarehouseId = item.WarehouseId,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice
                };

                order.Items.Add(orderItem);

                // Fiziksel stoktan düşmeden rezervasyon miktarını artır.
                inventory.ReservedQuantity += item.Quantity;
                inventory.UpdatedAt = DateTime.UtcNow;

                // Rezervasyon kaydını oluştur.
                var reservation = new StockReservation
                {
                    ProductId = product.Id,
                    WarehouseId = item.WarehouseId,
                    Quantity = item.Quantity,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow,
                    Order = order
                };

                _context.StockReservations.Add(reservation);
            }

            // 7. Sipariş toplamını hesapla.
            order.TotalAmount = totalAmount;

            // 8. Sipariş, kalemler ve rezervasyonları kaydet.
            // RowVersion sayesinde eşzamanlı stok güncellemeleri kontrol edilir.
            _context.Orders.Add(order);

            await _context.SaveChangesAsync();

            // 9. Bütün işlemler başarılıysa transaction tamamlanır.
            await transaction.CommitAsync();

            // 10. API yanıtını hazırla.
            return new OrderResponseDto
            {
                Id = order.Id,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                ShippingAddress = order.ShippingAddress,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                CreatedAt = order.CreatedAt,

                Items = order.Items.Select(i => new OrderItemResponseDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = products[i.ProductId].Name,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    LineTotal = i.UnitPrice * i.Quantity
                }).ToList()
            };
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();

            throw new InvalidOperationException(
                "Stok bilgisi başka bir işlem tarafından güncellendi. " +
                "Lütfen stok durumunu kontrol edip tekrar deneyin.");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}