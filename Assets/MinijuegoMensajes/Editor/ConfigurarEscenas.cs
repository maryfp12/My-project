using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class ConfigurarEscenas
{
    const string EscenaInicio = "inicio 2.0";
    const string EscenaCartas = "Juego";
    const string EscenaSala = "SalaMinijuegos";
    const string NombrePuerta = "Door";
    const string Marca = "BumbleCrush.ConexionSala.v2:";

    static readonly string[] Orden =
    {
        "MainMenu", EscenaInicio, EscenaSala, EscenaCartas, "JuegoMensajes", "JuegoAtrapa"
    };

    static ConfigurarEscenas()
    {
        if (EditorPrefs.GetBool(Marca + Application.dataPath, false)) return;
        EditorApplication.delayCall += EjecutarCuandoSePueda;
    }

    [MenuItem("Bumble Crush/Configurar escenas del juego")]
    public static void Configurar() => Conectar();

    static void EjecutarCuandoSePueda()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += EjecutarCuandoSePueda;
            return;
        }
        Conectar();
    }

    static void Conectar()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Bumble Crush", "Detén el modo Play y vuelve a usar este menú.", "Listo");
            return;
        }

        var resumen = new StringBuilder();
        var faltantes = ConfigurarBuild();
        resumen.AppendLine("✓ Escenas del build en orden: MainMenu → inicio 2.0 → SalaMinijuegos → minijuegos.");

        resumen.AppendLine(EditarEscena(EscenaInicio, ConectarPuerta));
        resumen.AppendLine(EditarEscena(EscenaCartas, ConectarCartas));

        if (faltantes.Count > 0)
            resumen.AppendLine("\nTodavía no existen: " + string.Join(", ", faltantes) + ". Su puerta en la sala dirá «Muy pronto» hasta que alguien cree la escena y vuelva a usar el menú Bumble Crush.");

        EditorPrefs.SetBool(Marca + Application.dataPath, true);
        Debug.Log("[Bumble Crush]\n" + resumen);
        EditorUtility.DisplayDialog("Bumble Crush", resumen.ToString(), "Listo");
    }

    static List<string> ConfigurarBuild()
    {
        var escenas = new List<EditorBuildSettingsScene>();
        var agregadas = new HashSet<string>();
        var faltantes = new List<string>();

        foreach (var nombre in Orden)
        {
            string ruta = BuscarEscena(nombre);
            if (ruta == null)
            {
                faltantes.Add(nombre);
                continue;
            }
            escenas.Add(new EditorBuildSettingsScene(ruta, true));
            agregadas.Add(ruta);
        }

        foreach (var escena in EditorBuildSettings.scenes)
            if (!agregadas.Contains(escena.path) && File.Exists(escena.path))
                escenas.Add(escena);

        EditorBuildSettings.scenes = escenas.ToArray();
        return faltantes;
    }

    static string EditarEscena(string nombre, System.Func<Scene, string> cambio)
    {
        string ruta = BuscarEscena(nombre);
        if (ruta == null) return $"• No encontré la escena «{nombre}».";

        var escena = SceneManager.GetSceneByPath(ruta);
        bool yaAbierta = escena.IsValid() && escena.isLoaded;
        if (!yaAbierta) escena = EditorSceneManager.OpenScene(ruta, OpenSceneMode.Additive);

        string resultado = cambio(escena);
        if (escena.isDirty) EditorSceneManager.SaveScene(escena);
        if (!yaAbierta) EditorSceneManager.CloseScene(escena, true);
        return resultado;
    }

    static string ConectarPuerta(Scene escena)
    {
        var puerta = Buscar(escena, NombrePuerta);
        if (puerta == null) return $"• En «{EscenaInicio}» no encontré un objeto llamado «{NombrePuerta}».";
        if (puerta.GetComponent<PuertaEscena>() != null) return $"✓ La puerta de «{EscenaInicio}» ya llevaba a la sala.";

        var componente = puerta.AddComponent<PuertaEscena>();
        float escala = Mathf.Max(1f, puerta.transform.lossyScale.x);
        var datos = new SerializedObject(componente);
        datos.FindProperty("distancia").floatValue = 1.3f * escala;
        datos.FindProperty("distanciaToque").floatValue = 0.55f * escala;
        datos.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(escena);
        return $"✓ La puerta de «{EscenaInicio}» ahora lleva a la sala de minijuegos (al tocarla o con E).";
    }

    static string ConectarCartas(Scene escena)
    {
        foreach (var raiz in escena.GetRootGameObjects())
        {
            foreach (var componente in raiz.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (componente == null || componente.GetType().Name != "JuegoCartas") continue;
                var datos = new SerializedObject(componente);
                var campo = datos.FindProperty("escenaMenu");
                if (campo == null) return "• El juego de cartas no tiene el campo «escenaMenu».";
                if (campo.stringValue == EscenaSala) return "✓ El juego de cartas ya volvía a la sala.";

                campo.stringValue = EscenaSala;
                datos.ApplyModifiedPropertiesWithoutUndo();
                EditorSceneManager.MarkSceneDirty(escena);
                return "✓ El juego de cartas ahora vuelve a la sala de minijuegos.";
            }
        }
        return $"• En «{EscenaCartas}» no encontré el componente JuegoCartas.";
    }

    static GameObject Buscar(Scene escena, string nombre)
    {
        foreach (var raiz in escena.GetRootGameObjects())
            foreach (var t in raiz.GetComponentsInChildren<Transform>(true))
                if (t.name == nombre) return t.gameObject;
        return null;
    }

    static string BuscarEscena(string nombre)
    {
        foreach (var guid in AssetDatabase.FindAssets(nombre + " t:Scene"))
        {
            string ruta = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileNameWithoutExtension(ruta) == nombre) return ruta;
        }
        return null;
    }
}
