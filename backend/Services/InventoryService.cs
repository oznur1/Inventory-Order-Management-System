
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
    public async Task<List<InventoryResponseDto>> GetAllAsync()
    {
        return await _context.InventoryBalances
            .AsNoTracking()
            .Select(i => new InventoryResponseDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product.Name,
                WarehouseId = i.WarehouseId,
                WarehouseName = i.Warehouse.Name,
                QuantityOnHand = i.QuantityOnHand,
                ReorderLevel = i.ReorderLevel,
                UpdatedAt = i.UpdatedAt
            })
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

        _context.InventoryBalances.Remove(inventory);
        await _context.SaveChangesAsync();

        return true;
    }
}
