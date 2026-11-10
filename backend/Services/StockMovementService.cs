using backend.Data;
using backend.DTOs;
using backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class StockMovementService
{
    private readonly AppDbContext _context;

    public StockMovementService(AppDbContext context)
    {
        _context = context;
    }

    // Stok hareketlerini listele
  
public async Task<List<StockMovementResponseDto>> GetAllAsync(
    int? productId = null,
    int? warehouseId = null,
    string? movementType = null,
    DateTime? from = null,
    DateTime? to = null)
{
    var query = _context.StockMovements
        .AsNoTracking()
        .AsQueryable();

    if (productId.HasValue)
    {
        query = query.Where(m => m.ProductId == productId.Value);
    }

    if (warehouseId.HasValue)
    {
        query = query.Where(m => m.WarehouseId == warehouseId.Value);
    }

    if (!string.IsNullOrWhiteSpace(movementType))
    {
        query = query.Where(m => m.MovementType == movementType);
    }

    if (from.HasValue)
    {
        query = query.Where(m => m.CreatedAt >= from.Value.Date);
    }

    if (to.HasValue)
    {
        var endDateExclusive = to.Value.Date.AddDays(1);
        query = query.Where(m => m.CreatedAt < endDateExclusive);
    }

    return await query
        .OrderByDescending(m => m.CreatedAt)
        .Select(m => new StockMovementResponseDto
        {
            Id = m.Id,
            ProductId = m.ProductId,
            ProductName = m.Product.Name,
            WarehouseId = m.WarehouseId,
            WarehouseName = m.Warehouse.Name,
            MovementType = m.MovementType,
            Quantity = m.Quantity,
            ReferenceNumber = m.ReferenceNumber,
            Notes = m.Notes,
            CreatedAt = m.CreatedAt
        })
        .ToListAsync();
}
    

public async Task<StockMovementReportDto> GetReportAsync()
{
    var totalInbound = await _context.StockMovements
        .Where(m => m.MovementType == "Inbound")
        .SumAsync(m => (int?)m.Quantity) ?? 0;

    var totalOutbound = await _context.StockMovements
        .Where(m => m.MovementType == "Outbound")
        .SumAsync(m => (int?)m.Quantity) ?? 0;

    var totalMovements = await _context.StockMovements
        .CountAsync();

    return new StockMovementReportDto
    {
        TotalMovements = totalMovements,
        TotalInboundQuantity = totalInbound,
        TotalOutboundQuantity = totalOutbound,
        NetStockChange = totalInbound - totalOutbound,
        GeneratedAt = DateTime.UtcNow
    };
}


    // Yeni stok hareketi oluştur
    public async Task<StockMovementResponseDto> CreateAsync(
        CreateStockMovementDto dto)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            // Hareket türünü kontrol et
            if (dto.MovementType != "Inbound" &&
                dto.MovementType != "Outbound")
            {
                throw new InvalidOperationException(
                    "MovementType Inbound veya Outbound olmalıdır.");
            }

            // Ürünü kontrol et
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == dto.ProductId);

            if (product is null)
            {
                throw new KeyNotFoundException(
                    "Belirtilen ürün bulunamadı.");
            }

            if (!product.IsActive)
            {
                throw new InvalidOperationException(
                    "Pasif ürün için stok hareketi oluşturulamaz.");
            }

            // Depoyu kontrol et
            var warehouse = await _context.Warehouses
                .FirstOrDefaultAsync(w => w.Id == dto.WarehouseId);

            if (warehouse is null)
            {
                throw new KeyNotFoundException(
                    "Belirtilen depo bulunamadı.");
            }

            if (!warehouse.IsActive)
            {
                throw new InvalidOperationException(
                    "Pasif depo için stok hareketi oluşturulamaz.");
            }

            // Ürün-depo stok kaydını getir
            var inventory = await _context.InventoryBalances
                .FirstOrDefaultAsync(i =>
                    i.ProductId == dto.ProductId &&
                    i.WarehouseId == dto.WarehouseId);

            if (inventory is null)
            {
                throw new InvalidOperationException(
                    "Bu ürün için belirtilen depoda stok kaydı bulunamadı.");
            }

            // Stok giriş işlemi
            if (dto.MovementType == "Inbound")
            {
                inventory.QuantityOnHand = checked(
                    inventory.QuantityOnHand + dto.Quantity);
            }

            // Stok çıkış işlemi
            if (dto.MovementType == "Outbound")
            {
                int availableQuantity =
                    inventory.QuantityOnHand - inventory.ReservedQuantity;

                if (availableQuantity < dto.Quantity)
                {
                    throw new InvalidOperationException(
                        $"Yetersiz stok. Kullanılabilir miktar: {availableQuantity}");
                }

                inventory.QuantityOnHand -= dto.Quantity;
            }

            inventory.UpdatedAt = DateTime.UtcNow;

            // Stok hareketi kaydını oluştur
            var movement = new StockMovement
            {
                ProductId = dto.ProductId,
                WarehouseId = dto.WarehouseId,
                MovementType = dto.MovementType,
                Quantity = dto.Quantity,
                ReferenceNumber = dto.ReferenceNumber,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.StockMovements.Add(movement);

            // Stok ve hareket kaydını birlikte kaydet
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return new StockMovementResponseDto
            {
                Id = movement.Id,
                ProductId = movement.ProductId,
                ProductName = product.Name,
                WarehouseId = movement.WarehouseId,
                WarehouseName = warehouse.Name,
                MovementType = movement.MovementType,
                Quantity = movement.Quantity,
                ReferenceNumber = movement.ReferenceNumber,
                Notes = movement.Notes,
                CreatedAt = movement.CreatedAt
            };
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();

            throw;
        }
        catch
        {
            await transaction.RollbackAsync();

            throw;
        }
    }
}