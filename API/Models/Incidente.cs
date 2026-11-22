namespace MonitorServidores.Models;

public class Incidente
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    public bool Eliminado { get; set; }

    public int ServidorId { get; set; }
    public Servidor Servidor { get; set; } = null!;

    public int TipoIncidenteId { get; set; }
    public TipoIncidente TipoIncidente { get; set; } = null!;

    public int PrioridadIncidenteId { get; set; }
    public PrioridadIncidente PrioridadIncidente { get; set; } = null!;

    public int EstadoIncidenteId { get; set; }
    public EstadoIncidente EstadoIncidente { get; set; } = null!;
}