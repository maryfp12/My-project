using System.Collections.Generic;

public class ListaMensajes
{
    NodoMensaje cabeza;
    NodoMensaje cola;

    public int Cantidad { get; private set; }
    public NodoMensaje Cabeza => cabeza;
    public bool EstaVacia => cabeza == null;

    public void AgregarAlFinal(NodoMensaje nodo)
    {
        nodo.Siguiente = null;
        if (cabeza == null)
        {
            cabeza = nodo;
            cola = nodo;
        }
        else
        {
            cola.Siguiente = nodo;
            cola = nodo;
        }
        Cantidad++;
    }

    public void AgregarAlInicio(NodoMensaje nodo)
    {
        nodo.Siguiente = cabeza;
        cabeza = nodo;
        if (cola == null) cola = nodo;
        Cantidad++;
    }

    public NodoMensaje SacarPrimero()
    {
        if (cabeza == null) return null;
        var nodo = cabeza;
        cabeza = nodo.Siguiente;
        if (cabeza == null) cola = null;
        nodo.Siguiente = null;
        Cantidad--;
        return nodo;
    }

    public NodoMensaje SacarEn(int indice)
    {
        if (indice < 0 || indice >= Cantidad) return null;
        if (indice == 0) return SacarPrimero();

        var anterior = cabeza;
        for (int i = 0; i < indice - 1; i++) anterior = anterior.Siguiente;

        var nodo = anterior.Siguiente;
        anterior.Siguiente = nodo.Siguiente;
        if (nodo == cola) cola = anterior;
        nodo.Siguiente = null;
        Cantidad--;
        return nodo;
    }

    public bool Quitar(NodoMensaje objetivo)
    {
        if (cabeza == null || objetivo == null) return false;
        if (cabeza == objetivo)
        {
            SacarPrimero();
            return true;
        }

        var anterior = cabeza;
        while (anterior.Siguiente != null && anterior.Siguiente != objetivo) anterior = anterior.Siguiente;
        if (anterior.Siguiente == null) return false;

        anterior.Siguiente = objetivo.Siguiente;
        if (objetivo == cola) cola = anterior;
        objetivo.Siguiente = null;
        Cantidad--;
        return true;
    }

    public void Mezclar()
    {
        var mezclada = new ListaMensajes();
        while (!EstaVacia) mezclada.AgregarAlFinal(SacarEn(UnityEngine.Random.Range(0, Cantidad)));
        cabeza = mezclada.cabeza;
        cola = mezclada.cola;
        Cantidad = mezclada.Cantidad;
    }

    public IEnumerable<NodoMensaje> Recorrer()
    {
        for (var nodo = cabeza; nodo != null; nodo = nodo.Siguiente) yield return nodo;
    }
}
