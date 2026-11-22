using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitorServidores.Data;
using MonitorServidores.Models;

namespace MonitorServidores.Controllers;

[ApiController]
[Route("api/estados-incidente")]
[Authorize]
public class EstadosIncidenteController : ControllerBase
{
    private readonly MonitorDbContext _context;

    public EstadosIncidenteController(MonitorDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetEstados()
    {
        var estados = await _context.EstadosIncidente
            .AsNoTracking()
            .Where(e => !e.Eliminado)
            .OrderBy(e => e.Id)
            .ToListAsync();

        return Ok(estados);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetEstado(int id)
    {
        var estado = await _context.EstadosIncidente
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id && !e.Eliminado);

        if (estado == null)
            return NotFound(new { mensaje = "Estado no encontrado." });

        return Ok(estado);
    }

    [HttpPost]
    public async Task<IActionResult> PostEstado(
        [FromBody] CrearEstadoIncidenteDto datos)
    {
        if (string.IsNullOrWhiteSpace(datos.Nombre))
            return BadRequest(new { mensaje = "El nombre es obligatorio." });

        var estado = new EstadoIncidente
        {
            Nombre = datos.Nombre.Trim(),
            Descripcion = datos.Descripcion,
            Eliminado = false
        };

        _context.EstadosIncidente.Add(estado);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetEstado), new { id = estado.Id }, estado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutEstado(
        int id, [FromBody] CrearEstadoIncidenteDto datos)
    {
        var estado = await _context.EstadosIncidente
            .FirstOrDefaultAsync(e => e.Id == id && !e.Eliminado);

        if (estado == null)
            return NotFound(new { mensaje = "Estado no encontrado o eliminado." });

        if (string.IsNullOrWhiteSpace(datos.Nombre))
            return BadRequest(new { mensaje = "El nombre es obligatorio." });

        estado.Nombre = datos.Nombre.Trim();
        estado.Descripcion = datos.Descripcion;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteEstado(int id)
    {
        var estado = await _context.EstadosIncidente
            .FirstOrDefaultAsync(e => e.Id == id);

        if (estado == null)
            return NotFound(new { mensaje = "Estado no encontrado." });

        var tieneIncidentes = await _context.Incidentes
            .AnyAsync(i => i.EstadoIncidenteId == id && !i.Eliminado);

        if (tieneIncidentes)
            return Conflict(new { mensaje = "No se puede eliminar un estado que tiene incidentes activos." });

        _context.EstadosIncidente.Remove(estado);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class CrearEstadoIncidenteDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
}
