using EasyPizza.Application.Interfaces.Repositories;
using EasyPizza.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EasyPizza.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IRepository<Product> _repository;

    public ProductsController(IRepository<Product> repository)
    {
        _repository = repository;
    }

    [HttpGet("{tenantSlug}")]
    public async Task<IActionResult> GetAll(string tenantSlug)
    {
        var products = await _repository.GetAllAsync();
        return Ok(products);
    }

    [HttpPost("{tenantSlug}")]
    public async Task<IActionResult> Create(string tenantSlug, [FromBody] CreateProductRequest request)
    {
        var product = new Product(request.CategoryId, request.Name, request.Description, request.Price);
        if (!string.IsNullOrEmpty(request.ImageUrl))
        {
            product.UpdateDetails(request.Name, request.Description, request.Price, request.ImageUrl, true);
        }
        await _repository.AddAsync(product);
        await _repository.SaveChangesAsync();
        return Ok(product);
    }

    [HttpPut("{tenantSlug}/{id:guid}")]
    public async Task<IActionResult> Update(string tenantSlug, Guid id, [FromBody] UpdateProductRequest request)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product == null) return NotFound();

        product.UpdateDetails(request.Name, request.Description, request.Price, request.ImageUrl, request.IsAvailable);
        await _repository.UpdateAsync(product);
        await _repository.SaveChangesAsync();

        return Ok(product);
    }

    [HttpDelete("{tenantSlug}/{id:guid}")]
    public async Task<IActionResult> Delete(string tenantSlug, Guid id)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product == null) return NotFound();

        await _repository.DeleteAsync(product);
        await _repository.SaveChangesAsync();

        return NoContent();
    }
}

public record CreateProductRequest(Guid CategoryId, string Name, string Description, decimal Price, string? ImageUrl);
public record UpdateProductRequest(string Name, string Description, decimal Price, string? ImageUrl, bool IsAvailable);
