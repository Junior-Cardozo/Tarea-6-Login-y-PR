using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitorServidores.Data;
using MonitorServidores.Models;

namespace MonitorServidores.Controllers;

[ApiController]
[Route("api/alertas")]
[Authorize]
public class AlertasController : ControllerBase
{
    private readonly MonitorDbContext _context;

    public AlertasController(MonitorDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAlertas()
    {
        var alertas = await _context.Alertas
            .AsNoTracking()
            .Where(a => a.Activo)
            .OrderBy(a => a.Id)
            .Select(a => new
            {
                a.Id,
                a.Titulo,
                a.Descripcion,
                a.FechaRegistro,
                a.ServidorId,
                Servidor = a.Servidor.Nombre,
                a.TipoAlertaId,
                TipoAlerta = a.TipoAlerta.Nombre,
                a.Activo
            })
            .ToListAsync();

        return Ok(alertas);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetAlerta(int id)
    {
        var alerta = await _context.Alertas
            .AsNoTracking()
            .Where(a => a.Id == id && a.Activo)
            .Select(a => new
            {
                a.Id,
                a.Titulo,
                a.Descripcion,
                a.FechaRegistro,
                a.ServidorId,
                Servidor = a.Servidor.Nombre,
                a.TipoAlertaId,
                TipoAlerta = a.TipoAlerta.Nombre,
                a.Activo
            })
            .FirstOrDefaultAsync();

        if (alerta == null)
        {
            return NotFound(new
            {
                mensaje = "Alerta no encontrada."
            });
        }

        return Ok(alerta);
    }

    [HttpPost]
    public async Task<IActionResult> PostAlerta(
        [FromBody] CrearAlertaDto datos)
    {
        if (string.IsNullOrWhiteSpace(datos.Titulo))
        {
            return BadRequest(new
            {
                mensaje = "El título de la alerta es obligatorio."
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

        if (!await _context.TiposAlerta.AnyAsync(t =>
            t.Id == datos.TipoAlertaId && t.Activo))
        {
            return BadRequest(new
            {
                mensaje = "El tipo de alerta no existe o está inactivo."
            });
        }

        var alerta = new Alerta
        {
            Titulo = datos.Titulo.Trim(),
            Descripcion = datos.Descripcion,
            FechaRegistro = DateTime.UtcNow,
            ServidorId = datos.ServidorId,
            TipoAlertaId = datos.TipoAlertaId,
            Activo = true
        };

        _context.Alertas.Add(alerta);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetAlerta),
            new { id = alerta.Id },
            alerta
        );
    }
}

public class CrearAlertaDto
{
    public string Titulo { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public int ServidorId { get; set; }

    public int TipoAlertaId { get; set; }
}
