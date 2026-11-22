using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitorServidores.Data;
using MonitorServidores.Models;

namespace MonitorServidores.Controllers;

[ApiController]
[Route("api/tipos-incidente")]
[Authorize]
public class TiposIncidenteController : ControllerBase
{
    private readonly MonitorDbContext _context;

    public TiposIncidenteController(MonitorDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetTiposIncidente()
    {
        var tipos = await _context.TiposIncidente
            .AsNoTracking()
            .Where(t => !t.Eliminado)
            .OrderBy(t => t.Id)
            .ToListAsync();

        return Ok(tipos);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetTipoIncidente(int id)
    {
        var tipo = await _context.TiposIncidente
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id && !t.Eliminado);

        if (tipo == null)
            return NotFound(new { mensaje = "Tipo de incidente no encontrado." });

        return Ok(tipo);
    }

    [HttpPost]
    public async Task<IActionResult> PostTipoIncidente(
        [FromBody] CrearTipoIncidenteDto datos)
    {
        if (string.IsNullOrWhiteSpace(datos.Nombre))
            return BadRequest(new { mensaje = "El nombre es obligatorio." });

        var tipo = new TipoIncidente
        {
            Nombre = datos.Nombre.Trim(),
            Descripcion = datos.Descripcion,
            Eliminado = false
        };

        _context.TiposIncidente.Add(tipo);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTipoIncidente), new { id = tipo.Id }, tipo);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutTipoIncidente(
        int id, [FromBody] CrearTipoIncidenteDto datos)
    {
        var tipo = await _context.TiposIncidente
            .FirstOrDefaultAsync(t => t.Id == id && !t.Eliminado);

        if (tipo == null)
            return NotFound(new { mensaje = "Tipo de incidente no encontrado o eliminado." });

        if (string.IsNullOrWhiteSpace(datos.Nombre))
            return BadRequest(new { mensaje = "El nombre es obligatorio." });

        tipo.Nombre = datos.Nombre.Trim();
        tipo.Descripcion = datos.Descripcion;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTipoIncidente(int id)
    {
        var tipo = await _context.TiposIncidente
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tipo == null)
            return NotFound(new { mensaje = "Tipo de incidente no encontrado." });

        var tieneIncidentes = await _context.Incidentes
            .AnyAsync(i => i.TipoIncidenteId == id && !i.Eliminado);

        if (tieneIncidentes)
            return Conflict(new { mensaje = "No se puede eliminar un tipo que tiene incidentes activos." });

        _context.TiposIncidente.Remove(tipo);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class CrearTipoIncidenteDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
}
