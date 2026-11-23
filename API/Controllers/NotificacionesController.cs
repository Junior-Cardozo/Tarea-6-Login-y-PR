using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitorServidores.Data;
using MonitorServidores.Models;

namespace MonitorServidores.Controllers;

[ApiController]
[Route("api/notificaciones")]
[Authorize]
public class NotificacionesController : ControllerBase
{
    private readonly MonitorDbContext _context;

    public NotificacionesController(MonitorDbContext context)
    {
        _context = context;
    }

    // GET: api/notificaciones
    [HttpGet]
    public async Task<IActionResult> GetNotificaciones()
    {
        var notificaciones = await _context.Notificaciones
            .AsNoTracking()
            .Where(n => n.Activo)
            .OrderBy(n => n.Id)
            .Select(n => new
            {
                n.Id,
                n.Mensaje,
                n.Canal,
                n.FechaRegistro,
                n.Enviada,
                n.AlertaId,
                Alerta = n.Alerta.Titulo,
                n.Activo
            })
            .ToListAsync();

        return Ok(notificaciones);
    }

    // GET: api/notificaciones/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetNotificacion(int id)
    {
        var notificacion = await _context.Notificaciones
            .AsNoTracking()
            .Where(n => n.Id == id && n.Activo)
            .Select(n => new
            {
                n.Id,
                n.Mensaje,
                n.Canal,
                n.FechaRegistro,
                n.Enviada,
                n.AlertaId,
                Alerta = n.Alerta.Titulo,
                n.Activo
            })
            .FirstOrDefaultAsync();

        if (notificacion == null)
        {
            return NotFound(new
            {
                mensaje = "Notificación no encontrada."
            });
        }

        return Ok(notificacion);
    }

    // POST: api/notificaciones
    [HttpPost]
    public async Task<IActionResult> PostNotificacion(
        [FromBody] CrearNotificacionDto datos)
    {
        if (string.IsNullOrWhiteSpace(datos.Mensaje))
        {
            return BadRequest(new
            {
                mensaje = "El mensaje es obligatorio."
            });
        }

        if (string.IsNullOrWhiteSpace(datos.Canal))
        {
            return BadRequest(new
            {
                mensaje = "El canal de notificación es obligatorio."
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

        var notificacion = new Notificacion
        {
            Mensaje = datos.Mensaje.Trim(),
            Canal = datos.Canal.Trim(),
            FechaRegistro = DateTime.UtcNow,
            Enviada = false,
            AlertaId = datos.AlertaId,
            Activo = true
        };

        _context.Notificaciones.Add(notificacion);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetNotificacion),
            new { id = notificacion.Id },
            notificacion
        );
    }
}

public class CrearNotificacionDto
{
    public string Mensaje { get; set; } = string.Empty;

    public string Canal { get; set; } = string.Empty;

    public int AlertaId { get; set; }
}
