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

    public async Task<ServiceResult<IncidenteResponse>> CrearAsync(CrearIncidenteRequest request, Actor actor)
    {
        if (string.IsNullOrWhiteSpace(request.Titulo))
            return ServiceResult<IncidenteResponse>.BadRequest("El título es obligatorio.");

        var reportante = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == actor.Id);
        if (reportante is null)
            return ServiceResult<IncidenteResponse>.BadRequest("El usuario del token no existe.");

        var incidente = new Incidente
        {
            Titulo = request.Titulo.Trim(),
            Descripcion = string.IsNullOrWhiteSpace(request.Descripcion) ? null : request.Descripcion.Trim(),
            Prioridad = request.Prioridad,
            Estado = EstadoIncidente.Pendiente,
            UsuarioId = reportante.Id,
            ReportadoPor = reportante,
            FechaCreacion = DateTime.UtcNow
        };

        _db.Incidentes.Add(incidente);
        await _db.SaveChangesAsync();

        return ServiceResult<IncidenteResponse>.Created(Map(incidente));
    }

    public async Task<List<IncidenteResponse>> ListarAsync(Actor actor)
    {
        var incidentes = await Visibles(actor)
            .OrderByDescending(i => i.FechaCreacion)
            .ToListAsync();

        return incidentes.Select(Map).ToList();
    }

    public async Task<ServiceResult<IncidenteResponse>> ObtenerAsync(int id, Actor actor)
    {
        var incidente = await Visibles(actor).FirstOrDefaultAsync(i => i.Id == id);
        if (incidente is null)
            return ServiceResult<IncidenteResponse>.NotFound(MensajeNoEncontrado);

        return ServiceResult<IncidenteResponse>.Ok(Map(incidente));
    }

    public async Task<ServiceResult<IncidenteResponse>> AsignarAsync(int id, int tecnicoId)
    {
        var incidente = await Query().FirstOrDefaultAsync(i => i.Id == id);
        if (incidente is null)
            return ServiceResult<IncidenteResponse>.NotFound(MensajeNoEncontrado);

        if (incidente.Estado != EstadoIncidente.Pendiente)
            return ServiceResult<IncidenteResponse>.BadRequest("Solo se puede asignar un incidente en estado Pendiente.");

        var tecnico = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == tecnicoId);
        if (tecnico is null || tecnico.Rol != Rol.Tecnico)
            return ServiceResult<IncidenteResponse>.BadRequest("Solo se puede asignar a un usuario con rol Técnico.");

        incidente.TecnicoId = tecnico.Id;
        incidente.Tecnico = tecnico;
        incidente.Estado = EstadoIncidente.Asignado;
        await _db.SaveChangesAsync();

        return ServiceResult<IncidenteResponse>.Ok(Map(incidente));
    }

    public async Task<ServiceResult<IncidenteResponse>> CambiarEstadoAsync(int id, EstadoIncidente destino, Actor actor)
    {
        var incidente = await Visibles(actor).FirstOrDefaultAsync(i => i.Id == id);
        if (incidente is null)
            return ServiceResult<IncidenteResponse>.NotFound(MensajeNoEncontrado);

        if (!TrySiguiente(incidente.Estado, out var siguiente))
        {
            var message = incidente.Estado == EstadoIncidente.Cerrado
                ? "Un incidente Cerrado no se puede reabrir."
                : "Un incidente Pendiente solo avanza a Asignado al asignar un técnico.";
            return ServiceResult<IncidenteResponse>.BadRequest(message);
        }

        if (destino != siguiente)
        {
            return ServiceResult<IncidenteResponse>.BadRequest(
                $"No se puede pasar de {incidente.Estado} a {destino}. El siguiente estado permitido es {siguiente}.");
        }

        incidente.Estado = destino;
        if (destino == EstadoIncidente.Resuelto)
            incidente.FechaResolucion = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ServiceResult<IncidenteResponse>.Ok(Map(incidente));
    }

    public async Task<ServiceResult<bool>> EliminarAsync(int id)
    {
        var incidente = await _db.Incidentes.FirstOrDefaultAsync(i => i.Id == id);
        if (incidente is null)
            return ServiceResult<bool>.NotFound(MensajeNoEncontrado);

        _db.Incidentes.Remove(incidente);
        await _db.SaveChangesAsync();
        return ServiceResult<bool>.Ok(true);
    }

    private static bool TrySiguiente(EstadoIncidente actual, out EstadoIncidente siguiente)
    {
        switch (actual)
        {
            case EstadoIncidente.Asignado:
                siguiente = EstadoIncidente.EnProceso;
                return true;
            case EstadoIncidente.EnProceso:
                siguiente = EstadoIncidente.Resuelto;
                return true;
            case EstadoIncidente.Resuelto:
                siguiente = EstadoIncidente.Cerrado;
                return true;
            default:
                siguiente = default;
                return false;
        }
    }

    private IQueryable<Incidente> Visibles(Actor actor)
    {
        var query = Query();
        return actor.Rol switch
        {
            Rol.Administrador => query,
            Rol.Tecnico => query.Where(i => i.TecnicoId == actor.Id),
            _ => query.Where(i => i.UsuarioId == actor.Id)
        };
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
