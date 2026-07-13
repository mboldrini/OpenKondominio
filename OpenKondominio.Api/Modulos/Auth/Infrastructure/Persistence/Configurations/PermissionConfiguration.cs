using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenKondominio.Api.Modulos.Auth.Domain.Entities;

namespace OpenKondominio.Api.Modulos.Auth.Infrastructure.Persistence.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<PermissionEntity>
{
    public void Configure(EntityTypeBuilder<PermissionEntity> builder)
    {
        builder.ToTable("permissions", "auth");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Chave).HasMaxLength(100).IsRequired();
        builder.HasIndex(p => p.Chave).IsUnique();
    }
}
