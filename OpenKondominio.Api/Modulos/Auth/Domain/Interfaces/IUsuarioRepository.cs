using OpenKondominio.Api.Modulos.Auth.Domain.Entities;

namespace OpenKondominio.Api.Modulos.Auth.Domain.Interfaces;

public interface IUsuarioRepository
{
    Task<UsuarioEntity?> BuscarPorEmailAsync(string email, CancellationToken ct = default);
    Task<UsuarioEntity?> BuscarPorIdAsync(Guid id, CancellationToken ct = default);
    Task AdicionarAsync(UsuarioEntity usuario, CancellationToken ct = default);
    Task SalvarAsync(CancellationToken ct = default);
}
