using NSubstitute;
using OpenKondominio.Api.Modulos.Auth.Application.Commands;
using OpenKondominio.Api.Modulos.Auth.Application.Services;
using OpenKondominio.Api.Modulos.Auth.Domain.Entities;
using OpenKondominio.Api.Modulos.Auth.Domain.Interfaces;
using OpenKondominio.Api.Modulos.Auth.Domain.ValueObjects;

namespace OpenKondominio.Tests.Auth.Application;

public class AuthServiceTests
{
    private readonly IUsuarioRepository _usuarioRepo = Substitute.For<IUsuarioRepository>();
    private readonly IRefreshTokenRepository _refreshRepo = Substitute.For<IRefreshTokenRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenService _jwt = Substitute.For<IJwtTokenService>();

    private AuthService CriarService() => new(_usuarioRepo, _refreshRepo, _hasher, _jwt);

    [Fact]
    public async Task Register_EmailJaCadastrado_LancaInvalidOperationException()
    {
        var usuario = new UsuarioEntity(new Email("user@test.com"), "Nome", "hash");
        _usuarioRepo.BuscarPorEmailAsync("user@test.com").Returns(usuario);

        var svc = CriarService();
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => svc.RegisterAsync(new RegisterCommand("user@test.com", "Nome", "senha")));
    }

    [Fact]
    public async Task Register_EmailNovo_CriaERetornaUsuarioDto()
    {
        _usuarioRepo.BuscarPorEmailAsync("novo@test.com").Returns((UsuarioEntity?)null);
        _hasher.Hash("senha123").Returns("hashBcrypt");

        var svc = CriarService();
        var result = await svc.RegisterAsync(new RegisterCommand("novo@test.com", "Novo Usuario", "senha123"));

        Assert.Equal("novo@test.com", result.Email);
        Assert.Equal("Novo Usuario", result.NomeCompleto);
        Assert.True(result.Ativo);
        await _usuarioRepo.Received(1).AdicionarAsync(Arg.Any<UsuarioEntity>());
        await _usuarioRepo.Received(1).SalvarAsync();
    }

    [Fact]
    public async Task Login_EmailNaoEncontrado_LancaUnauthorizedAccessException()
    {
        _usuarioRepo.BuscarPorEmailAsync("x@test.com").Returns((UsuarioEntity?)null);

        var svc = CriarService();
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => svc.LoginAsync(new LoginCommand("x@test.com", "qualquer")));
    }

    [Fact]
    public async Task Login_SenhaErrada_LancaUnauthorizedAccessException()
    {
        var usuario = new UsuarioEntity(new Email("user@test.com"), "Nome", "hashCorreto");
        _usuarioRepo.BuscarPorEmailAsync("user@test.com").Returns(usuario);
        _hasher.Verify("senhaErrada", "hashCorreto").Returns(false);

        var svc = CriarService();
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => svc.LoginAsync(new LoginCommand("user@test.com", "senhaErrada")));
    }

    [Fact]
    public async Task Login_Valido_RetornaTokenResponse()
    {
        var usuario = new UsuarioEntity(new Email("user@test.com"), "Nome", "hashCorreto");
        _usuarioRepo.BuscarPorEmailAsync("user@test.com").Returns(usuario);
        _hasher.Verify("senha", "hashCorreto").Returns(true);
        _jwt.GerarToken(usuario.Id, "user@test.com").Returns("jwt-token");

        var svc = CriarService();
        var result = await svc.LoginAsync(new LoginCommand("user@test.com", "senha"));

        Assert.Equal("jwt-token", result.AccessToken);
        Assert.NotEmpty(result.RefreshToken);
        Assert.Equal(900, result.ExpiresIn);
    }

    [Fact]
    public async Task Login_UsuarioInativo_LancaUnauthorizedAccessException()
    {
        var usuario = new UsuarioEntity(new Email("user@test.com"), "Nome", "hash");
        usuario.Desativar();
        _usuarioRepo.BuscarPorEmailAsync("user@test.com").Returns(usuario);
        _hasher.Verify("senha", "hash").Returns(true);

        var svc = CriarService();
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => svc.LoginAsync(new LoginCommand("user@test.com", "senha")));
    }

    [Fact]
    public async Task Refresh_TokenRevogado_LancaUnauthorizedAccessException()
    {
        var tokenHash = HashToken("tokenRaw");
        var tokenEntity = new RefreshTokenEntity(Guid.NewGuid(), tokenHash, DateTime.UtcNow.AddDays(7));
        tokenEntity.Revogar();
        _refreshRepo.BuscarPorHashAsync(tokenHash).Returns(tokenEntity);

        var svc = CriarService();
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => svc.RefreshAsync("tokenRaw"));
    }

    [Fact]
    public async Task Refresh_TokenNaoEncontrado_LancaUnauthorizedAccessException()
    {
        _refreshRepo.BuscarPorHashAsync(Arg.Any<string>()).Returns((RefreshTokenEntity?)null);

        var svc = CriarService();
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => svc.RefreshAsync("tokenInexistente"));
    }

    [Fact]
    public async Task Refresh_Valido_RevogaAntigoEEmiteNovo()
    {
        var usuarioId = Guid.NewGuid();
        var tokenHash = HashToken("tokenRaw");
        var tokenEntity = new RefreshTokenEntity(usuarioId, tokenHash, DateTime.UtcNow.AddDays(7));
        var usuario = new UsuarioEntity(new Email("user@test.com"), "Nome", "hash");

        _refreshRepo.BuscarPorHashAsync(tokenHash).Returns(tokenEntity);
        _usuarioRepo.BuscarPorIdAsync(usuarioId).Returns(usuario);
        _jwt.GerarToken(usuario.Id, "user@test.com").Returns("novo-jwt");

        var svc = CriarService();
        var result = await svc.RefreshAsync("tokenRaw");

        Assert.True(tokenEntity.Revogado);
        Assert.Equal("novo-jwt", result.AccessToken);
    }

    [Fact]
    public async Task Logout_TokenValido_RevogaToken()
    {
        var tokenHash = HashToken("tokenRaw");
        var tokenEntity = new RefreshTokenEntity(Guid.NewGuid(), tokenHash, DateTime.UtcNow.AddDays(7));
        _refreshRepo.BuscarPorHashAsync(tokenHash).Returns(tokenEntity);

        var svc = CriarService();
        await svc.LogoutAsync("tokenRaw");

        Assert.True(tokenEntity.Revogado);
        await _refreshRepo.Received(1).SalvarAsync();
    }

    private static string HashToken(string raw)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
