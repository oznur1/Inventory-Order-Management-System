
using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Warehouses;

public class UpdateWarehouseDto
{
    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Address { get; set; }

    public bool IsActive { get; set; } = true;
}
