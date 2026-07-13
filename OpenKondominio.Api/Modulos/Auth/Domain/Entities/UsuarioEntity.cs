using OpenKondominio.Api.Modulos.Auth.Domain.ValueObjects;
using OpenKondominio.Api.SharedKernel;

namespace OpenKondominio.Api.Modulos.Auth.Domain.Entities;

public class UsuarioEntity : Entity
{
    public Email Email { get; private set; } = null!;
    public string NomeCompleto { get; private set; } = string.Empty;
    public string SenhaHash { get; private set; } = string.Empty;
    public bool Ativo { get; private set; } = true;
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;

    public ICollection<RefreshTokenEntity> RefreshTokens { get; private set; } = [];
    public ICollection<UsuarioCondominioRoleEntity> CondominioRoles { get; private set; } = [];

    private UsuarioEntity() { }

    public UsuarioEntity(Email email, string nomeCompleto, string senhaHash)
    {
        Email = email;
        NomeCompleto = nomeCompleto;
        SenhaHash = senhaHash;
    }

    public void AlterarSenha(string novaSenhaHash) => SenhaHash = novaSenhaHash;
    public void Desativar() => Ativo = false;
}
