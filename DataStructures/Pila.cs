namespace GestionPistasWeb.DataStructures;

// Pila propia basada en nodos. El ultimo elemento que entra es el primero que sale.
public class Pila<T>
{
    private NodoSimple<T>? cima;
    private int count;

    public Pila()
    {
        cima = null;
        count = 0;
    }

    public void Apilar(T data)
    {
        NodoSimple<T> nuevo = new NodoSimple<T>(data);
        nuevo.Siguiente = cima;
        cima = nuevo;
        count++;
    }

    public T? Desapilar()
    {
        if (cima == null)
        {
            return default;
        }

        T data = cima.Data;
        cima = cima.Siguiente;
        count--;
        return data;
    }

    public T? VerCima()
    {
        if (cima == null)
        {
            return default;
        }
        return cima.Data;
    }

    public NodoSimple<T>? ObtenerCima()
    {
        return cima;
    }

    public bool EstaVacia()
    {
        return cima == null;
    }

    public int Contar()
    {
        return count;
    }
}
