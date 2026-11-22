using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitorServidores.Data;
using MonitorServidores.Models;

namespace MonitorServidores.Controllers;

[ApiController]
[Route("api/servidores")]
[Authorize]
public class ServidoresController : ControllerBase
{
    private readonly MonitorDbContext _context;

    public ServidoresController(MonitorDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetServidores()
    {
        var servidores = await _context.Servidores
            .AsNoTracking()
            .OrderBy(s => s.Id)
            .Select(s => new
            {
                s.Id,
                s.Nombre,
                s.DireccionIp,
                s.Descripcion,
                s.Activo,
                s.FechaRegistro,
                s.TipoServidorId,
                s.SistemaOperativoId
            })
            .ToListAsync();

        return Ok(servidores);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetServidor(int id)
    {
        var servidor = await _context.Servidores
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new
            {
                s.Id,
                s.Nombre,
                s.DireccionIp,
                s.Descripcion,
                s.Activo,
                s.FechaRegistro,
                s.TipoServidorId,
                s.SistemaOperativoId
            })
            .FirstOrDefaultAsync();

        if (servidor == null)
        {
            return NotFound(new
            {
                mensaje = "Servidor no encontrado."
            });
        }

        return Ok(servidor);
    }


    [HttpPost]
    public async Task<IActionResult> PostServidor(
        [FromBody] CrearServidorDto datos)
    {
        if (string.IsNullOrWhiteSpace(datos.Nombre) ||
            string.IsNullOrWhiteSpace(datos.DireccionIp))
        {
            return BadRequest(new
            {
                mensaje = "El nombre y la dirección IP son obligatorios."
            });
        }

        var servidor = new Servidor
        {
            Nombre = datos.Nombre.Trim(),
            DireccionIp = datos.DireccionIp.Trim(),
            Descripcion = datos.Descripcion,
            TipoServidorId = datos.TipoServidorId,
            SistemaOperativoId = datos.SistemaOperativoId,
            Activo = true,
            FechaRegistro = DateTime.UtcNow
        };

        _context.Servidores.Add(servidor);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetServidor),
            new { id = servidor.Id },
            new
            {
                servidor.Id,
                servidor.Nombre,
                servidor.DireccionIp,
                servidor.Descripcion,
                servidor.Activo,
                servidor.FechaRegistro,
                servidor.TipoServidorId,
                servidor.SistemaOperativoId
            }
        );
    }
}

// Datos recibidos para registrar un servidor
public class CrearServidorDto
{
    public string Nombre { get; set; } = string.Empty;

    public string DireccionIp { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public int? TipoServidorId { get; set; }

    public int? SistemaOperativoId { get; set; }
}