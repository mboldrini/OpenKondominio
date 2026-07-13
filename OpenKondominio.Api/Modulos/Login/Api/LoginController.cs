using Microsoft.AspNetCore.Mvc;
using OpenKondominio.Api.Modulos.Login.Application.Services;

namespace OpenKondominio.Api.Modulos.Login.Api;

[ApiController]
[Route("api/login")]
public class LoginController : ControllerBase
{
    [HttpGet("ping")]
    public IActionResult Ping()
        => Ok("Login OK");
}
