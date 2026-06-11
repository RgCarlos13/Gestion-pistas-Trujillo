namespace GestionPistasWeb.DataStructures;

// Arbol binario de busqueda propio. Guarda datos indexados por una clave entera.
public class ArbolBinarioBusqueda<T>
{
    private NodoArbol<T>? raiz;
    private int count;

    public ArbolBinarioBusqueda()
    {
        raiz = null;
        count = 0;
    }

    public void Insertar(int key, T data)
    {
        bool nuevoNodo = false;
        raiz = InsertarRec(raiz, key, data, ref nuevoNodo);
        if (nuevoNodo)
        {
            count++;
        }
    }

    private NodoArbol<T> InsertarRec(NodoArbol<T>? nodo, int key, T data, ref bool nuevoNodo)
    {
        if (nodo == null)
        {
            nuevoNodo = true;
            return new NodoArbol<T>(key, data);
        }

        if (key < nodo.Key)
        {
            nodo.Izquierdo = InsertarRec(nodo.Izquierdo, key, data, ref nuevoNodo);
        }
        else if (key > nodo.Key)
        {
            nodo.Derecho = InsertarRec(nodo.Derecho, key, data, ref nuevoNodo);
        }
        else
        {
            nodo.Data = data;
        }

        return nodo;
    }

    public T? Buscar(int key)
    {
        return BuscarRec(raiz, key);
    }

    private T? BuscarRec(NodoArbol<T>? nodo, int key)
    {
        if (nodo == null)
        {
            return default;
        }

        if (key == nodo.Key)
        {
            return nodo.Data;
        }

        if (key < nodo.Key)
        {
            return BuscarRec(nodo.Izquierdo, key);
        }

        return BuscarRec(nodo.Derecho, key);
    }

    public ListaSimple<T> RecorridoInOrden()
    {
        ListaSimple<T> resultado = new ListaSimple<T>();
        InOrdenRec(raiz, resultado);
        return resultado;
    }

    private void InOrdenRec(NodoArbol<T>? nodo, ListaSimple<T> resultado)
    {
        if (nodo == null)
        {
            return;
        }

        InOrdenRec(nodo.Izquierdo, resultado);
        resultado.AgregarAlFinal(nodo.Data);
        InOrdenRec(nodo.Derecho, resultado);
    }

    public ListaSimple<T> RecorridoPreOrden()
    {
        ListaSimple<T> resultado = new ListaSimple<T>();
        PreOrdenRec(raiz, resultado);
        return resultado;
    }

    private void PreOrdenRec(NodoArbol<T>? nodo, ListaSimple<T> resultado)
    {
        if (nodo == null)
        {
            return;
        }

        resultado.AgregarAlFinal(nodo.Data);
        PreOrdenRec(nodo.Izquierdo, resultado);
        PreOrdenRec(nodo.Derecho, resultado);
    }

    public ListaSimple<T> RecorridoPostOrden()
    {
        ListaSimple<T> resultado = new ListaSimple<T>();
        PostOrdenRec(raiz, resultado);
        return resultado;
    }

    private void PostOrdenRec(NodoArbol<T>? nodo, ListaSimple<T> resultado)
    {
        if (nodo == null)
        {
            return;
        }

        PostOrdenRec(nodo.Izquierdo, resultado);
        PostOrdenRec(nodo.Derecho, resultado);
        resultado.AgregarAlFinal(nodo.Data);
    }

    public bool Eliminar(int key)
    {
        bool eliminado = false;
        raiz = EliminarRec(raiz, key, ref eliminado);
        if (eliminado)
        {
            count--;
        }
        return eliminado;
    }

    private NodoArbol<T>? EliminarRec(NodoArbol<T>? nodo, int key, ref bool eliminado)
    {
        if (nodo == null)
        {
            return null;
        }

        if (key < nodo.Key)
        {
            nodo.Izquierdo = EliminarRec(nodo.Izquierdo, key, ref eliminado);
            return nodo;
        }

        if (key > nodo.Key)
        {
            nodo.Derecho = EliminarRec(nodo.Derecho, key, ref eliminado);
            return nodo;
        }

        eliminado = true;

        if (nodo.Izquierdo == null)
        {
            return nodo.Derecho;
        }

        if (nodo.Derecho == null)
        {
            return nodo.Izquierdo;
        }

        NodoArbol<T> sucesor = ObtenerMenor(nodo.Derecho);
        nodo.Key = sucesor.Key;
        nodo.Data = sucesor.Data;

        bool reemplazoEliminado = false;
        nodo.Derecho = EliminarRec(nodo.Derecho, sucesor.Key, ref reemplazoEliminado);
        return nodo;
    }

    private NodoArbol<T> ObtenerMenor(NodoArbol<T> nodo)
    {
        NodoArbol<T> actual = nodo;
        while (actual.Izquierdo != null)
        {
            actual = actual.Izquierdo;
        }
        return actual;
    }

    public int Contar()
    {
        return count;
    }
}
