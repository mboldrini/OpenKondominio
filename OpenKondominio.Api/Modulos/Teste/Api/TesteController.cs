using Microsoft.AspNetCore.Mvc;
using OpenKondominio.Api.Modulos.Teste.Application.Services;

namespace OpenKondominio.Api.Modulos.Teste.Api;

[ApiController]
[Route("api/teste")]
public class TesteController : ControllerBase
{
    [HttpGet("ping")]
    public IActionResult Ping()
        => Ok("Teste OK");
}
