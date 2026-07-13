using Microsoft.EntityFrameworkCore;
using OpenKondominio.Api.Modulos.Auth.Domain.Entities;
using OpenKondominio.Api.Modulos.Auth.Domain.Interfaces;
using OpenKondominio.Api.Modulos.Auth.Infrastructure.Persistence;

namespace OpenKondominio.Api.Modulos.Auth.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AuthDbContext _ctx;

    public RefreshTokenRepository(AuthDbContext ctx) => _ctx = ctx;

    public Task<RefreshTokenEntity?> BuscarPorHashAsync(string hash, CancellationToken ct = default) =>
        _ctx.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == hash, ct);

    public async Task AdicionarAsync(RefreshTokenEntity token, CancellationToken ct = default) =>
        await _ctx.RefreshTokens.AddAsync(token, ct);

    public Task SalvarAsync(CancellationToken ct = default) =>
        _ctx.SaveChangesAsync(ct);
}
