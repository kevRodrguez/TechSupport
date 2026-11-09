using System.ComponentModel.DataAnnotations;

namespace TechSupport.Web.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [Display(Name = "Usuario")]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;
}

public class CrearIncidenteViewModel
{
    [Required(ErrorMessage = "El título es obligatorio.")]
    [Display(Name = "Título")]
    public string Titulo { get; set; } = string.Empty;

    [Display(Name = "Descripción")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "La prioridad es obligatoria.")]
    [Display(Name = "Prioridad")]
    public string Prioridad { get; set; } = "Media";
}

public class AsignarViewModel
{
    public int IncidenteId { get; set; }

    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Seleccione un técnico.")]
    [Display(Name = "Técnico")]
    public int? TecnicoId { get; set; }

    public List<TecnicoOpcion> Tecnicos { get; set; } = [];
}

public class TecnicoOpcion
{
    public int Id { get; set; }

    public string NombreUsuario { get; set; } = string.Empty;
}

public class EstadoViewModel
{
    public int IncidenteId { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string EstadoActual { get; set; } = string.Empty;

    [Required(ErrorMessage = "Seleccione el estado destino.")]
    [Display(Name = "Nuevo estado")]
    public string? Estado { get; set; }
}

public static class Etiquetas
{
    public static readonly (string Value, string Label)[] Estados =
    [
        ("Pendiente", "Pendiente"),
        ("Asignado", "Asignado"),
        ("EnProceso", "En proceso"),
        ("Resuelto", "Resuelto"),
        ("Cerrado", "Cerrado")
    ];

    public static string Estado(string? estado) => estado switch
    {
        "EnProceso" => "En proceso",
        "Critica" => "Crítica",
        "Tecnico" => "Técnico",
        null or "" => "—",
        _ => estado
    };

    public static string Prioridad(string? prioridad) => prioridad switch
    {
        "Critica" => "Crítica",
        null or "" => "—",
        _ => prioridad
    };

    public static string Rol(string? rol) => rol switch
    {
        "Tecnico" => "Técnico",
        null or "" => "—",
        _ => rol
    };

    public static string Badge(string? valor) =>
        string.IsNullOrEmpty(valor) ? "badge" : $"badge badge-{valor}";

    public static string? SiguienteEstado(string? estado) => estado switch
    {
        "Asignado" => "EnProceso",
        "EnProceso" => "Resuelto",
        "Resuelto" => "Cerrado",
        _ => null
    };

    public static string Fecha(DateTime fecha) =>
        fecha.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
}
