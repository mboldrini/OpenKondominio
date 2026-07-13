using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenKondominio.Api.Modulos.Auth.Domain.Entities;

namespace OpenKondominio.Api.Modulos.Auth.Infrastructure.Persistence.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<UsuarioEntity>
{
    public void Configure(EntityTypeBuilder<UsuarioEntity> builder)
    {
        builder.ToTable("usuarios", "auth");
        builder.HasKey(u => u.Id);

        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Valor)
                .HasColumnName("email")
                .HasMaxLength(254)
                .IsRequired();
            email.HasIndex(e => e.Valor).IsUnique();
        });

        builder.Property(u => u.NomeCompleto).HasMaxLength(200).IsRequired();
        builder.Property(u => u.SenhaHash).HasMaxLength(100).IsRequired();
        builder.Property(u => u.Ativo).IsRequired();
        builder.Property(u => u.CriadoEm).IsRequired();

        builder.HasMany(u => u.RefreshTokens)
            .WithOne()
            .HasForeignKey(rt => rt.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.CondominioRoles)
            .WithOne()
            .HasForeignKey(cr => cr.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
