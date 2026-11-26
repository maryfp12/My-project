using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class CrearPanelFinal
{
    const string carpeta = "Assets/VolverMenu/";

    static Sprite Cargar(string nombre)
    {
        string ruta = carpeta + nombre;
        var imp = AssetImporter.GetAtPath(ruta) as TextureImporter;
        if (imp != null && imp.textureType != TextureImporterType.Sprite)
        {
            imp.textureType = TextureImporterType.Sprite;
            imp.mipmapEnabled = false;
            imp.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
    }

    static RectTransform Nuevo(string nombre, Transform padre, Vector2 tam, Vector2 pos)
    {
        var go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(padre, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = tam;
        rt.anchoredPosition = pos;
        return rt;
    }

    static Image Imagen(string nombre, Transform padre, string sprite, Vector2 tam, Vector2 pos)
    {
        var rt = Nuevo(nombre, padre, tam, pos);
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = Cargar(sprite);
        return img;
    }

    static TMP_Text Texto(string nombre, Transform padre, string contenido, float tam, Vector2 caja, Vector2 pos, Color color)
    {
        var rt = Nuevo(nombre, padre, caja, pos);
        var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        t.text = contenido;
        t.fontSize = tam;
        t.alignment = TextAlignmentOptions.Center;
        t.fontStyle = FontStyles.Bold;
        t.color = color;
        t.raycastTarget = false;
        return t;
    }

    static Button Boton(string nombre, Transform padre, string sprite, string etiqueta, Vector2 pos)
    {
        var img = Imagen(nombre, padre, sprite, new Vector2(416, 120), pos);
        var btn = img.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;
        Texto("Texto", img.transform, etiqueta, 52, new Vector2(400, 100), new Vector2(0, 8), Color.white);
        return btn;
    }

    [MenuItem("Herramientas/Crear panel final")]
    static void Crear()
    {
        var canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null) { Debug.LogError("No hay ningún Canvas en la escena."); return; }

        Color cafe = new Color32(122, 62, 18, 255);

        var raiz = Nuevo("PanelFinal", canvas.transform, Vector2.zero, Vector2.zero);
        raiz.anchorMin = Vector2.zero;
        raiz.anchorMax = Vector2.one;
        raiz.offsetMin = raiz.offsetMax = Vector2.zero;
        var fondo = raiz.gameObject.AddComponent<Image>();
        fondo.color = new Color(0f, 0f, 0f, 0.6f);

        var ventana = Imagen("Ventana", raiz, "panel_fondo.png", new Vector2(800, 680), Vector2.zero);
        var cinta = Imagen("Cinta", ventana.transform, "banner.png", new Vector2(900, 220), new Vector2(0, 335));
        Texto("TextoTitulo", cinta.transform, "¡GANASTE!", 80, new Vector2(700, 110), new Vector2(0, 20), cafe);

        var detalle = Texto("TextoDetalle", ventana.transform, "Encontraste todos los pares", 40, new Vector2(700, 60), new Vector2(0, 150), cafe);

        var abeja = Imagen("Abeja", ventana.transform, "abeja.png", new Vector2(190, 190), new Vector2(290, -230));
        abeja.transform.localRotation = Quaternion.Euler(0, 0, -8);

        var panel = raiz.gameObject.AddComponent<PanelFinal>();
        panel.textoDetalle = detalle;

        var bReintentar = Boton("BotonReintentar", ventana.transform, "boton_verde.png", "REINTENTAR", new Vector2(0, -10));
        var bMenu = Boton("BotonMenu", ventana.transform, "boton_azul.png", "MENÚ", new Vector2(0, -170));
        UnityEventTools.AddPersistentListener(bReintentar.onClick, panel.Reintentar);
        UnityEventTools.AddPersistentListener(bMenu.onClick, panel.IrAlMenu);

        Undo.RegisterCreatedObjectUndo(raiz.gameObject, "Crear panel final");
        raiz.SetAsLastSibling();
        raiz.gameObject.SetActive(false);
        Selection.activeGameObject = raiz.gameObject;
        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
    }
}
