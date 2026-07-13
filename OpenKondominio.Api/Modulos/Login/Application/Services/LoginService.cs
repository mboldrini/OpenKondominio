using OpenKondominio.Api.Modulos.Login.Domain.Entities;

namespace OpenKondominio.Api.Modulos.Login.Application.Services;

public class LoginService
{
    public LoginEntity Criar(string nome)
        => new LoginEntity();
}
