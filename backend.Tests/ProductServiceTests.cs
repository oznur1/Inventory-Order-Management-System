
using backend.Data;
using backend.DTOs.Products;
using backend.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using backend.Entities;

namespace backend.Tests;


public class ProductServiceTests
{
    [Fact]
    public async Task CreateAsync_WithValidData_CreatesProduct()
    {
        // Arrange
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new AppDbContext(options);

        await context.Database.EnsureCreatedAsync();

        var service = new ProductService(context);

        var dto = new CreateProductDto
        {
            SKU = "LAPTOP-001",
            Name = "Business Laptop",
            Description = "Professional laptop",
            UnitPrice = 25000
        };

        // Act
        var result = await service.CreateAsync(dto);

        // Assert
        Assert.True(result.Id > 0);
        Assert.Equal("LAPTOP-001", result.SKU);
        Assert.Equal("Business Laptop", result.Name);
        Assert.Equal("Professional laptop", result.Description);
        Assert.Equal(25000, result.UnitPrice);
        Assert.True(result.IsActive);

        var savedProduct = await context.Products.SingleAsync();

        Assert.Equal(result.Id, savedProduct.Id);
    }


[Fact]
public async Task CreateAsync_WithDuplicateSku_ThrowsException()
{
    // Arrange
    await using var connection =
        new SqliteConnection("Data Source=:memory:");

    await connection.OpenAsync();

    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlite(connection)
        .Options;

    await using var context = new AppDbContext(options);

    await context.Database.EnsureCreatedAsync();

    var service = new ProductService(context);

    var firstProduct = new CreateProductDto
    {
        SKU = "LAPTOP-001",
        Name = "Business Laptop",
        UnitPrice = 25000
    };

    var duplicateProduct = new CreateProductDto
    {
        SKU = "LAPTOP-001",
        Name = "Gaming Laptop",
        UnitPrice = 35000
    };

    await service.CreateAsync(firstProduct);

    // Act & Assert
    var exception = await Assert.ThrowsAsync<InvalidOperationException>(
        () => service.CreateAsync(duplicateProduct)
    );

    Assert.Equal(
        "Bu SKU kodu zaten kullanılıyor.",
        exception.Message
    );

    Assert.Equal(1, await context.Products.CountAsync());
}



[Fact]
public async Task GetAllAsync_ReturnsAllProducts()
{
    // Arrange
    await using var connection =
        new SqliteConnection("Data Source=:memory:");

    await connection.OpenAsync();

    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlite(connection)
        .Options;

    await using var context = new AppDbContext(options);

    await context.Database.EnsureCreatedAsync();

    var service = new ProductService(context);

    await service.CreateAsync(new CreateProductDto
    {
        SKU = "LAPTOP-001",
        Name = "Business Laptop",
        UnitPrice = 25000
    });

    await service.CreateAsync(new CreateProductDto
    {
        SKU = "MOUSE-001",
        Name = "Wireless Mouse",
        UnitPrice = 750
    });

    // Act
    var result = await service.GetAllAsync();

    // Assert
    Assert.Equal(2, result.Count);
    Assert.Contains(result, p => p.SKU == "LAPTOP-001");
    Assert.Contains(result, p => p.SKU == "MOUSE-001");
}


[Fact]
public async Task GetByIdAsync_WithExistingId_ReturnsProduct()
{
    // Arrange
    await using var connection =
        new SqliteConnection("Data Source=:memory:");

    await connection.OpenAsync();

    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlite(connection)
        .Options;

    await using var context = new AppDbContext(options);

    await context.Database.EnsureCreatedAsync();

    var service = new ProductService(context);

    var createdProduct = await service.CreateAsync(
        new CreateProductDto
        {
            SKU = "KEYBOARD-001",
            Name = "Mechanical Keyboard",
            UnitPrice = 1500
        });

    // Act
    var result = await service.GetByIdAsync(createdProduct.Id);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(createdProduct.Id, result.Id);
    Assert.Equal("KEYBOARD-001", result.SKU);
    Assert.Equal("Mechanical Keyboard", result.Name);
    Assert.Equal(1500, result.UnitPrice);
}


[Fact]
public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
{
    // Arrange
    await using var connection =
        new SqliteConnection("Data Source=:memory:");

    await connection.OpenAsync();

    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlite(connection)
        .Options;

    await using var context = new AppDbContext(options);

    await context.Database.EnsureCreatedAsync();

    var service = new ProductService(context);

    // Act
    var result = await service.GetByIdAsync(999);

    // Assert
    Assert.Null(result);
}


[Fact]
public async Task UpdateAsync_WithExistingProduct_UpdatesProduct()
{
    // Arrange
    await using var connection =
        new SqliteConnection("Data Source=:memory:");

    await connection.OpenAsync();

    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlite(connection)
        .Options;

    await using var context = new AppDbContext(options);

    await context.Database.EnsureCreatedAsync();

    var service = new ProductService(context);

    var createdProduct = await service.CreateAsync(
        new CreateProductDto
        {
            SKU = "LAPTOP-001",
            Name = "Business Laptop",
            UnitPrice = 25000
        });

    var updateDto = new UpdateProductDto
    {
        SKU = "LAPTOP-002",
        Name = "Updated Business Laptop",
        Description = "Updated product description",
        UnitPrice = 30000,
        IsActive = false
    };

    // Act
    var result = await service.UpdateAsync(
        createdProduct.Id,
        updateDto);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("LAPTOP-002", result.SKU);
    Assert.Equal("Updated Business Laptop", result.Name);
    Assert.Equal("Updated product description", result.Description);
    Assert.Equal(30000, result.UnitPrice);
    Assert.False(result.IsActive);

    var savedProduct = await context.Products.SingleAsync();

    Assert.Equal("LAPTOP-002", savedProduct.SKU);
    Assert.False(savedProduct.IsActive);
}

[Fact]
public async Task UpdateAsync_WithNonExistingProduct_ReturnsNull()
{
    // Arrange
    await using var connection =
        new SqliteConnection("Data Source=:memory:");

    await connection.OpenAsync();

    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlite(connection)
        .Options;

    await using var context = new AppDbContext(options);

    await context.Database.EnsureCreatedAsync();

    var service = new ProductService(context);

    var updateDto = new UpdateProductDto
    {
        SKU = "LAPTOP-002",
        Name = "Updated Laptop",
        Description = "Updated description",
        UnitPrice = 30000,
        IsActive = true
    };

    // Act
    var result = await service.UpdateAsync(999, updateDto);

    // Assert
    Assert.Null(result);
    Assert.Empty(await context.Products.ToListAsync());
}


[Fact]
public async Task UpdateAsync_WithDuplicateSku_ThrowsException()
{
    // Arrange
    await using var connection =
        new SqliteConnection("Data Source=:memory:");

    await connection.OpenAsync();

    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlite(connection)
        .Options;

    await using var context = new AppDbContext(options);

    await context.Database.EnsureCreatedAsync();

    var service = new ProductService(context);

    var firstProduct = await service.CreateAsync(
        new CreateProductDto
        {
            SKU = "LAPTOP-001",
            Name = "Business Laptop",
            UnitPrice = 25000
        });

    await service.CreateAsync(
        new CreateProductDto
        {
            SKU = "MOUSE-001",
            Name = "Wireless Mouse",
            UnitPrice = 750
        });

    var updateDto = new UpdateProductDto
    {
        SKU = "MOUSE-001",
        Name = "Updated Laptop",
        UnitPrice = 30000,
        IsActive = true
    };

    // Act & Assert
    var exception = await Assert.ThrowsAsync<InvalidOperationException>(
        () => service.UpdateAsync(firstProduct.Id, updateDto)
    );

    Assert.Equal(
        "Bu SKU kodu başka bir ürün tarafından kullanılıyor.",
        exception.Message
    );

    // Both products must remain unchanged.
    var products = await context.Products
        .OrderBy(p => p.Id)
        .ToListAsync();

    Assert.Equal(2, products.Count);
    Assert.Equal("LAPTOP-001", products[0].SKU);
    Assert.Equal("MOUSE-001", products[1].SKU);
}


[Fact]
public async Task DeleteAsync_WithExistingProduct_DeletesProduct()
{
    // Arrange
    await using var connection =
        new SqliteConnection("Data Source=:memory:");

    await connection.OpenAsync();

    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlite(connection)
        .Options;

    await using var context = new AppDbContext(options);

    await context.Database.EnsureCreatedAsync();

    var service = new ProductService(context);

    var createdProduct = await service.CreateAsync(
        new CreateProductDto
        {
            SKU = "LAPTOP-001",
            Name = "Business Laptop",
            UnitPrice = 25000
        });

    // Act
    var result = await service.DeleteAsync(createdProduct.Id);

    // Assert
    Assert.True(result);
    Assert.Empty(await context.Products.ToListAsync());
}


[Fact]
public async Task DeleteAsync_WithNonExistingProduct_ReturnsFalse()
{
    // Arrange
    await using var connection =
        new SqliteConnection("Data Source=:memory:");

    await connection.OpenAsync();

    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlite(connection)
        .Options;

    await using var context = new AppDbContext(options);

    await context.Database.EnsureCreatedAsync();

    var service = new ProductService(context);

    // Act
    var result = await service.DeleteAsync(999);

    // Assert
    Assert.False(result);
    Assert.Empty(await context.Products.ToListAsync());
}


[Fact]
public async Task DeleteAsync_WithRelatedInventory_ThrowsException()
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

    var service = new ProductService(context);

    var product = await service.CreateAsync(
        new CreateProductDto
        {
            SKU = "LAPTOP-001",
            Name = "Business Laptop",
            UnitPrice = 25000
        });

    var warehouse = new Warehouse
    {
        Code = "WH-001",
        Name = "Main Warehouse"
    };

    context.Warehouses.Add(warehouse);
    await context.SaveChangesAsync();

    context.InventoryBalances.Add(new InventoryBalance
    {
        ProductId = product.Id,
        WarehouseId = warehouse.Id,
        QuantityOnHand = 20,
        ReservedQuantity = 0,
        ReorderLevel = 5
    });

    await context.SaveChangesAsync();

    // Act
    var exception = await Assert.ThrowsAsync<InvalidOperationException>(
        () => service.DeleteAsync(product.Id));

    // Assert
    Assert.Equal(
        "İlişkili stok veya sipariş kayıtları bulunan ürün silinemez.",
        exception.Message);

    Assert.Single(await context.Products.ToListAsync());
    Assert.Single(await context.InventoryBalances.ToListAsync());
}


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
                CHECK (ReservedQuantity >= 0
                    AND ReservedQuantity <= QuantityOnHand),
            CONSTRAINT FK_InventoryBalances_Products_ProductId
                FOREIGN KEY (ProductId) REFERENCES Products(Id)
                ON DELETE RESTRICT,
            CONSTRAINT FK_InventoryBalances_Warehouses_WarehouseId
                FOREIGN KEY (WarehouseId) REFERENCES Warehouses(Id)
                ON DELETE RESTRICT
        );

        CREATE UNIQUE INDEX IX_InventoryBalances_ProductId_WarehouseId
        ON InventoryBalances(ProductId, WarehouseId);
        """);
}

}
