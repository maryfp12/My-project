using UnityEngine;
using UnityEngine.Events;

public class Cofre : MonoBehaviour
{
    public Animator animator;
    public GameObject mensaje;
    public KeyCode tecla = KeyCode.E;
    public UnityEvent alAbrir;

    bool jugadorCerca;
    bool abierto;

    void Start()
    {
        if (mensaje) mensaje.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !abierto)
        {
            jugadorCerca = true;
            if (mensaje) mensaje.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = false;
            if (mensaje) mensaje.SetActive(false);
        }
    }

    void Update()
    {
        if (jugadorCerca && !abierto && Input.GetKeyDown(tecla))
        {
            abierto = true;
            if (mensaje) mensaje.SetActive(false);
            if (animator) animator.Play("Open");
            alAbrir.Invoke();
        }
    }
}