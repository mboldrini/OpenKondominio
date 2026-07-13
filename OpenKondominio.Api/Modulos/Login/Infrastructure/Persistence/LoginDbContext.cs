using Microsoft.EntityFrameworkCore;
using OpenKondominio.Api.Modulos.Login.Domain.Entities;

namespace OpenKondominio.Api.Modulos.Login.Infrastructure.Persistence;

public class LoginDbContext : DbContext
{
    public LoginDbContext(DbContextOptions<LoginDbContext> options)
        : base(options) { }

    public DbSet<LoginEntity> Logins => Set<LoginEntity>();
}
