using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechSupport.Web.Models;
using TechSupport.Web.Services;

namespace TechSupport.Web.Controllers;

[Authorize]
public class IncidentesController : Controller
{
    private readonly ApiClient _api;

    public IncidentesController(ApiClient api)
    {
        _api = api;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var result = await _api.ListarAsync();
        if (!result.Success)
        {
            TempData["Error"] = result.Message;
            return View(new List<IncidenteDto>());
        }

        return View(result.Data ?? []);
    }

    [HttpGet]
    public IActionResult Crear()
    {
        return View(new CrearIncidenteViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(CrearIncidenteViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _api.CrearAsync(model.Titulo, model.Descripcion, model.Prioridad);
        if (!result.Success || result.Data is null)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "No se pudo crear el incidente.");
            return View(model);
        }

        TempData["Ok"] = "Incidente creado.";
        return RedirectToAction(nameof(Detalle), new { id = result.Data.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        var result = await _api.ObtenerAsync(id);
        if (!result.Success || result.Data is null)
        {
            TempData["Error"] = result.Message ?? "Incidente no encontrado.";
            return RedirectToAction(nameof(Index));
        }

        return View(result.Data);
    }

    [Authorize(Roles = "Administrador")]
    [HttpGet]
    public async Task<IActionResult> Asignar(int id)
    {
        var incidente = await _api.ObtenerAsync(id);
        if (!incidente.Success || incidente.Data is null)
        {
            TempData["Error"] = incidente.Message ?? "Incidente no encontrado.";
            return RedirectToAction(nameof(Index));
        }

        var tecnicos = await _api.TecnicosAsync();
        if (!tecnicos.Success)
        {
            TempData["Error"] = tecnicos.Message;
            return RedirectToAction(nameof(Detalle), new { id });
        }

        return View(new AsignarViewModel
        {
            IncidenteId = id,
            Titulo = incidente.Data.Titulo,
            Tecnicos = (tecnicos.Data ?? []).Select(t => new TecnicoOpcion
            {
                Id = t.Id,
                NombreUsuario = t.NombreUsuario
            }).ToList()
        });
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Asignar(AsignarViewModel model)
    {
        var tecnicos = await _api.TecnicosAsync();
        model.Tecnicos = (tecnicos.Data ?? []).Select(t => new TecnicoOpcion
        {
            Id = t.Id,
            NombreUsuario = t.NombreUsuario
        }).ToList();

        if (!ModelState.IsValid)
            return View(model);

        var result = await _api.AsignarAsync(model.IncidenteId, model.TecnicoId!.Value);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "No se pudo asignar el técnico.");
            return View(model);
        }

        TempData["Ok"] = "Técnico asignado.";
        return RedirectToAction(nameof(Detalle), new { id = model.IncidenteId });
    }

    [Authorize(Roles = "Administrador,Tecnico")]
    [HttpGet]
    public async Task<IActionResult> Estado(int id)
    {
        var incidente = await _api.ObtenerAsync(id);
        if (!incidente.Success || incidente.Data is null)
        {
            TempData["Error"] = incidente.Message ?? "Incidente no encontrado.";
            return RedirectToAction(nameof(Index));
        }

        var siguiente = Etiquetas.SiguienteEstado(incidente.Data.Estado);
        return View(new EstadoViewModel
        {
            IncidenteId = id,
            Titulo = incidente.Data.Titulo,
            EstadoActual = incidente.Data.Estado,
            Estado = siguiente
        });
    }

    [Authorize(Roles = "Administrador,Tecnico")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Estado(EstadoViewModel model)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(model.Estado))
        {
            ModelState.AddModelError(string.Empty, "Seleccione el estado destino.");
            return View(model);
        }

        var result = await _api.CambiarEstadoAsync(model.IncidenteId, model.Estado);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "No se pudo cambiar el estado.");
            return View(model);
        }

        TempData["Ok"] = "Estado actualizado.";
        return RedirectToAction(nameof(Detalle), new { id = model.IncidenteId });
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var result = await _api.EliminarAsync(id);
        if (!result.Success)
        {
            TempData["Error"] = result.Message ?? "No se pudo eliminar el incidente.";
            return RedirectToAction(nameof(Detalle), new { id });
        }

        TempData["Ok"] = "Incidente eliminado.";
        return RedirectToAction(nameof(Index));
    }
}
