using UnityEngine;

public class TriggerPuerta : MonoBehaviour
{
    public enum Lado { Afuera, Adentro }

    public Lado lado;
    public PuertaCamara puerta;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && puerta)
            puerta.Tocado(lado);
    }
}
