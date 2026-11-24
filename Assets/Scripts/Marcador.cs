using UnityEngine;
using TMPro;

public class Marcador : MonoBehaviour
{
    public static Marcador instancia;
    public TextMeshProUGUI textoPuntaje;
    public GameOver gameOver;       // arrastra GameOverControl
    public int puntaje = 5;
    public int meta = 10;

    bool terminado = false;

    void Awake() { instancia = this; }
    void Start() { Actualizar(); }

    public void Sumar(int cantidad)
    {
        if (terminado) return;

        puntaje += cantidad;
        Actualizar();

        if (puntaje >= meta)
        {
            terminado = true;
            if (gameOver != null) gameOver.Ganar();
        }
        else if (puntaje <= 0)
        {
            terminado = true;
            if (gameOver != null) gameOver.Terminar();
        }
    }

    void Actualizar()
    {
        if (textoPuntaje != null)
            textoPuntaje.text = "Puntos: " + puntaje;
    }
}
