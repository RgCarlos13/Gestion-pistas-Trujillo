namespace GestionPistasWeb.DataStructures;

// Lista doblemente enlazada propia. Permite recorrer hacia adelante y hacia atras usando nodos.
public class ListaDoble<T>
{
    private NodoDoble<T>? cabeza;
    private NodoDoble<T>? cola;
    private int count;

    public ListaDoble()
    {
        cabeza = null;
        cola = null;
        count = 0;
    }

    public void AgregarAlFinal(T data)
    {
        NodoDoble<T> nuevo = new NodoDoble<T>(data);
        if (cola == null)
        {
            cabeza = nuevo;
            cola = nuevo;
        }
        else
        {
            cola.Siguiente = nuevo;
            nuevo.Anterior = cola;
            cola = nuevo;
        }
        count++;
    }

    public void AgregarAlInicio(T data)
    {
        NodoDoble<T> nuevo = new NodoDoble<T>(data);
        if (cabeza == null)
        {
            cabeza = nuevo;
            cola = nuevo;
        }
        else
        {
            nuevo.Siguiente = cabeza;
            cabeza.Anterior = nuevo;
            cabeza = nuevo;
        }
        count++;
    }

    public ListaSimple<T> RecorrerAdelante()
    {
        ListaSimple<T> resultado = new ListaSimple<T>();
        NodoDoble<T>? actual = cabeza;
        while (actual != null)
        {
            resultado.AgregarAlFinal(actual.Data);
            actual = actual.Siguiente;
        }
        return resultado;
    }

    public ListaSimple<T> RecorrerAtras()
    {
        ListaSimple<T> resultado = new ListaSimple<T>();
        NodoDoble<T>? actual = cola;
        while (actual != null)
        {
            resultado.AgregarAlFinal(actual.Data);
            actual = actual.Anterior;
        }
        return resultado;
    }

    public NodoDoble<T>? ObtenerCabeza()
    {
        return cabeza;
    }

    public NodoDoble<T>? ObtenerCola()
    {
        return cola;
    }

    public int Contar()
    {
        return count;
    }

    public bool EstaVacia()
    {
        return cabeza == null;
    }
}
