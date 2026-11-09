using System.ComponentModel.DataAnnotations;
using TechSupport.API.Models;

namespace TechSupport.API.DTOs;

public class CrearIncidenteRequest
{
    [Required(ErrorMessage = "El título es obligatorio.")]
    public string Titulo { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public Prioridad Prioridad { get; set; } = Prioridad.Media;
}
