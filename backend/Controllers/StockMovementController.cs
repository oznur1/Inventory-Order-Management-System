using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockMovementController : ControllerBase
{
    private readonly StockMovementService _stockMovementService;

    public StockMovementController(
        StockMovementService stockMovementService)
    {
        _stockMovementService = stockMovementService;
    }

    // GET: api/StockMovement
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var movements = await _stockMovementService.GetAllAsync();

        return Ok(movements);
    }

    // POST: api/StockMovement
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateStockMovementDto dto)
    {
        try
        {
            var result =
                await _stockMovementService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetAll),
                result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new
            {
                message = "Stok başka bir işlem tarafından güncellendi. Lütfen tekrar deneyin."
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}