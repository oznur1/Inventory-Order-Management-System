
using System.Net;
using System.Net.Http.Json;
using backend.Tests.Infrastructure;

namespace backend.Tests.Integration;

public class InventoryApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public InventoryApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetInventory_ReturnsOk()
    {
        // Arrange
        await _factory.InitializeDatabaseAsync();

        using var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/Inventory");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetInventory_WithInvalidWarehouseId_ReturnsBadRequest()
    {
        // Arrange
        await _factory.InitializeDatabaseAsync();

        using var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/Inventory?warehouseId=0");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetInventoryById_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        await _factory.InitializeDatabaseAsync();

        using var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/Inventory/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateInventory_WithValidData_ReturnsCreated()
    {
        // Arrange
        await _factory.InitializeDatabaseAsync();

        using var client = _factory.CreateClient();

        var productId = await CreateProductAsync(client);
        var warehouseId = await CreateWarehouseAsync(client);

        var inventory = new
        {
            productId,
            warehouseId,
            quantityOnHand = 100,
            reorderLevel = 20
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/Inventory",
            inventory);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task CreateInventory_WithNonExistingProduct_ReturnsNotFound()
    {
        // Arrange
        await _factory.InitializeDatabaseAsync();

        using var client = _factory.CreateClient();

        var warehouseId = await CreateWarehouseAsync(client);

        var inventory = new
        {
            productId = 99999,
            warehouseId,
            quantityOnHand = 100,
            reorderLevel = 20
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/Inventory",
            inventory);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateInventory_WithNegativeQuantity_ReturnsBadRequest()
    {
        // Arrange
        await _factory.InitializeDatabaseAsync();

        using var client = _factory.CreateClient();

        var productId = await CreateProductAsync(client);
        var warehouseId = await CreateWarehouseAsync(client);

        var inventory = new
        {
            productId,
            warehouseId,
            quantityOnHand = -10,
            reorderLevel = 20
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/Inventory",
            inventory);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateInventory_WithDuplicateProductAndWarehouse_ReturnsConflict()
    {
        // Arrange
        await _factory.InitializeDatabaseAsync();

        using var client = _factory.CreateClient();

        var productId = await CreateProductAsync(client);
        var warehouseId = await CreateWarehouseAsync(client);

        var inventory = new
        {
            productId,
            warehouseId,
            quantityOnHand = 100,
            reorderLevel = 20
        };

        // Act
        var firstResponse = await client.PostAsJsonAsync(
            "/api/Inventory",
            inventory);

        var secondResponse = await client.PostAsJsonAsync(
            "/api/Inventory",
            inventory);

        // Assert
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task UpdateInventory_WithExistingInventory_ReturnsOk()
    {
        // Arrange
        await _factory.InitializeDatabaseAsync();

        using var client = _factory.CreateClient();

        var inventoryId = await CreateInventoryAsync(client);

        var updateDto = new
        {
            quantityOnHand = 150,
            reorderLevel = 30
        };

        // Act
        var response = await client.PutAsJsonAsync(
            $"/api/Inventory/{inventoryId}",
            updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateInventory_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        await _factory.InitializeDatabaseAsync();

        using var client = _factory.CreateClient();

        var updateDto = new
        {
            quantityOnHand = 150,
            reorderLevel = 30
        };

        // Act
        var response = await client.PutAsJsonAsync(
            "/api/Inventory/99999",
            updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteInventory_WithExistingInventory_ReturnsNoContent()
    {
        // Arrange
        await _factory.InitializeDatabaseAsync();

        using var client = _factory.CreateClient();

        var inventoryId = await CreateInventoryAsync(client);

        // Act
        var response = await client.DeleteAsync(
            $"/api/Inventory/{inventoryId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task GetLowStock_ReturnsOk()
    {
        // Arrange
        await _factory.InitializeDatabaseAsync();

        using var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/Inventory/low-stock");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetInventoryReport_ReturnsOk()
    {
        // Arrange
        await _factory.InitializeDatabaseAsync();

        using var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/Inventory/report");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static async Task<int> CreateProductAsync(HttpClient client)
    {
        var product = new
        {
            sku = $"INV-{Guid.NewGuid():N}",
            name = "Inventory Test Product",
            description = "Product for inventory integration tests",
            unitPrice = 100m
        };

        var response = await client.PostAsJsonAsync(
            "/api/Product",
            product);

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<ProductTestResponse>();

        return result!.Id;
    }

    private static async Task<int> CreateWarehouseAsync(HttpClient client)
    {
        var warehouse = new
        {
            code = $"WH-{Guid.NewGuid():N}",
            name = "Integration Test Warehouse",
            address = "Test Address"
        };

        var response = await client.PostAsJsonAsync(
            "/api/Warehouse",
            warehouse);

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<WarehouseTestResponse>();

        return result!.Id;
    }

    private static async Task<int> CreateInventoryAsync(HttpClient client)
    {
        var productId = await CreateProductAsync(client);
        var warehouseId = await CreateWarehouseAsync(client);

        var inventory = new
        {
            productId,
            warehouseId,
            quantityOnHand = 100,
            reorderLevel = 20
        };

        var response = await client.PostAsJsonAsync(
            "/api/Inventory",
            inventory);

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<InventoryTestResponse>();

        return result!.Id;
    }

    private class ProductTestResponse
    {
        public int Id { get; set; }
    }

    private class WarehouseTestResponse
    {
        public int Id { get; set; }
    }

    private class InventoryTestResponse
    {
        public int Id { get; set; }
    }
}
