using EasyPizza.Domain.Entities;
using EasyPizza.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace EasyPizza.Api.Controllers;

// CRUD admin dos grupos/itens de opção compartilhados por categoria (Tamanho, Borda) e do preço
// que cada produto define para cada item. Ver CategoryOptionGroup/CategoryOptionItem/
// ProductCategoryOptionPrice para o racional completo dessa modelagem.
[ApiController]
[Route("api/[controller]")]
public class CategoryOptionsController : ControllerBase
{
    private readonly EasyPizzaDbContext _context;

    public CategoryOptionsController(EasyPizzaDbContext context)
    {
        _context = context;
    }

    // Grupos (e itens) definidos para a categoria, independente de produto.
    [HttpGet("{tenantSlug}/category/{categoryId:guid}")]
    public async Task<IActionResult> GetByCategory(string tenantSlug, Guid categoryId)
    {
        var groups = await _context.CategoryOptionGroups
            .Where(g => g.CategoryId == categoryId)
            .Include(g => g.Items.OrderBy(i => i.DisplayOrder))
            .OrderBy(g => g.DisplayOrder)
            .ToListAsync();

        return Ok(groups);
    }

    [Authorize(Policy = "RequireTenant")]
    [HttpPost("{tenantSlug}/category/{categoryId:guid}")]
    public async Task<IActionResult> CreateGroup(string tenantSlug, Guid categoryId, [FromBody] CreateCategoryOptionGroupRequest request)
    {
        if (request.IsFlavorGroup && await _context.CategoryOptionGroups.AnyAsync(g => g.CategoryId == categoryId && g.IsFlavorGroup))
            return BadRequest(new { error = "Esta categoria já tem um grupo de Sabores." });

        var group = new CategoryOptionGroup(categoryId, request.Name, request.MinChoices, request.MaxChoices, request.DisplayOrder, request.HasUniformPricing, request.IsFlavorGroup, request.FlavorPriceStrategy);

        _context.CategoryOptionGroups.Add(group);
        await _context.SaveChangesAsync();
        return Ok(group);
    }

    [Authorize(Policy = "RequireTenant")]
    [HttpPut("{tenantSlug}/group/{id:guid}")]
    public async Task<IActionResult> UpdateGroup(string tenantSlug, Guid id, [FromBody] UpdateCategoryOptionGroupRequest request)
    {
        var group = await _context.CategoryOptionGroups.FindAsync(id);
        if (group == null) return NotFound();

        if (request.IsFlavorGroup && await _context.CategoryOptionGroups.AnyAsync(g => g.CategoryId == group.CategoryId && g.IsFlavorGroup && g.Id != id))
            return BadRequest(new { error = "Esta categoria já tem um grupo de Sabores." });

        group.UpdateDetails(request.Name, request.MinChoices, request.MaxChoices, request.DisplayOrder, request.HasUniformPricing, request.IsFlavorGroup, request.FlavorPriceStrategy);
        await _context.SaveChangesAsync();
        return Ok(group);
    }

    [Authorize(Policy = "RequireTenant")]
    [HttpDelete("{tenantSlug}/group/{id:guid}")]
    public async Task<IActionResult> DeleteGroup(string tenantSlug, Guid id)
    {
        var group = await _context.CategoryOptionGroups.FindAsync(id);
        if (group == null) return NotFound();

        // Cascata (config. no DbContext) remove Items e, por tabela, os ProductCategoryOptionPrices ligados.
        _context.CategoryOptionGroups.Remove(group);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [Authorize(Policy = "RequireTenant")]
    [HttpPost("{tenantSlug}/group/{groupId:guid}/items")]
    public async Task<IActionResult> CreateItem(string tenantSlug, Guid groupId, [FromBody] CreateCategoryOptionItemRequest request)
    {
        var group = await _context.CategoryOptionGroups.FindAsync(groupId);
        if (group == null) return NotFound();

        var (name, productId, error) = await ResolveFlavorItemAsync(group, request.Name, request.ProductId);
        if (error != null) return BadRequest(new { error });

        var item = new CategoryOptionItem(groupId, name, request.DisplayOrder, request.UniformPrice, productId);

        _context.CategoryOptionItems.Add(item);
        await _context.SaveChangesAsync();
        return Ok(item);
    }

    [Authorize(Policy = "RequireTenant")]
    [HttpPut("{tenantSlug}/item/{id:guid}")]
    public async Task<IActionResult> UpdateItem(string tenantSlug, Guid id, [FromBody] UpdateCategoryOptionItemRequest request)
    {
        var item = await _context.CategoryOptionItems.Include(i => i.Group).FirstOrDefaultAsync(i => i.Id == id);
        if (item == null) return NotFound();

        var (name, productId, error) = await ResolveFlavorItemAsync(item.Group!, request.Name, request.ProductId);
        if (error != null) return BadRequest(new { error });

        item.UpdateDetails(name, request.DisplayOrder, request.UniformPrice, productId);
        await _context.SaveChangesAsync();
        return Ok(item);
    }

    // Grupo de Sabores: o item representa um Produto de verdade da mesma categoria — nome vem do
    // Produto, não é digitado à mão. Fora desse grupo, ProductId é sempre ignorado (não faz sentido
    // linkar um Produto a um item de Tamanho/Borda/Adicionais).
    private async Task<(string Name, Guid? ProductId, string? Error)> ResolveFlavorItemAsync(CategoryOptionGroup group, string requestName, Guid? requestProductId)
    {
        if (!group.IsFlavorGroup) return (requestName, null, null);

        if (requestProductId == null) return (requestName, null, "Escolha um produto para representar o sabor.");

        var product = await _context.Products.FindAsync(requestProductId.Value);
        if (product == null || product.CategoryId != group.CategoryId)
            return (requestName, null, "Produto inválido para esta categoria.");

        return (product.Name, product.Id, null);
    }

    [Authorize(Policy = "RequireTenant")]
    [HttpDelete("{tenantSlug}/item/{id:guid}")]
    public async Task<IActionResult> DeleteItem(string tenantSlug, Guid id)
    {
        var item = await _context.CategoryOptionItems.FindAsync(id);
        if (item == null) return NotFound();

        // Cascata remove também os ProductCategoryOptionPrices desse item em qualquer produto.
        _context.CategoryOptionItems.Remove(item);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // Visão administrativa: todos os grupos/itens da categoria, indicando quais esse produto
    // específico já oferece (e a que preço) — para a tela de admin marcar/desmarcar cada opção.
    [Authorize(Policy = "RequireTenant")]
    [HttpGet("{tenantSlug}/category/{categoryId:guid}/product/{productId:guid}")]
    public async Task<IActionResult> GetForProduct(string tenantSlug, Guid categoryId, Guid productId)
    {
        var groups = await _context.CategoryOptionGroups
            .Where(g => g.CategoryId == categoryId)
            .Include(g => g.Items.OrderBy(i => i.DisplayOrder))
            .OrderBy(g => g.DisplayOrder)
            .ToListAsync();

        var productPrices = await _context.ProductCategoryOptionPrices
            .Where(p => p.ProductId == productId)
            .ToDictionaryAsync(p => p.CategoryOptionItemId);

        var result = groups.Select(g => new CategoryOptionAdminGroupDto
        {
            Id = g.Id,
            Name = g.Name,
            MinChoices = g.MinChoices,
            MaxChoices = g.MaxChoices,
            DisplayOrder = g.DisplayOrder,
            HasUniformPricing = g.HasUniformPricing,
            IsFlavorGroup = g.IsFlavorGroup,
            FlavorPriceStrategy = g.FlavorPriceStrategy,
            Items = g.Items.Select(i =>
            {
                productPrices.TryGetValue(i.Id, out var row);
                return new CategoryOptionAdminItemDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    DisplayOrder = i.DisplayOrder,
                    UniformPrice = i.UniformPrice,
                    IsOffered = row?.IsOffered ?? false,
                    // Preço efetivo: se o grupo é uniforme, sempre o preço do item (mesmo antes do
                    // produto oferecer — útil pra pré-visualizar quanto vai custar ao marcar). Senão,
                    // é o último preço configurado por este produto — mesmo que esteja desativado no
                    // momento, pra reativar não perder o valor.
                    AdditionalPrice = g.HasUniformPricing ? i.UniformPrice : row?.AdditionalPrice,
                    ProductId = i.ProductId
                };
            }).ToList()
        });

        return Ok(result);
    }

    // Define (cria ou atualiza) o preço desse produto para um item — é isso que faz o produto
    // "oferecer" a opção. Sem essa linha, o item simplesmente não aparece pra esse produto.
    [Authorize(Policy = "RequireTenant")]
    [HttpPut("{tenantSlug}/product/{productId:guid}/item/{itemId:guid}")]
    public async Task<IActionResult> SetProductPrice(string tenantSlug, Guid productId, Guid itemId, [FromBody] SetProductOptionPriceRequest request)
    {
        var item = await _context.CategoryOptionItems.Include(i => i.Group).FirstOrDefaultAsync(i => i.Id == itemId);
        if (item == null) return NotFound();

        // Em grupo de preço uniforme (ex: Borda), quem manda é sempre CategoryOptionItem.UniformPrice
        // — não guardamos um valor solto aqui (ficaria morto, nunca lido). O valor mandado pelo
        // cliente é ignorado nesse caso.
        decimal? price = item.Group!.HasUniformPricing ? null : request.AdditionalPrice;

        var existing = await _context.ProductCategoryOptionPrices
            .FirstOrDefaultAsync(p => p.ProductId == productId && p.CategoryOptionItemId == itemId);

        if (existing != null)
        {
            existing.UpdatePrice(price);
        }
        else
        {
            existing = new ProductCategoryOptionPrice(productId, itemId, price);
            _context.ProductCategoryOptionPrices.Add(existing);
        }

        await _context.SaveChangesAsync();
        return Ok(existing);
    }

    // Remove a oferta dessa opção pelo produto (ex: "essa pizza não vem em P"). Não apaga a linha —
    // só marca IsOffered=false, guardando o preço configurado caso o lojista marque de novo depois
    // (sem isso, reativar "esquecia" o preço e o admin tinha que digitar de novo toda vez).
    [Authorize(Policy = "RequireTenant")]
    [HttpDelete("{tenantSlug}/product/{productId:guid}/item/{itemId:guid}")]
    public async Task<IActionResult> RemoveProductPrice(string tenantSlug, Guid productId, Guid itemId)
    {
        var existing = await _context.ProductCategoryOptionPrices
            .FirstOrDefaultAsync(p => p.ProductId == productId && p.CategoryOptionItemId == itemId);

        if (existing == null) return NotFound();

        existing.SetOffered(false);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

public record CreateCategoryOptionGroupRequest(string Name, int MinChoices, int MaxChoices, int DisplayOrder, bool HasUniformPricing = false, bool IsFlavorGroup = false, FlavorPriceStrategy FlavorPriceStrategy = FlavorPriceStrategy.MaisCaro);
public record UpdateCategoryOptionGroupRequest(string Name, int MinChoices, int MaxChoices, int DisplayOrder, bool HasUniformPricing = false, bool IsFlavorGroup = false, FlavorPriceStrategy FlavorPriceStrategy = FlavorPriceStrategy.MaisCaro);

public record CreateCategoryOptionItemRequest(string Name, int DisplayOrder, decimal? UniformPrice = null, Guid? ProductId = null);
public record UpdateCategoryOptionItemRequest(string Name, int DisplayOrder, decimal? UniformPrice = null, Guid? ProductId = null);

public record SetProductOptionPriceRequest(decimal AdditionalPrice);

public class CategoryOptionAdminGroupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MinChoices { get; set; }
    public int MaxChoices { get; set; }
    public int DisplayOrder { get; set; }
    public bool HasUniformPricing { get; set; }
    public bool IsFlavorGroup { get; set; }
    public FlavorPriceStrategy FlavorPriceStrategy { get; set; }
    public List<CategoryOptionAdminItemDto> Items { get; set; } = new();
}

public class CategoryOptionAdminItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public decimal? UniformPrice { get; set; }
    public bool IsOffered { get; set; }
    public decimal? AdditionalPrice { get; set; }
    public Guid? ProductId { get; set; }
}
