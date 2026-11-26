using System.Collections.Generic;
using UnityEngine;

public class CintaTransportadora : MonoBehaviour
{
    public const int Capacidad = 4;

    const float Separacion = 2.6f;
    const float AltoSuperficie = 0.9f;
    const float Ancho = 1.6f;
    const float VelocidadBanda = 1.6f;
    const int CantidadFranjas = 22;

    static readonly Color ColorEstructura = new Color(0.30f, 0.33f, 0.42f);
    static readonly Color ColorBanda = new Color(0.16f, 0.17f, 0.21f);
    static readonly Color ColorFranja = new Color(0.32f, 0.34f, 0.40f);
    static readonly Color ColorBoca = new Color(0.20f, 0.12f, 0.05f);

    readonly List<Cartel> carteles = new List<Cartel>();
    Transform[] franjas;
    Transform[] rodillos;
    Vector3 entrada, final;
    float inicioX, finX;

    public bool Llena => carteles.Count >= Capacidad;
    public Vector3 Entrada => entrada;
    public Vector3 Tolva { get; private set; }

    public static CintaTransportadora Crear(float desdeX, float hastaX, float z)
    {
        var cinta = new GameObject("CintaTransportadora").AddComponent<CintaTransportadora>();
        cinta.Construir(desdeX, hastaX, z);
        return cinta;
    }

    public void Agregar(Cartel cartel)
    {
        cartel.transform.position = entrada;
        carteles.Add(cartel);
        Reordenar();
    }

    public void Quitar(Cartel cartel)
    {
        if (carteles.Remove(cartel)) Reordenar();
    }

    public void Vaciar() => carteles.Clear();

    void Reordenar()
    {
        for (int i = 0; i < carteles.Count; i++)
            carteles[i].MoverEnCinta(final + Vector3.left * Separacion * i);
    }

    void Construir(float desdeX, float hastaX, float z)
    {
        var t = transform;
        float largo = hastaX - desdeX;
        float centro = (desdeX + hastaX) * 0.5f;
        inicioX = desdeX;
        finX = hastaX;

        ConstructorEscena.Bloque(PrimitiveType.Cube, "Estructura", t, new Vector3(centro, 0.55f, z), new Vector3(largo + 0.4f, 0.5f, Ancho + 0.3f), ColorEstructura, true);
        ConstructorEscena.Bloque(PrimitiveType.Cube, "Banda", t, new Vector3(centro, 0.85f, z), new Vector3(largo, 0.1f, Ancho), ColorBanda);
        ConstructorEscena.Bloque(PrimitiveType.Cube, "Baranda", t, new Vector3(centro, 0.97f, z - Ancho * 0.5f - 0.08f), new Vector3(largo + 0.4f, 0.14f, 0.12f), Estilo.Miel);
        ConstructorEscena.Bloque(PrimitiveType.Cube, "Baranda", t, new Vector3(centro, 0.97f, z + Ancho * 0.5f + 0.08f), new Vector3(largo + 0.4f, 0.14f, 0.12f), Estilo.Miel);

        foreach (float x in new[] { desdeX + 0.3f, hastaX - 0.3f })
            foreach (float lado in new[] { -1f, 1f })
                ConstructorEscena.Bloque(PrimitiveType.Cube, "Pata", t, new Vector3(x, 0.15f, z + lado * (Ancho * 0.5f + 0.05f)), new Vector3(0.25f, 0.3f, 0.25f), ColorEstructura);

        var deLado = Quaternion.Euler(90f, 0f, 0f);
        rodillos = new[]
        {
            ConstructorEscena.Bloque(PrimitiveType.Cylinder, "Rodillo", t, new Vector3(desdeX, 0.7f, z), deLado, new Vector3(0.45f, Ancho * 0.5f + 0.15f, 0.45f), Estilo.PiedraClara).transform,
            ConstructorEscena.Bloque(PrimitiveType.Cylinder, "Rodillo", t, new Vector3(hastaX, 0.7f, z), deLado, new Vector3(0.45f, Ancho * 0.5f + 0.15f, 0.45f), Estilo.PiedraClara).transform
        };

        franjas = new Transform[CantidadFranjas];
        for (int i = 0; i < CantidadFranjas; i++)
        {
            float x = desdeX + i * largo / CantidadFranjas;
            franjas[i] = ConstructorEscena.Bloque(PrimitiveType.Cube, "Franja", t, new Vector3(x, 0.905f, z), new Vector3(0.07f, 0.01f, Ancho - 0.1f), ColorFranja).transform;
        }

        ConstructorEscena.Bloque(PrimitiveType.Cube, "Tope", t, new Vector3(hastaX + 0.3f, 1.1f, z), new Vector3(0.2f, 0.45f, Ancho + 0.3f), Estilo.Rojo);

        float tolvaX = desdeX - 0.9f;
        Tolva = new Vector3(tolvaX, 2.9f, z);
        ConstructorEscena.Bloque(PrimitiveType.Cube, "Tolva", t, new Vector3(tolvaX, 1.4f, z), new Vector3(1.6f, 2.8f, 2.2f), Estilo.MielOscura, true);
        ConstructorEscena.Bloque(PrimitiveType.Cube, "Techo", t, new Vector3(tolvaX, 2.9f, z), new Vector3(1.9f, 0.22f, 2.5f), Estilo.Miel);
        ConstructorEscena.Bloque(PrimitiveType.Cube, "Boca", t, new Vector3(desdeX - 0.08f, 1.45f, z), new Vector3(0.06f, 1.0f, 1.5f), ColorBoca);
        ConstructorEscena.Figura("Emblema", Mallas.Hexagono, t, new Vector3(tolvaX, 1.9f, z - 1.12f), Quaternion.identity, new Vector3(0.55f, 0.55f, 0.06f), Estilo.Miel);

        entrada = new Vector3(desdeX + 0.8f, AltoSuperficie, z);
        final = new Vector3(hastaX - 1.15f, AltoSuperficie, z);
    }

    void Update()
    {
        float avance = VelocidadBanda * Time.deltaTime;
        float largo = finX - inicioX;
        foreach (var franja in franjas)
        {
            var posicion = franja.localPosition;
            posicion.x += avance;
            if (posicion.x > finX) posicion.x -= largo;
            franja.localPosition = posicion;
        }
        foreach (var rodillo in rodillos) rodillo.Rotate(0f, -avance / 0.225f * Mathf.Rad2Deg, 0f, Space.Self);
    }
}
