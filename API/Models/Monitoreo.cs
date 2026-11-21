using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MonitorServidores.Models;

[Table("monitoreos")]
public class Monitoreo
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ServidorId { get; set; }

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public bool Disponible { get; set; }

    [Range(0, int.MaxValue)]
    public int? TiempoRespuestaMs { get; set; }

    [MaxLength(300)]
    public string? Observaciones { get; set; }

    public bool Activo { get; set; } = true;

    [ForeignKey(nameof(ServidorId))]
    public Servidor Servidor { get; set; } = null!;
}