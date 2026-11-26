using UnityEngine;

public class Abeja : MonoBehaviour
{
    Vector3 centro;
    Vector2 radio;
    float velocidad, angulo;
    Transform alaIzquierda, alaDerecha;

    public static Abeja Crear(Transform padre, Vector3 centro, Vector2 radio, float velocidad, float faseInicial)
    {
        var raiz = new GameObject("Abeja").transform;
        raiz.SetParent(padre, false);
        raiz.localScale = Vector3.one * 0.75f;

        var negro = new Color(0.12f, 0.1f, 0.08f);
        ConstructorEscena.Bloque(PrimitiveType.Sphere, "Cuerpo", raiz, Vector3.zero, new Vector3(0.3f, 0.26f, 0.4f), Estilo.Miel);
        ConstructorEscena.Bloque(PrimitiveType.Sphere, "Raya", raiz, new Vector3(0f, 0f, -0.04f), new Vector3(0.31f, 0.27f, 0.07f), negro);
        ConstructorEscena.Bloque(PrimitiveType.Sphere, "Raya", raiz, new Vector3(0f, 0f, -0.13f), new Vector3(0.24f, 0.21f, 0.06f), negro);
        ConstructorEscena.Bloque(PrimitiveType.Sphere, "Cabeza", raiz, new Vector3(0f, 0.02f, 0.2f), new Vector3(0.2f, 0.2f, 0.18f), negro);
        ConstructorEscena.Bloque(PrimitiveType.Sphere, "Ojo", raiz, new Vector3(0.06f, 0.06f, 0.28f), Vector3.one * 0.06f, Color.white);
        ConstructorEscena.Bloque(PrimitiveType.Sphere, "Ojo", raiz, new Vector3(-0.06f, 0.06f, 0.28f), Vector3.one * 0.06f, Color.white);

        var abeja = raiz.gameObject.AddComponent<Abeja>();
        abeja.alaIzquierda = CrearAla(raiz, -1f);
        abeja.alaDerecha = CrearAla(raiz, 1f);
        abeja.centro = centro;
        abeja.radio = radio;
        abeja.velocidad = velocidad;
        abeja.angulo = faseInicial;
        return abeja;
    }

    static Transform CrearAla(Transform cuerpo, float lado)
    {
        var bisagra = new GameObject("Ala").transform;
        bisagra.SetParent(cuerpo, false);
        bisagra.localPosition = new Vector3(0.06f * lado, 0.12f, 0.02f);
        ConstructorEscena.Bloque(PrimitiveType.Sphere, "Membrana", bisagra, new Vector3(0.12f * lado, 0.02f, 0f), new Vector3(0.24f, 0.03f, 0.15f), new Color(0.93f, 0.97f, 1f));
        return bisagra;
    }

    void Update()
    {
        angulo += velocidad * Time.deltaTime;
        float vuelo = Time.time * 6f + angulo;
        transform.position = centro + new Vector3(
            Mathf.Cos(angulo) * radio.x,
            Mathf.Sin(angulo * 2f) * 0.25f + Mathf.Sin(vuelo) * 0.05f,
            Mathf.Sin(angulo) * radio.y);

        var direccion = new Vector3(-Mathf.Sin(angulo) * radio.x, 0f, Mathf.Cos(angulo) * radio.y) * Mathf.Sign(velocidad);
        if (direccion.sqrMagnitude > 1e-4f) transform.rotation = Quaternion.LookRotation(direccion);

        float aleteo = Mathf.Sin(Time.time * 45f) * 35f;
        alaIzquierda.localRotation = Quaternion.Euler(0f, 0f, aleteo);
        alaDerecha.localRotation = Quaternion.Euler(0f, 0f, -aleteo);
    }
}
