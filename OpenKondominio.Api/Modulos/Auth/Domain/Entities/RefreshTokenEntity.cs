using OpenKondominio.Api.SharedKernel;

namespace OpenKondominio.Api.Modulos.Auth.Domain.Entities;

public class RefreshTokenEntity : Entity
{
    public Guid UsuarioId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiraEm { get; private set; }
    public bool Revogado { get; private set; }
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;

    private RefreshTokenEntity() { }

    public RefreshTokenEntity(Guid usuarioId, string tokenHash, DateTime expiraEm)
    {
        UsuarioId = usuarioId;
        Token = tokenHash;
        ExpiraEm = expiraEm;
    }

    public void Revogar() => Revogado = true;
    public bool EstaValido() => !Revogado && ExpiraEm > DateTime.UtcNow;
}
