using Microsoft.EntityFrameworkCore;
using TechSupport.API.Data;
using TechSupport.API.DTOs;
using TechSupport.API.Models;

namespace TechSupport.API.Services;

public class IncidenteService
{
    public const string MensajeNoEncontrado = "Incidente no encontrado.";

    private readonly AppDbContext _db;

    public IncidenteService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ServiceResult<IncidenteResponse>> CrearAsync(CrearIncidenteRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Titulo))
            return ServiceResult<IncidenteResponse>.BadRequest("El título es obligatorio.");

        var reportante = await _db.Usuarios.FirstAsync(u => u.NombreUsuario == "usuario");
        var incidente = new Incidente
        {
            Titulo = request.Titulo.Trim(),
            Descripcion = string.IsNullOrWhiteSpace(request.Descripcion) ? null : request.Descripcion.Trim(),
            Prioridad = request.Prioridad,
            Estado = EstadoIncidente.Pendiente,
            UsuarioId = reportante.Id,
            FechaCreacion = DateTime.UtcNow
        };

        _db.Incidentes.Add(incidente);
        await _db.SaveChangesAsync();
        await _db.Entry(incidente).Reference(i => i.ReportadoPor).LoadAsync();

        return ServiceResult<IncidenteResponse>.Created(Map(incidente));
    }

    public async Task<List<IncidenteResponse>> ListarAsync()
    {
        var incidentes = await Query()
            .OrderByDescending(i => i.FechaCreacion)
            .ToListAsync();

        return incidentes.Select(Map).ToList();
    }

    public async Task<ServiceResult<IncidenteResponse>> ObtenerAsync(int id)
    {
        var incidente = await Query().FirstOrDefaultAsync(i => i.Id == id);
        if (incidente is null)
            return ServiceResult<IncidenteResponse>.NotFound(MensajeNoEncontrado);

        return ServiceResult<IncidenteResponse>.Ok(Map(incidente));
    }

    private IQueryable<Incidente> Query() =>
        _db.Incidentes.Include(i => i.ReportadoPor).Include(i => i.Tecnico);

    internal static IncidenteResponse Map(Incidente incidente) => new()
    {
        Id = incidente.Id,
        Titulo = incidente.Titulo,
        Descripcion = incidente.Descripcion,
        Prioridad = incidente.Prioridad.ToString(),
        Estado = incidente.Estado.ToString(),
        UsuarioId = incidente.UsuarioId,
        ReportadoPor = incidente.ReportadoPor.NombreUsuario,
        TecnicoId = incidente.TecnicoId,
        Tecnico = incidente.Tecnico?.NombreUsuario,
        FechaCreacion = incidente.FechaCreacion,
        FechaResolucion = incidente.FechaResolucion
    };
}
