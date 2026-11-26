using UnityEngine;

public class ListaHobbies
{
    Nodo head;
    Nodo tail;

    public void Agregar(Hobby h)
    {
        Nodo newNode = new Nodo(h);

        if (head == null)
        {
            head = newNode;
            tail = newNode;
            return;
        }

        tail.next = newNode;
        tail = newNode;
    }

    public int Contar()
    {
        if (head == null) return 0;

        Nodo current = head;
        int n = 1;

        while (current.next != null)
        {
            current = current.next;
            n++;
        }

        return n;
    }

    public void Recorrer()
    {
        Nodo current = head;
        int total = Contar();

        for (int i = 0; i < total; i++)
        {
            Debug.Log(current.data.nombre);
            current = current.next;
        }
    }

    public void Eliminar(string nombre)
    {
        Nodo prev = null;
        Nodo current = head;

        while (current != null)
        {
            if (current.data.nombre == nombre)
            {
                if (current == head)
                {
                    head = current.next;
                    if (head == null) tail = null;
                    return;
                }

                prev.next = current.next;
                if (current == tail) tail = prev;
                return;
            }

            prev = current;
            current = current.next;
        }
    }
}