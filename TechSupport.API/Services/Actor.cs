using System.Security.Claims;
using TechSupport.API.Models;

namespace TechSupport.API.Services;

public record Actor(int Id, string Nombre, Rol Rol);

public static class ClaimsActor
{
    public static Actor From(ClaimsPrincipal user)
    {
        var idValue = user.FindFirstValue(ClaimTypes.NameIdentifier);
        var nombre = user.FindFirstValue(ClaimTypes.Name) ?? user.Identity?.Name ?? string.Empty;
        var rolValue = user.FindFirstValue(ClaimTypes.Role);

        if (!int.TryParse(idValue, out var id) || !Enum.TryParse<Rol>(rolValue, out var rol))
            throw new InvalidOperationException("El token no incluye los claims esperados.");

        return new Actor(id, nombre, rol);
    }
}
