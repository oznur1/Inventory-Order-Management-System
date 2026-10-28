
using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class StockReservationResponseDto
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    [StringLength(200)]
    public string? ProductName { get; set; }

    public int WarehouseId { get; set; }

    [StringLength(200)]
    public string? WarehouseName { get; set; }

    public int Quantity { get; set; }

    public int? OrderId { get; set; }

    [Required]
    [StringLength(30)]
    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }
}
