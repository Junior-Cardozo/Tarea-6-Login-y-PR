using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MonitorServidores.Models;

[Table("sistemas_operativos")]
public class SistemaOperativo
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Version { get; set; }

    [MaxLength(300)]
    public string? Descripcion { get; set; }

    public bool Activo { get; set; } = true;

    // Relación: un sistema operativo puede tener varios servidores
     public ICollection<Servidor> Servidores { get; set; }
    = new List<Servidor>();
}