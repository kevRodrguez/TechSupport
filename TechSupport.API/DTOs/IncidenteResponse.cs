namespace TechSupport.API.DTOs;

public class IncidenteResponse
{
    public int Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public string Prioridad { get; set; } = string.Empty;

    public string Estado { get; set; } = string.Empty;

    public int UsuarioId { get; set; }

    public string ReportadoPor { get; set; } = string.Empty;

    public int? TecnicoId { get; set; }

    public string? Tecnico { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaResolucion { get; set; }
}
