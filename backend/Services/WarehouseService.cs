
using backend.Data;
using backend.DTOs.Warehouses;
using backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class WarehouseService
{
    private readonly AppDbContext _context;

    public WarehouseService(AppDbContext context)
    {
        _context = context;
    }

    // Tüm depoları listele
    public async Task<List<WarehouseResponseDto>> GetAllAsync()
    {
        return await _context.Warehouses
            .AsNoTracking()
            .Select(w => new WarehouseResponseDto
            {
                Id = w.Id,
                Code = w.Code,
                Name = w.Name,
                Address = w.Address,
                IsActive = w.IsActive
            })
            .ToListAsync();
    }

    // ID ile depo getir
    public async Task<WarehouseResponseDto?> GetByIdAsync(int id)
    {
        return await _context.Warehouses
            .AsNoTracking()
            .Where(w => w.Id == id)
            .Select(w => new WarehouseResponseDto
            {
                Id = w.Id,
                Code = w.Code,
                Name = w.Name,
                Address = w.Address,
                IsActive = w.IsActive
            })
            .FirstOrDefaultAsync();
    }

    // Yeni depo oluştur
    public async Task<WarehouseResponseDto> CreateAsync(
        CreateWarehouseDto dto)
    {
        string normalizedCode = dto.Code.Trim();

        bool codeExists = await _context.Warehouses
            .AnyAsync(w => w.Code == normalizedCode);

        if (codeExists)
        {
            throw new InvalidOperationException(
                "Bu depo kodu zaten kullanılıyor.");
        }

        var warehouse = new Warehouse
        {
            Code = normalizedCode,
            Name = dto.Name.Trim(),
            Address = dto.Address?.Trim(),
            IsActive = true
        };

        _context.Warehouses.Add(warehouse);
        await _context.SaveChangesAsync();

        return new WarehouseResponseDto
        {
            Id = warehouse.Id,
            Code = warehouse.Code,
            Name = warehouse.Name,
            Address = warehouse.Address,
            IsActive = warehouse.IsActive
        };
    }

    // Mevcut depoyu güncelle
    public async Task<WarehouseResponseDto?> UpdateAsync(
        int id,
        UpdateWarehouseDto dto)
    {
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.Id == id);

        if (warehouse is null)
        {
            return null;
        }

        string normalizedCode = dto.Code.Trim();

        bool codeExists = await _context.Warehouses
            .AnyAsync(w => w.Code == normalizedCode && w.Id != id);

        if (codeExists)
        {
            throw new InvalidOperationException(
                "Bu depo kodu başka bir depo tarafından kullanılıyor.");
        }

        warehouse.Code = normalizedCode;
        warehouse.Name = dto.Name.Trim();
        warehouse.Address = dto.Address?.Trim();
        warehouse.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();

        return new WarehouseResponseDto
        {
            Id = warehouse.Id,
            Code = warehouse.Code,
            Name = warehouse.Name,
            Address = warehouse.Address,
            IsActive = warehouse.IsActive
        };
    }

    // Depoyu sil
    public async Task<bool> DeleteAsync(int id)
    {
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.Id == id);

        if (warehouse is null)
        {
            return false;
        }

        bool hasInventory = await _context.InventoryBalances
            .AnyAsync(i => i.WarehouseId == id);

        bool hasStockMovements = await _context.StockMovements
            .AnyAsync(s => s.WarehouseId == id);

        bool hasReservations = await _context.StockReservations
            .AnyAsync(s => s.WarehouseId == id);

        if (hasInventory || hasStockMovements || hasReservations)
        {
            throw new InvalidOperationException(
                "İlişkili stok veya rezervasyon kayıtları bulunan depo silinemez.");
        }

        _context.Warehouses.Remove(warehouse);
        await _context.SaveChangesAsync();

        return true;
    }
}
