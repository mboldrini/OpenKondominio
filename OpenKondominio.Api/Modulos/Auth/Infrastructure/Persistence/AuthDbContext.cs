using Microsoft.EntityFrameworkCore;
using OpenKondominio.Api.Modulos.Auth.Domain.Entities;

namespace OpenKondominio.Api.Modulos.Auth.Infrastructure.Persistence;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    public DbSet<UsuarioEntity> Usuarios => Set<UsuarioEntity>();
    public DbSet<RoleEntity> Roles => Set<RoleEntity>();
    public DbSet<PermissionEntity> Permissions => Set<PermissionEntity>();
    public DbSet<UsuarioCondominioRoleEntity> UsuarioCondominioRoles => Set<UsuarioCondominioRoleEntity>();
    public DbSet<RefreshTokenEntity> RefreshTokens => Set<RefreshTokenEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("auth");
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AuthDbContext).Assembly,
            t => t.Namespace?.Contains("Auth.Infrastructure.Persistence.Configurations") == true);
    }
}
