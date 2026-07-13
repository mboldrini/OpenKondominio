using OpenKondominio.Api.Modulos.Auth.Domain.Entities;

namespace OpenKondominio.Api.Modulos.Auth.Domain.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshTokenEntity?> BuscarPorHashAsync(string hash, CancellationToken ct = default);
    Task AdicionarAsync(RefreshTokenEntity token, CancellationToken ct = default);
    Task SalvarAsync(CancellationToken ct = default);
}
