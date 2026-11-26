using UnityEngine; 

[System.Serializable]
public class Hobby
{
    public string nombre;
    public Sprite imagen;
    public bool esBueno;
    public int puntos;

    public Hobby(string nombre, Sprite imagen, bool esBueno, int puntos)
    {
        this.nombre = nombre;
        this.imagen = imagen;
        this.esBueno = esBueno;
        this.puntos = puntos;
    }
}
