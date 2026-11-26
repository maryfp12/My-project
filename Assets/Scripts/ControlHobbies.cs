using UnityEngine;

public class ControlHobbies : MonoBehaviour
{
    public ListaHobbies buenos = new ListaHobbies();
    public ListaHobbies malos = new ListaHobbies();

    public void Recoger(Hobby h)
    {
        if (h.esBueno)
            buenos.Agregar(h);
        else
            malos.Agregar(h);

        Debug.Log("Buenos: " + buenos.Contar() + "  Malos: " + malos.Contar());
    }
}

