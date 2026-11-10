
using backend.DTOs.Inventory;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly InventoryService _inventoryService;

    public InventoryController(InventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    // GET: api/inventory
    // GET: api/inventory?warehouseId=1
    // GET: api/inventory?productId=5
    [HttpGet]
    public async Task<ActionResult<List<InventoryResponseDto>>> GetAll(
        [FromQuery] int? warehouseId,
        [FromQuery] int? productId)
    {
        if ((warehouseId.HasValue && warehouseId.Value <= 0) ||
            (productId.HasValue && productId.Value <= 0))
        {
            return BadRequest(new
            {
                message = "Depo ve ürün ID değerleri pozitif olmalıdır."
            });
        }

        var inventory = await _inventoryService.GetAllAsync(
            warehouseId,
            productId);

        return Ok(inventory);
    }

    // GET: api/inventory/low-stock
    [HttpGet("low-stock")]
    public async Task<ActionResult<List<LowStockAlertDto>>> GetLowStock()
    {
        var alerts = await _inventoryService.GetLowStockAsync();

        return Ok(alerts);
    }

    // GET: api/inventory/report
    [HttpGet("report")]
    public async Task<ActionResult<InventoryReportDto>> GetReport()
    {
        var report = await _inventoryService.GetReportAsync();

        return Ok(report);
    }

    // GET: api/inventory/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<InventoryResponseDto>> GetById(int id)
    {
        var inventory = await _inventoryService.GetByIdAsync(id);

        if (inventory is null)
        {
            return NotFound(new
            {
                message = "Stok kaydı bulunamadı."
            });
        }

        return Ok(inventory);
    }

    // POST: api/inventory
    [HttpPost]
    public async Task<ActionResult<InventoryResponseDto>> Create(
        [FromBody] CreateInventoryDto dto)
    {
        try
        {
            var inventory = await _inventoryService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = inventory.Id },
                inventory);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // PUT: api/inventory/1
    [HttpPut("{id:int}")]
    public async Task<ActionResult<InventoryResponseDto>> Update(
        int id,
        [FromBody] UpdateInventoryDto dto)
    {
        var inventory = await _inventoryService.UpdateAsync(id, dto);

        if (inventory is null)
        {
            return NotFound(new
            {
                message = "Stok kaydı bulunamadı."
            });
        }

        return Ok(inventory);
    }

    // DELETE: api/inventory/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        bool deleted = await _inventoryService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "Stok kaydı bulunamadı."
            });
        }

        return NoContent();
    }
}
