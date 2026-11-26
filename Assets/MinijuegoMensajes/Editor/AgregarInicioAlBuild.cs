using System.Linq;
using UnityEditor;

[InitializeOnLoad]
public static class AgregarInicioAlBuild
{
    const string Ruta = "Assets/Scenes/inicio 2.0.unity";

    static AgregarInicioAlBuild()
    {
        EditorApplication.delayCall += Agregar;
    }

    static void Agregar()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Ruta) == null) return;

        var escenas = EditorBuildSettings.scenes.ToList();
        if (escenas.Any(e => e.path == Ruta)) return;

        int posicion = escenas.FindIndex(e => e.path.EndsWith("/MainMenu.unity")) + 1;
        escenas.Insert(posicion, new EditorBuildSettingsScene(Ruta, true));
        EditorBuildSettings.scenes = escenas.ToArray();
        AssetDatabase.SaveAssets();
        UnityEngine.Debug.Log("[Bumble Crush] Escena «inicio 2.0» agregada a la lista de escenas del build.");
    }
}
