using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenKondominio.Api.Modulos.Auth.Domain.Entities;

namespace OpenKondominio.Api.Modulos.Auth.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<RoleEntity>
{
    public void Configure(EntityTypeBuilder<RoleEntity> builder)
    {
        builder.ToTable("roles", "auth");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Nome).HasMaxLength(100).IsRequired();
        builder.HasIndex(r => r.Nome).IsUnique();
        builder.Property(r => r.Descricao).HasMaxLength(500);

        builder.HasMany(r => r.Permissions)
            .WithMany()
            .UsingEntity(j => j.ToTable("role_permissions", "auth"));
    }
}
