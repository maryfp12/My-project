using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Transicion : MonoBehaviour
{
    const float Duracion = 0.45f;

    static readonly Color ColorVelo = new Color(0.08f, 0.1f, 0.18f);

    Image velo;
    bool saliendo;

    public static bool Disponible(string escena) => !string.IsNullOrEmpty(escena) && Application.CanStreamedLevelBeLoaded(escena);

    public static bool Ir(string escena)
    {
        if (!Disponible(escena))
        {
            Debug.LogWarning($"La escena \"{escena}\" no está en la lista de escenas del build. Usa el menú Bumble Crush > Configurar escenas del juego.");
            return false;
        }

        var actual = FindAnyObjectByType<Transicion>();
        if (actual != null && actual.saliendo) return true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        var transicion = Crear(0f);
        transicion.saliendo = true;
        transicion.StartCoroutine(transicion.Salir(escena));
        return true;
    }

    public static void Entrar()
    {
        var transicion = Crear(1f);
        transicion.StartCoroutine(transicion.Aparecer());
    }

    static Transicion Crear(float opacidad)
    {
        var lienzo = ConstructorEscena.CanvasPantalla("Transicion");
        lienzo.sortingOrder = 1000;
        var transicion = lienzo.gameObject.AddComponent<Transicion>();
        transicion.velo = ConstructorEscena.Imagen(lienzo.transform, "Velo", null, ColorVelo, Vector2.zero, Vector2.one);
        transicion.velo.raycastTarget = true;
        transicion.Pintar(opacidad);
        return transicion;
    }

    IEnumerator Salir(string escena)
    {
        for (float t = 0f; t < Duracion; t += Time.unscaledDeltaTime)
        {
            Pintar(t / Duracion);
            yield return null;
        }
        Pintar(1f);
        SceneManager.LoadScene(escena);
    }

    IEnumerator Aparecer()
    {
        yield return null;
        for (float t = 0f; t < Duracion; t += Time.unscaledDeltaTime)
        {
            Pintar(1f - t / Duracion);
            yield return null;
        }
        Destroy(gameObject);
    }

    void Pintar(float opacidad)
    {
        var color = ColorVelo;
        color.a = Mathf.Clamp01(opacidad);
        velo.color = color;
        velo.raycastTarget = opacidad > 0.05f;
    }
}
