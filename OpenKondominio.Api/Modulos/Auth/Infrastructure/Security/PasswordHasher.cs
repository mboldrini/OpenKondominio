using OpenKondominio.Api.Modulos.Auth.Domain.Interfaces;

namespace OpenKondominio.Api.Modulos.Auth.Infrastructure.Security;

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

    public bool Verify(string password, string hash) =>
        BCrypt.Net.BCrypt.Verify(password, hash);
}
