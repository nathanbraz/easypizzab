using EasyPizza.Application.Interfaces.Repositories;
using EasyPizza.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EasyPizza.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AddonsController : ControllerBase
{
    private readonly IRepository<ProductAddon> _repository;

    public AddonsController(IRepository<ProductAddon> repository)
    {
        _repository = repository;
    }

    [HttpGet("{tenantSlug}")]
    public async Task<IActionResult> GetAll(string tenantSlug)
    {
        var addons = await _repository.GetAllAsync();
        return Ok(addons);
    }

    [HttpPost("{tenantSlug}")]
    public async Task<IActionResult> Create(string tenantSlug, [FromBody] CreateAddonRequest request)
    {
        var addon = new ProductAddon(request.ProductId, request.Name, request.AdditionalPrice);
        await _repository.AddAsync(addon);
        await _repository.SaveChangesAsync();
        return Ok(addon);
    }

    [HttpPut("{tenantSlug}/{id:guid}")]
    public async Task<IActionResult> Update(string tenantSlug, Guid id, [FromBody] UpdateAddonRequest request)
    {
        var addon = await _repository.GetByIdAsync(id);
        if (addon == null) return NotFound();

        addon.UpdateDetails(request.Name, request.AdditionalPrice);
        await _repository.UpdateAsync(addon);
        await _repository.SaveChangesAsync();

        return Ok(addon);
    }

    [HttpDelete("{tenantSlug}/{id:guid}")]
    public async Task<IActionResult> Delete(string tenantSlug, Guid id)
    {
        var addon = await _repository.GetByIdAsync(id);
        if (addon == null) return NotFound();

        await _repository.DeleteAsync(addon);
        await _repository.SaveChangesAsync();

        return NoContent();
    }
}

public record CreateAddonRequest(Guid ProductId, string Name, decimal AdditionalPrice);
public record UpdateAddonRequest(string Name, decimal AdditionalPrice);
