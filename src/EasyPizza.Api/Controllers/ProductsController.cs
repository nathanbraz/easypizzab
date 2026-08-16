using EasyPizza.Application.Interfaces.Repositories;
using EasyPizza.Application.Interfaces.Services;
using EasyPizza.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace EasyPizza.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IRepository<Product> _repository;
    private readonly ICatalogService _catalogService;

    public ProductsController(IRepository<Product> repository, ICatalogService catalogService)
    {
        _repository = repository;
        _catalogService = catalogService;
    }

    [HttpGet("{tenantSlug}")]
    public async Task<IActionResult> GetAll(string tenantSlug)
    {
        // Traz todos os produtos (inclusive indisponíveis) já com as opções mescladas (próprias +
        // compartilhadas da categoria) — a listagem do admin usa isso pra mostrar o preço real
        // (ex: "A partir de R$35") em vez do preço bruto, que fica R$0 quando o produto usa Tamanho.
        // Já vem ordenado por nome (ver CatalogRepository.GetAllProductsWithOptionsAsync).
        var products = await _catalogService.GetAllProductsWithOptionsAsync();
        return Ok(products);
    }

    [Authorize(Policy = "RequireTenant")]
    [HttpPost("{tenantSlug}")]
    public async Task<IActionResult> Create(string tenantSlug, [FromBody] CreateProductRequest request)
    {
        var product = new Product(request.CategoryId, request.Name, request.Description, request.Price);
        if (request.ImageUrls != null && request.ImageUrls.Any() || request.ShowInCrossSell)
        {
            product.UpdateDetails(request.Name, request.Description, request.Price, request.ImageUrls ?? new List<string>(), true, request.ShowInCrossSell);
        }
        await _repository.AddAsync(product);
        await _repository.SaveChangesAsync();
        return Ok(product);
    }

    [Authorize(Policy = "RequireTenant")]
    [HttpPut("{tenantSlug}/{id:guid}")]
    public async Task<IActionResult> Update(string tenantSlug, Guid id, [FromBody] UpdateProductRequest request)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product == null) return NotFound();

        product.UpdateDetails(request.Name, request.Description, request.Price, request.ImageUrls ?? new List<string>(), request.IsAvailable, request.ShowInCrossSell);
        await _repository.UpdateAsync(product);
        await _repository.SaveChangesAsync();

        return Ok(product);
    }

    [Authorize(Policy = "RequireTenant")]
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

public record CreateProductRequest(Guid CategoryId, string Name, string Description, decimal Price, List<string>? ImageUrls, bool ShowInCrossSell = false);
public record UpdateProductRequest(string Name, string Description, decimal Price, List<string>? ImageUrls, bool IsAvailable, bool ShowInCrossSell = false);
