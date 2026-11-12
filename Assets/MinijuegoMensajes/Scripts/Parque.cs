using UnityEngine;

public static class Parque
{
    const float MuroZ = 0.9f;
    const float MuroMitad = 8.5f;

    public static void Construir()
    {
        var raiz = new GameObject("Parque").transform;
        var azar = new System.Random(12);

        Suelo(raiz);
        Muro(raiz, azar);
        Arboles(raiz, azar);
        Arbustos(raiz, azar);
        Flores(raiz, azar);
        Colinas(raiz);
        Nubes(raiz, azar);
        Abejas(raiz);
    }

    static float Rango(System.Random azar, float min, float max) => min + (float)azar.NextDouble() * (max - min);

    static void Suelo(Transform raiz)
    {
        ConstructorEscena.Bloque(PrimitiveType.Plane, "Pasto", raiz, new Vector3(0f, 0f, 30f), new Vector3(16f, 1f, 16f), Estilo.Pasto);
        ConstructorEscena.Bloque(PrimitiveType.Cube, "Plaza", raiz, new Vector3(0f, 0.02f, -2.6f), new Vector3(18f, 0.04f, 6.4f), Estilo.Plaza);

        for (float z = -5.2f; z <= 0.3f; z += 1.3f)
            ConstructorEscena.Bloque(PrimitiveType.Cube, "Junta", raiz, new Vector3(0f, 0.042f, z), new Vector3(18f, 0.005f, 0.05f), Estilo.JuntaPlaza);
        for (float x = -8.4f; x <= 8.4f; x += 1.6f)
            ConstructorEscena.Bloque(PrimitiveType.Cube, "Junta", raiz, new Vector3(x, 0.042f, -2.6f), new Vector3(0.05f, 0.005f, 6.4f), Estilo.JuntaPlaza);

        ConstructorEscena.Bloque(PrimitiveType.Cube, "Bordillo", raiz, new Vector3(0f, 0.05f, 0.25f), new Vector3(18f, 0.1f, 0.25f), Estilo.PiedraClara);
    }

    static void Muro(Transform raiz, System.Random azar)
    {
        var muro = new GameObject("Muro").transform;
        muro.SetParent(raiz, false);

        ConstructorEscena.Bloque(PrimitiveType.Cube, "Mortero", muro, new Vector3(0f, 0.42f, MuroZ + 0.04f), new Vector3(MuroMitad * 2f, 0.84f, 0.62f), Estilo.Junta);

        for (int fila = 0; fila < 2; fila++)
        {
            float y = 0.21f + fila * 0.425f;
            float x = -MuroMitad - (fila == 1 ? 0.55f : 0f);
            while (x < MuroMitad)
            {
                float ancho = Rango(azar, 0.9f, 1.45f);
                float inicio = Mathf.Max(x, -MuroMitad);
                float fin = Mathf.Min(x + ancho, MuroMitad);
                if (fin - inicio > 0.2f)
                {
                    var color = Color.Lerp(Estilo.Piedra, Estilo.PiedraClara, Rango(azar, 0f, 0.6f));
                    ConstructorEscena.Bloque(PrimitiveType.Cube, "Piedra", muro,
                        new Vector3((inicio + fin) * 0.5f, y, MuroZ),
                        new Vector3(fin - inicio - 0.05f, 0.4f, 0.7f), color);
                }
                x += ancho;
            }
        }

        float xTapa = -MuroMitad - 0.1f;
        while (xTapa < MuroMitad + 0.1f)
        {
            float ancho = Mathf.Min(Rango(azar, 1.2f, 1.7f), MuroMitad + 0.1f - xTapa);
            if (ancho > 0.25f)
            {
                ConstructorEscena.Bloque(PrimitiveType.Cube, "Tapa", muro,
                    new Vector3(xTapa + ancho * 0.5f, 0.93f, MuroZ - 0.02f),
                    new Vector3(ancho - 0.04f, 0.17f, 0.84f), Estilo.PiedraClara);
            }
            xTapa += ancho;
        }
    }

    static void Arboles(Transform raiz, System.Random azar)
    {
        var arboles = new GameObject("Arboles").transform;
        arboles.SetParent(raiz, false);

        Vector3[] frondosos =
        {
            new Vector3(-7.8f, 2.4f, 1.1f), new Vector3(8.1f, 2.8f, 1.15f), new Vector3(-10.8f, 6.5f, 1.3f),
            new Vector3(11f, 7f, 1.25f), new Vector3(-5.3f, 9f, 1.05f), new Vector3(5.8f, 9.8f, 1.1f),
            new Vector3(-1.8f, 13.5f, 1.3f), new Vector3(3f, 15.5f, 1.25f), new Vector3(-9.5f, 14.5f, 1.4f),
            new Vector3(9.8f, 15.5f, 1.35f), new Vector3(-14.5f, 4f, 1.45f), new Vector3(14.5f, 4.5f, 1.45f),
            new Vector3(0.5f, 21f, 1.6f), new Vector3(-6f, 19f, 1.5f), new Vector3(7f, 20.5f, 1.5f)
        };
        foreach (var a in frondosos) ArbolFrondoso(arboles, new Vector3(a.x, 0f, a.y), a.z, azar);

        Vector3[] cipreses =
        {
            new Vector3(-4.3f, 5.4f, 0.9f), new Vector3(4.2f, 5.9f, 0.95f), new Vector3(-12.8f, 9.5f, 1.1f),
            new Vector3(12.8f, 10.5f, 1.1f), new Vector3(-7.2f, 11.8f, 1f), new Vector3(7.6f, 12.8f, 1f)
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

        for (float x = -8f; x <= 8f; x += Rango(azar, 1.3f, 2f))
            Arbusto(arbustos, new Vector3(x, 0f, MuroZ + Rango(azar, 0.75f, 1.1f)), Rango(azar, 0.8f, 1.15f), azar);

        Vector3[] esquinas =
        {
            new Vector3(-9.3f, 0f, -1.4f), new Vector3(-9.6f, 0f, -3.6f), new Vector3(9.4f, 0f, -1.6f), new Vector3(9.7f, 0f, -3.8f)
        };
        foreach (var e in esquinas) Arbusto(arbustos, e, 1.25f, azar);
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
            float tam = Rango(azar, 0.8f, 1.15f);
            ConstructorEscena.Bloque(PrimitiveType.Sphere, "Hoja", arbusto, centro, Vector3.one * tam, Estilo.Hojas[azar.Next(Estilo.Hojas.Length)]);
        }
    }

    static void Flores(Transform raiz, System.Random azar)
    {
        var flores = new GameObject("Flores").transform;
        flores.SetParent(raiz, false);
        var tallo = new Color(0.3f, 0.6f, 0.22f);

        for (int i = 0; i < 55; i++)
        {
            var posicion = new Vector3(Rango(azar, -11f, 11f), 0f, Rango(azar, MuroZ + 1.4f, 8f));
            if (i % 4 == 0) posicion = new Vector3(Rango(azar, 8.4f, 11f) * (azar.Next(2) == 0 ? -1f : 1f), 0f, Rango(azar, -5f, 0.5f));
            float alto = Rango(azar, 0.1f, 0.16f);
            ConstructorEscena.Bloque(PrimitiveType.Cylinder, "Tallo", flores, posicion + Vector3.up * alto, new Vector3(0.03f, alto, 0.03f), tallo);
            ConstructorEscena.Bloque(PrimitiveType.Sphere, "Flor", flores, posicion + Vector3.up * (alto * 2f + 0.04f), Vector3.one * 0.17f, Estilo.Petalos[azar.Next(Estilo.Petalos.Length)]);
        }
    }

    static void Colinas(Transform raiz)
    {
        var colina = new Color(0.55f, 0.78f, 0.42f);
        Vector4[] colinas =
        {
            new Vector4(-30f, -4f, 58f, 34f), new Vector4(18f, -5f, 64f, 40f), new Vector4(55f, -3f, 52f, 30f),
            new Vector4(-62f, -3f, 50f, 30f), new Vector4(0f, -6f, 75f, 44f)
        };
        foreach (var c in colinas)
            ConstructorEscena.Bloque(PrimitiveType.Sphere, "Colina", raiz, new Vector3(c.x, c.y, c.z), new Vector3(c.w, c.w * 0.38f, c.w * 0.6f), colina);
    }

    static void Nubes(Transform raiz, System.Random azar)
    {
        for (int i = 0; i < 8; i++)
        {
            var posicion = new Vector3(Rango(azar, -50f, 50f), Rango(azar, 10f, 16f), Rango(azar, 34f, 50f));
            Nube.Crear(raiz, posicion, Rango(azar, 1.1f, 1.8f), Rango(azar, 0.3f, 0.7f), azar);
        }
    }

    static void Abejas(Transform raiz)
    {
        Abeja.Crear(raiz, new Vector3(-6f, 2.8f, -0.8f), new Vector2(1.2f, 0.7f), 1.1f, 0f);
        Abeja.Crear(raiz, new Vector3(6.3f, 3.4f, -0.2f), new Vector2(1.4f, 0.8f), -0.9f, 1.5f);
        Abeja.Crear(raiz, new Vector3(-2.8f, 5f, 2.2f), new Vector2(1.6f, 0.6f), 0.8f, 3f);
        Abeja.Crear(raiz, new Vector3(7f, 1.5f, -3.2f), new Vector2(0.9f, 0.6f), 1.3f, 4.2f);
        Abeja.Crear(raiz, new Vector3(-7.3f, 1.3f, -3.4f), new Vector2(1f, 0.7f), -1.2f, 2.1f);
    }
}
