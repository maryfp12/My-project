using UnityEngine;

public class Martillo : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 2f;
    public float distancia = 3f;

    Vector3 posInicial;

    void Start()
    {
        posInicial = transform.position;
    }

    void Update()
    {
        float x = Mathf.Sin(Time.time * velocidad) * distancia;
        transform.position = new Vector3(posInicial.x + x, posInicial.y, posInicial.z);
    }
}
