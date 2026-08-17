using Microsoft.AspNetCore.Mvc;

namespace EasyPizza.Api.Controllers;

// Endpoint público de health check — usado pelo Kamal Proxy pra saber se o container está de
// pé antes de rotear tráfego pra ele (ver proxy.healthcheck em config/deploy.yml). Não depende
// de tenant nem de banco de dados: só confirma que o processo está respondendo.
[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok();
}
