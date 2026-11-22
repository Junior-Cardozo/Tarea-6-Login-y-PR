using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitorServidores.Data;
using MonitorServidores.Models;
using System.ComponentModel.DataAnnotations;

namespace MonitorServidores.Controllers;

[ApiController]
[Route("api/monitoreos")]
[Authorize]
public class MonitoreosController : ControllerBase
{
    private readonly MonitorDbContext _context;

    public MonitoreosController(MonitorDbContext context)
    {
        _context = context;
    }

    // GET: api/monitoreos

    [HttpGet]
    public async Task<IActionResult> GetMonitoreos()
    {
        var monitoreos = await _context.Monitoreos
            .AsNoTracking()
            .Where(m => m.Activo)
            .OrderBy(m => m.Id)
            .Select(m => new
            {
                m.Id,
                m.ServidorId,
                m.FechaRegistro,
                m.Disponible,
                m.TiempoRespuestaMs,
                m.Observaciones,
                m.Activo
            })
            .ToListAsync();

        return Ok(monitoreos);
    }

    // GET: api/monitoreos/1

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetMonitoreo(int id)
    {
        var monitoreo = await _context.Monitoreos
            .AsNoTracking()
            .Where(m => m.Id == id && m.Activo)
            .Select(m => new
            {
                m.Id,
                m.ServidorId,
                m.FechaRegistro,
                m.Disponible,
                m.TiempoRespuestaMs,
                m.Observaciones,
                m.Activo
            })
            .FirstOrDefaultAsync();

        if (monitoreo == null)
        {
            return NotFound(new
            {
                mensaje = "Monitoreo no encontrado."
            });
        }

        return Ok(monitoreo);
    }

    // POST: api/monitoreos

    [HttpPost]
    public async Task<IActionResult> PostMonitoreo(
        [FromBody] CrearMonitoreoDto datos)
    {
        // Verificar que exista el servidor asociado
        var servidorExiste = await _context.Servidores
            .AnyAsync(s =>
                s.Id == datos.ServidorId &&
                s.Activo);

        if (!servidorExiste)
        {
            return BadRequest(new
            {
                mensaje = "El servidor indicado no existe o está inactivo."
            });
        }

        var monitoreo = new Monitoreo
        {
            ServidorId = datos.ServidorId,
            FechaRegistro = DateTime.UtcNow,
            Disponible = datos.Disponible,
            TiempoRespuestaMs = datos.TiempoRespuestaMs,
            Observaciones = datos.Observaciones,
            Activo = true
        };

        _context.Monitoreos.Add(monitoreo);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetMonitoreo),
            new { id = monitoreo.Id },
            new
            {
                monitoreo.Id,
                monitoreo.ServidorId,
                monitoreo.FechaRegistro,
                monitoreo.Disponible,
                monitoreo.TiempoRespuestaMs,
                monitoreo.Observaciones,
                monitoreo.Activo
            }
        );
    }
         // PUT: api/monitoreos/1
[HttpPut("{id:int}")]
public async Task<IActionResult> PutMonitoreo(
    int id, [FromBody] CrearMonitoreoDto datos)
{
    var monitoreo = await _context.Monitoreos
        .FirstOrDefaultAsync(m => m.Id == id && m.Activo);

    if (monitoreo == null)
        return NotFound(new { mensaje = "Monitoreo no encontrado." });

    var servidorExiste = await _context.Servidores
        .AnyAsync(s => s.Id == datos.ServidorId && s.Activo);

    if (!servidorExiste)
        return BadRequest(new
        {
            mensaje = "El servidor no existe o está inactivo."
        });

    monitoreo.ServidorId = datos.ServidorId;
    monitoreo.Disponible = datos.Disponible;
    monitoreo.TiempoRespuestaMs = datos.TiempoRespuestaMs;
    monitoreo.Observaciones = datos.Observaciones;

    await _context.SaveChangesAsync();

    return NoContent();
}

// DELETE: api/monitoreos/1
   [HttpDelete("{id:int}")]
   public async Task<IActionResult> DeleteMonitoreo(int id)
{
    var monitoreo = await _context.Monitoreos
        .FirstOrDefaultAsync(m => m.Id == id && m.Activo);

    if (monitoreo == null)
        return NotFound(new { mensaje = "Monitoreo no encontrado." });

    monitoreo.Activo = false;

    await _context.SaveChangesAsync();

    return NoContent();
}
}

// DTO PARA REGISTRAR MONITOREOS

public class CrearMonitoreoDto
{
    [Range(1, int.MaxValue)]
    public int ServidorId { get; set; }

    public bool Disponible { get; set; }

    [Range(0, int.MaxValue)]
    public int? TiempoRespuestaMs { get; set; }

    [MaxLength(300)]
    public string? Observaciones { get; set; }
}