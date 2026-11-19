using UnityEngine;
using TMPro;

public class Marcador : MonoBehaviour
{
    public static Marcador instancia;
    public TextMeshProUGUI textoPuntaje;
    public int puntaje = 5;
    public int meta = 10;

    void Awake() { instancia = this; }
    void Start() { Actualizar(); }

    public void Sumar(int cantidad)
    {
        puntaje += cantidad;
        Actualizar();
        if (puntaje >= meta) Debug.Log("¡Ganaste!");
        if (puntaje <= 0) Debug.Log("Perdiste");
    }

    void Actualizar() { textoPuntaje.text = "Puntos: " + puntaje; }
}
