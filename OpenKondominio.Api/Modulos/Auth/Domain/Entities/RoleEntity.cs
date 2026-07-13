using OpenKondominio.Api.SharedKernel;

namespace OpenKondominio.Api.Modulos.Auth.Domain.Entities;

public class RoleEntity : Entity
{
    public string Nome { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;

    public ICollection<PermissionEntity> Permissions { get; private set; } = [];

    private RoleEntity() { }

    public RoleEntity(string nome, string descricao)
    {
        Nome = nome;
        Descricao = descricao;
    }

    public void AdicionarPermissao(PermissionEntity permission)
    {
        if (Permissions.Any(p => p.Id == permission.Id)) return;
        Permissions.Add(permission);
    }

    public void RemoverPermissao(Guid permissionId)
    {
        var perm = Permissions.FirstOrDefault(p => p.Id == permissionId);
        if (perm is not null) Permissions.Remove(perm);
    }
}
