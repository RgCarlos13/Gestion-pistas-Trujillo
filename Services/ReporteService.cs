using GestionPistasWeb.DataStructures;
using GestionPistasWeb.Models;

namespace GestionPistasWeb.Services;

public class ReporteService
{
    private readonly TramoService _tramoService;

    public ReporteService(TramoService tramoService)
    {
        _tramoService = tramoService;
    }

    public ReporteViewModel GenerarReporte(int? buscarId, EstadoTramo? estadoCondicion, string? estadoCondicionTexto)
    {
        ReporteViewModel reporte = new ReporteViewModel()
        {
            TotalTramos = _tramoService.ContarTramos(),
            TramosPorEstado = new EstadisticaEstados()
            {
                Bueno = _tramoService.ContarPorEstado(EstadoTramo.Bueno),
                Regular = _tramoService.ContarPorEstado(EstadoTramo.Regular),
                Malo = _tramoService.ContarPorEstado(EstadoTramo.Malo),
                EnReparacion = _tramoService.ContarPorEstado(EstadoTramo.EnReparacion),
                Intransitable = _tramoService.ContarPorEstado(EstadoTramo.Intransitable)
            },
            UltimasAcciones = new ListaSimple<string>(),
            TramosOrdenadosPorId = _tramoService.ObtenerTramosOrdenadosPorId(),
            ResultadosCondicion = _tramoService.BuscarPorEstadoDesdeArbol(estadoCondicion),
            EstadoCondicionSeleccionado = estadoCondicionTexto ?? string.Empty
        };

        if (buscarId.HasValue)
        {
            reporte.IdBuscado = buscarId.Value;
            reporte.ResultadoBusquedaId = _tramoService.BuscarPorId(buscarId.Value);
            reporte.MensajeBusquedaId = reporte.ResultadoBusquedaId == null
                ? $"No se encontró un registro activo con el código #{buscarId.Value}."
                : $"Registro #{buscarId.Value} encontrado.";
        }

        int limiteAcciones = 10;
        NodoSimple<string>? accionActual = _tramoService.ObtenerPilaAcciones().ObtenerCima();

        while (accionActual != null && limiteAcciones > 0)
        {
            reporte.UltimasAcciones.AgregarAlFinal(accionActual.Data);
            limiteAcciones--;
            accionActual = accionActual.Siguiente;
        }

        return reporte;
    }
}

public class ReporteViewModel
{
    public int TotalTramos { get; set; }
    public EstadisticaEstados TramosPorEstado { get; set; } = new EstadisticaEstados();
    public int? IdBuscado { get; set; }
    public Tramo? ResultadoBusquedaId { get; set; }
    public string MensajeBusquedaId { get; set; } = string.Empty;
    public string EstadoCondicionSeleccionado { get; set; } = string.Empty;
    public ListaSimple<Tramo> ResultadosCondicion { get; set; } = new ListaSimple<Tramo>();
    public ListaSimple<Tramo> TramosOrdenadosPorId { get; set; } = new ListaSimple<Tramo>();
    public ListaSimple<string> UltimasAcciones { get; set; } = new ListaSimple<string>();
}

public class EstadisticaEstados
{
    public int Bueno { get; set; }
    public int Regular { get; set; }
    public int Malo { get; set; }
    public int EnReparacion { get; set; }
    public int Intransitable { get; set; }
}
