using UnityEngine;

public static class Parque
{
    public const float LimiteX = 12.6f;
    public const float LimiteSur = -7.2f;
    public const float LimiteNorte = 10.2f;

    const float GrosorMuro = 0.7f;
    const float AltoMuro = 1f;

    public static void Construir()
    {
        var raiz = new GameObject("Parque").transform;
        var azar = new System.Random(12);

        Suelo(raiz);
        Muros(raiz, azar);
        Faroles(raiz);
        Arboles(raiz, azar);
        Arbustos(raiz, azar);
        Flores(raiz, azar);
        Colinas(raiz);
        Nubes(raiz, azar);
        Abejas(raiz);
    }

    public static Vector3 DentroDeLaPlaza(Vector3 punto, float margen)
    {
        punto.x = Mathf.Clamp(punto.x, -LimiteX + margen, LimiteX - margen);
        punto.z = Mathf.Clamp(punto.z, LimiteSur + margen, LimiteNorte - margen);
        return punto;
    }

    static float Rango(System.Random azar, float min, float max) => min + (float)azar.NextDouble() * (max - min);

    static void Suelo(Transform raiz)
    {
        ConstructorEscena.Bloque(PrimitiveType.Plane, "Pasto", raiz, new Vector3(0f, -0.01f, 10f), new Vector3(20f, 1f, 20f), Estilo.Pasto, true);

        float ancho = LimiteX * 2f + GrosorMuro * 2f;
        float fondo = LimiteNorte - LimiteSur + GrosorMuro * 2f;
        float centroZ = (LimiteNorte + LimiteSur) * 0.5f;
        ConstructorEscena.Bloque(PrimitiveType.Cube, "Plaza", raiz, new Vector3(0f, -0.04f, centroZ), new Vector3(ancho, 0.1f, fondo), Estilo.Plaza, true);

        for (float z = LimiteSur + 1.6f; z < LimiteNorte; z += 1.6f)
            ConstructorEscena.Bloque(PrimitiveType.Cube, "Junta", raiz, new Vector3(0f, 0.012f, z), new Vector3(LimiteX * 2f, 0.005f, 0.05f), Estilo.JuntaPlaza);
        for (float x = -LimiteX + 1.6f; x < LimiteX; x += 1.6f)
            ConstructorEscena.Bloque(PrimitiveType.Cube, "Junta", raiz, new Vector3(x, 0.012f, centroZ), new Vector3(0.05f, 0.005f, LimiteNorte - LimiteSur), Estilo.JuntaPlaza);
    }

    static void Muros(Transform raiz, System.Random azar)
    {
        float x = LimiteX + GrosorMuro * 0.5f;
        float sur = LimiteSur - GrosorMuro * 0.5f;
        float norte = LimiteNorte + GrosorMuro * 0.5f;
        Muro(raiz, new Vector3(-x - 0.35f, 0f, sur), new Vector3(x + 0.35f, 0f, sur), azar);
        Muro(raiz, new Vector3(-x - 0.35f, 0f, norte), new Vector3(x + 0.35f, 0f, norte), azar);
        Muro(raiz, new Vector3(-x, 0f, sur), new Vector3(-x, 0f, norte), azar);
        Muro(raiz, new Vector3(x, 0f, sur), new Vector3(x, 0f, norte), azar);
    }

    static void Muro(Transform raiz, Vector3 desde, Vector3 hasta, System.Random azar)
    {
        var direccion = hasta - desde;
        float largo = direccion.magnitude;
        direccion /= largo;

        var muro = new GameObject("Muro").transform;
        muro.SetParent(raiz, false);
        muro.SetPositionAndRotation(desde, Quaternion.LookRotation(Vector3.Cross(direccion, Vector3.up)));

        var colisionador = muro.gameObject.AddComponent<BoxCollider>();
        colisionador.center = new Vector3(largo * 0.5f, AltoMuro * 0.5f, 0f);
        colisionador.size = new Vector3(largo, AltoMuro, GrosorMuro);

        ConstructorEscena.Bloque(PrimitiveType.Cube, "Mortero", muro, new Vector3(largo * 0.5f, 0.42f, 0.03f), new Vector3(largo, 0.84f, GrosorMuro - 0.08f), Estilo.Junta);

        for (int fila = 0; fila < 2; fila++)
        {
            float y = 0.21f + fila * 0.425f;
            float x = fila == 1 ? -0.55f : 0f;
            while (x < largo)
            {
                float ancho = Rango(azar, 0.9f, 1.45f);
                float inicio = Mathf.Max(x, 0f);
                float fin = Mathf.Min(x + ancho, largo);
                if (fin - inicio > 0.2f)
                {
                    var color = Color.Lerp(Estilo.Piedra, Estilo.PiedraClara, Rango(azar, 0f, 0.6f));
                    ConstructorEscena.Bloque(PrimitiveType.Cube, "Piedra", muro, new Vector3((inicio + fin) * 0.5f, y, 0f), new Vector3(fin - inicio - 0.05f, 0.4f, GrosorMuro), color);
                }
                x += ancho;
            }
        }

        float xTapa = 0f;
        while (xTapa < largo)
        {
            float ancho = Mathf.Min(Rango(azar, 1.2f, 1.7f), largo - xTapa);
            if (ancho > 0.25f)
                ConstructorEscena.Bloque(PrimitiveType.Cube, "Tapa", muro, new Vector3(xTapa + ancho * 0.5f, 0.93f, 0f), new Vector3(ancho - 0.04f, 0.17f, GrosorMuro + 0.12f), Estilo.PiedraClara);
            xTapa += ancho;
        }
    }

    static void Faroles(Transform raiz)
    {
        var poste = new Color(0.22f, 0.24f, 0.30f);
        Vector3[] posiciones =
        {
            new Vector3(-LimiteX + 0.8f, 0f, LimiteSur + 0.8f), new Vector3(LimiteX - 0.8f, 0f, LimiteSur + 0.8f),
            new Vector3(-LimiteX + 0.8f, 0f, LimiteNorte - 0.8f), new Vector3(LimiteX - 0.8f, 0f, LimiteNorte - 0.8f)
        };
        foreach (var p in posiciones)
        {
            ConstructorEscena.Bloque(PrimitiveType.Cylinder, "Farol", raiz, p + Vector3.up * 1.4f, new Vector3(0.14f, 1.4f, 0.14f), poste, true);
            ConstructorEscena.Bloque(PrimitiveType.Sphere, "Luz", raiz, p + Vector3.up * 2.95f, Vector3.one * 0.5f, Estilo.MielClara);
            ConstructorEscena.Bloque(PrimitiveType.Cylinder, "Sombrero", raiz, p + Vector3.up * 3.22f, new Vector3(0.55f, 0.04f, 0.55f), poste);
        }
    }

    static void Arboles(Transform raiz, System.Random azar)
    {
        var arboles = new GameObject("Arboles").transform;
        arboles.SetParent(raiz, false);

        Vector3[] frondosos =
        {
            new Vector3(-16.5f, 2f, 1.3f), new Vector3(16.5f, 3f, 1.3f), new Vector3(-18.5f, 9.5f, 1.4f),
            new Vector3(18.5f, 10f, 1.4f), new Vector3(-13.5f, 15f, 1.3f), new Vector3(14f, 16f, 1.3f),
            new Vector3(-6f, 15.5f, 1.4f), new Vector3(6.5f, 16.5f, 1.35f), new Vector3(0.5f, 20f, 1.6f),
            new Vector3(-11f, 20.5f, 1.5f), new Vector3(11.5f, 21f, 1.5f), new Vector3(-17f, -5f, 1.35f),
            new Vector3(17f, -4.5f, 1.35f), new Vector3(-9f, -12f, 1.3f), new Vector3(9.5f, -12.5f, 1.3f),
            new Vector3(0f, -14f, 1.4f)
        };
        foreach (var a in frondosos) ArbolFrondoso(arboles, new Vector3(a.x, 0f, a.y), a.z, azar);

        Vector3[] cipreses =
        {
            new Vector3(-15f, 12f, 1.1f), new Vector3(15.5f, 12.5f, 1.1f), new Vector3(-3f, 14f, 1f),
            new Vector3(3.5f, 14f, 1f), new Vector3(-19f, 3.5f, 1.15f), new Vector3(19f, 4f, 1.15f),
            new Vector3(-14.5f, -10f, 1f), new Vector3(14.5f, -10f, 1f)
        };
        foreach (var c in cipreses) Cipres(arboles, new Vector3(c.x, 0f, c.y), c.z);
    }

    static void ArbolFrondoso(Transform padre, Vector3 posicion, float escala, System.Random azar)
    {
        var arbol = new GameObject("Arbol").transform;
        arbol.SetParent(padre, false);
        arbol.localPosition = posicion;
        arbol.localScale = Vector3.one * escala;

        ConstructorEscena.Bloque(PrimitiveType.Cylinder, "Tronco", arbol, new Vector3(0f, 1f, 0f), new Vector3(0.36f, 1f, 0.36f), Estilo.Tronco);
        int bolas = 5 + azar.Next(2);
        for (int i = 0; i < bolas; i++)
        {
            float angulo = i * Mathf.PI * 2f / bolas + Rango(azar, 0f, 0.6f);
            float distancia = i == 0 ? 0f : Rango(azar, 0.55f, 0.85f);
            var centro = new Vector3(Mathf.Cos(angulo) * distancia, 2.7f + Rango(azar, -0.3f, 0.45f), Mathf.Sin(angulo) * distancia);
            float tam = i == 0 ? 2.1f : Rango(azar, 1.3f, 1.7f);
            ConstructorEscena.Bloque(PrimitiveType.Sphere, "Copa", arbol, centro, Vector3.one * tam, Estilo.Hojas[azar.Next(Estilo.Hojas.Length)]);
        }
    }

    static void Cipres(Transform padre, Vector3 posicion, float escala)
    {
        var arbol = new GameObject("Cipres").transform;
        arbol.SetParent(padre, false);
        arbol.localPosition = posicion;
        arbol.localScale = Vector3.one * escala;

        ConstructorEscena.Bloque(PrimitiveType.Cylinder, "Tronco", arbol, new Vector3(0f, 0.3f, 0f), new Vector3(0.25f, 0.3f, 0.25f), Estilo.Tronco);
        ConstructorEscena.Bloque(PrimitiveType.Capsule, "Copa", arbol, new Vector3(0f, 2.55f, 0f), new Vector3(1.25f, 2.2f, 1.25f), Estilo.Cipres);
    }

    static void Arbustos(Transform raiz, System.Random azar)
    {
        var arbustos = new GameObject("Arbustos").transform;
        arbustos.SetParent(raiz, false);

        float afueraX = LimiteX + GrosorMuro + 0.9f;
        for (float x = -LimiteX; x <= LimiteX; x += Rango(azar, 1.4f, 2.1f))
        {
            Arbusto(arbustos, new Vector3(x, 0f, LimiteNorte + GrosorMuro + 0.9f), Rango(azar, 0.85f, 1.2f), azar);
            Arbusto(arbustos, new Vector3(x, 0f, LimiteSur - GrosorMuro - 0.9f), Rango(azar, 0.85f, 1.2f), azar);
        }
        for (float z = LimiteSur; z <= LimiteNorte; z += Rango(azar, 1.6f, 2.4f))
        {
            Arbusto(arbustos, new Vector3(-afueraX, 0f, z), Rango(azar, 0.85f, 1.2f), azar);
            Arbusto(arbustos, new Vector3(afueraX, 0f, z), Rango(azar, 0.85f, 1.2f), azar);
        }
    }

    static void Arbusto(Transform padre, Vector3 posicion, float escala, System.Random azar)
    {
        var arbusto = new GameObject("Arbusto").transform;
        arbusto.SetParent(padre, false);
        arbusto.localPosition = posicion;
        arbusto.localScale = Vector3.one * escala;

        int bolas = 3 + azar.Next(2);
        for (int i = 0; i < bolas; i++)
        {
            var centro = new Vector3((i - (bolas - 1) * 0.5f) * 0.5f, Rango(azar, 0.35f, 0.5f), Rango(azar, -0.15f, 0.15f));
            ConstructorEscena.Bloque(PrimitiveType.Sphere, "Hoja", arbusto, centro, Vector3.one * Rango(azar, 0.8f, 1.15f), Estilo.Hojas[azar.Next(Estilo.Hojas.Length)]);
        }
    }

    static void Flores(Transform raiz, System.Random azar)
    {
        var flores = new GameObject("Flores").transform;
        flores.SetParent(raiz, false);
        var tallo = new Color(0.3f, 0.6f, 0.22f);

        int puestas = 0;
        while (puestas < 80)
        {
            var posicion = new Vector3(Rango(azar, -22f, 22f), 0f, Rango(azar, -14f, 22f));
            bool dentro = Mathf.Abs(posicion.x) < LimiteX + 2.4f && posicion.z > LimiteSur - 2.4f && posicion.z < LimiteNorte + 2.4f;
            if (dentro) continue;

            float alto = Rango(azar, 0.1f, 0.16f);
            ConstructorEscena.Bloque(PrimitiveType.Cylinder, "Tallo", flores, posicion + Vector3.up * alto, new Vector3(0.03f, alto, 0.03f), tallo);
            ConstructorEscena.Bloque(PrimitiveType.Sphere, "Flor", flores, posicion + Vector3.up * (alto * 2f + 0.04f), Vector3.one * 0.17f, Estilo.Petalos[azar.Next(Estilo.Petalos.Length)]);
            puestas++;
        }
    }

    static void Colinas(Transform raiz)
    {
        var colina = new Color(0.55f, 0.78f, 0.42f);
        Vector4[] colinas =
        {
            new Vector4(-30f, -4f, 58f, 34f), new Vector4(18f, -5f, 64f, 40f), new Vector4(55f, -3f, 52f, 30f),
            new Vector4(-62f, -3f, 50f, 30f), new Vector4(0f, -6f, 75f, 44f), new Vector4(-40f, -4f, -45f, 36f),
            new Vector4(40f, -4f, -50f, 40f)
        };
        foreach (var c in colinas)
            ConstructorEscena.Bloque(PrimitiveType.Sphere, "Colina", raiz, new Vector3(c.x, c.y, c.z), new Vector3(c.w, c.w * 0.38f, c.w * 0.6f), colina);
    }

    static void Nubes(Transform raiz, System.Random azar)
    {
        for (int i = 0; i < 10; i++)
        {
            float z = i % 3 == 0 ? Rango(azar, -50f, -34f) : Rango(azar, 34f, 50f);
            var posicion = new Vector3(Rango(azar, -50f, 50f), Rango(azar, 10f, 16f), z);
            Nube.Crear(raiz, posicion, Rango(azar, 1.1f, 1.8f), Rango(azar, 0.3f, 0.7f), azar);
        }
    }

    static void Abejas(Transform raiz)
    {
        Abeja.Crear(raiz, new Vector3(-6f, 3.4f, -4.5f), new Vector2(1.4f, 0.8f), 1.1f, 0f);
        Abeja.Crear(raiz, new Vector3(6.5f, 3.8f, -3.5f), new Vector2(1.5f, 0.9f), -0.9f, 1.5f);
        Abeja.Crear(raiz, new Vector3(0f, 5f, 9.5f), new Vector2(2f, 0.6f), 0.8f, 3f);
        Abeja.Crear(raiz, new Vector3(-14.5f, 2.2f, 5f), new Vector2(1f, 1.4f), 1.3f, 4.2f);
        Abeja.Crear(raiz, new Vector3(14.5f, 2.6f, 6f), new Vector2(1f, 1.4f), -1.2f, 2.1f);
    }
}
