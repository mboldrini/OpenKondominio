using OpenKondominio.Api.Modulos.Auth.Domain.Entities;

namespace OpenKondominio.Api.Modulos.Auth.Domain.Interfaces;

public interface IRoleRepository
{
    Task<RoleEntity?> BuscarPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<RoleEntity>> BuscarRolesPorUsuarioCondominioAsync(
        Guid usuarioId, Guid condominioId, CancellationToken ct = default);
    Task<IEnumerable<RoleEntity>> ListarAsync(CancellationToken ct = default);
    Task AdicionarAsync(RoleEntity role, CancellationToken ct = default);

    Task<bool> ExisteVinculoAsync(
        Guid usuarioId, Guid condominioId, Guid roleId, CancellationToken ct = default);
    Task AdicionarVinculoAsync(UsuarioCondominioRoleEntity vinculo, CancellationToken ct = default);
    Task RemoverVinculoAsync(
        Guid usuarioId, Guid condominioId, Guid roleId, CancellationToken ct = default);

    Task<PermissionEntity?> BuscarPermissaoPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<PermissionEntity>> ListarPermissoesAsync(CancellationToken ct = default);
    Task AdicionarPermissaoAsync(PermissionEntity permission, CancellationToken ct = default);

    Task SalvarAsync(CancellationToken ct = default);
}
