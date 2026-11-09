namespace TechSupport.API.DTOs;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;

    public string NombreUsuario { get; set; } = string.Empty;

    public string Rol { get; set; } = string.Empty;
}
