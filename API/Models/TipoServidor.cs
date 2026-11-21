using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MonitorServidores.Models;

[Table("tipos_servidor")]
public class TipoServidor
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Descripcion { get; set; }

    public bool Activo { get; set; } = true;
    // Relación: un tipo puede tener varios servidores
     public ICollection<Servidor> Servidores { get; set; }
    = new List<Servidor>();
}