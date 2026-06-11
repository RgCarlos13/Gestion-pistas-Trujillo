namespace GestionPistasWeb.DataStructures;

public class NodoSimple<T>
{
    public T Data { get; set; }
    public NodoSimple<T>? Siguiente { get; set; }

    public NodoSimple(T data)
    {
        Data = data;
        Siguiente = null;
    }
}
