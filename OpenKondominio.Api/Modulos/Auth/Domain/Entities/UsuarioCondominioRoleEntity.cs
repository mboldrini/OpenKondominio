namespace OpenKondominio.Api.Modulos.Auth.Domain.Entities;

public class UsuarioCondominioRoleEntity
{
    public Guid UsuarioId { get; private set; }
    public Guid CondominioId { get; private set; }
    public Guid RoleId { get; private set; }
    public DateTime AtivoDesde { get; private set; } = DateTime.UtcNow;

    private UsuarioCondominioRoleEntity() { }

    public UsuarioCondominioRoleEntity(Guid usuarioId, Guid condominioId, Guid roleId)
    {
        UsuarioId = usuarioId;
        CondominioId = condominioId;
        RoleId = roleId;
    }
}
