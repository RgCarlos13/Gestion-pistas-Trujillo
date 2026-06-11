using Microsoft.AspNetCore.Mvc;
using GestionPistasWeb.Models;
using GestionPistasWeb.Services;

namespace GestionPistasWeb.Controllers;

public class ReportesController : Controller
{
    private readonly ReporteService _reporteService;

    public ReportesController(ReporteService reporteService)
    {
        _reporteService = reporteService;
    }

    public IActionResult Index(int? buscarId, string? estadoCondicion)
    {
        EstadoTramo? filtroEstado = null;
        if (!string.IsNullOrWhiteSpace(estadoCondicion))
        {
            EstadoTramo estado;
            if (Enum.TryParse<EstadoTramo>(estadoCondicion, out estado))
                filtroEstado = estado;
        }

        ReporteViewModel reporte = _reporteService.GenerarReporte(buscarId, filtroEstado, estadoCondicion);
        return View(reporte);
    }
}
