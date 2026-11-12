
using backend.Data;
using backend.DTOs;
using backend.Entities;
using backend.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Tests;

public class StockMovementServiceTests
{
    // 1. INBOUND: Stok miktarı artmalı.
    [Fact]
    public async Task CreateAsync_WithInboundMovement_IncreasesStock()
    {
        await using var db = await CreateDatabaseAsync();
        var context = db.Context;

        var data = await AddInventoryAsync(context, 20);

        var service = new StockMovementService(context);

        var result = await service.CreateAsync(new CreateStockMovementDto
        {
            ProductId = data.Product.Id,
            WarehouseId = data.Warehouse.Id,
            MovementType = "Inbound",
            Quantity = 5,
            ReferenceNumber = "IN-001",
            Notes = "Test inbound"
        });

        var inventory = await context.InventoryBalances.SingleAsync();
        var movement = await context.StockMovements.SingleAsync();

        Assert.Equal(25, inventory.QuantityOnHand);
        Assert.Equal("Inbound", movement.MovementType);
        Assert.Equal(5, movement.Quantity);
        Assert.Equal("IN-001", result.ReferenceNumber);
        Assert.Equal(data.Product.Name, result.ProductName);
        Assert.Equal(data.Warehouse.Name, result.WarehouseName);
    }

    // 2. OUTBOUND: Stok miktarı azalmalı.
    [Fact]
    public async Task CreateAsync_WithOutboundMovement_DecreasesStock()
    {
        await using var db = await CreateDatabaseAsync();
        var context = db.Context;

        var data = await AddInventoryAsync(context, 20);

        var service = new StockMovementService(context);

        await service.CreateAsync(new CreateStockMovementDto
        {
            ProductId = data.Product.Id,
            WarehouseId = data.Warehouse.Id,
            MovementType = "Outbound",
            Quantity = 5
        });

        var inventory = await context.InventoryBalances.SingleAsync();

        Assert.Equal(15, inventory.QuantityOnHand);
        Assert.Equal(1, await context.StockMovements.CountAsync());
    }

    // 3. Yetersiz stokta işlem yapılmamalı.
    [Fact]
    public async Task CreateAsync_WithInsufficientStock_ThrowsExceptionAndDoesNotChangeStock()
    {
        await using var db = await CreateDatabaseAsync();
        var context = db.Context;

        var data = await AddInventoryAsync(context, 5);

        var service = new StockMovementService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateStockMovementDto
            {
                ProductId = data.Product.Id,
                WarehouseId = data.Warehouse.Id,
                MovementType = "Outbound",
                Quantity = 10
            }));

        var inventory = await context.InventoryBalances.SingleAsync();

        Assert.Equal(5, inventory.QuantityOnHand);
        Assert.Empty(await context.StockMovements.ToListAsync());
    }

    // 4. Geçersiz hareket türü kabul edilmemeli.
    [Fact]
    public async Task CreateAsync_WithInvalidMovementType_ThrowsException()
    {
        await using var db = await CreateDatabaseAsync();
        var context = db.Context;

        var data = await AddInventoryAsync(context, 20);

        var service = new StockMovementService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateStockMovementDto
            {
                ProductId = data.Product.Id,
                WarehouseId = data.Warehouse.Id,
                MovementType = "Invalid",
                Quantity = 5
            }));

        Assert.Empty(await context.StockMovements.ToListAsync());
    }

    // 5. Pasif ürün için hareket oluşturulmamalı.
    [Fact]
    public async Task CreateAsync_WithInactiveProduct_ThrowsException()
    {
        await using var db = await CreateDatabaseAsync();
        var context = db.Context;

        var data = await AddInventoryAsync(
            context,
            20,
            productIsActive: false);

        var service = new StockMovementService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateStockMovementDto
            {
                ProductId = data.Product.Id,
                WarehouseId = data.Warehouse.Id,
                MovementType = "Inbound",
                Quantity = 5
            }));

        Assert.Empty(await context.StockMovements.ToListAsync());
    }

    // 6. Pasif depo için hareket oluşturulmamalı.
    [Fact]
    public async Task CreateAsync_WithInactiveWarehouse_ThrowsException()
    {
        await using var db = await CreateDatabaseAsync();
        var context = db.Context;

        var data = await AddInventoryAsync(
            context,
            20,
            warehouseIsActive: false);

        var service = new StockMovementService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateStockMovementDto
            {
                ProductId = data.Product.Id,
                WarehouseId = data.Warehouse.Id,
                MovementType = "Inbound",
                Quantity = 5
            }));

        Assert.Empty(await context.StockMovements.ToListAsync());
    }

    // 7. Olmayan ürün için hareket oluşturulmamalı.
    [Fact]
    public async Task CreateAsync_WithNonExistingProduct_ThrowsException()
    {
        await using var db = await CreateDatabaseAsync();
        var context = db.Context;

        var data = await AddInventoryAsync(context, 20);

        var service = new StockMovementService(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.CreateAsync(new CreateStockMovementDto
            {
                ProductId = 999,
                WarehouseId = data.Warehouse.Id,
                MovementType = "Inbound",
                Quantity = 5
            }));
    }

    // 8. Olmayan depo için hareket oluşturulmamalı.
    [Fact]
    public async Task CreateAsync_WithNonExistingWarehouse_ThrowsException()
    {
        await using var db = await CreateDatabaseAsync();
        var context = db.Context;

        var data = await AddInventoryAsync(context, 20);

        var service = new StockMovementService(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.CreateAsync(new CreateStockMovementDto
            {
                ProductId = data.Product.Id,
                WarehouseId = 999,
                MovementType = "Inbound",
                Quantity = 5
            }));
    }

    // 9. Ürün ve depo var ancak stok kaydı yoksa işlem yapılmamalı.
    [Fact]
    public async Task CreateAsync_WithoutInventoryRecord_ThrowsException()
    {
        await using var db = await CreateDatabaseAsync();
        var context = db.Context;

        var data = await AddProductAndWarehouseAsync(context);

        var service = new StockMovementService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateStockMovementDto
            {
                ProductId = data.Product.Id,
                WarehouseId = data.Warehouse.Id,
                MovementType = "Inbound",
                Quantity = 5
            }));

        Assert.Empty(await context.StockMovements.ToListAsync());
    }

    // 10. Rezerve edilmiş stok, çıkış işleminde kullanılamamalı.
    [Fact]
    public async Task CreateAsync_WithReservedStock_RejectsUnavailableOutboundQuantity()
    {
        await using var db = await CreateDatabaseAsync();
        var context = db.Context;

        var data = await AddInventoryAsync(
            context,
            quantityOnHand: 10,
            reservedQuantity: 7);

        var service = new StockMovementService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateStockMovementDto
            {
                ProductId = data.Product.Id,
                WarehouseId = data.Warehouse.Id,
                MovementType = "Outbound",
                Quantity = 4
            }));

        var inventory = await context.InventoryBalances.SingleAsync();

        Assert.Equal(10, inventory.QuantityOnHand);
        Assert.Equal(7, inventory.ReservedQuantity);
        Assert.Empty(await context.StockMovements.ToListAsync());
    }

    // 11. Stok miktarı int sınırını aşarsa işlem yapılmamalı.
    [Fact]
    public async Task CreateAsync_WithInboundOverflow_ThrowsException()
    {
        await using var db = await CreateDatabaseAsync();
        var context = db.Context;

        var data = await AddInventoryAsync(
            context,
            int.MaxValue);

        var service = new StockMovementService(context);

        await Assert.ThrowsAsync<OverflowException>(() =>
            service.CreateAsync(new CreateStockMovementDto
            {
                ProductId = data.Product.Id,
                WarehouseId = data.Warehouse.Id,
                MovementType = "Inbound",
                Quantity = 1
            }));

        var inventory = await context.InventoryBalances.SingleAsync();

        Assert.Equal(int.MaxValue, inventory.QuantityOnHand);
        Assert.Empty(await context.StockMovements.ToListAsync());
    }

    // 12. Tüm stok hareketleri listelenmeli ve tarihe göre sıralanmalı.
    [Fact]
    public async Task GetAllAsync_ReturnsAllMovementsOrderedByCreatedAtDescending()
    {
        await using var db = await CreateDatabaseAsync();
        var context = db.Context;

        var data = await AddInventoryAsync(context, 20);

        await AddMovementAsync(
            context, data.Product, data.Warehouse,
            "Inbound", 5, new DateTime(2026, 11, 10, 10, 0, 0, DateTimeKind.Utc));

        await AddMovementAsync(
            context, data.Product, data.Warehouse,
            "Outbound", 2, new DateTime(2026, 11, 11, 10, 0, 0, DateTimeKind.Utc));

        var service = new StockMovementService(context);

        var result = await service.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("Outbound", result[0].MovementType);
        Assert.Equal("Inbound", result[1].MovementType);
        Assert.All(result, item =>
        {
            Assert.Equal(data.Product.Name, item.ProductName);
            Assert.Equal(data.Warehouse.Name, item.WarehouseName);
        });
    }

    // 13. Ürün ID filtresi doğru çalışmalı.
    [Fact]
    public async Task GetAllAsync_WithProductId_ReturnsOnlyMatchingMovements()
    {
        await using var db = await CreateDatabaseAsync();
        var context = db.Context;

        var first = await AddInventoryAsync(context, 20);
        var second = await AddInventoryAsync(context, 30);

        await AddMovementAsync(
            context, first.Product, first.Warehouse,
            "Inbound", 5, DateTime.UtcNow);

        await AddMovementAsync(
            context, second.Product, second.Warehouse,
            "Outbound", 3, DateTime.UtcNow);

        var service = new StockMovementService(context);

        var result = await service.GetAllAsync(
            productId: first.Product.Id);

        var movement = Assert.Single(result);

        Assert.Equal(first.Product.Id, movement.ProductId);
        Assert.Equal("Inbound", movement.MovementType);
    }

    // 14. Depo ID filtresi doğru çalışmalı.
    [Fact]
    public async Task GetAllAsync_WithWarehouseId_ReturnsOnlyMatchingMovements()
    {
        await using var db = await CreateDatabaseAsync();
        var context = db.Context;

        var first = await AddInventoryAsync(context, 20);
        var second = await AddInventoryAsync(context, 30);

        await AddMovementAsync(
            context, first.Product, first.Warehouse,
            "Inbound", 5, DateTime.UtcNow);

        await AddMovementAsync(
            context, second.Product, second.Warehouse,
            "Outbound", 3, DateTime.UtcNow);

        var service = new StockMovementService(context);

        var result = await service.GetAllAsync(
            warehouseId: second.Warehouse.Id);

        var movement = Assert.Single(result);

        Assert.Equal(second.Warehouse.Id, movement.WarehouseId);
        Assert.Equal("Outbound", movement.MovementType);
    }

    // 15. Hareket türü filtresi doğru çalışmalı.
    [Fact]
    public async Task GetAllAsync_WithMovementType_ReturnsOnlyMatchingMovements()
    {
        await using var db = await CreateDatabaseAsync();
        var context = db.Context;

        var data = await AddInventoryAsync(context, 20);

        await AddMovementAsync(
            context, data.Product, data.Warehouse,
            "Inbound", 5, DateTime.UtcNow);

        await AddMovementAsync(
            context, data.Product, data.Warehouse,
            "Outbound", 3, DateTime.UtcNow);

        var service = new StockMovementService(context);

        var result = await service.GetAllAsync(
            movementType: "Inbound");

        var movement = Assert.Single(result);

        Assert.Equal("Inbound", movement.MovementType);
        Assert.Equal(5, movement.Quantity);
    }

    // 16. Tarih aralığı filtresi doğru çalışmalı.
    [Fact]
    public async Task GetAllAsync_WithDateRange_ReturnsMovementsWithinRange()
    {
        await using var db = await CreateDatabaseAsync();
        var context = db.Context;

        var data = await AddInventoryAsync(context, 20);

        await AddMovementAsync(
            context, data.Product, data.Warehouse,
            "Inbound", 2,
            new DateTime(2026, 11, 9, 12, 0, 0, DateTimeKind.Utc));

        await AddMovementAsync(
            context, data.Product, data.Warehouse,
            "Inbound", 5,
            new DateTime(2026, 11, 10, 12, 0, 0, DateTimeKind.Utc));

        await AddMovementAsync(
            context, data.Product, data.Warehouse,
            "Outbound", 3,
            new DateTime(2026, 11, 11, 12, 0, 0, DateTimeKind.Utc));

        await AddMovementAsync(
            context, data.Product, data.Warehouse,
            "Outbound", 4,
            new DateTime(2026, 11, 12, 12, 0, 0, DateTimeKind.Utc));

        var service = new StockMovementService(context);

        var result = await service.GetAllAsync(
            from: new DateTime(2026, 11, 10),
            to: new DateTime(2026, 11, 11));

        Assert.Equal(2, result.Count);
        Assert.All(result, movement =>
        {
            Assert.True(movement.CreatedAt.Date >= new DateTime(2026, 11, 10));
            Assert.True(movement.CreatedAt.Date <= new DateTime(2026, 11, 11));
        });
    }

    // 17. Birden fazla filtre birlikte çalışmalı.
    [Fact]
    public async Task GetAllAsync_WithCombinedFilters_ReturnsMatchingMovements()
    {
        await using var db = await CreateDatabaseAsync();
        var context = db.Context;

        var first = await AddInventoryAsync(context, 20);
        var second = await AddInventoryAsync(context, 30);

        var targetDate = new DateTime(
            2026, 11, 10, 12, 0, 0, DateTimeKind.Utc);

        await AddMovementAsync(
            context, first.Product, first.Warehouse,
            "Inbound", 5, targetDate);

        await AddMovementAsync(
            context, first.Product, first.Warehouse,
            "Outbound", 3, targetDate);

        await AddMovementAsync(
            context, second.Product, second.Warehouse,
            "Inbound", 7, targetDate);

        var service = new StockMovementService(context);

        var result = await service.GetAllAsync(
            productId: first.Product.Id,
            warehouseId: first.Warehouse.Id,
            movementType: "Inbound",
            from: new DateTime(2026, 11, 10),
            to: new DateTime(2026, 11, 10));

        var movement = Assert.Single(result);

        Assert.Equal(first.Product.Id, movement.ProductId);
        Assert.Equal(first.Warehouse.Id, movement.WarehouseId);
        Assert.Equal("Inbound", movement.MovementType);
        Assert.Equal(5, movement.Quantity);
    }

    // 18. Stok hareketi raporu doğru hesaplanmalı.
    [Fact]
    public async Task GetReportAsync_ReturnsCorrectMovementTotals()
    {
        await using var db = await CreateDatabaseAsync();
        var context = db.Context;

        var data = await AddInventoryAsync(context, 20);

        await AddMovementAsync(
            context, data.Product, data.Warehouse,
            "Inbound", 10, DateTime.UtcNow);

        await AddMovementAsync(
            context, data.Product, data.Warehouse,
            "Inbound", 5, DateTime.UtcNow);

        await AddMovementAsync(
            context, data.Product, data.Warehouse,
            "Outbound", 7, DateTime.UtcNow);

        var service = new StockMovementService(context);

        var result = await service.GetReportAsync();

        Assert.Equal(3, result.TotalMovements);
        Assert.Equal(15, result.TotalInboundQuantity);
        Assert.Equal(7, result.TotalOutboundQuantity);
        Assert.Equal(8, result.NetStockChange);
        Assert.NotEqual(default, result.GeneratedAt);
    }

    // 19. Hiç stok hareketi yoksa rapor sıfır dönmeli.
    [Fact]
    public async Task GetReportAsync_WithNoMovements_ReturnsZeroTotals()
    {
        await using var db = await CreateDatabaseAsync();
        var context = db.Context;

        var service = new StockMovementService(context);

        var result = await service.GetReportAsync();

        Assert.Equal(0, result.TotalMovements);
        Assert.Equal(0, result.TotalInboundQuantity);
        Assert.Equal(0, result.TotalOutboundQuantity);
        Assert.Equal(0, result.NetStockChange);
        Assert.NotEqual(default, result.GeneratedAt);
    }

    // TEST VERİTABANI OLUŞTURMA

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

    // SQLite, SQL Server RowVersion alanını otomatik üretmediği için
    // test veritabanında varsayılan değer tanımlıyoruz.
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
    private static async Task<(Product Product, Warehouse Warehouse, InventoryBalance Inventory)>
        AddInventoryAsync(
            AppDbContext context,
            int quantityOnHand,
            int reservedQuantity = 0,
            bool productIsActive = true,
            bool warehouseIsActive = true)
    {
        var data = await AddProductAndWarehouseAsync(context);

        data.Product.IsActive = productIsActive;
        data.Warehouse.IsActive = warehouseIsActive;

        var inventory = new InventoryBalance
        {
            ProductId = data.Product.Id,
            WarehouseId = data.Warehouse.Id,
            QuantityOnHand = quantityOnHand,
            ReservedQuantity = reservedQuantity,
            ReorderLevel = 5,
            UpdatedAt = DateTime.UtcNow
        };

        context.InventoryBalances.Add(inventory);

        await context.SaveChangesAsync();

        return (data.Product, data.Warehouse, inventory);
    }

    // Test amaçlı stok hareketi oluşturma.
    private static async Task AddMovementAsync(
        AppDbContext context,
        Product product,
        Warehouse warehouse,
        string movementType,
        int quantity,
        DateTime createdAt)
    {
        var movement = new StockMovement
        {
            ProductId = product.Id,
            WarehouseId = warehouse.Id,
            MovementType = movementType,
            Quantity = quantity,
            ReferenceNumber = $"REF-{Guid.NewGuid():N}",
            Notes = "Test movement",
            CreatedAt = createdAt
        };

        context.StockMovements.Add(movement);

        await context.SaveChangesAsync();
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
