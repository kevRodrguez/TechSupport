using System.ComponentModel.DataAnnotations;

namespace TechSupport.API.DTOs;

public class AsignarIncidenteRequest
{
    [Required(ErrorMessage = "Debe indicar el técnico.")]
    public int? TecnicoId { get; set; }
}
