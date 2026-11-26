using System.Collections.Generic;
using UnityEngine;

public static class Mallas
{
    public const float AnchoBurbuja = 3.6f;
    public const float AltoBurbuja = 1.8f;

    static Mesh burbuja, hexagono;

    public static Mesh Burbuja
    {
        get
        {
            if (burbuja == null) burbuja = CrearBurbuja(AnchoBurbuja, AltoBurbuja, 0.42f, 0.14f);
            return burbuja;
        }
    }

    public static Mesh Hexagono
    {
        get
        {
            if (hexagono == null) hexagono = Extruir(Poligono(6, 1f), 1f, false);
            return hexagono;
        }
    }

    static Mesh CrearBurbuja(float ancho, float alto, float radio, float profundidad)
    {
        const int segmentos = 10;
        float ax = ancho * 0.5f - radio;
        float ay = alto * 0.5f - radio;
        float baseY = -alto * 0.5f;

        var contorno = new List<Vector2>();
        AgregarArco(contorno, new Vector2(ax, -ay), radio, -90f, 0f, segmentos);
        AgregarArco(contorno, new Vector2(ax, ay), radio, 0f, 90f, segmentos);
        AgregarArco(contorno, new Vector2(-ax, ay), radio, 90f, 180f, segmentos);
        AgregarArco(contorno, new Vector2(-ax, -ay), radio, 180f, 270f, segmentos);
        contorno.Add(new Vector2(-0.62f, baseY));
        contorno.Add(new Vector2(-0.38f, baseY - 0.42f));
        contorno.Add(new Vector2(-0.02f, baseY));
        return Extruir(contorno, profundidad, true);
    }

    static void AgregarArco(List<Vector2> contorno, Vector2 centro, float radio, float desde, float hasta, int segmentos)
    {
        for (int i = 0; i <= segmentos; i++)
        {
            float angulo = Mathf.Lerp(desde, hasta, i / (float)segmentos) * Mathf.Deg2Rad;
            contorno.Add(centro + new Vector2(Mathf.Cos(angulo), Mathf.Sin(angulo)) * radio);
        }
    }

    static List<Vector2> Poligono(int lados, float radio)
    {
        var puntos = new List<Vector2>();
        for (int i = 0; i < lados; i++)
        {
            float angulo = i * Mathf.PI * 2f / lados;
            puntos.Add(new Vector2(Mathf.Cos(angulo), Mathf.Sin(angulo)) * radio);
        }
        return puntos;
    }

    static Mesh Extruir(List<Vector2> contorno, float profundidad, bool suave)
    {
        int n = contorno.Count;
        var vertices = new List<Vector3>();
        var normales = new List<Vector3>();
        var triangulos = new List<int>();
        float frente = -profundidad * 0.5f;
        float atras = profundidad * 0.5f;

        int centroFrente = vertices.Count;
        vertices.Add(new Vector3(0f, 0f, frente));
        normales.Add(Vector3.back);
        foreach (var p in contorno)
        {
            vertices.Add(new Vector3(p.x, p.y, frente));
            normales.Add(Vector3.back);
        }
        for (int i = 0; i < n; i++)
        {
            triangulos.Add(centroFrente);
            triangulos.Add(centroFrente + 1 + (i + 1) % n);
            triangulos.Add(centroFrente + 1 + i);
        }

        int centroAtras = vertices.Count;
        vertices.Add(new Vector3(0f, 0f, atras));
        normales.Add(Vector3.forward);
        foreach (var p in contorno)
        {
            vertices.Add(new Vector3(p.x, p.y, atras));
            normales.Add(Vector3.forward);
        }
        for (int i = 0; i < n; i++)
        {
            triangulos.Add(centroAtras);
            triangulos.Add(centroAtras + 1 + i);
            triangulos.Add(centroAtras + 1 + (i + 1) % n);
        }

        if (suave)
        {
            int baseFrente = vertices.Count;
            for (int i = 0; i < n; i++)
            {
                var normal = (NormalBorde(contorno[(i - 1 + n) % n], contorno[i]) + NormalBorde(contorno[i], contorno[(i + 1) % n])).normalized;
                vertices.Add(new Vector3(contorno[i].x, contorno[i].y, frente));
                normales.Add(normal);
            }
            int baseAtras = vertices.Count;
            for (int i = 0; i < n; i++)
            {
                vertices.Add(new Vector3(contorno[i].x, contorno[i].y, atras));
                normales.Add(normales[baseFrente + i]);
            }
            for (int i = 0; i < n; i++)
            {
                int a = i;
                int b = (i + 1) % n;
                triangulos.Add(baseFrente + a);
                triangulos.Add(baseFrente + b);
                triangulos.Add(baseAtras + b);
                triangulos.Add(baseFrente + a);
                triangulos.Add(baseAtras + b);
                triangulos.Add(baseAtras + a);
            }
        }
        else
        {
            for (int i = 0; i < n; i++)
            {
                var a = contorno[i];
                var b = contorno[(i + 1) % n];
                var normal = NormalBorde(a, b);
                int k = vertices.Count;
                vertices.Add(new Vector3(a.x, a.y, frente));
                vertices.Add(new Vector3(b.x, b.y, frente));
                vertices.Add(new Vector3(b.x, b.y, atras));
                vertices.Add(new Vector3(a.x, a.y, atras));
                for (int j = 0; j < 4; j++) normales.Add(normal);
                triangulos.Add(k);
                triangulos.Add(k + 1);
                triangulos.Add(k + 2);
                triangulos.Add(k);
                triangulos.Add(k + 2);
                triangulos.Add(k + 3);
            }
        }

        var malla = new Mesh();
        malla.SetVertices(vertices);
        malla.SetNormals(normales);
        malla.SetTriangles(triangulos, 0);
        malla.RecalculateBounds();
        return malla;
    }

    static Vector3 NormalBorde(Vector2 a, Vector2 b)
    {
        var borde = b - a;
        return new Vector3(borde.y, -borde.x, 0f).normalized;
    }
}
