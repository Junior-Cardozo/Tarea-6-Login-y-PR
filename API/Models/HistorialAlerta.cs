using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MonitorServidores.Models;

[Table("historial_alertas")]
public class HistorialAlerta
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Accion { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Detalle { get; set; }

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public bool Activo { get; set; } = true;

    public int AlertaId { get; set; }
    public Alerta Alerta { get; set; } = null!;
}