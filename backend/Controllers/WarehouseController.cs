
using backend.DTOs.Warehouses;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarehouseController : ControllerBase
{
    private readonly WarehouseService _warehouseService;

    public WarehouseController(WarehouseService warehouseService)
    {
        _warehouseService = warehouseService;
    }

    // GET: api/warehouse
    [HttpGet]
    public async Task<ActionResult<List<WarehouseResponseDto>>> GetAll()
    {
        var warehouses = await _warehouseService.GetAllAsync();
        return Ok(warehouses);
    }

    // GET: api/warehouse/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<WarehouseResponseDto>> GetById(int id)
    {
        var warehouse = await _warehouseService.GetByIdAsync(id);

        if (warehouse is null)
        {
            return NotFound(new { message = "Depo bulunamadı." });
        }

        return Ok(warehouse);
    }

    // POST: api/warehouse
    [HttpPost]
    public async Task<ActionResult<WarehouseResponseDto>> Create(
        [FromBody] CreateWarehouseDto dto)
    {
        try
        {
            var warehouse = await _warehouseService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = warehouse.Id },
                warehouse);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // PUT: api/warehouse/1
    [HttpPut("{id:int}")]
    public async Task<ActionResult<WarehouseResponseDto>> Update(
        int id,
        [FromBody] UpdateWarehouseDto dto)
    {
        try
        {
            var warehouse = await _warehouseService.UpdateAsync(id, dto);

            if (warehouse is null)
            {
                return NotFound(new { message = "Depo bulunamadı." });
            }

            return Ok(warehouse);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // DELETE: api/warehouse/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            bool deleted = await _warehouseService.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound(new { message = "Depo bulunamadı." });
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
