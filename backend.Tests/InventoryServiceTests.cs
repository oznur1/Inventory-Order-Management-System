
using backend.Data;
using backend.DTOs.Inventory;
using backend.Entities;
using backend.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Tests;

public class InventoryServiceTests
{
    // 1. Tüm stok kayıtları listelenmeli.
    [Fact]
    public async Task GetAllAsync_ReturnsAllInventoryRecords()
    {
        await using var db = await CreateDatabaseAsync();
        var context = db.Context;

        var first = await AddInventoryAsync(context, 20);
        var second = await AddInventoryAsync(context, 30);

        var service = new InventoryService(context);

        var result = await service.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.ProductId == first.Product.Id);
        Assert.Contains(result, x => x.ProductId == second.Product.Id);

        Assert.All(result, item =>
        {
            Assert.NotEmpty(item.ProductName);
            Assert.NotEmpty(item.WarehouseName);
        });
    }

    // 2. Depo ID filtresi çalışmalı.
    [Fact]
    public async Task GetAllAsync_WithWarehouseId_ReturnsMatchingRecords()
    {
        await using var db = await CreateDatabaseAsync();
        var context = db.Context;

        var first = await AddInventoryAsync(context, 20);
        await AddInventoryAsync(context, 30);

        var service = new InventoryService(context);

        var result = await service.GetAllAsync(
            warehouseId: first.Warehouse.Id);

        var inventory = Assert.Single(result);

        Assert.Equal(first.Warehouse.Id, inventory.WarehouseId);
        Assert.Equal(first.Product.Id, inventory.ProductId);
    }

    // 3. Ürün ID filtresi çalışmalı.
    [Fact]
    public async Task GetAllAsync_WithProductId_ReturnsMatchingRecords()
    {
        await using var db = await CreateDatabaseAsync();
        var context = db.Context;

        var first = await AddInventoryAsync(context, 20);
        await AddInventoryAsync(context, 30);

        var service = new InventoryService(context);

        var result = await service.GetAllAsync(
            productId: first.Product.Id);

        var inventory = Assert.Single(result);

        Assert.Equal(first.Product.Id, inventory.ProductId);
        Assert.Equal(20, inventory.QuantityOnHand);
    }

    // 4. Ürün ve depo filtreleri birlikte çalışmalı.
    [Fact]
    public async Task GetAllAsync_WithCombinedFilters_ReturnsMatchingRecords()
    {
        await using var db = await CreateDatabaseAsync();
        var context = db.Context;

        var first = await AddInventoryAsync(context, 20);
        await AddInventoryAsync(context, 30);

        var service = new InventoryService(context);

        var result = await service.GetAllAsync(
            warehouseId: first.Warehouse.Id,
            productId: first.Product.Id);

        var inventory = Assert.Single(result);

        Assert.Equal(first.Product.Id, inventory.ProductId);
        Assert.Equal(first.Warehouse.Id, inventory.WarehouseId);
    }

    // 5. Düşük stok kayıtları doğru bulunmalı ve sıralanmalı.
    [Fact]
    public async Task GetLowStockAsync_ReturnsLowStockRecordsOrderedByQuantity()
    {
        await using var db = await CreateDatabaseAsync();
        var context = db.Context;

        var first = await AddInventoryAsync(
            context,
            quantityOnHand: 5,
            reservedQuantity: 2,
            reorderLevel: 10);

        var second = await AddInventoryAsync(
            context,
            quantityOnHand: 2,
            reservedQuantity: 1,
            reorderLevel: 5);

        await AddInventoryAsync(
            context,
            quantityOnHand: 20,
            reorderLevel: 5);

        var service = new InventoryService(context);

        var result = await service.GetLowStockAsync();

        Assert.Equal(2, result.Count);

        Assert.Equal(2, result[0].QuantityOnHand);
        Assert.Equal(5, result[1].QuantityOnHand);

        // StockDeficit = ReorderLevel - QuantityOnHand
        Assert.Equal(3, result[0].StockDeficit);
        Assert.Equal(5, result[1].StockDeficit);

        Assert.Equal(second.Product.Name, result[0].ProductName);
        Assert.Equal(first.Product.Name, result[1].ProductName);

        Assert.Equal(1, result[0].AvailableQuantity);
        Assert.Equal(3, result[1].AvailableQuantity);
    }

    // 6. Düşük stok yoksa boş liste dönmeli.
    [Fact]
    public async Task GetLowStockAsync_WithNoLowStock_ReturnsEmptyList()
    {
        await using var db = await CreateDatabaseAsync();
        var context = db.Context;

        await AddInventoryAsync(
            context,
            quantityOnHand: 50,
            reorderLevel: 10);

        var service = new InventoryService(context);

        var result = await service.GetLowStockAsync();

        Assert.Empty(result);
    }

    // 7. Stok kaydı ID ile bulunmalı.
    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsInventory()
    {
        await using var db = await CreateDatabaseAsync();
        var context = db.Context;

        var data = await AddInventoryAsync(
            context,
            quantityOnHand: 20,
            reservedQuantity: 5,
            reorderLevel: 8);

        var service = new InventoryService(context);

        var result = await service.GetByIdAsync(data.Inventory.Id);

        Assert.NotNull(result);

        Assert.Equal(data.Inventory.Id, result.Id);
        Assert.Equal(data.Product.Id, result.ProductId);
        Assert.Equal(data.Product.Name, result.ProductName);
        Assert.Equal(data.Warehouse.Id, result.WarehouseId);
        Assert.Equal(data.Warehouse.Name, result.WarehouseName);

        Assert.Equal(20, result.QuantityOnHand);
        Assert.Equal(5, result.ReservedQuantity);
        Assert.Equal(15, result.AvailableQuantity);
        Assert.Equal(8, result.ReorderLevel);
    }

    // 8. Olmayan ID için null dönmeli.
    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
    {
        await using var db = await CreateDatabaseAsync();
        var context = db.Context;

        var service = new InventoryService(context);

        var result = await service.GetByIdAsync(999);

        Assert.Null(result);
    }

    // 9. Yeni stok kaydı başarıyla oluşturulmalı.
    [Fact]
    public async Task CreateAsync_WithValidData_CreatesInventory()
    {
        await using var db = await CreateDatabaseAsync();
        var context = db.Context;

        var data = await AddProductAndWarehouseAsync(context);

        var service = new InventoryService(context);

        var result = await service.CreateAsync(new CreateInventoryDto
        {
            ProductId = data.Product.Id,
            WarehouseId = data.Warehouse.Id,
            QuantityOnHand = 25,
            ReorderLevel = 5
        });

        Assert.True(result.Id > 0);
        Assert.Equal(data.Product.Id, result.ProductId);
        Assert.Equal(data.Warehouse.Id, result.WarehouseId);
        Assert.Equal(25, result.QuantityOnHand);
        Assert.Equal(0, result.ReservedQuantity);
        Assert.Equal(25, result.AvailableQuantity);
        Assert.Equal(5, result.ReorderLevel);

        Assert.Equal(1, await context.InventoryBalances.CountAsync());
    }

    // 10. Olmayan ürün için stok kaydı oluşturulmamalı.
    [Fact]
    public async Task CreateAsync_WithNonExistingProduct_ThrowsException()
    {
        await using var db = await CreateDatabaseAsync();
        var context = db.Context;

        var data = await AddProductAndWarehouseAsync(context);

        var service = new InventoryService(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.CreateAsync(new CreateInventoryDto
            {
                ProductId = 999,
                WarehouseId = data.Warehouse.Id,
                QuantityOnHand = 10,
                ReorderLevel = 5
            }));

        Assert.Empty(await context.InventoryBalances.ToListAsync());
    }

    // 11. Olmayan depo için stok kaydı oluşturulmamalı.
    [Fact]
    public async Task CreateAsync_WithNonExistingWarehouse_ThrowsException()
    {
        await using var db = await CreateDatabaseAsync();
        var context = db.Context;

        var data = await AddProductAndWarehouseAsync(context);

        var service = new InventoryService(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.CreateAsync(new CreateInventoryDto
            {
                ProductId = data.Product.Id,
                WarehouseId = 999,
                QuantityOnHand = 10,
                ReorderLevel = 5
            }));

        Assert.Empty(await context.InventoryBalances.ToListAsync());
    }

    // 12. Aynı ürün ve depo için ikinci kayıt oluşturulmamalı.
    [Fact]
    public async Task CreateAsync_WithDuplicateProductAndWarehouse_ThrowsException()
    {
        await using var db = await CreateDatabaseAsync();
        var context = db.Context;

        var data = await AddInventoryAsync(context, 20);

        var service = new InventoryService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateInventoryDto
            {
                ProductId = data.Product.Id,
                WarehouseId = data.Warehouse.Id,
                QuantityOnHand = 30,
                ReorderLevel = 5
            }));

        Assert.Equal(1, await context.InventoryBalances.CountAsync());
    }

    // 13. Sıfır stokla yeni kayıt oluşturulabilmeli.
    [Fact]
    public async Task CreateAsync_WithZeroQuantity_CreatesInventory()
    {
        await using var db = await CreateDatabaseAsync();
        var context = db.Context;

        var data = await AddProductAndWarehouseAsync(context);

        var service = new InventoryService(context);

        var result = await service.CreateAsync(new CreateInventoryDto
        {
            ProductId = data.Product.Id,
            WarehouseId = data.Warehouse.Id,
            QuantityOnHand = 0,
            ReorderLevel = 5
        });

        Assert.Equal(0, result.QuantityOnHand);
        Assert.Equal(0, result.AvailableQuantity);
    }

    // 14. Stok kaydı başarıyla güncellenmeli.
    [Fact]
    public async Task UpdateAsync_WithExistingId_UpdatesInventory()
    {
        await using var db = await CreateDatabaseAsync();
        var context = db.Context;

        var data = await AddInventoryAsync(
            context,
            quantityOnHand: 20,
            reservedQuantity: 5,
            reorderLevel: 10);

        var service = new InventoryService(context);

        var result = await service.UpdateAsync(
            data.Inventory.Id,
            new UpdateInventoryDto
            {
                QuantityOnHand = 30,
                ReorderLevel = 15
            });

        Assert.NotNull(result);
        Assert.Equal(30, result.QuantityOnHand);
        Assert.Equal(5, result.ReservedQuantity);
        Assert.Equal(25, result.AvailableQuantity);
        Assert.Equal(15, result.ReorderLevel);

        var inventory = await context.InventoryBalances.SingleAsync();

        Assert.Equal(30, inventory.QuantityOnHand);
        Assert.Equal(15, inventory.ReorderLevel);
    }

    // 15. Olmayan stok kaydı güncellenmemeli.
    [Fact]
    public async Task UpdateAsync_WithNonExistingId_ReturnsNull()
    {
        await using var db = await CreateDatabaseAsync();
        var context = db.Context;

        var service = new InventoryService(context);

        var result = await service.UpdateAsync(
            999,
            new UpdateInventoryDto
            {
                QuantityOnHand = 20,
                ReorderLevel = 5
            });

        Assert.Null(result);
    }

    // 16. Stok kaydı başarıyla silinmeli.
    [Fact]
    public async Task DeleteAsync_WithExistingId_DeletesInventory()
    {
        await using var db = await CreateDatabaseAsync();
        var context = db.Context;

        var data = await AddInventoryAsync(context, 20);

        var service = new InventoryService(context);

        var result = await service.DeleteAsync(data.Inventory.Id);

        Assert.True(result);
        Assert.Empty(await context.InventoryBalances.ToListAsync());
    }

    // 17. Olmayan stok kaydı silinmeye çalışılırsa false dönmeli.
    [Fact]
    public async Task DeleteAsync_WithNonExistingId_ReturnsFalse()
    {
        await using var db = await CreateDatabaseAsync();
        var context = db.Context;

        var service = new InventoryService(context);

        var result = await service.DeleteAsync(999);

        Assert.False(result);
    }

    // 18. Genel stok raporu doğru hesaplanmalı.
    [Fact]
    public async Task GetReportAsync_ReturnsCorrectTotals()
    {
        await using var db = await CreateDatabaseAsync();
        var context = db.Context;

        var first = await AddInventoryAsync(
            context,
            quantityOnHand: 20,
            reservedQuantity: 5,
            reorderLevel: 10);

        await AddInventoryAsync(
            context,
            quantityOnHand: 5,
            reservedQuantity: 2,
            reorderLevel: 10);

        // Aynı ürün, farklı bir depo kaydı.
        var anotherWarehouse = new Warehouse
        {
            Code = $"WH-{Guid.NewGuid():N}",
            Name = "Second Warehouse",
            Address = "Test Address",
            IsActive = true
        };

        context.Warehouses.Add(anotherWarehouse);
        await context.SaveChangesAsync();

        context.InventoryBalances.Add(new InventoryBalance
        {
            ProductId = first.Product.Id,
            WarehouseId = anotherWarehouse.Id,
            QuantityOnHand = 15,
            ReservedQuantity = 3,
            ReorderLevel = 5,
            UpdatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        var service = new InventoryService(context);

        var result = await service.GetReportAsync();

        Assert.Equal(3, result.TotalInventoryRecords);
        Assert.Equal(2, result.TotalProducts);
        Assert.Equal(3, result.TotalWarehouses);
        Assert.Equal(40, result.TotalQuantityOnHand);
        Assert.Equal(10, result.TotalReservedQuantity);
        Assert.Equal(30, result.TotalAvailableQuantity);

        // Yalnızca 5 adet stok bulunan kayıt düşük stok seviyesinde.
        Assert.Equal(1, result.LowStockCount);

        Assert.NotEqual(default, result.GeneratedAt);
    }

    // 19. Boş veritabanında rapor değerleri sıfır olmalı.
    [Fact]
    public async Task GetReportAsync_WithNoInventory_ReturnsZeroTotals()
    {
        await using var db = await CreateDatabaseAsync();
        var context = db.Context;

        var service = new InventoryService(context);

        var result = await service.GetReportAsync();

        Assert.Equal(0, result.TotalInventoryRecords);
        Assert.Equal(0, result.TotalProducts);
        Assert.Equal(0, result.TotalWarehouses);
        Assert.Equal(0, result.TotalQuantityOnHand);
        Assert.Equal(0, result.TotalReservedQuantity);
        Assert.Equal(0, result.TotalAvailableQuantity);
        Assert.Equal(0, result.LowStockCount);
        Assert.NotEqual(default, result.GeneratedAt);
    }

    // TEST VERİTABANI

    private static async Task<TestDatabase> CreateDatabaseAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new AppDbContext(options);

        try
        {
            await InitializeDatabaseAsync(context);

            return new TestDatabase(connection, context);
        }
        catch
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
            throw;
        }
    }

    // SQLite için RowVersion varsayılan değeri.
    private static async Task InitializeDatabaseAsync(AppDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        await context.Database.ExecuteSqlRawAsync(
            "DROP TABLE IF EXISTS InventoryBalances;");

        await context.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE InventoryBalances (
                Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ProductId INTEGER NOT NULL,
                WarehouseId INTEGER NOT NULL,
                QuantityOnHand INTEGER NOT NULL,
                ReservedQuantity INTEGER NOT NULL DEFAULT 0,
                ReorderLevel INTEGER NOT NULL,
                RowVersion BLOB NOT NULL DEFAULT X'00',
                UpdatedAt TEXT NOT NULL,
                CONSTRAINT CK_InventoryBalances_QuantityOnHand
                    CHECK (QuantityOnHand >= 0),
                CONSTRAINT CK_InventoryBalances_ReservedQuantity
                    CHECK (
                        ReservedQuantity >= 0
                        AND ReservedQuantity <= QuantityOnHand
                    ),
                CONSTRAINT FK_InventoryBalances_Products_ProductId
                    FOREIGN KEY (ProductId)
                    REFERENCES Products(Id)
                    ON DELETE RESTRICT,
                CONSTRAINT FK_InventoryBalances_Warehouses_WarehouseId
                    FOREIGN KEY (WarehouseId)
                    REFERENCES Warehouses(Id)
                    ON DELETE RESTRICT
            );

            CREATE UNIQUE INDEX
                IX_InventoryBalances_ProductId_WarehouseId
            ON InventoryBalances(ProductId, WarehouseId);
            """);
    }

    // Ürün ve depo oluşturma.
    private static async Task<(Product Product, Warehouse Warehouse)>
        AddProductAndWarehouseAsync(AppDbContext context)
    {
        var product = new Product
        {
            SKU = $"SKU-{Guid.NewGuid():N}",
            Name = "Test Product",
            Description = "Test Description",
            UnitPrice = 100,
            IsActive = true
        };

        var warehouse = new Warehouse
        {
            Code = $"WH-{Guid.NewGuid():N}",
            Name = "Test Warehouse",
            Address = "Test Address",
            IsActive = true
        };

        context.Products.Add(product);
        context.Warehouses.Add(warehouse);

        await context.SaveChangesAsync();

        return (product, warehouse);
    }

    // Ürün, depo ve stok kaydı oluşturma.
    private static async Task<(
        Product Product,
        Warehouse Warehouse,
        InventoryBalance Inventory)>
        AddInventoryAsync(
            AppDbContext context,
            int quantityOnHand,
            int reservedQuantity = 0,
            int reorderLevel = 10)
    {
        var data = await AddProductAndWarehouseAsync(context);

        var inventory = new InventoryBalance
        {
            ProductId = data.Product.Id,
            WarehouseId = data.Warehouse.Id,
            QuantityOnHand = quantityOnHand,
            ReservedQuantity = reservedQuantity,
            ReorderLevel = reorderLevel,
            UpdatedAt = DateTime.UtcNow
        };

        context.InventoryBalances.Add(inventory);

        await context.SaveChangesAsync();

        return (data.Product, data.Warehouse, inventory);
    }

    // SQLite bağlantısını test boyunca açık tutar.
    private sealed class TestDatabase(
        SqliteConnection connection,
        AppDbContext context) : IAsyncDisposable
    {
        public AppDbContext Context => context;

        public async ValueTask DisposeAsync()
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }
}
