// Depo bilgileri vve depo kodu.


namespace backend.Entities;

public class Warehouse
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Address { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<InventoryBalance> InventoryBalances { get; set; }
        = new List<InventoryBalance>();
}