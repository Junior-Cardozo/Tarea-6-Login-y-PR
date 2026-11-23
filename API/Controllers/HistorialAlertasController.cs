using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitorServidores.Data;
using MonitorServidores.Models;

namespace MonitorServidores.Controllers;

[ApiController]
[Route("api/historial-alertas")]
[Authorize]
public class HistorialAlertasController : ControllerBase
{
    private readonly MonitorDbContext _context;

    public HistorialAlertasController(MonitorDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetHistorialAlertas()
    {
        var historial = await _context.HistorialAlertas
            .AsNoTracking()
            .Where(h => h.Activo)
            .OrderBy(h => h.Id)
            .Select(h => new
            {
                h.Id,
                h.Accion,
                h.Detalle,
                h.FechaRegistro,
                h.AlertaId,
                Alerta = h.Alerta.Titulo,
                h.Activo
            })
            .ToListAsync();

        return Ok(historial);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetHistorialAlerta(int id)
    {
        var historial = await _context.HistorialAlertas
            .AsNoTracking()
            .Where(h => h.Id == id && h.Activo)
            .Select(h => new
            {
                h.Id,
                h.Accion,
                h.Detalle,
                h.FechaRegistro,
                h.AlertaId,
                Alerta = h.Alerta.Titulo,
                h.Activo
            })
            .FirstOrDefaultAsync();

        if (historial == null)
        {
            return NotFound(new
            {
                mensaje = "Registro del historial no encontrado."
            });
        }

        return Ok(historial);
    }

    [HttpPost]
    public async Task<IActionResult> PostHistorialAlerta(
        [FromBody] CrearHistorialAlertaDto datos)
    {
        if (string.IsNullOrWhiteSpace(datos.Accion))
        {
            return BadRequest(new
            {
                mensaje = "La acción es obligatoria."
            });
        }

        if (!await _context.Alertas.AnyAsync(a =>
            a.Id == datos.AlertaId && a.Activo))
        {
            return BadRequest(new
            {
                mensaje = "La alerta no existe o está inactiva."
            });
        }

        var historial = new HistorialAlerta
        {
            Accion = datos.Accion.Trim(),
            Detalle = datos.Detalle,
            FechaRegistro = DateTime.UtcNow,
            AlertaId = datos.AlertaId,
            Activo = true
        };

        _context.HistorialAlertas.Add(historial);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetHistorialAlerta),
            new { id = historial.Id },
            historial
        );
    }

// PUT: api/historial-alertas/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutHistorialAlerta(
    int id,
    [FromBody] ActualizarHistorialAlertaDto datos)
    {
    if (string.IsNullOrWhiteSpace(datos.Accion))
    {
        return BadRequest("La acción es obligatoria.");
    }

    if (datos.Accion.Trim().Length > 100)
    {
        return BadRequest("La acción no puede superar los 100 caracteres.");
    }

    if (datos.Detalle?.Length > 500)
    {
        return BadRequest("El detalle no puede superar los 500 caracteres.");
    }

    var historial = await _context.HistorialAlertas
        .FirstOrDefaultAsync(h => h.Id == id && h.Activo);

    if (historial == null)
    {
        return NotFound("Registro de historial no encontrado o inactivo.");
    }

    var alertaExiste = await _context.Alertas
        .AnyAsync(a => a.Id == datos.AlertaId && a.Activo);

    if (!alertaExiste)
    {
        return BadRequest("La alerta no existe o está inactiva.");
    }

    historial.Accion = datos.Accion.Trim();
    historial.Detalle = string.IsNullOrWhiteSpace(datos.Detalle)
        ? null
        : datos.Detalle.Trim();
    historial.AlertaId = datos.AlertaId;

    await _context.SaveChangesAsync();

    return Ok(new
        {
        mensaje = "Registro de historial actualizado correctamente.",
        historial.Id,
        historial.Accion,
        historial.Detalle,
        historial.AlertaId
        });
    }

// DELETE lógico: api/historial-alertas/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteHistorialAlerta(int id)
    {
    var historial = await _context.HistorialAlertas
        .FirstOrDefaultAsync(h => h.Id == id && h.Activo);

    if (historial == null)
    {
        return NotFound("Registro de historial no encontrado o ya está inactivo.");
    }

    historial.Activo = false;

    await _context.SaveChangesAsync();

    return Ok(new
        {
        mensaje = "Registro de historial desactivado correctamente.",
        historial.Id,
        historial.Accion,
        historial.Activo
        });
    }
}
public class ActualizarHistorialAlertaDto
{
    public string Accion { get; set; } = string.Empty;
    public string? Detalle { get; set; }
    public int AlertaId { get; set; }
}

public class CrearHistorialAlertaDto
{
    public string Accion { get; set; } = string.Empty;

    public string? Detalle { get; set; }

    public int AlertaId { get; set; }
}
