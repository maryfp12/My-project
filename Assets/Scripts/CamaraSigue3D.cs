using UnityEngine;

public class CamaraSigue3D : MonoBehaviour
{
    public Transform objetivo;
    public float altura = 4f;
    public float distanciaAtras = 8f;
    public float alturaMirada = 2f;
    public float distanciaAdelante = 10f;
    public float suavizadoGiro = 6f;

    float yaw;

    void Start()
    {
        if (objetivo == null) return;
        yaw = objetivo.eulerAngles.y;
        Colocar();
    }

    void LateUpdate()
    {
        if (objetivo == null) return;
        // solo se suaviza el giro; la posición va directa para que no tiemble
        yaw = Mathf.LerpAngle(yaw, objetivo.eulerAngles.y, suavizadoGiro * Time.deltaTime);
        Colocar();
    }

    void Colocar()
    {
        Vector3 adelante = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;

        transform.position = objetivo.position - adelante * distanciaAtras + Vector3.up * altura;

        Vector3 punto = objetivo.position + adelante * distanciaAdelante + Vector3.up * alturaMirada;
        transform.rotation = Quaternion.LookRotation(punto - transform.position);
    }
}
