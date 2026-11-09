using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechSupport.API.Data;
using TechSupport.API.DTOs;
using TechSupport.API.Models;

namespace TechSupport.API.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize(Roles = "Administrador")]
public class UsuariosController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsuariosController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("tecnicos")]
    public async Task<ActionResult<List<TecnicoResponse>>> Tecnicos()
    {
        var tecnicos = await _db.Usuarios
            .Where(u => u.Rol == Rol.Tecnico)
            .OrderBy(u => u.NombreUsuario)
            .Select(u => new TecnicoResponse
            {
                Id = u.Id,
                NombreUsuario = u.NombreUsuario
            })
            .ToListAsync();

        return Ok(tecnicos);
    }
}
