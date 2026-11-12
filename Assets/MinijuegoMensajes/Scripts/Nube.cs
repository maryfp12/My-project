using UnityEngine;

public class Nube : MonoBehaviour
{
    const float Limite = 55f;

    float velocidad;

    public static Nube Crear(Transform padre, Vector3 posicion, float escala, float velocidad, System.Random azar)
    {
        var raiz = new GameObject("Nube").transform;
        raiz.SetParent(padre, false);
        raiz.position = posicion;
        raiz.localScale = Vector3.one * escala;

        int partes = 4 + azar.Next(3);
        for (int i = 0; i < partes; i++)
        {
            float x = (i - (partes - 1) * 0.5f) * 1.3f;
            float tam = 1.8f + (float)azar.NextDouble() * 1.4f - Mathf.Abs(x) * 0.25f;
            var pos = new Vector3(x, (float)azar.NextDouble() * 0.5f, (float)azar.NextDouble() * 0.6f);
            ConstructorEscena.Bloque(PrimitiveType.Sphere, "Bola", raiz, pos, new Vector3(tam, tam * 0.75f, tam * 0.8f), Color.white);
        }

        var nube = raiz.gameObject.AddComponent<Nube>();
        nube.velocidad = velocidad;
        return nube;
    }

    void Update()
    {
        var posicion = transform.position;
        posicion.x += velocidad * Time.deltaTime;
        if (posicion.x > Limite) posicion.x = -Limite;
        transform.position = posicion;
    }
}
