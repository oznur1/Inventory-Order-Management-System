
using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Inventory;

public class UpdateInventoryDto
{
    [Range(0, int.MaxValue)]
    public int QuantityOnHand { get; set; }

    [Range(0, int.MaxValue)]
    public int ReorderLevel { get; set; }
}
