
using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class OrderResponseDto
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string CustomerName { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(254)]
    public string? CustomerEmail { get; set; }

    [StringLength(500)]
    public string? ShippingAddress { get; set; }

    [Required]
    [StringLength(30)]
    public string Status { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "9999999999999999.99")]
    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<OrderItemResponseDto> Items { get; set; } = new();
}

public class OrderItemResponseDto
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    [StringLength(200)]
    public string? ProductName { get; set; }

    public int Quantity { get; set; }

    [Range(typeof(decimal), "0", "9999999999999999.99")]
    public decimal UnitPrice { get; set; }

    [Range(typeof(decimal), "0", "9999999999999999.99")]
    public decimal LineTotal { get; set; }
}
