namespace GestionPistasWeb.DataStructures;

public class NodoArbol<T>
{
    public T Data { get; set; }
    public int Key { get; set; }
    public NodoArbol<T>? Izquierdo { get; set; }
    public NodoArbol<T>? Derecho { get; set; }

    public NodoArbol(int key, T data)
    {
        Key = key;
        Data = data;
        Izquierdo = null;
        Derecho = null;
    }
}
