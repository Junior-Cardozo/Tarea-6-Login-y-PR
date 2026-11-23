using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitorServidores.Data;
using MonitorServidores.Models;

namespace MonitorServidores.Controllers;

[ApiController]
[Route("api/tipos-alerta")]
[Authorize]
public class TiposAlertaController : ControllerBase
{
    private readonly MonitorDbContext _context;

    public TiposAlertaController(MonitorDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetTiposAlerta()
    {
        var tipos = await _context.TiposAlerta
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

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetTipoAlerta(int id)
    {
        var tipo = await _context.TiposAlerta
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
                mensaje = "Tipo de alerta no encontrado."
            });
        }

        return Ok(tipo);
    }

    [HttpPost]
    public async Task<IActionResult> PostTipoAlerta(
        [FromBody] CrearTipoAlertaDto datos)
    {
        if (string.IsNullOrWhiteSpace(datos.Nombre))
        {
            return BadRequest(new
            {
                mensaje = "El nombre es obligatorio."
            });
        }

        var tipo = new TipoAlerta
        {
            Nombre = datos.Nombre.Trim(),
            Descripcion = datos.Descripcion,
            Activo = true
        };

        _context.TiposAlerta.Add(tipo);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetTipoAlerta),
            new { id = tipo.Id },
            tipo
        );
    }
}

public class CrearTipoAlertaDto
{
    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }
}
