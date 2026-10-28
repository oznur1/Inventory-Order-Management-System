
using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Products;

public class CreateProductDto
{
    [Required]
    [StringLength(50)]
    public string SKU { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Range(0, 999999999)]
    public decimal UnitPrice { get; set; }
}
