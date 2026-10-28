
namespace backend.DTOs.Products;

public class ProductResponseDto
{
    public int Id { get; set; }

    public string SKU { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal UnitPrice { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}
