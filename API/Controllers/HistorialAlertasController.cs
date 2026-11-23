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
}

public class CrearHistorialAlertaDto
{
    public string Accion { get; set; } = string.Empty;

    public string? Detalle { get; set; }

    public int AlertaId { get; set; }
}
