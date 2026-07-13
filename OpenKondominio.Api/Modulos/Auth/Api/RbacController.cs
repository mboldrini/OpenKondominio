using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenKondominio.Api.Modulos.Auth.Application.Commands;
using OpenKondominio.Api.Modulos.Auth.Application.Services;

namespace OpenKondominio.Api.Modulos.Auth.Api;

[ApiController]
[Route("api/rbac")]
[Authorize(Policy = "condominio:admin")]
public class RbacController : ControllerBase
{
    private readonly RbacService _rbac;

    public RbacController(RbacService rbac) => _rbac = rbac;

    [HttpGet("roles")]
    public async Task<IActionResult> ListarRoles(CancellationToken ct) =>
        Ok(await _rbac.ListarRolesAsync(ct));

    [HttpPost("roles")]
    public async Task<IActionResult> CriarRole([FromBody] CriarRoleRequest req, CancellationToken ct)
    {
        var role = await _rbac.CriarRoleAsync(req.Nome, req.Descricao, ct);
        return Created(string.Empty, new { role.Id, role.Nome, role.Descricao });
    }

    [HttpPost("roles/{roleId:guid}/permissions")]
    public async Task<IActionResult> AdicionarPermissao(
        Guid roleId, [FromBody] PermissaoRequest req, CancellationToken ct)
    {
        try
        {
            await _rbac.AdicionarPermissaoARoleAsync(roleId, req.PermissionId, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("roles/{roleId:guid}/permissions/{permissionId:guid}")]
    public async Task<IActionResult> RemoverPermissao(Guid roleId, Guid permissionId, CancellationToken ct)
    {
        try
        {
            await _rbac.RemoverPermissaoDaRoleAsync(roleId, permissionId, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("permissions")]
    public async Task<IActionResult> ListarPermissoes(CancellationToken ct) =>
        Ok(await _rbac.ListarPermissoesAsync(ct));

    [HttpPost("permissions")]
    public async Task<IActionResult> CriarPermissao(
        [FromBody] CriarPermissaoRequest req, CancellationToken ct)
    {
        try
        {
            var perm = await _rbac.CriarPermissaoAsync(req.Chave, ct);
            return Created(string.Empty, new { perm.Id, perm.Chave });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("usuarios/{usuarioId:guid}/roles")]
    public async Task<IActionResult> AssignRole(
        Guid usuarioId, [FromBody] AssignRoleRequest req, CancellationToken ct)
    {
        try
        {
            await _rbac.AssignRoleAsync(
                new AssignRoleCommand(usuarioId, req.CondominioId, req.RoleId), ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("usuarios/{usuarioId:guid}/roles/{roleId:guid}")]
    public async Task<IActionResult> RevokeRole(
        Guid usuarioId, Guid roleId, [FromQuery] Guid condominioId, CancellationToken ct)
    {
        await _rbac.RevokeRoleAsync(new RevokeRoleCommand(usuarioId, condominioId, roleId), ct);
        return NoContent();
    }

    [HttpGet("usuarios/{usuarioId:guid}/permissions")]
    public async Task<IActionResult> GetPermissions(
        Guid usuarioId, [FromQuery] Guid condominioId, CancellationToken ct) =>
        Ok(await _rbac.GetPermissionsAsync(usuarioId, condominioId, ct));
}

public record CriarRoleRequest(string Nome, string Descricao);
public record PermissaoRequest(Guid PermissionId);
public record CriarPermissaoRequest(string Chave);
public record AssignRoleRequest(Guid CondominioId, Guid RoleId);
