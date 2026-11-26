using System;
using UnityEngine;

public static class SpritesUI
{
    const int RadioRedondeado = 40;

    static Sprite redondeado, hexagono, circulo;

    public static Sprite Redondeado
    {
        get
        {
            if (redondeado == null) redondeado = CrearRedondeado(96, RadioRedondeado);
            return redondeado;
        }
    }

    public static Sprite Hexagono
    {
        get
        {
            if (hexagono == null) hexagono = CrearSimple(256, DistanciaHexagono);
            return hexagono;
        }
    }

    public static Sprite Circulo
    {
        get
        {
            if (circulo == null) circulo = CrearSimple(128, DistanciaCirculo);
            return circulo;
        }
    }

    static Sprite CrearRedondeado(int tam, int radio)
    {
        float mitad = tam * 0.5f;
        var textura = Pintar(tam, (x, y) =>
        {
            float qx = Mathf.Abs(x - mitad) - (mitad - radio);
            float qy = Mathf.Abs(y - mitad) - (mitad - radio);
            float exterior = new Vector2(Mathf.Max(qx, 0f), Mathf.Max(qy, 0f)).magnitude;
            return exterior + Mathf.Min(Mathf.Max(qx, qy), 0f) - radio;
        });
        return Sprite.Create(textura, new Rect(0f, 0f, tam, tam), new Vector2(0.5f, 0.5f), 100f, 0,
            SpriteMeshType.FullRect, new Vector4(radio, radio, radio, radio));
    }

    static Sprite CrearSimple(int tam, Func<float, float, float, float> distancia)
    {
        float mitad = tam * 0.5f;
        var textura = Pintar(tam, (x, y) => distancia(x - mitad, y - mitad, mitad - 2f));
        return Sprite.Create(textura, new Rect(0f, 0f, tam, tam), new Vector2(0.5f, 0.5f), 100f);
    }

    static float DistanciaHexagono(float x, float y, float radio)
    {
        float apotema = radio * 0.8660254f;
        float a = Mathf.Abs(x);
        float b = Mathf.Abs(0.5f * x + 0.8660254f * y);
        float c = Mathf.Abs(0.5f * x - 0.8660254f * y);
        return Mathf.Max(a, Mathf.Max(b, c)) - apotema;
    }

    static float DistanciaCirculo(float x, float y, float radio) => Mathf.Sqrt(x * x + y * y) - radio;

    static Texture2D Pintar(int tam, Func<float, float, float> distancia)
    {
        var textura = new Texture2D(tam, tam, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };
        var pixeles = new Color32[tam * tam];
        for (int y = 0; y < tam; y++)
        {
            for (int x = 0; x < tam; x++)
            {
                float d = distancia(x + 0.5f, y + 0.5f);
                byte alfa = (byte)(Mathf.Clamp01(0.5f - d) * 255f);
                pixeles[y * tam + x] = new Color32(255, 255, 255, alfa);
            }
        }
        textura.SetPixels32(pixeles);
        textura.Apply();
        return textura;
    }
}
