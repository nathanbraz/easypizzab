using EasyPizza.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace EasyPizza.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuController : ControllerBase
{
    private readonly ICatalogService _catalogService;

    public MenuController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    // Called by the Frontend app (Client View) to load everything at once
    [HttpGet("{tenantSlug}")]
    public async Task<IActionResult> GetMenu(string tenantSlug)
    {
        var catalog = await _catalogService.GetCatalogAsync();
        return Ok(catalog);
    }
}
