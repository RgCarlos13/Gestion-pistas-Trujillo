using System.Text.Json.Serialization;
using GestionPistasWeb.DataStructures;

namespace GestionPistasWeb.Models;

public enum EstadoTramo
{
    Bueno,
    Regular,
    Malo,
    EnReparacion,
    Intransitable
}

public class Tramo
{
    public int Id { get; set; }
    public string ZonaTrabajo { get; set; } = string.Empty;
    public string Zona { get; set; } = string.Empty;
    public string DireccionReferencia { get; set; } = string.Empty;
    public string TipoVia { get; set; } = string.Empty;
    public EstadoTramo Estado { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string ReportadoPor { get; set; } = string.Empty;

    // Coordenadas del tramo seleccionado en el mapa Leaflet.
    public double LatInicio { get; set; }
    public double LngInicio { get; set; }
    public double LatFin { get; set; }
    public double LngFin { get; set; }

    // Ruta en formato JSON generada por el mapa para que el tramo siga la pista.
    public string RutaCoordenadas { get; set; } = string.Empty;

    // Evidencia fotográfica guardada dentro de wwwroot/uploads.
    public string RutaEvidencia { get; set; } = string.Empty;
    public string NombreEvidencia { get; set; } = string.Empty;

    public DateTime FechaRegistro { get; set; }
    public DateTime FechaActualizacion { get; set; }
    public bool Activo { get; set; } = true;

    [JsonIgnore]
    public ListaDoble<string> HistorialCambios { get; set; } = new ListaDoble<string>();

    [JsonIgnore]
    public bool TieneCoordenadas
    {
        get
        {
            return LatInicio != 0 && LngInicio != 0 && LatFin != 0 && LngFin != 0;
        }
    }

    public Tramo()
    {
        Id = 0;
        FechaRegistro = DateTime.Now;
        FechaActualizacion = DateTime.Now;
    }
}
