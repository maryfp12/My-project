using UnityEngine;

public class CamaraSigue3D : MonoBehaviour
{
    public Transform objetivo;
    public Vector3 direccionCamino = new Vector3(0, 0, 1); // hacia dónde va el camino
    public float altura = 3f;
    public float distanciaAtras = 6f;
    public float alturaMirada = 2.5f;
    public float distanciaAdelante = 6f;
    public float suavizado = 8f;

    void Start()
    {
        if (objetivo == null) return;
        transform.position = CalcularDestino();
        transform.rotation = CalcularRotacion(transform.position);
    }

    void LateUpdate()
    {
    if (objetivo == null) return;
    transform.position = CalcularDestino();
    transform.rotation = CalcularRotacion(transform.position);
    }

    Vector3 CalcularDestino()
    {
        Vector3 dir = direccionCamino.normalized;
        return objetivo.position - dir * distanciaAtras + Vector3.up * altura;
    }

    Quaternion CalcularRotacion(Vector3 desde)
    {
        Vector3 dir = direccionCamino.normalized;
        Vector3 punto = objetivo.position + dir * distanciaAdelante + Vector3.up * alturaMirada;
        return Quaternion.LookRotation(punto - desde);
    }
}