
using backend.Data;
using backend.Entities;
using backend.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Tests;

public class OrderServiceTests
{
    // Shared SQLite test database setup.
    private static async Task InitializeDatabaseAsync(
        AppDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        // SQLite does not generate SQL Server RowVersion values.
        // This custom table is used only by the tests.
        await context.Database.ExecuteSqlRawAsync("""
            DROP TABLE InventoryBalances;
            """);

        await context.Database.ExecuteSqlRawAsync("""
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
                    FOREIGN KEY (ProductId) REFERENCES Products (Id),
                CONSTRAINT FK_InventoryBalances_Warehouses_WarehouseId
                    FOREIGN KEY (WarehouseId) REFERENCES Warehouses (Id)
            );

            CREATE UNIQUE INDEX IX_InventoryBalances_ProductId_WarehouseId
            ON InventoryBalances (ProductId, WarehouseId);
            """);
    }

    [Fact]
    public async Task CreateAsync_WithSufficientStock_ReservesStockAndCreatesOrder()
    {
        // Arrange
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new AppDbContext(options);

        await InitializeDatabaseAsync(context);

        var product = new Product
        {
            Name = "Test Laptop",
            SKU = "TEST-LAPTOP-001",
            UnitPrice = 25000,
            IsActive = true
        };

        var warehouse = new Warehouse
        {
            Name = "Test Warehouse",
            Code = "TEST-WH-001",
            IsActive = true
        };

        context.Products.Add(product);
        context.Warehouses.Add(warehouse);

        await context.SaveChangesAsync();

        var inventory = new InventoryBalance
        {
            ProductId = product.Id,
            WarehouseId = warehouse.Id,
            QuantityOnHand = 10,
            ReservedQuantity = 0,
            ReorderLevel = 2
        };

        context.InventoryBalances.Add(inventory);

        await context.SaveChangesAsync();

        var service = new OrderService(context);

        var dto = new backend.DTOs.CreateOrderDto
        {
            CustomerName = "Test Customer",
            Items =
            [
                new backend.DTOs.OrderItemDto
                {
                    ProductId = product.Id,
                    WarehouseId = warehouse.Id,
                    Quantity = 3
                }
            ]
        };

        // Act
        var result = await service.CreateAsync(dto);

        // Assert: Order
        Assert.True(result.Id > 0);
        Assert.Equal("Test Customer", result.CustomerName);
        Assert.Equal("Pending", result.Status);
        Assert.Equal(75000m, result.TotalAmount);

        // Assert: Inventory reservation
        var updatedInventory =
            await context.InventoryBalances.SingleAsync();

        Assert.Equal(10, updatedInventory.QuantityOnHand);
        Assert.Equal(3, updatedInventory.ReservedQuantity);
        Assert.Equal(7, updatedInventory.AvailableQuantity);

        // Assert: Reservation record
        var reservation =
            await context.StockReservations.SingleAsync();

        Assert.Equal(3, reservation.Quantity);
        Assert.Equal("Active", reservation.Status);
        Assert.Equal(result.Id, reservation.OrderId);
    }

    [Fact]
    public async Task CreateAsync_WithInsufficientStock_ThrowsExceptionAndDoesNotReserveStock()
    {
        // Arrange
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new AppDbContext(options);

        await InitializeDatabaseAsync(context);

        var product = new Product
        {
            Name = "Test Laptop",
            SKU = "TEST-LAPTOP-002",
            UnitPrice = 25000,
            IsActive = true
        };

        var warehouse = new Warehouse
        {
            Name = "Test Warehouse",
            Code = "TEST-WH-002",
            IsActive = true
        };

        context.Products.Add(product);
        context.Warehouses.Add(warehouse);

        await context.SaveChangesAsync();

        var inventory = new InventoryBalance
        {
            ProductId = product.Id,
            WarehouseId = warehouse.Id,
            QuantityOnHand = 2,
            ReservedQuantity = 0,
            ReorderLevel = 1
        };

        context.InventoryBalances.Add(inventory);

        await context.SaveChangesAsync();

        var service = new OrderService(context);

        var dto = new backend.DTOs.CreateOrderDto
        {
            CustomerName = "Test Customer",
            Items =
            [
                new backend.DTOs.OrderItemDto
                {
                    ProductId = product.Id,
                    WarehouseId = warehouse.Id,
                    Quantity = 3
                }
            ]
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateAsync(dto)
        );

        Assert.Contains("Insufficient stock", exception.Message);

        // Assert: Stock remains unchanged.
        var updatedInventory =
            await context.InventoryBalances.SingleAsync();

        Assert.Equal(2, updatedInventory.QuantityOnHand);
        Assert.Equal(0, updatedInventory.ReservedQuantity);
        Assert.Equal(2, updatedInventory.AvailableQuantity);

        // Assert: No order or reservation is created.
        Assert.Empty(await context.Orders.ToListAsync());
        Assert.Empty(await context.StockReservations.ToListAsync());
    }

    [Fact]
    public async Task ShipAsync_WithPendingOrder_ConsumesStockAndCompletesReservation()
    {
        // Arrange
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new AppDbContext(options);

        await InitializeDatabaseAsync(context);

        var product = new Product
        {
            Name = "Test Laptop",
            SKU = "TEST-LAPTOP-003",
            UnitPrice = 25000,
            IsActive = true
        };

        var warehouse = new Warehouse
        {
            Name = "Test Warehouse",
            Code = "TEST-WH-003",
            IsActive = true
        };

        context.Products.Add(product);
        context.Warehouses.Add(warehouse);

        await context.SaveChangesAsync();

        var inventory = new InventoryBalance
        {
            ProductId = product.Id,
            WarehouseId = warehouse.Id,
            QuantityOnHand = 10,
            ReservedQuantity = 0,
            ReorderLevel = 2
        };

        context.InventoryBalances.Add(inventory);

        await context.SaveChangesAsync();

        var service = new OrderService(context);

        var dto = new backend.DTOs.CreateOrderDto
        {
            CustomerName = "Test Customer",
            Items =
            [
                new backend.DTOs.OrderItemDto
                {
                    ProductId = product.Id,
                    WarehouseId = warehouse.Id,
                    Quantity = 3
                }
            ]
        };

        // Create a pending order.
        var createdOrder = await service.CreateAsync(dto);

        // Act: Ship the order.
        var shippedOrder = await service.ShipAsync(createdOrder.Id);

        // Assert: Order status.
        Assert.NotNull(shippedOrder);
        Assert.Equal("Shipped", shippedOrder.Status);

        // Assert: Physical stock and reservation.
        var updatedInventory =
            await context.InventoryBalances.SingleAsync();

        Assert.Equal(7, updatedInventory.QuantityOnHand);
        Assert.Equal(0, updatedInventory.ReservedQuantity);
        Assert.Equal(7, updatedInventory.AvailableQuantity);

        // Assert: Reservation completed.
        var reservation =
            await context.StockReservations.SingleAsync();

        Assert.Equal("Fulfilled", reservation.Status);

        // Assert: Outbound stock movement created.
        var movement =
            await context.StockMovements.SingleAsync();

        Assert.Equal("Outbound", movement.MovementType);
        Assert.Equal(3, movement.Quantity);
        Assert.Equal(
            $"ORDER-{createdOrder.Id}",
            movement.ReferenceNumber
        );

        // Act & Assert: The same order cannot be shipped twice.
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ShipAsync(createdOrder.Id)
        );

        // Assert: Stock must not be consumed again.
        var inventoryAfterSecondAttempt =
            await context.InventoryBalances.SingleAsync();

        Assert.Equal(7, inventoryAfterSecondAttempt.QuantityOnHand);
        Assert.Equal(0, inventoryAfterSecondAttempt.ReservedQuantity);

        // Assert: No additional stock movement is created.
        Assert.Equal(1, await context.StockMovements.CountAsync());
    }


[Fact]
public async Task ShipAsync_WithNonExistingOrder_ReturnsNull()
{
    // Arrange
    await using var connection =
        new SqliteConnection("Data Source=:memory:");

    await connection.OpenAsync();

    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlite(connection)
        .Options;

    await using var context = new AppDbContext(options);

    await InitializeDatabaseAsync(context);

    var service = new OrderService(context);

    // Act
    var result = await service.ShipAsync(999);

    // Assert
    Assert.Null(result);

    Assert.Empty(await context.Orders.ToListAsync());

    Assert.Empty(await context.StockMovements.ToListAsync());
}

}
