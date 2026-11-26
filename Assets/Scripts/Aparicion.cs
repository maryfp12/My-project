using UnityEngine;
using System.Collections.Generic;

public class Aparicion : MonoBehaviour
{
    public GameObject[] objetos;
    public int cantidad = 10;

    void Start()
    {
        // Lista con todos los puntos
        List<Transform> puntos = new List<Transform>();
        foreach (Transform p in transform) puntos.Add(p);

        // Elegir 'cantidad' puntos al azar, sin repetir
        for (int i = 0; i < cantidad && puntos.Count > 0; i++)
        {
            int idx = Random.Range(0, puntos.Count);
            Transform punto = puntos[idx];
            puntos.RemoveAt(idx);

            int n = Random.Range(0, objetos.Length);
            Instantiate(objetos[n], punto.position, Quaternion.identity);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        foreach (Transform punto in transform)
            Gizmos.DrawSphere(punto.position, 0.4f);
    }
}
