namespace GestionPistasWeb.DataStructures;

public class NodoDoble<T>
{
    public T Data { get; set; }
    public NodoDoble<T>? Anterior { get; set; }
    public NodoDoble<T>? Siguiente { get; set; }

    public NodoDoble(T data)
    {
        Data = data;
        Anterior = null;
        Siguiente = null;
    }
}
