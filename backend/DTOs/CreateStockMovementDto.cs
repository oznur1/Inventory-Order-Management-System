
using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class CreateStockMovementDto
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int WarehouseId { get; set; }

    [Required]
    [RegularExpression("^(Inbound|Outbound)$",
        ErrorMessage = "MovementType Inbound veya Outbound olmalıdır.")]
    public string MovementType { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [StringLength(50)]
    public string? ReferenceNumber { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}
