using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitorServidores.Data;
using MonitorServidores.Models;
using System.ComponentModel.DataAnnotations;

namespace MonitorServidores.Controllers;

[ApiController]
[Route("api/sistemas-operativos")]
[Authorize]
public class SistemasOperativosController : ControllerBase
{
    private readonly MonitorDbContext _context;

    public SistemasOperativosController(MonitorDbContext context)
    {
        _context = context;
    }

    // GET: api/sistemas-operativos

    [HttpGet]
    public async Task<IActionResult> GetSistemasOperativos()
    {
        var sistemas = await _context.SistemasOperativos
            .AsNoTracking()
            .Where(s => s.Activo)
            .OrderBy(s => s.Id)
            .Select(s => new
            {
                s.Id,
                s.Nombre,
                s.Version,
                s.Descripcion,
                s.Activo
            })
            .ToListAsync();

        return Ok(sistemas);
    }

    // GET: api/sistemas-operativos/1

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetSistemaOperativo(int id)
    {
        var sistema = await _context.SistemasOperativos
            .AsNoTracking()
            .Where(s => s.Id == id && s.Activo)
            .Select(s => new
            {
                s.Id,
                s.Nombre,
                s.Version,
                s.Descripcion,
                s.Activo
            })
            .FirstOrDefaultAsync();

        if (sistema == null)
        {
            return NotFound(new
            {
                mensaje = "Sistema operativo no encontrado."
            });
        }

        return Ok(sistema);
    }

    // POST: api/sistemas-operativos

    [HttpPost]
    public async Task<IActionResult> PostSistemaOperativo(
        [FromBody] CrearSistemaOperativoDto datos)
    {
        if (string.IsNullOrWhiteSpace(datos.Nombre))
        {
            return BadRequest(new
            {
                mensaje = "El nombre del sistema operativo es obligatorio."
            });
        }

        var sistema = new SistemaOperativo
        {
            Nombre = datos.Nombre.Trim(),
            Version = datos.Version,
            Descripcion = datos.Descripcion,
            Activo = true
        };

        _context.SistemasOperativos.Add(sistema);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetSistemaOperativo),
            new { id = sistema.Id },
            new
            {
                sistema.Id,
                sistema.Nombre,
                sistema.Version,
                sistema.Descripcion,
                sistema.Activo
            }
        );
    }
        // PUT: api/sistemas-operativos/1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutSistemaOperativo(
    int id, [FromBody] CrearSistemaOperativoDto datos)
{
    var sistema = await _context.SistemasOperativos
        .FirstOrDefaultAsync(s => s.Id == id && s.Activo);

    if (sistema == null)
        return NotFound(new { mensaje = "Sistema operativo no encontrado." });

    if (string.IsNullOrWhiteSpace(datos.Nombre))
        return BadRequest(new { mensaje = "El nombre es obligatorio." });

    sistema.Nombre = datos.Nombre.Trim();
    sistema.Version = datos.Version;
    sistema.Descripcion = datos.Descripcion;

    await _context.SaveChangesAsync();

    return NoContent();
}

// DELETE: api/sistemas-operativos/1
[HttpDelete("{id:int}")]
public async Task<IActionResult> DeleteSistemaOperativo(int id)
{
    var sistema = await _context.SistemasOperativos
        .FirstOrDefaultAsync(s => s.Id == id && s.Activo);

    if (sistema == null)
        return NotFound(new { mensaje = "Sistema operativo no encontrado." });

    sistema.Activo = false;

    await _context.SaveChangesAsync();

    return NoContent();
}
}


// DTO para registrar sistemas operativos


public class CrearSistemaOperativoDto
{
    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Version { get; set; }

    [MaxLength(300)]
    public string? Descripcion { get; set; }
}