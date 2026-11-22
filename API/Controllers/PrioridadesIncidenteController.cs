using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitorServidores.Data;
using MonitorServidores.Models;

namespace MonitorServidores.Controllers;

[ApiController]
[Route("api/prioridades-incidente")]
[Authorize]
public class PrioridadesIncidenteController : ControllerBase
{
    private readonly MonitorDbContext _context;

    public PrioridadesIncidenteController(MonitorDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetPrioridades()
    {
        var prioridades = await _context.PrioridadesIncidente
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .ToListAsync();

        return Ok(prioridades);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetPrioridad(int id)
    {
        var prioridad = await _context.PrioridadesIncidente
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (prioridad == null)
            return NotFound(new { mensaje = "Prioridad no encontrada." });

        return Ok(prioridad);
    }

    [HttpPost]
    public async Task<IActionResult> PostPrioridad(
        [FromBody] CrearPrioridadIncidenteDto datos)
    {
        if (string.IsNullOrWhiteSpace(datos.Nombre))
            return BadRequest(new { mensaje = "El nombre es obligatorio." });

        var prioridad = new PrioridadIncidente
        {
            Nombre = datos.Nombre.Trim(),
            Descripcion = datos.Descripcion
        };

        _context.PrioridadesIncidente.Add(prioridad);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetPrioridad),
            new { id = prioridad.Id },
            prioridad
        );
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutPrioridad(
        int id, [FromBody] CrearPrioridadIncidenteDto datos)
    {
        var prioridad = await _context.PrioridadesIncidente
            .FirstOrDefaultAsync(p => p.Id == id);

        if (prioridad == null)
            return NotFound(new { mensaje = "Prioridad no encontrada." });

        if (string.IsNullOrWhiteSpace(datos.Nombre))
            return BadRequest(new { mensaje = "El nombre es obligatorio." });

        prioridad.Nombre = datos.Nombre.Trim();
        prioridad.Descripcion = datos.Descripcion;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletePrioridad(int id)
    {
        var prioridad = await _context.PrioridadesIncidente
            .FirstOrDefaultAsync(p => p.Id == id);

        if (prioridad == null)
            return NotFound(new { mensaje = "Prioridad no encontrada." });

        var tieneIncidentes = await _context.Incidentes
            .AnyAsync(i => i.PrioridadIncidenteId == id && !i.Eliminado);

        if (tieneIncidentes)
            return Conflict(new { mensaje = "No se puede eliminar una prioridad que tiene incidentes activos." });

        _context.PrioridadesIncidente.Remove(prioridad);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class CrearPrioridadIncidenteDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
}
