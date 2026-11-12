using System;
using UnityEngine;
using UnityEngine.UI;

public class TarjetaMensaje : MonoBehaviour
{
    enum Estado { Quieta, Llegando, Leyendo, Volando }

    const float DuracionLlegada = 0.55f;
    const float DuracionVuelo = 0.5f;

    static readonly Color ColorUsuario = new Color(0.22f, 0.38f, 0.75f);
    static readonly Color ColorHora = new Color(0.6f, 0.63f, 0.72f);
    static readonly Color ColorBarraFondo = new Color(0.9f, 0.92f, 0.96f);
    static readonly Color ColorLinea = new Color(0.85f, 0.87f, 0.91f);

    public NodoMensaje Nodo { get; private set; }
    public bool Clasificable => estado == Estado.Llegando || estado == Estado.Leyendo;

    Estado estado;
    Vector3 origen, destino, escalaOrigen, escalaDestino, objetivoQuieta;
    float progreso, duracionLectura, tiempoLectura, alturaArco, fase;
    bool girar, moviendo;
    RectTransform relleno;
    Image imagenRelleno;
    Action<TarjetaMensaje> alAgotarse;
    Action alTerminar;

    public static TarjetaMensaje Crear(NodoMensaje nodo, bool previa)
    {
        var raiz = new GameObject(previa ? "MensajeEnLista" : "Mensaje");
        var t = raiz.transform;

        ConstructorEscena.Figura("Borde", Mallas.Burbuja, t, new Vector3(0f, -0.03f, 0.05f), Quaternion.identity, new Vector3(1.045f, 1.08f, 0.8f), Estilo.BordeBurbuja);
        ConstructorEscena.Figura("Burbuja", Mallas.Burbuja, t, Vector3.zero, Quaternion.identity, Vector3.one, Color.white);

        var lienzo = ConstructorEscena.CanvasMundo("Contenido", t, new Vector3(0f, 0f, -0.08f), new Vector2(720f, 360f), 0.005f).transform;

        var colorAvatar = Color.HSVToRGB(UnityEngine.Random.value, 0.5f, 0.95f);
        var avatar = ConstructorEscena.Imagen(lienzo, "Avatar", SpritesUI.Circulo, colorAvatar, Vector2.zero, Vector2.zero);
        ConstructorEscena.Fijar(avatar.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -24f), new Vector2(66f, 66f));
        var inicial = ConstructorEscena.Texto(avatar.transform, "Inicial", Estilo.Negrita, 36, TextAnchor.MiddleCenter, Color.white, Vector2.zero, Vector2.one);
        inicial.text = nodo.Usuario.Length > 1 ? nodo.Usuario.Substring(1, 1).ToUpperInvariant() : "?";

        var usuario = ConstructorEscena.Texto(lienzo, "Usuario", Estilo.Negrita, 32, TextAnchor.MiddleLeft, ColorUsuario, Vector2.zero, Vector2.zero);
        ConstructorEscena.Fijar(usuario.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(110f, -24f), new Vector2(430f, 66f));
        usuario.text = nodo.Usuario;

        var hora = ConstructorEscena.Texto(lienzo, "Hora", Estilo.SemiNegrita, 26, TextAnchor.MiddleRight, ColorHora, Vector2.zero, Vector2.zero);
        ConstructorEscena.Fijar(hora.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-32f, -24f), new Vector2(160f, 66f));
        hora.text = "ahora";

        var tarjeta = raiz.AddComponent<TarjetaMensaje>();
        tarjeta.Nodo = nodo;
        tarjeta.fase = UnityEngine.Random.value * 10f;

        if (previa)
        {
            ConstructorEscena.Imagen(lienzo, "Linea", SpritesUI.Redondeado, ColorLinea, new Vector2(0.06f, 0.52f), new Vector2(0.94f, 0.64f), 2.5f);
            ConstructorEscena.Imagen(lienzo, "Linea", SpritesUI.Redondeado, ColorLinea, new Vector2(0.06f, 0.33f), new Vector2(0.78f, 0.45f), 2.5f);
            ConstructorEscena.Imagen(lienzo, "Linea", SpritesUI.Redondeado, ColorLinea, new Vector2(0.06f, 0.14f), new Vector2(0.55f, 0.26f), 2.5f);
            return tarjeta;
        }

        var cuerpo = ConstructorEscena.Texto(lienzo, "Texto", Estilo.SemiNegrita, 40, TextAnchor.MiddleCenter, Estilo.TextoOscuro, new Vector2(0.05f, 0.19f), new Vector2(0.95f, 0.75f));
        cuerpo.verticalOverflow = VerticalWrapMode.Truncate;
        cuerpo.resizeTextForBestFit = true;
        cuerpo.resizeTextMinSize = 24;
        cuerpo.resizeTextMaxSize = 42;
        cuerpo.lineSpacing = 0.95f;
        cuerpo.text = nodo.Texto;

        var barra = ConstructorEscena.Imagen(lienzo, "Tiempo", SpritesUI.Redondeado, ColorBarraFondo, new Vector2(0.06f, 0.065f), new Vector2(0.94f, 0.125f), 4f);
        tarjeta.imagenRelleno = ConstructorEscena.Imagen(barra.transform, "Restante", SpritesUI.Redondeado, Estilo.Miel, Vector2.zero, Vector2.one, 4f);
        tarjeta.relleno = tarjeta.imagenRelleno.rectTransform;
        return tarjeta;
    }

    public void Presentar(Vector3 lectura, float segundos, Action<TarjetaMensaje> agotado)
    {
        origen = transform.position;
        escalaOrigen = transform.localScale;
        destino = lectura;
        escalaDestino = Vector3.one;
        duracionLectura = Mathf.Max(0.5f, segundos);
        tiempoLectura = 0f;
        progreso = 0f;
        alAgotarse = agotado;
        estado = Estado.Llegando;
    }

    public void Lanzar(Vector3 hacia, float arco, float escalaFinal, bool conGiro, Action terminado)
    {
        origen = transform.position;
        escalaOrigen = transform.localScale;
        destino = hacia;
        escalaDestino = Vector3.one * escalaFinal;
        alturaArco = arco;
        girar = conGiro;
        progreso = 0f;
        alTerminar = terminado;
        estado = Estado.Volando;
    }

    public void MoverA(Vector3 objetivo)
    {
        objetivoQuieta = objetivo;
        moviendo = true;
    }

    void Update()
    {
        fase += Time.deltaTime;
        switch (estado)
        {
            case Estado.Quieta:
                if (moviendo)
                {
                    var meta = objetivoQuieta + Vector3.up * Mathf.Sin(fase * 2f) * 0.04f;
                    transform.position = Vector3.Lerp(transform.position, meta, 1f - Mathf.Exp(-8f * Time.deltaTime));
                }
                break;
            case Estado.Llegando:
                Llegar();
                break;
            case Estado.Leyendo:
                Leer();
                break;
            case Estado.Volando:
                Volar();
                break;
        }
    }

    void Llegar()
    {
        progreso += Time.deltaTime / DuracionLlegada;
        float t = Mathf.Clamp01(progreso);
        float k = 1f - Mathf.Pow(1f - t, 3f);
        transform.position = Vector3.Lerp(origen, destino, k) + Vector3.up * Mathf.Sin(t * Mathf.PI) * 0.5f;
        transform.localScale = Vector3.LerpUnclamped(escalaOrigen, escalaDestino, k) * (1f + Mathf.Sin(t * Mathf.PI) * 0.06f);
        ActualizarBarra(1f);
        if (t >= 1f) estado = Estado.Leyendo;
    }

    void Leer()
    {
        tiempoLectura += Time.deltaTime;
        float restante = Mathf.Clamp01(1f - tiempoLectura / duracionLectura);
        ActualizarBarra(restante);

        transform.position = destino + Vector3.up * Mathf.Sin(fase * 2f) * 0.05f;
        float temblor = restante < 0.25f ? Mathf.Sin(fase * 32f) * 2.5f : Mathf.Sin(fase * 1.3f) * 1.2f;
        transform.rotation = Quaternion.Euler(0f, 0f, temblor);

        if (restante > 0f) return;
        estado = Estado.Quieta;
        alAgotarse?.Invoke(this);
    }

    void ActualizarBarra(float restante)
    {
        if (relleno == null) return;
        relleno.anchorMax = new Vector2(restante, 1f);
        imagenRelleno.color = restante > 0.3f ? Estilo.Miel : Color.Lerp(Estilo.Rojo, Estilo.Miel, restante / 0.3f);
    }

    void Volar()
    {
        progreso += Time.deltaTime / DuracionVuelo;
        float t = Mathf.Clamp01(progreso);
        float k = t * t * (3f - 2f * t);
        transform.position = Vector3.Lerp(origen, destino, k) + Vector3.up * Mathf.Sin(t * Mathf.PI) * alturaArco;
        transform.localScale = Vector3.Lerp(escalaOrigen, escalaDestino, k);
        if (girar) transform.Rotate(0f, 0f, 600f * Time.deltaTime, Space.Self);

        if (t < 1f) return;
        estado = Estado.Quieta;
        alTerminar?.Invoke();
        Destroy(gameObject);
    }
}
