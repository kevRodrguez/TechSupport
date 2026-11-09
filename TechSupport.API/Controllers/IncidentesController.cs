using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechSupport.API.DTOs;
using TechSupport.API.Models;
using TechSupport.API.Services;

namespace TechSupport.API.Controllers;

[ApiController]
[Route("api/incidentes")]
[Authorize]
public class IncidentesController : ControllerBase
{
    private readonly IncidenteService _service;

    public IncidentesController(IncidenteService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<IncidenteResponse>>> Listar()
    {
        return Ok(await _service.ListarAsync(ClaimsActor.From(User)));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<IncidenteResponse>> Obtener(int id)
    {
        return Traducir(await _service.ObtenerAsync(id, ClaimsActor.From(User)));
    }

    [HttpPost]
    public async Task<ActionResult<IncidenteResponse>> Crear(CrearIncidenteRequest request)
    {
        var result = await _service.CrearAsync(request, ClaimsActor.From(User));
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return CreatedAtAction(nameof(Obtener), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id:int}/asignar")]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<IncidenteResponse>> Asignar(int id, AsignarIncidenteRequest request)
    {
        return Traducir(await _service.AsignarAsync(id, request.TecnicoId!.Value));
    }

    [HttpPut("{id:int}/estado")]
    [Authorize(Roles = "Administrador,Tecnico")]
    public async Task<ActionResult<IncidenteResponse>> CambiarEstado(int id, CambiarEstadoRequest request)
    {
        return Traducir(await _service.CambiarEstadoAsync(id, request.Estado!.Value, ClaimsActor.From(User)));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var result = await _service.EliminarAsync(id);
        if (!result.Success)
            return NotFound(new { message = result.Message });

        return NoContent();
    }

    private ActionResult<IncidenteResponse> Traducir(ServiceResult<IncidenteResponse> result)
    {
        if (result.Success)
            return Ok(result.Data);

        if (result.StatusCode == StatusCodes.Status404NotFound)
            return NotFound(new { message = result.Message });

        return BadRequest(new { message = result.Message });
    }
}
