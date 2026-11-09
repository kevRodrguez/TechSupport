using System.ComponentModel.DataAnnotations;
using TechSupport.API.Models;

namespace TechSupport.API.DTOs;

public class CambiarEstadoRequest
{
    [Required(ErrorMessage = "Debe indicar el estado destino.")]
    public EstadoIncidente? Estado { get; set; }
}
