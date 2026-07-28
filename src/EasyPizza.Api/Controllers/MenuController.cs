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

    // Chamado pelo aplicativo Frontend (Visão do Cliente) para carregar tudo de uma vez
    [HttpGet("{tenantSlug}")]
    public async Task<IActionResult> GetMenu(string tenantSlug)
    {
        var catalog = await _catalogService.GetCatalogAsync();
        return Ok(catalog);
    }
}
