using System.Text;
using System.Text.Json;
using GestionPistasWeb.DataStructures;
using GestionPistasWeb.Models;

namespace GestionPistasWeb.Services;

public class TramoService
{
    private readonly ListaSimple<Tramo> _tramos;
    private readonly ArbolBinarioBusqueda<Tramo> _arbolTramos;
    private readonly Cola<Tramo> _colaOrdenRegistro;
    private readonly Pila<string> _pilaAcciones;
    private readonly Pila<int> _pilaRegistros;
    private readonly ILogger<TramoService> _logger;

    public int UltimoId { get; set; } = 0;

    public TramoService(ILogger<TramoService> logger)
    {
        _tramos = new ListaSimple<Tramo>();
        _arbolTramos = new ArbolBinarioBusqueda<Tramo>();
        _colaOrdenRegistro = new Cola<Tramo>();
        _pilaAcciones = new Pila<string>();
        _pilaRegistros = new Pila<int>();
        _logger = logger;
        _logger.LogInformation("TramoService inicializado.");
    }

    public Tramo RegistrarTramo(Tramo tramo)
    {
        UltimoId++;
        tramo.Id = UltimoId;
        tramo.FechaRegistro = DateTime.Now;
        tramo.FechaActualizacion = DateTime.Now;
        tramo.Activo = true;
        tramo.HistorialCambios.AgregarAlFinal($"REGISTRO: Tramo creado el {tramo.FechaRegistro:dd/MM/yyyy HH:mm}");

        _tramos.AgregarAlFinal(tramo);
        _arbolTramos.Insertar(tramo.Id, tramo);
        _pilaRegistros.Apilar(tramo.Id);
        _colaOrdenRegistro.Encolar(tramo);

        string accion = $"Se registró el tramo #{tramo.Id} en {tramo.DireccionReferencia} con estado {tramo.Estado}.";
        _pilaAcciones.Apilar(accion);
        _logger.LogInformation("Tramo registrado: #{Id} - {Direccion}", tramo.Id, tramo.DireccionReferencia);
        return tramo;
    }

    public void AgregarTramoDesdePersistencia(Tramo tramo)
    {
        if (tramo.HistorialCambios == null)
        {
            tramo.HistorialCambios = new ListaDoble<string>();
        }

        if (tramo.HistorialCambios.EstaVacia())
        {
            tramo.HistorialCambios.AgregarAlFinal($"CARGA: Tramo recuperado desde archivo el {DateTime.Now:dd/MM/yyyy HH:mm}");
        }

        _tramos.AgregarAlFinal(tramo);

        if (tramo.Activo)
        {
            _arbolTramos.Insertar(tramo.Id, tramo);
            _pilaRegistros.Apilar(tramo.Id);
            _colaOrdenRegistro.Encolar(tramo);
        }
    }

    public void ApilarAccion(string accion)
    {
        _pilaAcciones.Apilar(accion);
    }

    public ListaSimple<Tramo> ObtenerTramos()
    {
        return _tramos;
    }

    public Tramo? BuscarPorId(int id)
    {
        return _arbolTramos.Buscar(id);
    }

    public ListaSimple<Tramo> Filtrar(EstadoTramo? estado)
    {
        ListaSimple<Tramo> resultado = new ListaSimple<Tramo>();
        NodoSimple<Tramo>? actual = _tramos.ObtenerCabeza();

        while (actual != null)
        {
            Tramo tramo = actual.Data;
            if (tramo.Activo)
            {
                if (!estado.HasValue || tramo.Estado == estado.Value)
                {
                    resultado.AgregarAlFinal(tramo);
                }
            }
            actual = actual.Siguiente;
        }

        return resultado;
    }

    public ListaSimple<Tramo> ObtenerTramosConCoordenadas()
    {
        ListaSimple<Tramo> resultado = new ListaSimple<Tramo>();
        NodoSimple<Tramo>? actual = _tramos.ObtenerCabeza();

        while (actual != null)
        {
            Tramo tramo = actual.Data;
            if (tramo.Activo && tramo.TieneCoordenadas)
            {
                resultado.AgregarAlFinal(tramo);
            }
            actual = actual.Siguiente;
        }

        return resultado;
    }

    public string ConstruirMapaJson(ListaSimple<Tramo> tramos)
    {
        StringBuilder sb = new StringBuilder();
        bool primero = true;
        sb.Append('[');

        NodoSimple<Tramo>? actual = tramos.ObtenerCabeza();
        while (actual != null)
        {
            Tramo tramo = actual.Data;
            if (tramo.TieneCoordenadas)
            {
                if (!primero)
                {
                    sb.Append(',');
                }
                primero = false;

                sb.Append('{');
                AgregarPropiedadNumero(sb, "id", tramo.Id, true);
                AgregarPropiedadTexto(sb, "zonaTrabajo", tramo.ZonaTrabajo);
                AgregarPropiedadTexto(sb, "zona", tramo.Zona);
                AgregarPropiedadTexto(sb, "direccionReferencia", tramo.DireccionReferencia);
                AgregarPropiedadTexto(sb, "tipoVia", tramo.TipoVia);
                AgregarPropiedadTexto(sb, "estado", tramo.Estado.ToString());
                AgregarPropiedadTexto(sb, "descripcion", tramo.Descripcion);
                AgregarPropiedadTexto(sb, "reportadoPor", tramo.ReportadoPor);
                AgregarPropiedadNumero(sb, "latInicio", tramo.LatInicio);
                AgregarPropiedadNumero(sb, "lngInicio", tramo.LngInicio);
                AgregarPropiedadNumero(sb, "latFin", tramo.LatFin);
                AgregarPropiedadNumero(sb, "lngFin", tramo.LngFin);
                AgregarPropiedadTexto(sb, "rutaCoordenadas", tramo.RutaCoordenadas);
                AgregarPropiedadTexto(sb, "rutaEvidencia", tramo.RutaEvidencia);
                sb.Append('}');
            }
            actual = actual.Siguiente;
        }

        sb.Append(']');
        return sb.ToString();
    }

    private static void AgregarPropiedadTexto(StringBuilder sb, string nombre, string valor)
    {
        sb.Append(',');
        sb.Append(JsonSerializer.Serialize(nombre));
        sb.Append(':');
        sb.Append(JsonSerializer.Serialize(valor ?? string.Empty));
    }

    private static void AgregarPropiedadNumero(StringBuilder sb, string nombre, double valor, bool primera)
    {
        if (!primera)
        {
            sb.Append(',');
        }
        sb.Append(JsonSerializer.Serialize(nombre));
        sb.Append(':');
        sb.Append(valor.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    private static void AgregarPropiedadNumero(StringBuilder sb, string nombre, double valor)
    {
        AgregarPropiedadNumero(sb, nombre, valor, false);
    }

    public bool CambiarEstado(int id, EstadoTramo nuevoEstado)
    {
        Tramo? tramo = _arbolTramos.Buscar(id);
        if (tramo == null)
        {
            return false;
        }

        EstadoTramo estadoAnterior = tramo.Estado;
        tramo.Estado = nuevoEstado;
        tramo.FechaActualizacion = DateTime.Now;

        string cambio = $"Se cambió el estado del tramo #{id} de {estadoAnterior} a {nuevoEstado}.";
        tramo.HistorialCambios.AgregarAlFinal(cambio);
        _pilaAcciones.Apilar(cambio);
        _logger.LogInformation("Estado cambiado: {Cambio}", cambio);

        return true;
    }

    public bool DesactivarTramo(int id)
    {
        Tramo? tramo = _arbolTramos.Buscar(id);
        if (tramo == null || !tramo.Activo)
        {
            return false;
        }

        tramo.Activo = false;
        tramo.FechaActualizacion = DateTime.Now;
        _arbolTramos.Eliminar(id);

        string accion = $"Se eliminó el tramo #{id} de los registros activos.";
        tramo.HistorialCambios.AgregarAlFinal(accion);
        _pilaAcciones.Apilar(accion);
        _logger.LogInformation("Tramo {Id} desactivado.", id);
        return true;
    }

    public Tramo? DeshacerUltimoRegistro()
    {
        while (!_pilaRegistros.EstaVacia())
        {
            int id = _pilaRegistros.Desapilar();
            Tramo? tramo = _arbolTramos.Buscar(id);
            if (tramo == null || !tramo.Activo)
            {
                continue;
            }

            tramo.Activo = false;
            tramo.FechaActualizacion = DateTime.Now;
            _arbolTramos.Eliminar(tramo.Id);
            string accion = $"Se deshizo el último registro: tramo #{tramo.Id}.";
            tramo.HistorialCambios.AgregarAlFinal(accion);
            _pilaAcciones.Apilar(accion);
            _logger.LogInformation("Último registro deshecho: {Id}", tramo.Id);
            return tramo;
        }
        return null;
    }

    public Cola<Tramo> ObtenerColaOrdenRegistro()
    {
        return _colaOrdenRegistro;
    }

    public Pila<string> ObtenerPilaAcciones()
    {
        return _pilaAcciones;
    }

    public ListaSimple<Tramo> ObtenerTramosOrdenadosPorId()
    {
        ListaSimple<Tramo> resultado = new ListaSimple<Tramo>();
        ListaSimple<Tramo> ordenados = _arbolTramos.RecorridoInOrden();
        NodoSimple<Tramo>? actual = ordenados.ObtenerCabeza();

        while (actual != null)
        {
            Tramo tramo = actual.Data;
            if (tramo.Activo)
            {
                resultado.AgregarAlFinal(tramo);
            }
            actual = actual.Siguiente;
        }

        return resultado;
    }

    public ListaSimple<Tramo> BuscarPorEstadoDesdeArbol(EstadoTramo? estado)
    {
        ListaSimple<Tramo> resultado = new ListaSimple<Tramo>();
        ListaSimple<Tramo> ordenados = _arbolTramos.RecorridoInOrden();
        NodoSimple<Tramo>? actual = ordenados.ObtenerCabeza();

        while (actual != null)
        {
            Tramo tramo = actual.Data;
            if (tramo.Activo)
            {
                if (!estado.HasValue || tramo.Estado == estado.Value)
                {
                    resultado.AgregarAlFinal(tramo);
                }
            }
            actual = actual.Siguiente;
        }

        return resultado;
    }

    public ListaSimple<string> ObtenerAcciones()
    {
        ListaSimple<string> acciones = new ListaSimple<string>();
        NodoSimple<string>? actual = _pilaAcciones.ObtenerCima();

        while (actual != null)
        {
            acciones.AgregarAlFinal(actual.Data);
            actual = actual.Siguiente;
        }

        return acciones;
    }

    public int ContarTramos()
    {
        int count = 0;
        NodoSimple<Tramo>? actual = _tramos.ObtenerCabeza();

        while (actual != null)
        {
            if (actual.Data.Activo)
            {
                count++;
            }
            actual = actual.Siguiente;
        }

        return count;
    }

    public int ContarPorEstado(EstadoTramo estado)
    {
        int count = 0;
        NodoSimple<Tramo>? actual = _tramos.ObtenerCabeza();

        while (actual != null)
        {
            Tramo tramo = actual.Data;
            if (tramo.Activo && tramo.Estado == estado)
            {
                count++;
            }
            actual = actual.Siguiente;
        }

        return count;
    }
}
