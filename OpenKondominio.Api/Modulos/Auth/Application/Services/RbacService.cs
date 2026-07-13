using Microsoft.Extensions.Caching.Memory;
using OpenKondominio.Api.Modulos.Auth.Application.Commands;
using OpenKondominio.Api.Modulos.Auth.Domain.Entities;
using OpenKondominio.Api.Modulos.Auth.Domain.Interfaces;

namespace OpenKondominio.Api.Modulos.Auth.Application.Services;

public class RbacService
{
    private readonly IRoleRepository _roleRepo;
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly IMemoryCache _cache;

    public RbacService(IRoleRepository roleRepo, IUsuarioRepository usuarioRepo, IMemoryCache cache)
    {
        _roleRepo = roleRepo;
        _usuarioRepo = usuarioRepo;
        _cache = cache;
    }

    public async Task AssignRoleAsync(AssignRoleCommand cmd, CancellationToken ct = default)
    {
        _ = await _usuarioRepo.BuscarPorIdAsync(cmd.UsuarioId, ct)
            ?? throw new InvalidOperationException("Usuário não encontrado.");

        _ = await _roleRepo.BuscarPorIdAsync(cmd.RoleId, ct)
            ?? throw new InvalidOperationException("Role não encontrada.");

        if (await _roleRepo.ExisteVinculoAsync(cmd.UsuarioId, cmd.CondominioId, cmd.RoleId, ct))
            return;

        var vinculo = new UsuarioCondominioRoleEntity(cmd.UsuarioId, cmd.CondominioId, cmd.RoleId);
        await _roleRepo.AdicionarVinculoAsync(vinculo, ct);
        await _roleRepo.SalvarAsync(ct);

        InvalidarCache(cmd.UsuarioId, cmd.CondominioId);
    }

    public async Task RevokeRoleAsync(RevokeRoleCommand cmd, CancellationToken ct = default)
    {
        await _roleRepo.RemoverVinculoAsync(cmd.UsuarioId, cmd.CondominioId, cmd.RoleId, ct);
        await _roleRepo.SalvarAsync(ct);
        InvalidarCache(cmd.UsuarioId, cmd.CondominioId);
    }

    public async Task<IEnumerable<string>> GetPermissionsAsync(
        Guid usuarioId, Guid condominioId, CancellationToken ct = default)
    {
        var cacheKey = $"perms:{usuarioId}:{condominioId}";

        if (_cache.TryGetValue(cacheKey, out IEnumerable<string>? cached) && cached is not null)
            return cached;

        var roles = await _roleRepo.BuscarRolesPorUsuarioCondominioAsync(usuarioId, condominioId, ct);
        var permissions = roles
            .SelectMany(r => r.Permissions.Select(p => p.Chave))
            .Distinct()
            .ToList();

        _cache.Set(cacheKey, (IEnumerable<string>)permissions, TimeSpan.FromSeconds(60));
        return permissions;
    }

    public async Task<RoleEntity> CriarRoleAsync(string nome, string descricao, CancellationToken ct = default)
    {
        var role = new RoleEntity(nome, descricao);
        await _roleRepo.AdicionarAsync(role, ct);
        await _roleRepo.SalvarAsync(ct);
        return role;
    }

    public async Task AdicionarPermissaoARoleAsync(Guid roleId, Guid permissionId, CancellationToken ct = default)
    {
        var role = await _roleRepo.BuscarPorIdAsync(roleId, ct)
            ?? throw new InvalidOperationException("Role não encontrada.");
        var permission = await _roleRepo.BuscarPermissaoPorIdAsync(permissionId, ct)
            ?? throw new InvalidOperationException("Permissão não encontrada.");
        role.AdicionarPermissao(permission);
        await _roleRepo.SalvarAsync(ct);
    }

    public async Task RemoverPermissaoDaRoleAsync(Guid roleId, Guid permissionId, CancellationToken ct = default)
    {
        var role = await _roleRepo.BuscarPorIdAsync(roleId, ct)
            ?? throw new InvalidOperationException("Role não encontrada.");
        role.RemoverPermissao(permissionId);
        await _roleRepo.SalvarAsync(ct);
    }

    public async Task<PermissionEntity> CriarPermissaoAsync(string chave, CancellationToken ct = default)
    {
        var permission = new PermissionEntity(chave);
        await _roleRepo.AdicionarPermissaoAsync(permission, ct);
        await _roleRepo.SalvarAsync(ct);
        return permission;
    }

    public Task<IEnumerable<RoleEntity>> ListarRolesAsync(CancellationToken ct = default)
        => _roleRepo.ListarAsync(ct);

    public Task<IEnumerable<PermissionEntity>> ListarPermissoesAsync(CancellationToken ct = default)
        => _roleRepo.ListarPermissoesAsync(ct);

    private void InvalidarCache(Guid usuarioId, Guid condominioId)
        => _cache.Remove($"perms:{usuarioId}:{condominioId}");
}
