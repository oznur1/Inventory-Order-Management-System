
using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class CreateOrderDto
{
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string CustomerName { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(254)]
    public string? CustomerEmail { get; set; }

    [StringLength(500)]
    public string? ShippingAddress { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "Sipariş en az bir ürün içermelidir.")]
    public List<OrderItemDto> Items { get; set; } = new();
}
