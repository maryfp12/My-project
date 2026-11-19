using UnityEngine;

public class caida_objeto : MonoBehaviour
{
    public int puntos = 1;

    void OnTriggerEnter(Collider otro)
    {
        if (otro.CompareTag("Player"))
        {
            Marcador.instancia.Sumar(puntos);
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (transform.position.y < -2f) Destroy(gameObject);
    }
}