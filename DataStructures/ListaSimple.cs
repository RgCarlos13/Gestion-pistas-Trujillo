namespace GestionPistasWeb.DataStructures;

// Lista simplemente enlazada propia basada en nodos.
public class ListaSimple<T>
{
    private NodoSimple<T>? cabeza;
    private int count;

    public ListaSimple()
    {
        cabeza = null;
        count = 0;
    }

    public void AgregarAlFinal(T data)
    {
        NodoSimple<T> nuevo = new NodoSimple<T>(data);
        if (cabeza == null)
        {
            cabeza = nuevo;
        }
        else
        {
            NodoSimple<T> actual = cabeza;
            while (actual.Siguiente != null)
            {
                actual = actual.Siguiente;
            }
            actual.Siguiente = nuevo;
        }
        count++;
    }

    public void AgregarAlInicio(T data)
    {
        NodoSimple<T> nuevo = new NodoSimple<T>(data);
        nuevo.Siguiente = cabeza;
        cabeza = nuevo;
        count++;
    }

    public NodoSimple<T>? ObtenerCabeza()
    {
        return cabeza;
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
