using System;
using UnityEngine;
using UnityEngine.UI;

public class Cartel : MonoBehaviour
{
    enum Estado { EnCinta, EnSuelo, Cargado, Volando }

    public const float AltoPoste = 0.6f;

    const float AnchoTablero = 2.3f;
    const float AltoTablero = 1.3f;
    const float VelocidadCinta = 1.6f;
    const float DuracionVuelo = 0.55f;

    static readonly Color ColorUsuario = new Color(0.22f, 0.38f, 0.75f);
    static readonly Color ColorBarraFondo = new Color(0.9f, 0.92f, 0.96f);

    public NodoMensaje Nodo { get; private set; }
    public bool Recogible => !vencido && (estado == Estado.EnCinta || estado == Estado.EnSuelo);
    public float Restante01 => tiempoTotal > 0f ? Mathf.Clamp01(tiempoRestante / tiempoTotal) : 0f;

    Estado estado;
    Transform tablero, camara;
    Renderer marco;
    RectTransform relleno;
    Image imagenRelleno;
    Vector3 objetivo, origenVuelo, destinoVuelo, escalaOrigen, escalaFinal;
    float tiempoTotal, tiempoRestante, progreso, arco, fase;
    bool girar, resaltado, vencido;
    Action<Cartel> alVencer;
    Action alTerminar;

    public static Cartel Crear(NodoMensaje nodo, Vector3 posicion, float segundos, Transform camara, Action<Cartel> vencido)
    {
        var raiz = new GameObject("Cartel");
        raiz.transform.position = posicion;
        var t = raiz.transform;

        ConstructorEscena.Bloque(PrimitiveType.Cylinder, "Poste", t, new Vector3(0f, AltoPoste * 0.5f, 0f), new Vector3(0.09f, AltoPoste * 0.5f, 0.09f), Estilo.Tronco);

        var tablero = new GameObject("Tablero").transform;
        tablero.SetParent(t, false);
        tablero.localPosition = new Vector3(0f, AltoPoste + AltoTablero * 0.5f, 0f);
        var marco = ConstructorEscena.Bloque(PrimitiveType.Cube, "Marco", tablero, new Vector3(0f, 0f, 0.01f), new Vector3(AnchoTablero, AltoTablero, 0.1f), Estilo.Miel);
        ConstructorEscena.Bloque(PrimitiveType.Cube, "Cara", tablero, new Vector3(0f, 0f, -0.02f), new Vector3(AnchoTablero - 0.16f, AltoTablero - 0.16f, 0.08f), Color.white);

        var lienzo = ConstructorEscena.CanvasMundo("Contenido", tablero, new Vector3(0f, 0f, -0.065f), new Vector2(840f, 460f), 0.0025f).transform;

        var colorAvatar = Color.HSVToRGB(UnityEngine.Random.value, 0.5f, 0.95f);
        var avatar = ConstructorEscena.Imagen(lienzo, "Avatar", SpritesUI.Circulo, colorAvatar, Vector2.zero, Vector2.zero);
        ConstructorEscena.Fijar(avatar.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -22f), new Vector2(72f, 72f));
        var inicial = ConstructorEscena.Texto(avatar.transform, "Inicial", Estilo.Negrita, 40, TextAnchor.MiddleCenter, Color.white, Vector2.zero, Vector2.one);
        inicial.text = nodo.Usuario.Length > 1 ? nodo.Usuario.Substring(1, 1).ToUpperInvariant() : "?";

        var usuario = ConstructorEscena.Texto(lienzo, "Usuario", Estilo.Negrita, 36, TextAnchor.MiddleLeft, ColorUsuario, Vector2.zero, Vector2.zero);
        ConstructorEscena.Fijar(usuario.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(116f, -22f), new Vector2(600f, 72f));
        usuario.text = nodo.Usuario;

        var cuerpo = ConstructorEscena.Texto(lienzo, "Texto", Estilo.SemiNegrita, 52, TextAnchor.MiddleCenter, Estilo.TextoOscuro, new Vector2(0.04f, 0.19f), new Vector2(0.96f, 0.78f));
        cuerpo.verticalOverflow = VerticalWrapMode.Truncate;
        cuerpo.resizeTextForBestFit = true;
        cuerpo.resizeTextMinSize = 28;
        cuerpo.resizeTextMaxSize = 54;
        cuerpo.text = nodo.Texto;

        var barra = ConstructorEscena.Imagen(lienzo, "Tiempo", SpritesUI.Redondeado, ColorBarraFondo, new Vector2(0.05f, 0.06f), new Vector2(0.95f, 0.12f), 3.5f);
        var cartel = raiz.AddComponent<Cartel>();
        cartel.imagenRelleno = ConstructorEscena.Imagen(barra.transform, "Restante", SpritesUI.Redondeado, Estilo.Miel, Vector2.zero, Vector2.one, 3.5f);
        cartel.relleno = cartel.imagenRelleno.rectTransform;
        cartel.Nodo = nodo;
        cartel.tablero = tablero;
        cartel.marco = marco.GetComponent<Renderer>();
        cartel.camara = camara;
        cartel.tiempoTotal = segundos;
        cartel.tiempoRestante = segundos;
        cartel.alVencer = vencido;
        cartel.fase = UnityEngine.Random.value * 10f;
        cartel.MirarCamara(1f);
        return cartel;
    }

    public void MoverEnCinta(Vector3 destino)
    {
        estado = Estado.EnCinta;
        objetivo = destino;
    }

    public void Cargar() => estado = Estado.Cargado;

    public void Sostener(Vector3 posicion)
    {
        transform.position = Vector3.Lerp(transform.position, posicion, 1f - Mathf.Exp(-18f * Time.deltaTime));
    }

    public void Soltar(Vector3 suelo)
    {
        estado = Estado.EnSuelo;
        transform.position = suelo;
    }

    public void Resaltar(bool activo)
    {
        if (resaltado == activo) return;
        resaltado = activo;
        ConstructorEscena.Pintar(marco, activo ? Estilo.MielClara : Estilo.Miel);
        if (!activo) tablero.localScale = Vector3.one;
    }

    public void Lanzar(Vector3 hacia, float altura, float escala, bool conGiro, Action terminado)
    {
        origenVuelo = transform.position;
        destinoVuelo = hacia;
        escalaOrigen = transform.localScale;
        escalaFinal = Vector3.one * escala;
        arco = altura;
        girar = conGiro;
        progreso = 0f;
        alTerminar = terminado;
        estado = Estado.Volando;
    }

    public void Explotar()
    {
        ConstructorEscena.Pintar(marco, Estilo.Rojo);
        Lanzar(transform.position + Vector3.up * 0.4f, 0.3f, 0.01f, true, null);
    }

    void Update()
    {
        float dt = Time.deltaTime;
        fase += dt;
        if (estado == Estado.Volando)
        {
            Volar(dt);
            return;
        }

        if (!vencido)
        {
            tiempoRestante -= dt;
            ActualizarBarra();
            if (tiempoRestante <= 0f)
            {
                vencido = true;
                alVencer?.Invoke(this);
                return;
            }
        }

        if (estado == Estado.EnCinta) transform.position = Vector3.MoveTowards(transform.position, objetivo, VelocidadCinta * dt);
        MirarCamara(dt);

        tablero.localScale = Vector3.one * (resaltado ? 1.06f + Mathf.Sin(fase * 8f) * 0.04f : 1f);
        float temblor = Restante01 < 0.25f ? Mathf.Sin(fase * 30f) * 2.5f : 0f;
        tablero.localRotation = Quaternion.Euler(0f, 0f, temblor);
    }

    void MirarCamara(float dt)
    {
        if (camara == null) return;
        var direccion = transform.position - camara.position;
        direccion.y = 0f;
        if (direccion.sqrMagnitude < 0.01f) return;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direccion), 1f - Mathf.Exp(-10f * dt));
    }

    void ActualizarBarra()
    {
        float restante = Restante01;
        relleno.anchorMax = new Vector2(restante, 1f);
        imagenRelleno.color = restante > 0.3f ? Estilo.Miel : Color.Lerp(Estilo.Rojo, Estilo.Miel, restante / 0.3f);
    }

    void Volar(float dt)
    {
        progreso += dt / DuracionVuelo;
        float t = Mathf.Clamp01(progreso);
        float k = t * t * (3f - 2f * t);
        transform.position = Vector3.Lerp(origenVuelo, destinoVuelo, k) + Vector3.up * Mathf.Sin(t * Mathf.PI) * arco;
        transform.localScale = Vector3.Lerp(escalaOrigen, escalaFinal, k);
        if (girar) transform.Rotate(0f, 540f * dt, 0f, Space.World);

        if (t < 1f) return;
        alTerminar?.Invoke();
        Destroy(gameObject);
    }
}
