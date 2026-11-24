using UnityEngine;

public class Martillo : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 2f;
    public float distancia = 3f;

    Vector3 posInicial;
    Rigidbody rb;

    void Start()
    {
        posInicial = transform.position;
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float x = Mathf.Sin(Time.time * velocidad) * distancia;
        Vector3 nuevaPos = new Vector3(posInicial.x + x, posInicial.y, posInicial.z);
        rb.MovePosition(nuevaPos);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameOver gameOver = FindObjectOfType<GameOver>();
            if (gameOver != null)
                gameOver.Terminar();
        }
    }
}
