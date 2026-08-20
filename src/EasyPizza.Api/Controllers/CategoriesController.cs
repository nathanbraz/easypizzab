using EasyPizza.Application.Interfaces.Repositories;
using EasyPizza.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace EasyPizza.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly IRepository<ProductCategory> _repository;

    public CategoriesController(IRepository<ProductCategory> repository)
    {
        _repository = repository;
    }

    [HttpGet("{tenantSlug}")]
    public async Task<IActionResult> GetAll(string tenantSlug)
    {
        var categories = await _repository.GetAllAsync();
        return Ok(categories.OrderBy(c => c.DisplayOrder));
    }

    [Authorize(Policy = "RequireTenant")]
    [HttpPost("{tenantSlug}")]
    public async Task<IActionResult> Create(string tenantSlug, [FromBody] CreateCategoryRequest request)
    {
        var category = new ProductCategory(request.Name, request.DisplayOrder);
        await _repository.AddAsync(category);
        await _repository.SaveChangesAsync();
        return Ok(category);
    }

    [Authorize(Policy = "RequireTenant")]
    [HttpPut("{tenantSlug}/{id:guid}")]
    public async Task<IActionResult> Update(string tenantSlug, Guid id, [FromBody] UpdateCategoryRequest request)
    {
        var category = await _repository.GetByIdAsync(id);
        if (category == null) return NotFound();

        category.UpdateDetails(request.Name, request.DisplayOrder);
        await _repository.UpdateAsync(category);
        await _repository.SaveChangesAsync();

        return Ok(category);
    }

    [Authorize(Policy = "RequireTenant")]
    [HttpDelete("{tenantSlug}/{id:guid}")]
    public async Task<IActionResult> Delete(string tenantSlug, Guid id)
    {
        var category = await _repository.GetByIdAsync(id);
        if (category == null) return NotFound();

        await _repository.DeleteAsync(category);
        await _repository.SaveChangesAsync();

        return NoContent();
    }
}

public record CreateCategoryRequest(string Name, int DisplayOrder);
public record UpdateCategoryRequest(string Name, int DisplayOrder);
