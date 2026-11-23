using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MonitorServidores.Models;

[Table("alertas")]
public class Alerta
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Titulo { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Descripcion { get; set; }

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public bool Activo { get; set; } = true;

    public int ServidorId { get; set; }
    public Servidor Servidor { get; set; } = null!;

    public int TipoAlertaId { get; set; }
    public TipoAlerta TipoAlerta { get; set; } = null!;

    public ICollection<Notificacion> Notificaciones { get; set; }
        = new List<Notificacion>();

    public ICollection<HistorialAlerta> Historial { get; set; }
        = new List<HistorialAlerta>();
}