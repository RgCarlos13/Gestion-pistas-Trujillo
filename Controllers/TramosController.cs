using Microsoft.AspNetCore.Mvc;
using GestionPistasWeb.Models;
using GestionPistasWeb.Services;
using GestionPistasWeb.ViewModels;

namespace GestionPistasWeb.Controllers;

public class TramosController : Controller
{
    private const string ZonaTrabajoSistema = "Centro Histórico de Trujillo";

    private readonly TramoService _tramoService;
    private readonly PersistenciaService _persistenciaService;
    private readonly IWebHostEnvironment _env;

    public TramosController(TramoService tramoService, PersistenciaService persistenciaService, IWebHostEnvironment env)
    {
        _tramoService = tramoService;
        _persistenciaService = persistenciaService;
        _env = env;
    }

    public IActionResult Index(string? estado)
    {
        EstadoTramo? filtroEstado = null;
        if (!string.IsNullOrEmpty(estado))
        {
            EstadoTramo estadoConvertido;
            if (Enum.TryParse<EstadoTramo>(estado, out estadoConvertido))
                filtroEstado = estadoConvertido;
        }

        GestionPistasWeb.DataStructures.ListaSimple<Tramo> tramos = _tramoService.Filtrar(filtroEstado);
        TramosIndexViewModel modelo = new TramosIndexViewModel()
        {
            Tramos = tramos,
            TotalTramos = tramos.Contar(),
            TramosMapaJson = _tramoService.ConstruirMapaJson(tramos),
            EstadoSeleccionado = estado,
            ZonaTrabajo = ZonaTrabajoSistema
        };

        return View(modelo);
    }

    [HttpGet]
    public IActionResult MapaJson()
    {
        string json = _tramoService.ConstruirMapaJson(_tramoService.ObtenerTramosConCoordenadas());
        return Content(json, "application/json");
    }

    public IActionResult Crear()
    {
        return View(new Tramo { ZonaTrabajo = ZonaTrabajoSistema, Zona = "Centro" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Crear(Tramo tramo, IFormFile? evidencia)
    {
        tramo.ZonaTrabajo = ZonaTrabajoSistema;
        if (string.IsNullOrWhiteSpace(tramo.Zona))
            tramo.Zona = "Centro";

        ValidarTramo(tramo);
        ValidarEvidencia(evidencia);

        if (!ModelState.IsValid)
            return View(tramo);

        Tramo tramoCreado = _tramoService.RegistrarTramo(tramo);

        if (evidencia != null && evidencia.Length > 0)
        {
            (string rutaPublica, string nombreOriginal) resultado = GuardarEvidencia(tramoCreado.Id, evidencia);
            tramoCreado.RutaEvidencia = resultado.rutaPublica;
            tramoCreado.NombreEvidencia = resultado.nombreOriginal;
            tramoCreado.FechaActualizacion = DateTime.Now;
        }

        _persistenciaService.GuardarTramos(_tramoService);
        TempData["Mensaje"] = $"Tramo #{tramoCreado.Id} registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Detalle(int id)
    {
        Tramo? tramo = _tramoService.BuscarPorId(id);
        if (tramo == null)
            return NotFound();

        ViewBag.TramoMapaJson = _tramoService.ConstruirMapaJson(CrearListaUnTramo(tramo));
        return View(tramo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CambiarEstado(int id, string nuevoEstado)
    {
        EstadoTramo estado;
        if (string.IsNullOrEmpty(nuevoEstado) || !Enum.TryParse<EstadoTramo>(nuevoEstado, out estado))
        {
            TempData["Error"] = "Estado inválido.";
            return RedirectToAction(nameof(Detalle), new { id = id });
        }

        if (_tramoService.CambiarEstado(id, estado))
        {
            _persistenciaService.GuardarTramos(_tramoService);
            TempData["Mensaje"] = $"Estado del tramo #{id} actualizado a {nuevoEstado}.";
        }
        else
        {
            TempData["Error"] = $"No se encontró el tramo #{id}.";
        }

        return RedirectToAction(nameof(Detalle), new { id = id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Eliminar(int id)
    {
        if (_tramoService.DesactivarTramo(id))
        {
            _persistenciaService.GuardarTramos(_tramoService);
            TempData["Mensaje"] = $"Tramo #{id} eliminado de los registros activos correctamente.";
        }
        else
        {
            TempData["Error"] = $"No se encontró el tramo #{id}.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeshacerUltimo()
    {
        Tramo? tramo = _tramoService.DeshacerUltimoRegistro();
        if (tramo == null)
        {
            TempData["Error"] = "No hay registros activos para deshacer.";
        }
        else
        {
            _persistenciaService.GuardarTramos(_tramoService);
            TempData["Mensaje"] = $"Se deshizo el último registro: tramo #{tramo.Id}.";
        }

        return RedirectToAction(nameof(Index));
    }

    private GestionPistasWeb.DataStructures.ListaSimple<Tramo> CrearListaUnTramo(Tramo tramo)
    {
        GestionPistasWeb.DataStructures.ListaSimple<Tramo> lista = new GestionPistasWeb.DataStructures.ListaSimple<Tramo>();
        lista.AgregarAlFinal(tramo);
        return lista;
    }

    private void ValidarTramo(Tramo tramo)
    {
        if (string.IsNullOrWhiteSpace(tramo.DireccionReferencia))
            ModelState.AddModelError(nameof(tramo.DireccionReferencia), "La dirección o referencia es obligatoria.");
        if (string.IsNullOrWhiteSpace(tramo.Descripcion))
            ModelState.AddModelError(nameof(tramo.Descripcion), "La descripción es obligatoria.");
        if (string.IsNullOrWhiteSpace(tramo.ReportadoPor))
            ModelState.AddModelError(nameof(tramo.ReportadoPor), "El nombre de quien reporta es obligatorio.");
        if (tramo.LatInicio == 0 || tramo.LngInicio == 0 || tramo.LatFin == 0 || tramo.LngFin == 0)
        {
            ModelState.AddModelError(nameof(tramo.LatInicio), "Selecciona el punto de inicio y fin del tramo en el mapa.");
            return;
        }

        if (!EstaDentroDelCentro(tramo.LatInicio, tramo.LngInicio) || !EstaDentroDelCentro(tramo.LatFin, tramo.LngFin))
            ModelState.AddModelError(nameof(tramo.LatInicio), "Solo se pueden registrar tramos dentro del Centro Histórico de Trujillo.");

        if (string.IsNullOrWhiteSpace(tramo.RutaCoordenadas))
            ModelState.AddModelError(nameof(tramo.LatInicio), "El tramo debe validarse sobre una pista antes de registrar.");
    }

    private bool EstaDentroDelCentro(double latitud, double longitud)
    {
        bool dentro = false;

        RevisarBorde(latitud, longitud, -8.10600, -79.03415, -8.10445, -79.03265, ref dentro);
        RevisarBorde(latitud, longitud, -8.10445, -79.03265, -8.10395, -79.03055, ref dentro);
        RevisarBorde(latitud, longitud, -8.10395, -79.03055, -8.10420, -79.02825, ref dentro);
        RevisarBorde(latitud, longitud, -8.10420, -79.02825, -8.10505, -79.02580, ref dentro);
        RevisarBorde(latitud, longitud, -8.10505, -79.02580, -8.10645, -79.02360, ref dentro);
        RevisarBorde(latitud, longitud, -8.10645, -79.02360, -8.10850, -79.02165, ref dentro);
        RevisarBorde(latitud, longitud, -8.10850, -79.02165, -8.11075, -79.02085, ref dentro);
        RevisarBorde(latitud, longitud, -8.11075, -79.02085, -8.11315, -79.02145, ref dentro);
        RevisarBorde(latitud, longitud, -8.11315, -79.02145, -8.11505, -79.02335, ref dentro);
        RevisarBorde(latitud, longitud, -8.11505, -79.02335, -8.11625, -79.02585, ref dentro);
        RevisarBorde(latitud, longitud, -8.11625, -79.02585, -8.11665, -79.02865, ref dentro);
        RevisarBorde(latitud, longitud, -8.11665, -79.02865, -8.11605, -79.03120, ref dentro);
        RevisarBorde(latitud, longitud, -8.11605, -79.03120, -8.11445, -79.03320, ref dentro);
        RevisarBorde(latitud, longitud, -8.11445, -79.03320, -8.11225, -79.03435, ref dentro);
        RevisarBorde(latitud, longitud, -8.11225, -79.03435, -8.10985, -79.03475, ref dentro);
        RevisarBorde(latitud, longitud, -8.10985, -79.03475, -8.10765, -79.03455, ref dentro);
        RevisarBorde(latitud, longitud, -8.10765, -79.03455, -8.10600, -79.03415, ref dentro);

        return dentro;
    }

    private void RevisarBorde(double latitud, double longitud, double latA, double lngA, double latB, double lngB, ref bool dentro)
    {
        bool cruzaAltura = (latA > latitud) != (latB > latitud);
        if (!cruzaAltura) return;

        double lngCruce = (lngB - lngA) * (latitud - latA) / (latB - latA) + lngA;
        if (longitud < lngCruce)
            dentro = !dentro;
    }

    private void ValidarEvidencia(IFormFile? evidencia)
    {
        if (evidencia == null || evidencia.Length == 0)
            return;

        string extension = Path.GetExtension(evidencia.FileName).ToLowerInvariant();
        if (!ExtensionPermitida(extension))
            ModelState.AddModelError("evidencia", "La evidencia debe ser una imagen JPG, JPEG, PNG o WEBP.");

        if (evidencia.Length > 5 * 1024 * 1024)
            ModelState.AddModelError("evidencia", "La imagen no debe superar los 5 MB.");

        if (!string.IsNullOrWhiteSpace(evidencia.ContentType) && !evidencia.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            ModelState.AddModelError("evidencia", "El archivo seleccionado no parece ser una imagen válida.");
    }

    private bool ExtensionPermitida(string extension)
    {
        return extension == ".jpg" ||
               extension == ".jpeg" ||
               extension == ".png" ||
               extension == ".webp";
    }

    private (string rutaPublica, string nombreOriginal) GuardarEvidencia(int tramoId, IFormFile evidencia)
    {
        string uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
        if (!Directory.Exists(uploadsDir))
            Directory.CreateDirectory(uploadsDir);

        string extension = Path.GetExtension(evidencia.FileName).ToLowerInvariant();
        string fileName = $"tramo_{tramoId}_{Guid.NewGuid():N}{extension}";
        string filePath = Path.Combine(uploadsDir, fileName);

        using (FileStream stream = new FileStream(filePath, FileMode.Create))
        {
            evidencia.CopyTo(stream);
        }

        return ($"/uploads/{fileName}", Path.GetFileName(evidencia.FileName));
    }
}
