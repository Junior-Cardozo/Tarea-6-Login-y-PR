namespace MonitorServidores.Models;

public class TipoIncidente
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool Eliminado { get; set; } = false;

    public ICollection<Incidente> Incidentes { get; set; } = new List<Incidente>();
}