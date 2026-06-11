using GestionPistasWeb.DataStructures;
using GestionPistasWeb.Models;

namespace GestionPistasWeb.ViewModels;

public class TramosIndexViewModel
{
    public ListaSimple<Tramo> Tramos { get; set; } = new ListaSimple<Tramo>();
    public int TotalTramos { get; set; }
    public string TramosMapaJson { get; set; } = "null";
    public string? EstadoSeleccionado { get; set; }
    public string ZonaTrabajo { get; set; } = "Centro Histórico de Trujillo";
}
