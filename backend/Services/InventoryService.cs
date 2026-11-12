
using backend.Data;
using backend.DTOs.Inventory;
using backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class InventoryService
{
    private readonly AppDbContext _context;

    public InventoryService(AppDbContext context)
    {
        _context = context;
    }

    // Tüm stok kayıtlarını listele
    public async Task<List<InventoryResponseDto>> GetAllAsync(
        int? warehouseId = null,
        int? productId = null)
    {
        var query = _context.InventoryBalances
            .AsNoTracking()
            .AsQueryable();

        if (warehouseId.HasValue)
        {
            query = query.Where(
                i => i.WarehouseId == warehouseId.Value);
        }

        if (productId.HasValue)
        {
            query = query.Where(
                i => i.ProductId == productId.Value);
        }

        return await query
            .Select(i => new InventoryResponseDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product.Name,
                WarehouseId = i.WarehouseId,
                WarehouseName = i.Warehouse.Name,
                QuantityOnHand = i.QuantityOnHand,
                ReservedQuantity = i.ReservedQuantity,
                AvailableQuantity =
                    i.QuantityOnHand - i.ReservedQuantity,
                ReorderLevel = i.ReorderLevel,
                UpdatedAt = i.UpdatedAt
            })
            .ToListAsync();
    }

    // Düşük stok seviyesindeki ürünleri listele
    public async Task<List<LowStockAlertDto>> GetLowStockAsync()
    {
        return await _context.InventoryBalances
            .AsNoTracking()
            .Where(i => i.QuantityOnHand <= i.ReorderLevel)
            .Select(i => new LowStockAlertDto
            {
                InventoryId = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product.Name,
                WarehouseId = i.WarehouseId,
                WarehouseName = i.Warehouse.Name,
                QuantityOnHand = i.QuantityOnHand,
                ReservedQuantity = i.ReservedQuantity,
                AvailableQuantity =
                    i.QuantityOnHand - i.ReservedQuantity,
                ReorderLevel = i.ReorderLevel,
                StockDeficit =
                    i.ReorderLevel - i.QuantityOnHand,
                UpdatedAt = i.UpdatedAt
            })
            .OrderBy(i => i.QuantityOnHand)
            .ToListAsync();
    }

    // ID ile stok kaydını getir
    public async Task<InventoryResponseDto?> GetByIdAsync(int id)
    {
        return await _context.InventoryBalances
            .AsNoTracking()
            .Where(i => i.Id == id)
            .Select(i => new InventoryResponseDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product.Name,
                WarehouseId = i.WarehouseId,
                WarehouseName = i.Warehouse.Name,
                QuantityOnHand = i.QuantityOnHand,
                ReservedQuantity = i.ReservedQuantity,
                AvailableQuantity =
                    i.QuantityOnHand - i.ReservedQuantity,
                ReorderLevel = i.ReorderLevel,
                UpdatedAt = i.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    // Yeni stok kaydı oluştur
    public async Task<InventoryResponseDto> CreateAsync(
        CreateInventoryDto dto)
    {
        bool productExists = await _context.Products
            .AnyAsync(p => p.Id == dto.ProductId);

        if (!productExists)
        {
            throw new KeyNotFoundException(
                "Belirtilen ürün bulunamadı.");
        }

        bool warehouseExists = await _context.Warehouses
            .AnyAsync(w => w.Id == dto.WarehouseId);

        if (!warehouseExists)
        {
            throw new KeyNotFoundException(
                "Belirtilen depo bulunamadı.");
        }

        bool inventoryExists = await _context.InventoryBalances
            .AnyAsync(i =>
                i.ProductId == dto.ProductId &&
                i.WarehouseId == dto.WarehouseId);

        if (inventoryExists)
        {
            throw new InvalidOperationException(
                "Bu ürün için bu depoda zaten stok kaydı var.");
        }

        var inventory = new InventoryBalance
        {
            ProductId = dto.ProductId,
            WarehouseId = dto.WarehouseId,
            QuantityOnHand = dto.QuantityOnHand,
            ReorderLevel = dto.ReorderLevel,
            UpdatedAt = DateTime.UtcNow
        };

        _context.InventoryBalances.Add(inventory);

        await _context.SaveChangesAsync();

        return await GetByIdAsync(inventory.Id)
            ?? throw new InvalidOperationException(
                "Oluşturulan stok kaydı getirilemedi.");
    }

    // Mevcut stok miktarını güncelle
    public async Task<InventoryResponseDto?> UpdateAsync(
        int id,
        UpdateInventoryDto dto)
    {
        var inventory = await _context.InventoryBalances
            .FirstOrDefaultAsync(i => i.Id == id);

        if (inventory is null)
        {
            return null;
        }

        if (dto.QuantityOnHand < inventory.ReservedQuantity)
        {
            throw new InvalidOperationException(
                $"Stok miktarı, rezerve edilen miktardan düşük olamaz. " +
                $"Rezerve edilen miktar: {inventory.ReservedQuantity}");
        }

        inventory.QuantityOnHand = dto.QuantityOnHand;
        inventory.ReorderLevel = dto.ReorderLevel;
        inventory.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    // Stok kaydını sil
    public async Task<bool> DeleteAsync(int id)
    {
        var inventory = await _context.InventoryBalances
            .FirstOrDefaultAsync(i => i.Id == id);

        if (inventory is null)
        {
            return false;
        }

        bool hasStockMovements = await _context.StockMovements
            .AnyAsync(m =>
                m.ProductId == inventory.ProductId &&
                m.WarehouseId == inventory.WarehouseId);

        bool hasReservations = await _context.StockReservations
            .AnyAsync(r =>
                r.ProductId == inventory.ProductId &&
                r.WarehouseId == inventory.WarehouseId);

        if (hasStockMovements || hasReservations)
        {
            throw new InvalidOperationException(
                "Inventory cannot be deleted because related stock movements or reservations exist.");
        }

        _context.InventoryBalances.Remove(inventory);

        await _context.SaveChangesAsync();

        return true;
    }

    // Genel stok raporunu oluştur
    public async Task<InventoryReportDto> GetReportAsync()
    {
        // Ürün sayısı stok kayıtlarından bağımsız hesaplanır.
        var totalProducts = await _context.Products
            .AsNoTracking()
            .CountAsync();

        // Depo sayısı depo tablosundan hesaplanır.
        var totalWarehouses = await _context.Warehouses
            .AsNoTracking()
            .CountAsync();

        // Stok miktarları InventoryBalances üzerinden hesaplanır.
        var inventoryData = await _context.InventoryBalances
            .AsNoTracking()
            .GroupBy(i => 1)
            .Select(g => new
            {
                TotalInventoryRecords = g.Count(),

                TotalQuantityOnHand = g.Sum(
                    i => i.QuantityOnHand),

                TotalReservedQuantity = g.Sum(
                    i => i.ReservedQuantity),

                TotalAvailableQuantity = g.Sum(
                    i => i.QuantityOnHand - i.ReservedQuantity),

                LowStockCount = g.Count(
                    i => i.QuantityOnHand <= i.ReorderLevel)
            })
            .FirstOrDefaultAsync();

        return new InventoryReportDto
        {
            TotalInventoryRecords =
                inventoryData?.TotalInventoryRecords ?? 0,

            TotalProducts = totalProducts,

            TotalWarehouses = totalWarehouses,

            TotalQuantityOnHand =
                inventoryData?.TotalQuantityOnHand ?? 0,

            TotalReservedQuantity =
                inventoryData?.TotalReservedQuantity ?? 0,

            TotalAvailableQuantity =
                inventoryData?.TotalAvailableQuantity ?? 0,

            LowStockCount =
                inventoryData?.LowStockCount ?? 0,

            GeneratedAt = DateTime.UtcNow
        };
    }
}
