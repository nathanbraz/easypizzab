using EasyPizza.Domain.Entities;
using EasyPizza.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyPizza.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductOptionsController : ControllerBase
{
    private readonly EasyPizzaDbContext _context;

    public ProductOptionsController(EasyPizzaDbContext context)
    {
        _context = context;
    }

    [HttpGet("{tenantSlug}/product/{productId:guid}")]
    public async Task<IActionResult> GetByProduct(string tenantSlug, Guid productId)
    {
        var groups = await _context.ProductOptionGroups
            .Include(g => g.Options)
            .Where(g => g.ProductId == productId)
            .OrderBy(g => g.DisplayOrder)
            .ToListAsync();
            
        return Ok(groups);
    }

    [HttpPost("{tenantSlug}/product/{productId:guid}")]
    public async Task<IActionResult> CreateGroup(string tenantSlug, Guid productId, [FromBody] CreateOptionGroupRequest request)
    {
        var group = new ProductOptionGroup(
            productId, 
            request.Name, 
            request.GroupType ?? "single",
            request.IsRequired, 
            request.MinChoices, 
            request.MaxChoices, 
            request.DisplayOrder
        );
        
        _context.ProductOptionGroups.Add(group);
        await _context.SaveChangesAsync();
        return Ok(group);
    }

    [HttpPut("{tenantSlug}/group/{id:guid}")]
    public async Task<IActionResult> UpdateGroup(string tenantSlug, Guid id, [FromBody] UpdateOptionGroupRequest request)
    {
        var group = await _context.ProductOptionGroups.FindAsync(id);
        if (group == null) return NotFound();

        group.UpdateDetails(request.Name, request.GroupType ?? "single", request.IsRequired, request.MinChoices, request.MaxChoices, request.DisplayOrder);
        _context.ProductOptionGroups.Update(group);
        await _context.SaveChangesAsync();
        return Ok(group);
    }

    [HttpDelete("{tenantSlug}/group/{id:guid}")]
    public async Task<IActionResult> DeleteGroup(string tenantSlug, Guid id)
    {
        var group = await _context.ProductOptionGroups.FindAsync(id);
        if (group == null) return NotFound();

        _context.ProductOptionGroups.Remove(group);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{tenantSlug}/group/{groupId:guid}/items")]
    public async Task<IActionResult> CreateItem(string tenantSlug, Guid groupId, [FromBody] CreateOptionItemRequest request)
    {
        var item = new ProductOptionItem(
            groupId, 
            request.Name, 
            request.AdditionalPrice, 
            request.DisplayOrder
        );
        
        _context.ProductOptionItems.Add(item);
        await _context.SaveChangesAsync();
        return Ok(item);
    }

    [HttpPut("{tenantSlug}/item/{id:guid}")]
    public async Task<IActionResult> UpdateItem(string tenantSlug, Guid id, [FromBody] UpdateOptionItemRequest request)
    {
        var item = await _context.ProductOptionItems.FindAsync(id);
        if (item == null) return NotFound();

        item.UpdateDetails(request.Name, request.AdditionalPrice, request.DisplayOrder);
        _context.ProductOptionItems.Update(item);
        await _context.SaveChangesAsync();
        return Ok(item);
    }

    [HttpDelete("{tenantSlug}/item/{id:guid}")]
    public async Task<IActionResult> DeleteItem(string tenantSlug, Guid id)
    {
        var item = await _context.ProductOptionItems.FindAsync(id);
        if (item == null) return NotFound();

        _context.ProductOptionItems.Remove(item);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

public record CreateOptionGroupRequest(string Name, string GroupType, bool IsRequired, int MinChoices, int MaxChoices, int DisplayOrder);
public record UpdateOptionGroupRequest(string Name, string GroupType, bool IsRequired, int MinChoices, int MaxChoices, int DisplayOrder);

public record CreateOptionItemRequest(string Name, decimal AdditionalPrice, int DisplayOrder);
public record UpdateOptionItemRequest(string Name, decimal AdditionalPrice, int DisplayOrder);
