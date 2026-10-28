
using backend.Data;
using backend.DTOs.Products;
using backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class ProductService
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context)
    {
        _context = context;
    }

    // Tüm ürünleri listele
    public async Task<List<ProductResponseDto>> GetAllAsync()
    {
        return await _context.Products
            .AsNoTracking()
            .Select(p => new ProductResponseDto
            {
                Id = p.Id,
                SKU = p.SKU,
                Name = p.Name,
                Description = p.Description,
                UnitPrice = p.UnitPrice,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();
    }

    // ID ile ürün getir
    public async Task<ProductResponseDto?> GetByIdAsync(int id)
    {
        return await _context.Products
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ProductResponseDto
            {
                Id = p.Id,
                SKU = p.SKU,
                Name = p.Name,
                Description = p.Description,
                UnitPrice = p.UnitPrice,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    // Yeni ürün oluştur
    public async Task<ProductResponseDto> CreateAsync(
        CreateProductDto dto)
    {
        string normalizedSku = dto.SKU.Trim();

        bool skuExists = await _context.Products
            .AnyAsync(p => p.SKU == normalizedSku);

        if (skuExists)
        {
            throw new InvalidOperationException(
                "Bu SKU kodu zaten kullanılıyor.");
        }

        var product = new Product
        {
            SKU = normalizedSku,
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            UnitPrice = dto.UnitPrice,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return new ProductResponseDto
        {
            Id = product.Id,
            SKU = product.SKU,
            Name = product.Name,
            Description = product.Description,
            UnitPrice = product.UnitPrice,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt
        };
    }

    // Mevcut ürünü güncelle
    public async Task<ProductResponseDto?> UpdateAsync(
        int id,
        UpdateProductDto dto)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null)
        {
            return null;
        }

        string normalizedSku = dto.SKU.Trim();

        bool skuExists = await _context.Products
            .AnyAsync(p => p.SKU == normalizedSku && p.Id != id);

        if (skuExists)
        {
            throw new InvalidOperationException(
                "Bu SKU kodu başka bir ürün tarafından kullanılıyor.");
        }

        product.SKU = normalizedSku;
        product.Name = dto.Name.Trim();
        product.Description = dto.Description?.Trim();
        product.UnitPrice = dto.UnitPrice;
        product.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();

        return new ProductResponseDto
        {
            Id = product.Id,
            SKU = product.SKU,
            Name = product.Name,
            Description = product.Description,
            UnitPrice = product.UnitPrice,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt
        };
    }

    // Ürünü sil
    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null)
        {
            return false;
        }

        bool hasInventory = await _context.InventoryBalances
            .AnyAsync(i => i.ProductId == id);

        bool hasStockMovements = await _context.StockMovements
            .AnyAsync(s => s.ProductId == id);

        bool hasOrderItems = await _context.OrderItems
            .AnyAsync(o => o.ProductId == id);

        bool hasReservations = await _context.StockReservations
            .AnyAsync(s => s.ProductId == id);

        if (hasInventory || hasStockMovements ||
            hasOrderItems || hasReservations)
        {
            throw new InvalidOperationException(
                "İlişkili stok veya sipariş kayıtları bulunan ürün silinemez.");
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return true;
    }
}
