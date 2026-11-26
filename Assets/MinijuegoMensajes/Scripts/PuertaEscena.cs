using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PuertaEscena : MonoBehaviour
{
    [SerializeField] string escena = "SalaMinijuegos";
    [SerializeField] string destino = "la sala de minijuegos";
    [SerializeField] float distancia = 7f;
    [SerializeField] float distanciaToque = 3f;
    [SerializeField] bool entrarAlAcercarse = true;

    Transform jugador;
    Renderer[] renderers;
    GameObject panel;
    Text aviso;

    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
        BuscarJugador();
        ConstruirAviso();
    }

    void Update()
    {
        if (jugador == null)
        {
            BuscarJugador();
            if (jugador == null) return;
        }

        var diferencia = Centro() - jugador.position;
        diferencia.y = 0f;
        bool cerca = diferencia.magnitude < distancia;
        panel.SetActive(cerca);
        if (!cerca) return;

        bool disponible = Transicion.Disponible(escena);
        bool tocando = entrarAlAcercarse && diferencia.magnitude < distanciaToque;
        aviso.text = !disponible
            ? $"Falta agregar \"{escena}\" a las escenas del build"
            : tocando ? $"Entrando a {destino}…" : $"Toca la puerta o presiona <b>E</b>  ·  {destino}";

        if (disponible && (tocando || PresionoEntrar())) Transicion.Ir(escena);
    }

    void BuscarJugador()
    {
        var etiquetado = GameObject.FindWithTag("Player");
        if (etiquetado != null) jugador = etiquetado.transform;
        else if (Camera.main != null) jugador = Camera.main.transform;
    }

    Vector3 Centro()
    {
        if (renderers.Length == 0) return transform.position;
        var limites = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) limites.Encapsulate(renderers[i].bounds);
        return limites.center;
    }

    static bool PresionoEntrar()
    {
        var teclado = Keyboard.current;
        if (teclado != null && teclado.eKey.wasPressedThisFrame) return true;
        var mando = Gamepad.current;
        return mando != null && mando.buttonSouth.wasPressedThisFrame;
    }

    void ConstruirAviso()
    {
        var lienzo = ConstructorEscena.CanvasPantalla("AvisoPuerta");
        lienzo.sortingOrder = 50;
        var fondo = ConstructorEscena.Imagen(lienzo.transform, "Panel", SpritesUI.Redondeado, Estilo.Crema, Vector2.zero, Vector2.zero, 0.9f);
        ConstructorEscena.Fijar(fondo.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 60f), new Vector2(820f, 110f));
        ConstructorEscena.Contorno(fondo, Estilo.Miel, 5f);
        ConstructorEscena.Sombra(fondo, new Color(0f, 0f, 0f, 0.25f), new Vector2(0f, -8f));
        aviso = ConstructorEscena.Texto(fondo.transform, "Texto", Estilo.SemiNegrita, 36, TextAnchor.MiddleCenter, Estilo.TextoOscuro, new Vector2(0.04f, 0.1f), new Vector2(0.96f, 0.9f));
        panel = fondo.gameObject;
        panel.SetActive(false);
    }
}
