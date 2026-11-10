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
public async Task<IActionResult> GetAll(
    [FromQuery] int? productId,
    [FromQuery] int? warehouseId,
    [FromQuery] string? movementType,
    [FromQuery] DateTime? from,
    [FromQuery] DateTime? to)
{
    if ((productId.HasValue && productId.Value <= 0) ||
        (warehouseId.HasValue && warehouseId.Value <= 0))
    {
        return BadRequest(new
        {
            message = "Ürün ve depo ID değerleri pozitif olmalıdır."
        });
    }

    if (!string.IsNullOrWhiteSpace(movementType))
    {
        if (!string.Equals(movementType, "Inbound",
                StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(movementType, "Outbound",
                StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message = "Hareket türü Inbound veya Outbound olmalıdır."
            });
        }

        movementType = movementType.Trim().Equals(
            "Inbound", StringComparison.OrdinalIgnoreCase)
            ? "Inbound"
            : "Outbound";
    }

    if (from.HasValue && to.HasValue &&
        from.Value.Date > to.Value.Date)
    {
        return BadRequest(new
        {
            message = "Başlangıç tarihi bitiş tarihinden sonra olamaz."
        });
    }

    var movements = await _stockMovementService.GetAllAsync(
        productId,
        warehouseId,
        movementType,
        from,
        to);

    return Ok(movements);
}
   

// GET: api/StockMovement/report
[HttpGet("report")]
public async Task<IActionResult> GetReport()
{
    var report = await _stockMovementService.GetReportAsync();

    return Ok(report);
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