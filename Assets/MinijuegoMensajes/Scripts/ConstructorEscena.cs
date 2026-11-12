using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class ConstructorEscena
{
    static Material plantilla;
    static readonly Dictionary<Color, Material> materiales = new Dictionary<Color, Material>();

    public static Material MaterialDe(Color color)
    {
        if (materiales.TryGetValue(color, out var existente) && existente != null) return existente;
        var material = NuevoMaterial(color);
        materiales[color] = material;
        return material;
    }

    public static Material NuevoMaterial(Color color, float brillo = 0.2f)
    {
        if (plantilla == null)
        {
            var temporal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plantilla = temporal.GetComponent<Renderer>().sharedMaterial;
            Object.Destroy(temporal);
        }
        var material = new Material(plantilla) { color = color };
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", brillo);
        return material;
    }

    public static GameObject Bloque(PrimitiveType tipo, string nombre, Transform padre, Vector3 posicion, Vector3 escala, Color color, bool conCollider = false)
    {
        return Bloque(tipo, nombre, padre, posicion, Quaternion.identity, escala, color, conCollider);
    }

    public static GameObject Bloque(PrimitiveType tipo, string nombre, Transform padre, Vector3 posicion, Quaternion rotacion, Vector3 escala, Color color, bool conCollider = false)
    {
        var go = GameObject.CreatePrimitive(tipo);
        go.name = nombre;
        if (!conCollider) Object.Destroy(go.GetComponent<Collider>());
        Ubicar(go.transform, padre, posicion, rotacion, escala);
        go.GetComponent<Renderer>().sharedMaterial = MaterialDe(color);
        return go;
    }

    public static GameObject Figura(string nombre, Mesh malla, Transform padre, Vector3 posicion, Quaternion rotacion, Vector3 escala, Color color)
    {
        var go = new GameObject(nombre, typeof(MeshFilter), typeof(MeshRenderer));
        go.GetComponent<MeshFilter>().sharedMesh = malla;
        go.GetComponent<MeshRenderer>().sharedMaterial = MaterialDe(color);
        Ubicar(go.transform, padre, posicion, rotacion, escala);
        return go;
    }

    public static void Pintar(Renderer renderer, Color color) => renderer.material.color = color;

    static void Ubicar(Transform t, Transform padre, Vector3 posicion, Quaternion rotacion, Vector3 escala)
    {
        t.SetParent(padre, false);
        t.localPosition = posicion;
        t.localRotation = rotacion;
        t.localScale = escala;
    }

    public static Canvas CanvasMundo(string nombre, Transform padre, Vector3 posicionLocal, Vector2 tamano, float escala)
    {
        var go = new GameObject(nombre, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        go.transform.SetParent(padre, false);
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        go.GetComponent<CanvasScaler>().dynamicPixelsPerUnit = 2f;
        var rt = (RectTransform)go.transform;
        rt.sizeDelta = tamano;
        rt.localPosition = posicionLocal;
        rt.localRotation = Quaternion.identity;
        rt.localScale = Vector3.one * escala;
        return canvas;
    }

    public static Canvas CanvasPantalla(string nombre)
    {
        var go = new GameObject(nombre, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var escalador = go.GetComponent<CanvasScaler>();
        escalador.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escalador.referenceResolution = new Vector2(1920f, 1080f);
        escalador.matchWidthOrHeight = 0.5f;
        return canvas;
    }

    public static RectTransform Rect(Transform padre, string nombre, Vector2 anclaMin, Vector2 anclaMax)
    {
        var go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(padre, false);
        var rt = (RectTransform)go.transform;
        Estirar(rt, anclaMin, anclaMax);
        return rt;
    }

    public static Text Texto(Transform padre, string nombre, Font fuente, int tamano, TextAnchor alineacion, Color color, Vector2 anclaMin, Vector2 anclaMax)
    {
        var go = new GameObject(nombre, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(padre, false);
        Estirar((RectTransform)go.transform, anclaMin, anclaMax);
        var texto = go.GetComponent<Text>();
        texto.font = fuente;
        texto.fontSize = tamano;
        texto.alignment = alineacion;
        texto.color = color;
        texto.horizontalOverflow = HorizontalWrapMode.Wrap;
        texto.verticalOverflow = VerticalWrapMode.Overflow;
        texto.raycastTarget = false;
        return texto;
    }

    public static Image Imagen(Transform padre, string nombre, Sprite sprite, Color color, Vector2 anclaMin, Vector2 anclaMax, float multiplicador = 1f)
    {
        var go = new GameObject(nombre, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(padre, false);
        Estirar((RectTransform)go.transform, anclaMin, anclaMax);
        var imagen = go.GetComponent<Image>();
        imagen.sprite = sprite;
        imagen.color = color;
        imagen.raycastTarget = false;
        if (sprite != null && sprite.border != Vector4.zero)
        {
            imagen.type = Image.Type.Sliced;
            imagen.pixelsPerUnitMultiplier = multiplicador;
        }
        else
        {
            imagen.preserveAspect = true;
        }
        return imagen;
    }

    public static void Fijar(RectTransform rt, Vector2 ancla, Vector2 pivote, Vector2 posicion, Vector2 tamano)
    {
        rt.anchorMin = ancla;
        rt.anchorMax = ancla;
        rt.pivot = pivote;
        rt.anchoredPosition = posicion;
        rt.sizeDelta = tamano;
    }

    public static void Contorno(Graphic grafico, Color color, float grosor)
    {
        var contorno = grafico.gameObject.AddComponent<Outline>();
        contorno.effectColor = color;
        contorno.effectDistance = new Vector2(grosor, -grosor);
    }

    public static void Sombra(Graphic grafico, Color color, Vector2 distancia)
    {
        var sombra = grafico.gameObject.AddComponent<Shadow>();
        sombra.effectColor = color;
        sombra.effectDistance = distancia;
    }

    static void Estirar(RectTransform rt, Vector2 anclaMin, Vector2 anclaMax)
    {
        rt.anchorMin = anclaMin;
        rt.anchorMax = anclaMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
