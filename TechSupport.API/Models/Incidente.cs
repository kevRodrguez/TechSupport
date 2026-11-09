using System.ComponentModel.DataAnnotations;

namespace TechSupport.API.Models;

public class Incidente
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Titulo { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Descripcion { get; set; }

    public Prioridad Prioridad { get; set; }

    public EstadoIncidente Estado { get; set; }

    public int UsuarioId { get; set; }

    public Usuario ReportadoPor { get; set; } = null!;

    public int? TecnicoId { get; set; }

    public Usuario? Tecnico { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaResolucion { get; set; }
}
