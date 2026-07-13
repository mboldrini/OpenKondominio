using OpenKondominio.Api.SharedKernel;

namespace OpenKondominio.Api.Modulos.Login.Domain.Entities;

public class LoginEntity : Entity
{
    public string Nome { get; private set; } = string.Empty;
}
