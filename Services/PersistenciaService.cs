using System.Text.Json;
using GestionPistasWeb.DataStructures;
using GestionPistasWeb.Models;

namespace GestionPistasWeb.Services;

public class PersistenciaService
{
    private readonly string _rutaArchivo;
    private readonly ILogger<PersistenciaService> _logger;

    public PersistenciaService(IWebHostEnvironment env, ILogger<PersistenciaService> logger)
    {
        _logger = logger;
        string dataDir = Path.Combine(env.ContentRootPath, "App_Data");
        if (!Directory.Exists(dataDir))
        {
            Directory.CreateDirectory(dataDir);
        }
        _rutaArchivo = Path.Combine(dataDir, "tramos.json");
    }

    public void GuardarTramos(TramoService tramoService)
    {
        try
        {
            DatosPersistencia datos = new DatosPersistencia()
            {
                UltimoId = tramoService.UltimoId
            };

            NodoTramoPersistencia? ultimoTramo = null;
            NodoSimple<Tramo>? actualTramo = tramoService.ObtenerTramos().ObtenerCabeza();

            while (actualTramo != null)
            {
                NodoTramoPersistencia nuevo = new NodoTramoPersistencia()
                {
                    Data = actualTramo.Data
                };

                if (datos.PrimerTramo == null)
                {
                    datos.PrimerTramo = nuevo;
                }
                else if (ultimoTramo != null)
                {
                    ultimoTramo.Siguiente = nuevo;
                }

                ultimoTramo = nuevo;
                datos.TotalTramos++;
                actualTramo = actualTramo.Siguiente;
            }

            NodoAccionPersistencia? ultimaAccion = null;
            NodoSimple<string>? actualAccion = tramoService.ObtenerAcciones().ObtenerCabeza();

            while (actualAccion != null)
            {
                NodoAccionPersistencia nuevo = new NodoAccionPersistencia()
                {
                    Data = actualAccion.Data
                };

                if (datos.PrimeraAccion == null)
                {
                    datos.PrimeraAccion = nuevo;
                }
                else if (ultimaAccion != null)
                {
                    ultimaAccion.Siguiente = nuevo;
                }

                ultimaAccion = nuevo;
                datos.TotalAcciones++;
                actualAccion = actualAccion.Siguiente;
            }

            JsonSerializerOptions opciones = new JsonSerializerOptions()
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(datos, opciones);
            File.WriteAllText(_rutaArchivo, json);
            _logger.LogInformation("Datos guardados correctamente en {Archivo}", _rutaArchivo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar datos en {Archivo}", _rutaArchivo);
        }
    }

    public void CargarTramos(TramoService tramoService)
    {
        try
        {
            if (!File.Exists(_rutaArchivo))
            {
                _logger.LogInformation("No se encontró archivo de datos. El sistema iniciará sin registros de prueba.");
                GuardarTramos(tramoService);
                return;
            }

            string json = File.ReadAllText(_rutaArchivo);
            if (string.IsNullOrWhiteSpace(json))
            {
                _logger.LogInformation("Archivo de datos vacío. El sistema iniciará sin registros.");
                return;
            }

            DatosPersistencia? datos = JsonSerializer.Deserialize<DatosPersistencia>(json);
            if (datos == null || datos.PrimerTramo == null)
            {
                _logger.LogInformation("No hay tramos guardados. El sistema iniciará limpio.");
                return;
            }

            tramoService.UltimoId = datos.UltimoId;

            NodoTramoPersistencia? actualTramo = datos.PrimerTramo;
            while (actualTramo != null)
            {
                if (actualTramo.Data != null)
                {
                    tramoService.AgregarTramoDesdePersistencia(actualTramo.Data);
                }

                actualTramo = actualTramo.Siguiente;
            }

            CargarAccionesDesdeNodo(datos.PrimeraAccion, tramoService);

            _logger.LogInformation("Datos cargados correctamente. {Cantidad} tramos recuperados.", datos.TotalTramos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar datos. El sistema iniciará limpio para evitar datos de prueba.");
        }
    }

    private void CargarAccionesDesdeNodo(NodoAccionPersistencia? nodo, TramoService tramoService)
    {
        if (nodo == null)
        {
            return;
        }

        CargarAccionesDesdeNodo(nodo.Siguiente, tramoService);
        if (!string.IsNullOrWhiteSpace(nodo.Data))
        {
            tramoService.ApilarAccion(nodo.Data);
        }
    }
}

public class DatosPersistencia
{
    public NodoTramoPersistencia? PrimerTramo { get; set; }
    public int UltimoId { get; set; }
    public int TotalTramos { get; set; }
    public NodoAccionPersistencia? PrimeraAccion { get; set; }
    public int TotalAcciones { get; set; }
}

public class NodoTramoPersistencia
{
    public Tramo? Data { get; set; }
    public NodoTramoPersistencia? Siguiente { get; set; }
}

public class NodoAccionPersistencia
{
    public string Data { get; set; } = string.Empty;
    public NodoAccionPersistencia? Siguiente { get; set; }
}
