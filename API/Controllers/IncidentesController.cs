using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitorServidores.Data;
using MonitorServidores.Models;

namespace MonitorServidores.Controllers;

[ApiController]
[Route("api/incidentes")]
[Authorize]
public class IncidentesController : ControllerBase
{
    private readonly MonitorDbContext _context;

    public IncidentesController(MonitorDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetIncidentes()
    {
        var incidentes = await _context.Incidentes
            .AsNoTracking()
            .Where(i => !i.Eliminado)
            .OrderBy(i => i.Id)
            .Select(i => new
            {
                i.Id,
                i.Titulo,
                i.Descripcion,
                i.FechaRegistro,
                i.ServidorId,
                i.TipoIncidenteId,
                i.PrioridadIncidenteId,
                i.EstadoIncidenteId
            })
            .ToListAsync();

        return Ok(incidentes);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetIncidente(int id)
    {
        var incidente = await _context.Incidentes
            .AsNoTracking()
            .Where(i => i.Id == id && !i.Eliminado)
            .Select(i => new
            {
                i.Id,
                i.Titulo,
                i.Descripcion,
                i.FechaRegistro,
                i.ServidorId,
                i.TipoIncidenteId,
                i.PrioridadIncidenteId,
                i.EstadoIncidenteId
            })
            .FirstOrDefaultAsync();

        if (incidente == null)
        {
            return NotFound(new
            {
                mensaje = "Incidente no encontrado."
            });
        }

        return Ok(incidente);
    }

    [HttpPost]
    public async Task<IActionResult> PostIncidente(
        [FromBody] CrearIncidenteDto datos)
    {
        if (string.IsNullOrWhiteSpace(datos.Titulo))
        {
            return BadRequest(new
            {
                mensaje = "El título del incidente es obligatorio."
            });
        }

        if (!await _context.Servidores.AnyAsync(s =>
            s.Id == datos.ServidorId && s.Activo))
        {
            return BadRequest(new
            {
                mensaje = "El servidor no existe o está inactivo."
            });
        }

        if (!await _context.TiposIncidente.AnyAsync(t =>
            t.Id == datos.TipoIncidenteId))
        {
            return BadRequest(new
            {
                mensaje = "El tipo de incidente no es válido."
            });
        }

        if (!await _context.PrioridadesIncidente.AnyAsync(p =>
            p.Id == datos.PrioridadIncidenteId))
        {
            return BadRequest(new
            {
                mensaje = "La prioridad del incidente no es válida."
            });
        }

        if (!await _context.EstadosIncidente.AnyAsync(e =>
            e.Id == datos.EstadoIncidenteId))
        {
            return BadRequest(new
            {
                mensaje = "El estado del incidente no es válido."
            });
        }

        var incidente = new Incidente
        {
            Titulo = datos.Titulo.Trim(),
            Descripcion = datos.Descripcion,
            FechaRegistro = DateTime.UtcNow,
            ServidorId = datos.ServidorId,
            TipoIncidenteId = datos.TipoIncidenteId,
            PrioridadIncidenteId = datos.PrioridadIncidenteId,
            EstadoIncidenteId = datos.EstadoIncidenteId,
            Eliminado = false
        };

        _context.Incidentes.Add(incidente);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetIncidente),
            new { id = incidente.Id },
            incidente
        );
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutIncidente(
        int id, [FromBody] CrearIncidenteDto datos)
    {
        var incidente = await _context.Incidentes
            .FirstOrDefaultAsync(i => i.Id == id && !i.Eliminado);

        if (incidente == null)
        {
            return NotFound(new
            {
                mensaje = "Incidente no encontrado."
            });
        }

        if (string.IsNullOrWhiteSpace(datos.Titulo))
        {
            return BadRequest(new
            {
                mensaje = "El título del incidente es obligatorio."
            });
        }

        if (!await _context.Servidores.AnyAsync(s =>
            s.Id == datos.ServidorId && s.Activo))
        {
            return BadRequest(new
            {
                mensaje = "El servidor no existe o está inactivo."
            });
        }

        if (!await _context.TiposIncidente.AnyAsync(t =>
            t.Id == datos.TipoIncidenteId))
        {
            return BadRequest(new
            {
                mensaje = "El tipo de incidente no es válido."
            });
        }

        if (!await _context.PrioridadesIncidente.AnyAsync(p =>
            p.Id == datos.PrioridadIncidenteId))
        {
            return BadRequest(new
            {
                mensaje = "La prioridad del incidente no es válida."
            });
        }

        if (!await _context.EstadosIncidente.AnyAsync(e =>
            e.Id == datos.EstadoIncidenteId))
        {
            return BadRequest(new
            {
                mensaje = "El estado del incidente no es válido."
            });
        }

        incidente.Titulo = datos.Titulo.Trim();
        incidente.Descripcion = datos.Descripcion;
        incidente.ServidorId = datos.ServidorId;
        incidente.TipoIncidenteId = datos.TipoIncidenteId;
        incidente.PrioridadIncidenteId = datos.PrioridadIncidenteId;
        incidente.EstadoIncidenteId = datos.EstadoIncidenteId;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteIncidente(int id)
    {
        var incidente = await _context.Incidentes
            .FirstOrDefaultAsync(i => i.Id == id && !i.Eliminado);

        if (incidente == null)
        {
            return NotFound(new
            {
                mensaje = "Incidente no encontrado."
            });
        }

        incidente.Eliminado = true;

        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class CrearIncidenteDto
{
    public string Titulo { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public int ServidorId { get; set; }

    public int TipoIncidenteId { get; set; }

    public int PrioridadIncidenteId { get; set; }

    public int EstadoIncidenteId { get; set; }
}
