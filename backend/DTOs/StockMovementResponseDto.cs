
using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class StockMovementResponseDto
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    [StringLength(200)]
    public string? ProductName { get; set; }

    public int WarehouseId { get; set; }

    [StringLength(200)]
    public string? WarehouseName { get; set; }

    [Required]
    public string MovementType { get; set; } = string.Empty;

    public int Quantity { get; set; }

    [StringLength(50)]
    public string? ReferenceNumber { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
}
