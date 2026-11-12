
using System.Net;
using System.Net.Http.Json;
using backend.Tests.Infrastructure;

namespace backend.Tests.Integration;

public class ProductsApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public ProductsApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetProducts_ReturnsSuccessStatusCode()
    {
        // Arrange
        await _factory.InitializeDatabaseAsync();

        using var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/Product");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_WithValidData_ReturnsCreated()
    {
        // Arrange
        await _factory.InitializeDatabaseAsync();

        using var client = _factory.CreateClient();

        var product = new
        {
            sku = $"TEST-{Guid.NewGuid():N}",
            name = "Integration Test Product",
            description = "Created by API integration test",
            unitPrice = 150.50m
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/Product",
            product);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task CreateProduct_WithEmptySku_ReturnsBadRequest()
    {
        // Arrange
        await _factory.InitializeDatabaseAsync();

        using var client = _factory.CreateClient();

        var product = new
        {
            sku = "",
            name = "Test Product",
            description = "Invalid SKU",
            unitPrice = 100m
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/Product",
            product);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_WithEmptyName_ReturnsBadRequest()
    {
        // Arrange
        await _factory.InitializeDatabaseAsync();

        using var client = _factory.CreateClient();

        var product = new
        {
            sku = $"TEST-{Guid.NewGuid():N}",
            name = "",
            description = "Invalid name",
            unitPrice = 100m
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/Product",
            product);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_WithNegativePrice_ReturnsBadRequest()
    {
        // Arrange
        await _factory.InitializeDatabaseAsync();

        using var client = _factory.CreateClient();

        var product = new
        {
            sku = $"TEST-{Guid.NewGuid():N}",
            name = "Invalid Price Product",
            description = "Negative price",
            unitPrice = -10m
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/Product",
            product);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_WithDuplicateSku_ReturnsConflict()
    {
        // Arrange
        await _factory.InitializeDatabaseAsync();

        using var client = _factory.CreateClient();

        var sku = $"DUPLICATE-{Guid.NewGuid():N}";

        var product = new
        {
            sku,
            name = "First Product",
            description = "First product with this SKU",
            unitPrice = 100m
        };

        // Act
        var firstResponse = await client.PostAsJsonAsync(
            "/api/Product",
            product);

        var secondProduct = new
        {
            sku,
            name = "Second Product",
            description = "Duplicate SKU",
            unitPrice = 200m
        };

        var secondResponse = await client.PostAsJsonAsync(
            "/api/Product",
            secondProduct);

        // Assert
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task GetProductById_WithExistingProduct_ReturnsOk()
    {
        // Arrange
        await _factory.InitializeDatabaseAsync();

        using var client = _factory.CreateClient();

        var product = new
        {
            sku = $"GET-{Guid.NewGuid():N}",
            name = "Product For Get Test",
            description = "Get test",
            unitPrice = 100m
        };

        var createResponse = await client.PostAsJsonAsync(
            "/api/Product",
            product);

        var createdProduct = await createResponse.Content
            .ReadFromJsonAsync<ProductTestResponse>();

        // Act
        var response = await client.GetAsync(
            $"/api/Product/{createdProduct!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetProductById_WithNonExistingProduct_ReturnsNotFound()
    {
        // Arrange
        await _factory.InitializeDatabaseAsync();

        using var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/Product/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProduct_WithExistingProduct_ReturnsOk()
    {
        // Arrange
        await _factory.InitializeDatabaseAsync();

        using var client = _factory.CreateClient();

        var product = new
        {
            sku = $"UPDATE-{Guid.NewGuid():N}",
            name = "Original Product",
            description = "Before update",
            unitPrice = 100m
        };

        var createResponse = await client.PostAsJsonAsync(
            "/api/Product",
            product);

        var createdProduct = await createResponse.Content
            .ReadFromJsonAsync<ProductTestResponse>();

        var updateDto = new
        {
            sku = product.sku,
            name = "Updated Product",
            description = "After update",
            unitPrice = 250m,
            isActive = true
        };

        // Act
        var response = await client.PutAsJsonAsync(
            $"/api/Product/{createdProduct!.Id}",
            updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProduct_WithNonExistingProduct_ReturnsNotFound()
    {
        // Arrange
        await _factory.InitializeDatabaseAsync();

        using var client = _factory.CreateClient();

        var updateDto = new
        {
            sku = "NOT-FOUND",
            name = "Updated Product",
            description = "Product does not exist",
            unitPrice = 100m,
            isActive = true
        };

        // Act
        var response = await client.PutAsJsonAsync(
            "/api/Product/99999",
            updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProduct_WithExistingProduct_ReturnsNoContent()
    {
        // Arrange
        await _factory.InitializeDatabaseAsync();

        using var client = _factory.CreateClient();

        var product = new
        {
            sku = $"DELETE-{Guid.NewGuid():N}",
            name = "Product For Delete Test",
            description = "Delete test",
            unitPrice = 100m
        };

        var createResponse = await client.PostAsJsonAsync(
            "/api/Product",
            product);

        var createdProduct = await createResponse.Content
            .ReadFromJsonAsync<ProductTestResponse>();

        // Act
        var response = await client.DeleteAsync(
            $"/api/Product/{createdProduct!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private class ProductTestResponse
    {
        public int Id { get; set; }
    }
}
