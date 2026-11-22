using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitorServidores.Data;
using MonitorServidores.Models;
using System.ComponentModel.DataAnnotations;

namespace MonitorServidores.Controllers;

[ApiController]
[Route("api/tipos-servidor")]
[Authorize]
public class TiposServidorController : ControllerBase
{
    private readonly MonitorDbContext _context;

    public TiposServidorController(MonitorDbContext context)
    {
        _context = context;
    }

    // GET: api/tipos-servidor
    [HttpGet]
    public async Task<IActionResult> GetTiposServidor()
    {
        var tipos = await _context.TiposServidor
            .AsNoTracking()
            .Where(t => t.Activo)
            .OrderBy(t => t.Id)
            .Select(t => new
            {
                t.Id,
                t.Nombre,
                t.Descripcion,
                t.Activo
            })
            .ToListAsync();

        return Ok(tipos);
    }

    // GET: api/tipos-servidor/1
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetTipoServidor(int id)
    {
        var tipo = await _context.TiposServidor
            .AsNoTracking()
            .Where(t => t.Id == id && t.Activo)
            .Select(t => new
            {
                t.Id,
                t.Nombre,
                t.Descripcion,
                t.Activo
            })
            .FirstOrDefaultAsync();

        if (tipo == null)
        {
            return NotFound(new
            {
                mensaje = "Tipo de servidor no encontrado."
            });
        }

        return Ok(tipo);
    }

    // POST: api/tipos-servidor
    [HttpPost]
    public async Task<IActionResult> PostTipoServidor(
        [FromBody] CrearTipoServidorDto datos)
    {
        if (string.IsNullOrWhiteSpace(datos.Nombre))
        {
            return BadRequest(new
            {
                mensaje = "El nombre es obligatorio."
            });
        }

        var tipo = new TipoServidor
        {
            Nombre = datos.Nombre.Trim(),
            Descripcion = datos.Descripcion,
            Activo = true
        };

        _context.TiposServidor.Add(tipo);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetTipoServidor),
            new { id = tipo.Id },
            new
            {
                tipo.Id,
                tipo.Nombre,
                tipo.Descripcion,
                tipo.Activo
            }
        );
    }
          // PUT: api/tipos-servidor/1
[HttpPut("{id:int}")]
public async Task<IActionResult> PutTipoServidor(
    int id, [FromBody] CrearTipoServidorDto datos)
{
    var tipo = await _context.TiposServidor
        .FirstOrDefaultAsync(t => t.Id == id && t.Activo);

    if (tipo == null)
        return NotFound(new { mensaje = "Tipo de servidor no encontrado." });

    if (string.IsNullOrWhiteSpace(datos.Nombre))
        return BadRequest(new { mensaje = "El nombre es obligatorio." });

    tipo.Nombre = datos.Nombre.Trim();
    tipo.Descripcion = datos.Descripcion;

    await _context.SaveChangesAsync();

    return NoContent();
}

// DELETE: api/tipos-servidor/1
   [HttpDelete("{id:int}")] 
   public async Task<IActionResult> DeleteTipoServidor(int id)
{
    var tipo = await _context.TiposServidor
        .FirstOrDefaultAsync(t => t.Id == id && t.Activo);

    if (tipo == null)
        return NotFound(new { mensaje = "Tipo de servidor no encontrado." });

    tipo.Activo = false;

    await _context.SaveChangesAsync();

    return NoContent();
}
}

// Datos para registrar un tipo de servidor
public class CrearTipoServidorDto
{
    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Descripcion { get; set; }
}