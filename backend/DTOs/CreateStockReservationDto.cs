
using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class CreateStockReservationDto
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int WarehouseId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Range(1, int.MaxValue)]
    public int? OrderId { get; set; }

    public DateTime? ExpiresAt { get; set; }
}
