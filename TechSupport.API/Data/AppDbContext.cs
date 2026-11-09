using Microsoft.EntityFrameworkCore;
using TechSupport.API.Models;

namespace TechSupport.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<Incidente> Incidentes => Set<Incidente>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Usuario>()
            .HasIndex(u => u.NombreUsuario)
            .IsUnique();

        builder.Entity<Incidente>()
            .HasOne(i => i.ReportadoPor)
            .WithMany()
            .HasForeignKey(i => i.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Incidente>()
            .HasOne(i => i.Tecnico)
            .WithMany()
            .HasForeignKey(i => i.TecnicoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
