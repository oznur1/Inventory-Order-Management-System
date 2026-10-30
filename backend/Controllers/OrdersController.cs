using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrdersController(OrderService orderService)
    {
        _orderService = orderService;
    }

    // POST: api/Orders
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateOrderDto dto)
    {
        try
        {
            var result = await _orderService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    // GET: api/Orders/{id}
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        return NotFound(new
        {
            message = "Sipariş sorgulama henüz eklenmedi."
        });
    }
}