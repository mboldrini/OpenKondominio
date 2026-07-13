using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenKondominio.Api.Modulos.Auth.Domain.Entities;

namespace OpenKondominio.Api.Modulos.Auth.Infrastructure.Persistence.Configurations;

public class UsuarioCondominioRoleConfiguration : IEntityTypeConfiguration<UsuarioCondominioRoleEntity>
{
    public void Configure(EntityTypeBuilder<UsuarioCondominioRoleEntity> builder)
    {
        builder.ToTable("usuario_condominio_roles", "auth");
        builder.HasKey(ucr => new { ucr.UsuarioId, ucr.CondominioId, ucr.RoleId });
        builder.Property(ucr => ucr.AtivoDesde).IsRequired();

        builder.HasOne<RoleEntity>()
            .WithMany()
            .HasForeignKey(ucr => ucr.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
