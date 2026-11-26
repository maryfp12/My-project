using UnityEngine;

public class CamaraSigue3D : MonoBehaviour
{
    public Transform objetivo;
    public float altura = 4f;
    public float distancia = 8f;

    void LateUpdate()
    {
        transform.position = objetivo.position - objetivo.forward * distancia + Vector3.up * altura;
        transform.LookAt(objetivo.position + Vector3.up * 2f);
    }
}
