using System.Security.Cryptography;
using System.Text;
using OpenKondominio.Api.Modulos.Auth.Application.Commands;
using OpenKondominio.Api.Modulos.Auth.Application.DTOs;
using OpenKondominio.Api.Modulos.Auth.Domain.Entities;
using OpenKondominio.Api.Modulos.Auth.Domain.Interfaces;
using OpenKondominio.Api.Modulos.Auth.Domain.ValueObjects;

namespace OpenKondominio.Api.Modulos.Auth.Application.Services;

public class AuthService
{
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly IRefreshTokenRepository _refreshRepo;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenService _jwt;

    public AuthService(
        IUsuarioRepository usuarioRepo,
        IRefreshTokenRepository refreshRepo,
        IPasswordHasher hasher,
        IJwtTokenService jwt)
    {
        _usuarioRepo = usuarioRepo;
        _refreshRepo = refreshRepo;
        _hasher = hasher;
        _jwt = jwt;
    }

    public async Task<UsuarioDto> RegisterAsync(RegisterCommand cmd, CancellationToken ct = default)
    {
        var existente = await _usuarioRepo.BuscarPorEmailAsync(cmd.Email, ct);
        if (existente is not null)
            throw new InvalidOperationException("Email já cadastrado.");

        var email = new Email(cmd.Email);
        var senhaHash = _hasher.Hash(cmd.Senha);
        var usuario = new UsuarioEntity(email, cmd.NomeCompleto, senhaHash);

        await _usuarioRepo.AdicionarAsync(usuario, ct);
        await _usuarioRepo.SalvarAsync(ct);

        return new UsuarioDto(usuario.Id, usuario.Email.Valor, usuario.NomeCompleto, usuario.Ativo);
    }

    public async Task<TokenResponse> LoginAsync(LoginCommand cmd, CancellationToken ct = default)
    {
        var usuario = await _usuarioRepo.BuscarPorEmailAsync(cmd.Email, ct);

        if (usuario is null || !_hasher.Verify(cmd.Senha, usuario.SenhaHash) || !usuario.Ativo)
            throw new UnauthorizedAccessException("Credenciais inválidas.");

        return await GerarTokenResponseAsync(usuario, ct);
    }

    public async Task<TokenResponse> RefreshAsync(string refreshTokenRaw, CancellationToken ct = default)
    {
        var hash = HashToken(refreshTokenRaw);
        var token = await _refreshRepo.BuscarPorHashAsync(hash, ct);

        if (token is null || !token.EstaValido())
            throw new UnauthorizedAccessException("Refresh token inválido ou expirado.");

        token.Revogar();
        await _refreshRepo.SalvarAsync(ct);

        var usuario = await _usuarioRepo.BuscarPorIdAsync(token.UsuarioId, ct)
            ?? throw new InvalidOperationException("Usuário não encontrado.");

        return await GerarTokenResponseAsync(usuario, ct);
    }

    public async Task LogoutAsync(string refreshTokenRaw, CancellationToken ct = default)
    {
        var hash = HashToken(refreshTokenRaw);
        var token = await _refreshRepo.BuscarPorHashAsync(hash, ct);
        if (token is null) return;
        token.Revogar();
        await _refreshRepo.SalvarAsync(ct);
    }

    private async Task<TokenResponse> GerarTokenResponseAsync(UsuarioEntity usuario, CancellationToken ct)
    {
        var accessToken = _jwt.GerarToken(usuario.Id, usuario.Email.Valor);

        var rawToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        var hash = HashToken(rawToken);
        var refreshToken = new RefreshTokenEntity(usuario.Id, hash, DateTime.UtcNow.AddDays(7));

        await _refreshRepo.AdicionarAsync(refreshToken, ct);
        await _refreshRepo.SalvarAsync(ct);

        return new TokenResponse(accessToken, rawToken, 900);
    }

    private static string HashToken(string raw)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
