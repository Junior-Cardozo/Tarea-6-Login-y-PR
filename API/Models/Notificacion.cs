using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MonitorServidores.Models;

[Table("notificaciones")]
public class Notificacion
{
    public int Id { get; set; }

    [Required]
    [MaxLength(500)]
    public string Mensaje { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Canal { get; set; } = string.Empty;

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public bool Enviada { get; set; } = false;

    public bool Activo { get; set; } = true;

    public int AlertaId { get; set; }
    public Alerta Alerta { get; set; } = null!;
}