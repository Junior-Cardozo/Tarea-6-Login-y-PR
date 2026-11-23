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

    // Tablas del módulo de Incidentes
    public DbSet<Incidente> Incidentes { get; set; }
    public DbSet<TipoIncidente> TiposIncidente { get; set; }
    public DbSet<PrioridadIncidente> PrioridadesIncidente { get; set; }
    public DbSet<EstadoIncidente> EstadosIncidente { get; set; }

    // Tablas del Modulo de alertas de Diego
    public DbSet<Alerta> Alertas { get; set; }
    public DbSet<TipoAlerta> TiposAlerta { get; set; }
    public DbSet<Notificacion> Notificaciones { get; set; }
    public DbSet<HistorialAlerta> HistorialAlertas { get; set; }

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

        // Relaciones del módulo de Incidentes
        modelBuilder.Entity<Incidente>()
            .HasOne(i => i.Servidor)
            .WithMany()
            .HasForeignKey(i => i.ServidorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Incidente>()
            .HasOne(i => i.TipoIncidente)
            .WithMany(t => t.Incidentes)
            .HasForeignKey(i => i.TipoIncidenteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Incidente>()
            .HasOne(i => i.PrioridadIncidente)
            .WithMany(p => p.Incidentes)
            .HasForeignKey(i => i.PrioridadIncidenteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Incidente>()
            .HasOne(i => i.EstadoIncidente)
            .WithMany(e => e.Incidentes)
            .HasForeignKey(i => i.EstadoIncidenteId)
            .OnDelete(DeleteBehavior.Restrict);

        // Modulo de alertas
        modelBuilder.Entity<Alerta>()
            .HasOne(a => a.Servidor)
            .WithMany()
            .HasForeignKey(a => a.ServidorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Alerta>()
            .HasOne(a => a.TipoAlerta)
            .WithMany(t => t.Alertas)
            .HasForeignKey(a => a.TipoAlertaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Notificacion>()
            .HasOne(n => n.Alerta)
            .WithMany(a => a.Notificaciones)
            .HasForeignKey(n => n.AlertaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HistorialAlerta>()
            .HasOne(h => h.Alerta)
            .WithMany(a => a.Historial)
            .HasForeignKey(h => h.AlertaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}