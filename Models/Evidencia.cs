namespace GestionPistasWeb.Models;

public class Evidencia
{
    public int Id { get; set; }
    public int TramoId { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaSubida { get; set; }

    public Evidencia()
    {
        FechaSubida = DateTime.Now;
    }
}
