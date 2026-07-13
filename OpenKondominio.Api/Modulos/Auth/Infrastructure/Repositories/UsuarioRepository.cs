using Microsoft.EntityFrameworkCore;
using OpenKondominio.Api.Modulos.Auth.Domain.Entities;
using OpenKondominio.Api.Modulos.Auth.Domain.Interfaces;
using OpenKondominio.Api.Modulos.Auth.Infrastructure.Persistence;

namespace OpenKondominio.Api.Modulos.Auth.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AuthDbContext _ctx;

    public UsuarioRepository(AuthDbContext ctx) => _ctx = ctx;

    public Task<UsuarioEntity?> BuscarPorEmailAsync(string email, CancellationToken ct = default) =>
        _ctx.Usuarios
            .FirstOrDefaultAsync(u => u.Email.Valor == email.ToLowerInvariant(), ct);

    public Task<UsuarioEntity?> BuscarPorIdAsync(Guid id, CancellationToken ct = default) =>
        _ctx.Usuarios.FindAsync([id], ct).AsTask();

    public async Task AdicionarAsync(UsuarioEntity usuario, CancellationToken ct = default) =>
        await _ctx.Usuarios.AddAsync(usuario, ct);

    public Task SalvarAsync(CancellationToken ct = default) =>
        _ctx.SaveChangesAsync(ct);
}
