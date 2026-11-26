using UnityEngine;

public static class Estilo
{
    public static readonly Color Cielo = new Color(0.56f, 0.80f, 0.97f);
    public static readonly Color Neblina = new Color(0.72f, 0.86f, 0.97f);
    public static readonly Color Pasto = new Color(0.47f, 0.76f, 0.32f);
    public static readonly Color Plaza = new Color(0.90f, 0.87f, 0.80f);
    public static readonly Color JuntaPlaza = new Color(0.80f, 0.77f, 0.70f);
    public static readonly Color Piedra = new Color(0.79f, 0.77f, 0.73f);
    public static readonly Color PiedraClara = new Color(0.88f, 0.86f, 0.82f);
    public static readonly Color Junta = new Color(0.55f, 0.53f, 0.50f);
    public static readonly Color Tronco = new Color(0.50f, 0.33f, 0.20f);
    public static readonly Color Cipres = new Color(0.27f, 0.55f, 0.24f);
    public static readonly Color[] Hojas =
    {
        new Color(0.40f, 0.72f, 0.28f),
        new Color(0.48f, 0.79f, 0.33f),
        new Color(0.34f, 0.64f, 0.25f)
    };
    public static readonly Color[] Petalos =
    {
        new Color(0.96f, 0.47f, 0.64f),
        new Color(1f, 0.98f, 0.95f),
        new Color(1f, 0.86f, 0.30f),
        new Color(0.76f, 0.62f, 0.96f)
    };

    public static readonly Color Miel = new Color(1f, 0.78f, 0.18f);
    public static readonly Color MielClara = new Color(1f, 0.89f, 0.50f);
    public static readonly Color MielOscura = new Color(0.82f, 0.52f, 0.06f);
    public static readonly Color Cafe = new Color(0.42f, 0.23f, 0.07f);
    public static readonly Color Verde = new Color(0.38f, 0.80f, 0.38f);
    public static readonly Color VerdeOscuro = new Color(0.17f, 0.52f, 0.22f);
    public static readonly Color Rojo = new Color(0.95f, 0.38f, 0.35f);
    public static readonly Color RojoOscuro = new Color(0.66f, 0.17f, 0.17f);
    public static readonly Color Azul = new Color(0.30f, 0.62f, 0.95f);
    public static readonly Color AzulOscuro = new Color(0.13f, 0.34f, 0.70f);
    public static readonly Color Corazon = new Color(0.94f, 0.28f, 0.40f);
    public static readonly Color Crema = new Color(1f, 0.97f, 0.89f);
    public static readonly Color TextoOscuro = new Color(0.20f, 0.22f, 0.32f);
    public static readonly Color BordeBurbuja = new Color(0.68f, 0.79f, 0.95f);

    static Font negrita, seminegrita, simbolos;

    public static Font Negrita
    {
        get
        {
            if (negrita == null) negrita = Cargar("Fuentes/Fredoka-Bold");
            return negrita;
        }
    }

    public static Font SemiNegrita
    {
        get
        {
            if (seminegrita == null) seminegrita = Cargar("Fuentes/Fredoka-SemiBold");
            return seminegrita;
        }
    }

    public static Font Simbolos
    {
        get
        {
            if (simbolos == null) simbolos = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return simbolos;
        }
    }

    public static Color Oscurecer(Color color, float factor) =>
        new Color(color.r * factor, color.g * factor, color.b * factor, color.a);

    static Font Cargar(string ruta)
    {
        var fuente = Resources.Load<Font>(ruta);
        return fuente != null ? fuente : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }
}
