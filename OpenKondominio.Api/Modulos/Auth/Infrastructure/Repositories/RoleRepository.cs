using Microsoft.EntityFrameworkCore;
using OpenKondominio.Api.Modulos.Auth.Domain.Entities;
using OpenKondominio.Api.Modulos.Auth.Domain.Interfaces;
using OpenKondominio.Api.Modulos.Auth.Infrastructure.Persistence;

namespace OpenKondominio.Api.Modulos.Auth.Infrastructure.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly AuthDbContext _ctx;

    public RoleRepository(AuthDbContext ctx) => _ctx = ctx;

    public Task<RoleEntity?> BuscarPorIdAsync(Guid id, CancellationToken ct = default) =>
        _ctx.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<IEnumerable<RoleEntity>> BuscarRolesPorUsuarioCondominioAsync(
        Guid usuarioId, Guid condominioId, CancellationToken ct = default)
    {
        var roleIds = await _ctx.UsuarioCondominioRoles
            .Where(ucr => ucr.UsuarioId == usuarioId && ucr.CondominioId == condominioId)
            .Select(ucr => ucr.RoleId)
            .ToListAsync(ct);

        return await _ctx.Roles
            .Include(r => r.Permissions)
            .Where(r => roleIds.Contains(r.Id))
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<RoleEntity>> ListarAsync(CancellationToken ct = default) =>
        await _ctx.Roles.Include(r => r.Permissions).ToListAsync(ct);

    public async Task AdicionarAsync(RoleEntity role, CancellationToken ct = default) =>
        await _ctx.Roles.AddAsync(role, ct);

    public Task<bool> ExisteVinculoAsync(
        Guid usuarioId, Guid condominioId, Guid roleId, CancellationToken ct = default) =>
        _ctx.UsuarioCondominioRoles.AnyAsync(
            ucr => ucr.UsuarioId == usuarioId
                   && ucr.CondominioId == condominioId
                   && ucr.RoleId == roleId, ct);

    public async Task AdicionarVinculoAsync(
        UsuarioCondominioRoleEntity vinculo, CancellationToken ct = default) =>
        await _ctx.UsuarioCondominioRoles.AddAsync(vinculo, ct);

    public async Task RemoverVinculoAsync(
        Guid usuarioId, Guid condominioId, Guid roleId, CancellationToken ct = default)
    {
        var vinculo = await _ctx.UsuarioCondominioRoles
            .FirstOrDefaultAsync(ucr =>
                ucr.UsuarioId == usuarioId
                && ucr.CondominioId == condominioId
                && ucr.RoleId == roleId, ct);

        if (vinculo is not null) _ctx.UsuarioCondominioRoles.Remove(vinculo);
    }

    public Task<PermissionEntity?> BuscarPermissaoPorIdAsync(Guid id, CancellationToken ct = default) =>
        _ctx.Permissions.FindAsync([id], ct).AsTask();

    public async Task<IEnumerable<PermissionEntity>> ListarPermissoesAsync(CancellationToken ct = default) =>
        await _ctx.Permissions.ToListAsync(ct);

    public async Task AdicionarPermissaoAsync(PermissionEntity permission, CancellationToken ct = default) =>
        await _ctx.Permissions.AddAsync(permission, ct);

    public Task SalvarAsync(CancellationToken ct = default) =>
        _ctx.SaveChangesAsync(ct);
}
