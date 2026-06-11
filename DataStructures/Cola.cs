namespace GestionPistasWeb.DataStructures;

// Cola propia basada en nodos. El primer elemento que entra es el primero que sale.
public class Cola<T>
{
    private NodoSimple<T>? frente;
    private NodoSimple<T>? final;
    private int count;

    public Cola()
    {
        frente = null;
        final = null;
        count = 0;
    }

    public void Encolar(T data)
    {
        NodoSimple<T> nuevo = new NodoSimple<T>(data);
        if (final == null)
        {
            frente = nuevo;
            final = nuevo;
        }
        else
        {
            final.Siguiente = nuevo;
            final = nuevo;
        }
        count++;
    }

    public T? Desencolar()
    {
        if (frente == null)
        {
            return default;
        }

        T data = frente.Data;
        frente = frente.Siguiente;
        if (frente == null)
        {
            final = null;
        }
        count--;
        return data;
    }

    public T? VerPrimero()
    {
        if (frente == null)
        {
            return default;
        }
        return frente.Data;
    }

    public NodoSimple<T>? ObtenerFrente()
    {
        return frente;
    }

    public bool EstaVacia()
    {
        return frente == null;
    }

    public int Contar()
    {
        return count;
    }
}
