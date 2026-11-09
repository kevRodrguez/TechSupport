using Microsoft.AspNetCore.Authentication;
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

    public async Task<IActionResult> Index()
    {
        var result = await _api.ListarAsync();
        if (await SesionVencida(result.StatusCode))
            return RedirectToAction("Login", "Account");

        if (!result.Success)
        {
            TempData["Error"] = result.Message;
            return View(Array.Empty<IncidenteDto>());
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
        if (await SesionVencida(result.StatusCode))
            return RedirectToAction("Login", "Account");

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
        var incidente = await _api.ObtenerAsync(id);
        if (await SesionVencida(incidente.StatusCode))
            return RedirectToAction("Login", "Account");

        if (!incidente.Success || incidente.Data is null)
            return View("NoEncontrado", incidente.Message ?? "Incidente no encontrado.");

        return View(incidente.Data);
    }

    [HttpGet]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Asignar(int id)
    {
        var incidente = await _api.ObtenerAsync(id);
        if (await SesionVencida(incidente.StatusCode))
            return RedirectToAction("Login", "Account");

        if (!incidente.Success || incidente.Data is null)
            return View("NoEncontrado", incidente.Message ?? "Incidente no encontrado.");

        return View(await CargarAsignar(incidente.Data, null));
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Asignar(AsignarViewModel model)
    {
        var incidente = await _api.ObtenerAsync(model.IncidenteId);
        if (await SesionVencida(incidente.StatusCode))
            return RedirectToAction("Login", "Account");

        if (!incidente.Success || incidente.Data is null)
            return View("NoEncontrado", incidente.Message ?? "Incidente no encontrado.");

        if (!ModelState.IsValid)
            return View(await CargarAsignar(incidente.Data, model.TecnicoId));

        var result = await _api.AsignarAsync(model.IncidenteId, model.TecnicoId!.Value);
        if (await SesionVencida(result.StatusCode))
            return RedirectToAction("Login", "Account");

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "No se pudo asignar el técnico.");
            return View(await CargarAsignar(incidente.Data, model.TecnicoId));
        }

        TempData["Ok"] = "Técnico asignado.";
        return RedirectToAction(nameof(Detalle), new { id = model.IncidenteId });
    }

    [HttpGet]
    [Authorize(Roles = "Administrador,Tecnico")]
    public async Task<IActionResult> Estado(int id)
    {
        var incidente = await _api.ObtenerAsync(id);
        if (await SesionVencida(incidente.StatusCode))
            return RedirectToAction("Login", "Account");

        if (!incidente.Success || incidente.Data is null)
            return View("NoEncontrado", incidente.Message ?? "Incidente no encontrado.");

        return View(new EstadoViewModel
        {
            IncidenteId = incidente.Data.Id,
            Titulo = incidente.Data.Titulo,
            EstadoActual = incidente.Data.Estado,
            Estado = Etiquetas.SiguienteEstado(incidente.Data.Estado)
        });
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Tecnico")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Estado(EstadoViewModel model)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(model.Estado))
        {
            ModelState.AddModelError(string.Empty, "Debe indicar el estado destino.");
            return View(model);
        }

        var result = await _api.CambiarEstadoAsync(model.IncidenteId, model.Estado);
        if (await SesionVencida(result.StatusCode))
            return RedirectToAction("Login", "Account");

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "No se pudo cambiar el estado.");
            return View(model);
        }

        TempData["Ok"] = "Estado actualizado.";
        return RedirectToAction(nameof(Detalle), new { id = model.IncidenteId });
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var result = await _api.EliminarAsync(id);
        if (await SesionVencida(result.StatusCode))
            return RedirectToAction("Login", "Account");

        if (!result.Success)
        {
            TempData["Error"] = result.Message ?? "No se pudo eliminar el incidente.";
            return RedirectToAction(nameof(Detalle), new { id });
        }

        TempData["Ok"] = "Incidente eliminado.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<AsignarViewModel> CargarAsignar(IncidenteDto incidente, int? tecnicoId)
    {
        var vm = new AsignarViewModel
        {
            IncidenteId = incidente.Id,
            Titulo = incidente.Titulo,
            TecnicoId = tecnicoId
        };

        var tecnicos = await _api.TecnicosAsync();
        if (!tecnicos.Success)
            ModelState.AddModelError(string.Empty, tecnicos.Message ?? "No se pudieron cargar los técnicos.");
        else
        {
            vm.Tecnicos = (tecnicos.Data ?? [])
                .Select(t => new TecnicoOpcion { Id = t.Id, NombreUsuario = t.NombreUsuario })
                .ToList();
        }

        return vm;
    }

    private async Task<bool> SesionVencida(int statusCode)
    {
        if (statusCode != StatusCodes.Status401Unauthorized)
            return false;

        await HttpContext.SignOutAsync();
        HttpContext.Session.Clear();
        TempData["Error"] = "La sesión expiró. Vuelve a iniciar sesión.";
        return true;
    }
}
