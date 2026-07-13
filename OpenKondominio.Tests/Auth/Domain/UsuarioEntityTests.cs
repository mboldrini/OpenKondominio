using OpenKondominio.Api.Modulos.Auth.Domain.Entities;
using OpenKondominio.Api.Modulos.Auth.Domain.ValueObjects;

namespace OpenKondominio.Tests.Auth.Domain;

public class UsuarioEntityTests
{
    private static UsuarioEntity CriarUsuario() =>
        new(new Email("user@test.com"), "Nome Teste", "hashSenha123");

    [Fact]
    public void AlterarSenha_AtualizaHash()
    {
        var usuario = CriarUsuario();
        usuario.AlterarSenha("novoHash456");
        Assert.Equal("novoHash456", usuario.SenhaHash);
    }

    [Fact]
    public void Desativar_SetaAtivoFalse()
    {
        var usuario = CriarUsuario();
        Assert.True(usuario.Ativo);
        usuario.Desativar();
        Assert.False(usuario.Ativo);
    }

    [Fact]
    public void NovoCriado_AtivoTrue()
    {
        var usuario = CriarUsuario();
        Assert.True(usuario.Ativo);
    }

    [Fact]
    public void RefreshToken_EstaValido_RetornaFalseQuandoRevogado()
    {
        var token = new RefreshTokenEntity(Guid.NewGuid(), "hash", DateTime.UtcNow.AddDays(7));
        Assert.True(token.EstaValido());
        token.Revogar();
        Assert.False(token.EstaValido());
    }

    [Fact]
    public void RefreshToken_EstaValido_RetornaFalseQuandoExpirado()
    {
        var token = new RefreshTokenEntity(Guid.NewGuid(), "hash", DateTime.UtcNow.AddMinutes(-1));
        Assert.False(token.EstaValido());
    }

    [Fact]
    public void PermissionEntity_ChaveVazia_LancaExcecao()
    {
        Assert.Throws<ArgumentException>(() => new PermissionEntity(""));
        Assert.Throws<ArgumentException>(() => new PermissionEntity("   "));
    }

    [Fact]
    public void Role_AdicionarPermissao_Idempotente()
    {
        var role = new RoleEntity("Admin", "Administrador");
        var perm = new PermissionEntity("financeiro:ler");
        role.AdicionarPermissao(perm);
        role.AdicionarPermissao(perm);
        Assert.Single(role.Permissions);
    }

    [Fact]
    public void Role_RemoverPermissao_RemovePorId()
    {
        var role = new RoleEntity("Admin", "Administrador");
        var perm = new PermissionEntity("financeiro:ler");
        role.AdicionarPermissao(perm);
        role.RemoverPermissao(perm.Id);
        Assert.Empty(role.Permissions);
    }
}
