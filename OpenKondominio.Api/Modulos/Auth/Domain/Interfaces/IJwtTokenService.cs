namespace OpenKondominio.Api.Modulos.Auth.Domain.Interfaces;

public interface IJwtTokenService
{
    string GerarToken(Guid userId, string email);
}
