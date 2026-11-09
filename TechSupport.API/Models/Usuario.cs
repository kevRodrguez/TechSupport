using System.ComponentModel.DataAnnotations;

namespace TechSupport.API.Models;

public class Usuario
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public Rol Rol { get; set; }
}
