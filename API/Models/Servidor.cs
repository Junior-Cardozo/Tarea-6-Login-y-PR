using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MonitorServidores.Models;

[Table("servidores")]
public class Servidor
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string DireccionIp { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Descripcion { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    
    // Relación con el tipo de servidor
     public int? TipoServidorId { get; set; }
    public TipoServidor? TipoServidor { get; set; }

// Relación con el sistema operativo
    public int? SistemaOperativoId { get; set; }
     public SistemaOperativo? SistemaOperativo { get; set; }

// Un servidor puede tener muchos monitoreos
     public ICollection<Monitoreo> Monitoreos { get; set; }
    = new List<Monitoreo>();
}