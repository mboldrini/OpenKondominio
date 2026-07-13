using OpenKondominio.Api.SharedKernel;

namespace OpenKondominio.Api.Modulos.Auth.Domain.Entities;

public class PermissionEntity : Entity
{
    public string Chave { get; private set; } = string.Empty;

    private PermissionEntity() { }

    public PermissionEntity(string chave)
    {
        if (string.IsNullOrWhiteSpace(chave))
            throw new ArgumentException("Chave de permissão não pode ser vazia.", nameof(chave));
        Chave = chave;
    }
}
