using Microsoft.EntityFrameworkCore;
using OpenKondominio.Api.Modulos.Teste.Domain.Entities;

namespace OpenKondominio.Api.Modulos.Teste.Infrastructure.Persistence;

public class TesteDbContext : DbContext
{
    public TesteDbContext(DbContextOptions<TesteDbContext> options)
        : base(options) { }

    public DbSet<TesteEntity> Testes => Set<TesteEntity>();
}
