using Microsoft.AspNetCore.Mvc;
using TechSupport.API.DTOs;
using TechSupport.API.Services;

namespace TechSupport.API.Controllers;

[ApiController]
[Route("api/incidentes")]
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
        return Ok(await _service.ListarAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<IncidenteResponse>> Obtener(int id)
    {
        var result = await _service.ObtenerAsync(id);
        if (!result.Success)
            return NotFound(new { message = result.Message });

        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<ActionResult<IncidenteResponse>> Crear(CrearIncidenteRequest request)
    {
        var result = await _service.CrearAsync(request);
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return CreatedAtAction(nameof(Obtener), new { id = result.Data!.Id }, result.Data);
    }
}
