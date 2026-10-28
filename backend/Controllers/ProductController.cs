
using backend.DTOs.Products;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly ProductService _productService;

    public ProductController(ProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProductResponseDto>>> GetAll()
    {
        var products = await _productService.GetAllAsync();
        return Ok(products);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductResponseDto>> GetById(int id)
    {
        var product = await _productService.GetByIdAsync(id);

        if (product is null)
        {
            return NotFound(new { message = "Ürün bulunamadı." });
        }

        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<ProductResponseDto>> Create(
        [FromBody] CreateProductDto dto)
    {
        try
        {
            var product = await _productService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = product.Id },
                product);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductResponseDto>> Update(
        int id,
        [FromBody] UpdateProductDto dto)
    {
        try
        {
            var product = await _productService.UpdateAsync(id, dto);

            if (product is null)
            {
                return NotFound(new { message = "Ürün bulunamadı." });
            }

            return Ok(product);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            bool deleted = await _productService.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound(new { message = "Ürün bulunamadı." });
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
