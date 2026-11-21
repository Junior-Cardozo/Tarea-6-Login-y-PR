using Microsoft.EntityFrameworkCore;
using MonitorServidores.Models;

namespace MonitorServidores.Data;

public class MonitorDbContext : DbContext
{
    public MonitorDbContext(DbContextOptions<MonitorDbContext> options)
        : base(options)
    {
    }

    // Tablas del módulo de Yasmin
    public DbSet<Servidor> Servidores { get; set; }
    public DbSet<TipoServidor> TiposServidor { get; set; }
    public DbSet<SistemaOperativo> SistemasOperativos { get; set; } 
    public DbSet<Monitoreo> Monitoreos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // TipoServidor (1) -> (N) Servidores
        modelBuilder.Entity<Servidor>()
            .HasOne(s => s.TipoServidor)
            .WithMany(t => t.Servidores)
            .HasForeignKey(s => s.TipoServidorId)
            .OnDelete(DeleteBehavior.Restrict);

        // SistemaOperativo (1) -> (N) Servidores
        modelBuilder.Entity<Servidor>()
            .HasOne(s => s.SistemaOperativo)
            .WithMany(so => so.Servidores)
            .HasForeignKey(s => s.SistemaOperativoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Servidor (1) -> (N) Monitoreos
        modelBuilder.Entity<Monitoreo>()
            .HasOne(m => m.Servidor)
            .WithMany(s => s.Monitoreos)
            .HasForeignKey(m => m.ServidorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}