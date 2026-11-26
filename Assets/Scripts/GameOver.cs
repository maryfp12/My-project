using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOver : MonoBehaviour
{
    [Header("Panel de derrota")]
    public GameObject panel;
    public TextMeshProUGUI textoPuntajeFinal;

    [Header("Panel de victoria (opcional)")]
    public GameObject panelVictoria;
    public TextMeshProUGUI textoPuntajeVictoria;

    [Header("Caída")]
    public Transform jugador;
    public float alturaCaida = -10f;

    [Header("Escenas")]
    public string nombreMenu = "SalaMiniJuegos";

    bool terminado = false;

    void Start()
    {
        Time.timeScale = 1f;
        panel.SetActive(false);
        if (panelVictoria != null) panelVictoria.SetActive(false);
    }

    void Update()
    {
        if (!terminado && jugador != null && jugador.position.y < alturaCaida)
            Terminar();
    }

    // Perder (caída o puntos en 0)
    public void Terminar()
    {
        if (terminado) return;
        terminado = true;

        if (textoPuntajeFinal != null)
            textoPuntajeFinal.text = "Puntaje: " + PuntajeActual();

        panel.SetActive(true);
        Time.timeScale = 0f;
    }

    // Ganar
    public void Ganar()
    {
        if (terminado) return;
        terminado = true;

        if (textoPuntajeVictoria != null)
            textoPuntajeVictoria.text = "Puntaje: " + PuntajeActual();

        if (panelVictoria != null) panelVictoria.SetActive(true);
        Time.timeScale = 0f;
    }

    int PuntajeActual()
    {
        return Marcador.instancia != null ? Marcador.instancia.puntaje : 0;
    }

    public void Reintentar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void IrAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nombreMenu);
    }
}
