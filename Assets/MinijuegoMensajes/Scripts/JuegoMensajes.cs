using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class JuegoMensajes : MonoBehaviour
{
    class Panal
    {
        public Transform raiz, icono;
        public Renderer tapa;
        public Color color;
        public float fase;
        public Vector3 Objetivo => raiz.position + Vector3.up * 1.55f;
    }

    [Header("Reglas")]
    [SerializeField] int scoreInicial = 3;
    [SerializeField] float segundosPorMensaje = 9f;
    [SerializeField] float segundosMinimos = 4f;
    [SerializeField] float aceleracionPorMensaje = 0.2f;

    [Header("Escenas")]
    [SerializeField] string escenaMenu = "MainMenu";

    [Header("Personaje")]
    [SerializeField] GameObject personaje;
    [SerializeField] Texture2D texturaPersonaje;
    [SerializeField] float alturaPersonaje = 1.7f;

    const int CantidadPrevias = 4;
    const float EscalaPrevia = 0.3f;
    const float DuracionAviso = 1.1f;

    static readonly Vector3 PosicionLectura = new Vector3(0f, 3.4f, -0.7f);
    static readonly Vector3 PosicionNino = new Vector3(0f, 0f, -1.7f);
    static readonly Vector3 PosicionCamara = new Vector3(0f, 2.65f, -8.2f);
    static readonly Vector3 ObjetivoCamara = new Vector3(0f, 2f, 0f);
    static readonly Vector3 PosicionPanalNegativo = new Vector3(-3.4f, 0f, -1.4f);
    static readonly Vector3 PosicionPanalPositivo = new Vector3(3.4f, 0f, -1.4f);

    ListaMensajes pendientes;
    ListaMensajes historial;
    TarjetaMensaje actual;
    readonly TarjetaMensaje[] previas = new TarjetaMensaje[CantidadPrevias];
    readonly GameObject[] eslabones = new GameObject[CantidadPrevias - 1];

    int score, aciertos, errores, partida;
    float segundosActuales, tiempoAviso, escalaAviso = 1f, escalaScore = 1f;
    bool jugando;

    Camera camara;
    PoseNino nino;
    Panal panalPositivo, panalNegativo;

    GameObject panelFin, panelConsejo;
    RectTransform insigniaScore;
    Text textoScore, textoRestantes, textoAviso, textoConsejo, tituloFin, subtituloFin, textoRepaso;
    Text numeroAciertos, numeroErrores, numeroFinal;

    static Vector3 PosicionPrevia(int indice) => new Vector3(3.35f + indice * 1.3f, 4.7f, 2.6f);

    void Start()
    {
        ConfigurarCamaraYLuz();
        Parque.Construir();
        panalNegativo = CrearPanal("Negativo", PosicionPanalNegativo, Estilo.Rojo, Estilo.RojoOscuro, "NEGATIVO", "A", "←", false);
        panalPositivo = CrearPanal("Positivo", PosicionPanalPositivo, Estilo.Verde, Estilo.VerdeOscuro, "POSITIVO", "D", "→", true);
        CrearEslabones();
        ColocarNino();
        ConstruirHud();
        Iniciar();
    }

    void Update()
    {
        AnimarPanales();
        AnimarHud();

        var teclado = Keyboard.current;
        if (!jugando)
        {
            if (teclado == null) return;
            if (teclado.rKey.wasPressedThisFrame || teclado.enterKey.wasPressedThisFrame) Iniciar();
            else if (teclado.mKey.wasPressedThisFrame) IrAlMenu();
            return;
        }

        if (actual == null || !actual.Clasificable) return;

        if (teclado != null)
        {
            if (teclado.aKey.wasPressedThisFrame || teclado.leftArrowKey.wasPressedThisFrame)
            {
                Clasificar(false);
                return;
            }
            if (teclado.dKey.wasPressedThisFrame || teclado.rightArrowKey.wasPressedThisFrame)
            {
                Clasificar(true);
                return;
            }
        }

        var raton = Mouse.current;
        if (raton == null || !raton.leftButton.wasPressedThisFrame) return;
        if (!Physics.Raycast(camara.ScreenPointToRay(raton.position.ReadValue()), out var golpe, 100f)) return;
        if (golpe.transform.IsChildOf(panalPositivo.raiz)) Clasificar(true);
        else if (golpe.transform.IsChildOf(panalNegativo.raiz)) Clasificar(false);
    }

    void Iniciar()
    {
        LimpiarTarjetas();
        pendientes = BancoMensajes.Crear();
        pendientes.Mezclar();
        historial = new ListaMensajes();
        score = scoreInicial;
        aciertos = 0;
        errores = 0;
        segundosActuales = segundosPorMensaje;
        jugando = true;
        panelFin.SetActive(false);
        MostrarConsejo("");
        Avisar("¡Clasifica los mensajes!", Color.white);
        ActualizarPrevias(false);
        StartCoroutine(SeguirLuego(partida, 0.6f));
        ActualizarHud();
    }

    void SiguienteTarjeta()
    {
        if (!jugando) return;
        if (pendientes.EstaVacia)
        {
            Terminar();
            return;
        }

        var nodo = pendientes.SacarPrimero();
        actual = TarjetaMensaje.Crear(nodo, false);
        actual.transform.position = PosicionPrevia(0);
        actual.transform.localScale = Vector3.one * EscalaPrevia;
        actual.Presentar(PosicionLectura, segundosActuales, AlAgotarse);
        segundosActuales = Mathf.Max(segundosMinimos, segundosActuales - aceleracionPorMensaje);

        if (nino != null) nino.AvisarMensaje();
        ActualizarPrevias(true);
        ActualizarHud();
    }

    void Clasificar(bool eligioPositivo)
    {
        var tarjeta = actual;
        actual = null;
        bool acierto = tarjeta.Nodo.EsPositivo == eligioPositivo;
        var panal = eligioPositivo ? panalPositivo : panalNegativo;

        StartCoroutine(Destellar(panal, acierto));
        if (nino != null)
        {
            nino.Senalar(panal.Objetivo, 1f);
            if (acierto) nino.Celebrar();
            else nino.Negar();
        }
        Avisar(acierto ? "¡Correcto!  +1" : "¡Ups!  −1", acierto ? Estilo.Verde : Estilo.Rojo);

        int ronda = partida;
        tarjeta.Lanzar(panal.Objetivo, 1.2f, 0.1f, true, () => StartCoroutine(SeguirLuego(ronda, 0.3f)));
        Registrar(tarjeta.Nodo, acierto);
    }

    void AlAgotarse(TarjetaMensaje tarjeta)
    {
        if (tarjeta != actual) return;
        actual = null;
        Avisar("¡Se te pasó!  −1", Estilo.Rojo);
        if (nino != null) nino.Negar();

        int ronda = partida;
        tarjeta.Lanzar(PosicionNino + Vector3.up * (alturaPersonaje + 0.2f), 0.2f, 0.05f, false, () => StartCoroutine(SeguirLuego(ronda, 0.3f)));
        Registrar(tarjeta.Nodo, false);
    }

    void Registrar(NodoMensaje nodo, bool acierto)
    {
        nodo.Acertado = acierto;
        historial.AgregarAlFinal(nodo);

        if (acierto)
        {
            score++;
            aciertos++;
            MostrarConsejo("");
        }
        else
        {
            score--;
            errores++;
            MostrarConsejo((nodo.EsPositivo ? "Ese mensaje era POSITIVO. " : "Ese mensaje era NEGATIVO. ") + nodo.Consejo);
        }

        escalaScore = 1.35f;
        ActualizarHud();
        if (score <= 0) Terminar();
    }

    IEnumerator SeguirLuego(int ronda, float segundos)
    {
        yield return new WaitForSeconds(segundos);
        if (ronda == partida) SiguienteTarjeta();
    }

    void Terminar()
    {
        jugando = false;
        LimpiarTarjetas();
        MostrarConsejo("");

        bool gano = score > 0;
        tituloFin.text = gano ? "¡Ganaste!" : "¡Perdiste!";
        tituloFin.color = gano ? Estilo.Verde : Estilo.Rojo;
        subtituloFin.text = gano
            ? "Clasificaste toda la lista sin quedarte sin puntos."
            : "Tu score llegó a 0. ¡Inténtalo otra vez!";
        numeroAciertos.text = aciertos.ToString();
        numeroErrores.text = errores.ToString();
        numeroFinal.text = Mathf.Max(score, 0).ToString();

        var repaso = new StringBuilder();
        int mostrados = 0;
        foreach (var nodo in historial.Recorrer())
        {
            if (nodo.Acertado) continue;
            if (mostrados == 3)
            {
                repaso.Append("…");
                break;
            }
            string tipo = nodo.EsPositivo ? "positivo" : "negativo";
            repaso.AppendLine($"• \"{nodo.Texto}\"  (era <b>{tipo}</b>)");
            mostrados++;
        }
        textoRepaso.text = mostrados == 0 ? "¡No fallaste ningún mensaje!" : "<b>Para repasar:</b>\n" + repaso;
        panelFin.SetActive(true);
    }

    void IrAlMenu()
    {
        if (Application.CanStreamedLevelBeLoaded(escenaMenu))
        {
            SceneManager.LoadScene(escenaMenu);
            return;
        }
        string aviso = $"Agrega \"{escenaMenu}\" a las escenas del build";
        if (panelFin.activeSelf) subtituloFin.text = aviso;
        else Avisar(aviso, Color.white);
    }

    void LimpiarTarjetas()
    {
        partida++;
        if (actual != null) Destroy(actual.gameObject);
        actual = null;
        BorrarPrevias();
    }

    void BorrarPrevias()
    {
        for (int i = 0; i < previas.Length; i++)
        {
            if (previas[i] != null) Destroy(previas[i].gameObject);
            previas[i] = null;
        }
    }

    void ActualizarPrevias(bool desplazar)
    {
        BorrarPrevias();
        var nodo = pendientes.Cabeza;
        int cantidad = 0;
        for (int i = 0; i < CantidadPrevias && nodo != null; i++, nodo = nodo.Siguiente)
        {
            var previa = TarjetaMensaje.Crear(nodo, true);
            previa.transform.position = PosicionPrevia(desplazar ? i + 1 : i);
            previa.transform.localScale = Vector3.one * EscalaPrevia;
            previa.MoverA(PosicionPrevia(i));
            previas[i] = previa;
            cantidad++;
        }
        for (int i = 0; i < eslabones.Length; i++) eslabones[i].SetActive(i < cantidad - 1);
    }

    void ConfigurarCamaraYLuz()
    {
        camara = Camera.main;
        if (camara == null)
        {
            var go = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            go.tag = "MainCamera";
            camara = go.GetComponent<Camera>();
        }
        camara.transform.SetPositionAndRotation(PosicionCamara, Quaternion.LookRotation(ObjetivoCamara - PosicionCamara));
        camara.fieldOfView = 48f;
        camara.nearClipPlane = 0.1f;
        camara.farClipPlane = 200f;
        camara.clearFlags = CameraClearFlags.SolidColor;
        camara.backgroundColor = Estilo.Cielo;
        var datos = camara.GetUniversalAdditionalCameraData();
        datos.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
        datos.renderShadows = true;

        var luz = new GameObject("Sol", typeof(Light)).GetComponent<Light>();
        luz.type = LightType.Directional;
        luz.color = new Color(1f, 0.96f, 0.88f);
        luz.intensity = 1.25f;
        luz.shadows = LightShadows.Soft;
        luz.shadowStrength = 0.55f;
        luz.transform.rotation = Quaternion.Euler(42f, -38f, 0f);

        RenderSettings.skybox = null;
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.68f, 0.79f, 0.96f);
        RenderSettings.ambientEquatorColor = new Color(0.70f, 0.76f, 0.64f);
        RenderSettings.ambientGroundColor = new Color(0.42f, 0.46f, 0.34f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = Estilo.Neblina;
        RenderSettings.fogStartDistance = 24f;
        RenderSettings.fogEndDistance = 80f;
        DynamicGI.UpdateEnvironment();
    }

    void ColocarNino()
    {
        var modelo = personaje != null ? personaje : Resources.Load<GameObject>("Nino/Walking");
        if (modelo == null) return;

        var textura = texturaPersonaje;
        if (textura == null && personaje == null) textura = Resources.Load<Texture2D>("Nino/texture_diffuse");

        var instancia = Instantiate(modelo);
        instancia.name = "Nino";
        foreach (var componente in instancia.GetComponentsInChildren<MonoBehaviour>()) componente.enabled = false;
        foreach (var control in instancia.GetComponentsInChildren<CharacterController>()) control.enabled = false;
        foreach (var animador in instancia.GetComponentsInChildren<Animator>()) animador.enabled = false;
        foreach (var camaraExtra in instancia.GetComponentsInChildren<Camera>()) camaraExtra.enabled = false;
        foreach (var oido in instancia.GetComponentsInChildren<AudioListener>()) oido.enabled = false;

        if (textura != null)
        {
            var material = ConstructorEscena.NuevoMaterial(Color.white, 0.12f);
            material.mainTexture = textura;
            foreach (var r in instancia.GetComponentsInChildren<Renderer>())
            {
                var materiales = r.sharedMaterials;
                for (int i = 0; i < materiales.Length; i++) materiales[i] = material;
                r.sharedMaterials = materiales;
            }
        }

        nino = instancia.AddComponent<PoseNino>();
        nino.Preparar(PosicionNino, alturaPersonaje);
    }

    Panal CrearPanal(string nombre, Vector3 posicion, Color color, Color oscuro, string titulo, string tecla, string flecha, bool positivo)
    {
        var raiz = new GameObject("Panal" + nombre).transform;
        raiz.position = posicion;
        var colisionador = raiz.gameObject.AddComponent<BoxCollider>();
        colisionador.center = new Vector3(0f, 1.4f, 0f);
        colisionador.size = new Vector3(2.4f, 2.9f, 2.4f);

        var acostado = Quaternion.Euler(90f, 0f, 0f);
        ConstructorEscena.Figura("Base", Mallas.Hexagono, raiz, new Vector3(0f, 0.22f, 0f), acostado, new Vector3(1.15f, 1.15f, 0.44f), oscuro);
        var tapa = ConstructorEscena.Figura("Tapa", Mallas.Hexagono, raiz, new Vector3(0f, 0.5f, 0f), acostado, new Vector3(1f, 1f, 0.14f), color);

        var icono = new GameObject("Icono").transform;
        icono.SetParent(raiz, false);
        icono.localPosition = new Vector3(0f, 1.55f, 0f);
        var deFrente = Quaternion.Euler(90f, 0f, 0f);
        ConstructorEscena.Bloque(PrimitiveType.Cylinder, "Aro", icono, new Vector3(0f, 0f, 0.03f), deFrente, new Vector3(1.08f, 0.04f, 1.08f), oscuro);
        ConstructorEscena.Bloque(PrimitiveType.Cylinder, "Disco", icono, Vector3.zero, deFrente, new Vector3(0.95f, 0.05f, 0.95f), Color.white);

        if (positivo)
        {
            ConstructorEscena.Bloque(PrimitiveType.Cube, "Corazon", icono, new Vector3(0f, -0.05f, -0.08f), Quaternion.Euler(0f, 0f, 45f), new Vector3(0.36f, 0.36f, 0.16f), Estilo.Corazon);
            ConstructorEscena.Bloque(PrimitiveType.Sphere, "Corazon", icono, new Vector3(-0.127f, 0.077f, -0.08f), Quaternion.identity, new Vector3(0.36f, 0.36f, 0.16f), Estilo.Corazon);
            ConstructorEscena.Bloque(PrimitiveType.Sphere, "Corazon", icono, new Vector3(0.127f, 0.077f, -0.08f), Quaternion.identity, new Vector3(0.36f, 0.36f, 0.16f), Estilo.Corazon);
        }
        else
        {
            ConstructorEscena.Bloque(PrimitiveType.Cube, "Cruz", icono, new Vector3(0f, 0f, -0.08f), Quaternion.Euler(0f, 0f, 45f), new Vector3(0.6f, 0.15f, 0.14f), Estilo.Rojo);
            ConstructorEscena.Bloque(PrimitiveType.Cube, "Cruz", icono, new Vector3(0f, 0f, -0.08f), Quaternion.Euler(0f, 0f, -45f), new Vector3(0.6f, 0.15f, 0.14f), Estilo.Rojo);
        }

        var etiqueta = ConstructorEscena.CanvasMundo("Etiqueta", raiz, new Vector3(0f, 2.55f, 0f), new Vector2(460f, 230f), 0.0055f).transform;
        etiqueta.rotation = camara.transform.rotation;

        var textoTitulo = ConstructorEscena.Texto(etiqueta, "Titulo", Estilo.Negrita, 76, TextAnchor.MiddleCenter, Color.white, new Vector2(0f, 0.5f), Vector2.one);
        textoTitulo.text = titulo;
        ConstructorEscena.Contorno(textoTitulo, oscuro, 4f);
        ConstructorEscena.Sombra(textoTitulo, new Color(0f, 0f, 0f, 0.25f), new Vector2(0f, -6f));

        Tecla(etiqueta, tecla, Estilo.Negrita, new Vector2(-48f, 18f));
        Tecla(etiqueta, flecha, Estilo.Simbolos, new Vector2(48f, 18f));

        return new Panal { raiz = raiz, icono = icono, tapa = tapa.GetComponent<Renderer>(), color = color, fase = positivo ? 1.3f : 0f };
    }

    static void Tecla(Transform padre, string simbolo, Font fuente, Vector2 posicion)
    {
        var tecla = ConstructorEscena.Imagen(padre, "Tecla", SpritesUI.Redondeado, Color.white, Vector2.zero, Vector2.zero, 2.2f);
        ConstructorEscena.Fijar(tecla.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), posicion, new Vector2(80f, 80f));
        ConstructorEscena.Sombra(tecla, new Color(0.4f, 0.45f, 0.55f, 0.8f), new Vector2(0f, -7f));
        var texto = ConstructorEscena.Texto(tecla.transform, "Simbolo", fuente, 48, TextAnchor.MiddleCenter, Estilo.TextoOscuro, Vector2.zero, Vector2.one);
        texto.fontStyle = fuente == Estilo.Simbolos ? FontStyle.Bold : FontStyle.Normal;
        texto.text = simbolo;
    }

    void CrearEslabones()
    {
        for (int i = 0; i < eslabones.Length; i++)
        {
            var centro = (PosicionPrevia(i) + PosicionPrevia(i + 1)) * 0.5f;
            eslabones[i] = ConstructorEscena.Bloque(PrimitiveType.Capsule, "Siguiente", null, centro, Quaternion.Euler(0f, 0f, 90f), new Vector3(0.08f, 0.12f, 0.08f), Estilo.MielOscura);
        }
    }

    void AnimarPanales()
    {
        AnimarPanal(panalNegativo);
        AnimarPanal(panalPositivo);
    }

    static void AnimarPanal(Panal panal)
    {
        float t = Time.time * 1.4f + panal.fase;
        panal.icono.localRotation = Quaternion.Euler(0f, Mathf.Sin(t) * 18f, 0f);
        panal.icono.localPosition = new Vector3(0f, 1.55f + Mathf.Sin(t * 1.3f) * 0.06f, 0f);
    }

    IEnumerator Destellar(Panal panal, bool acierto)
    {
        const float duracion = 0.45f;
        var destello = acierto ? Color.white : Estilo.Oscurecer(panal.color, 0.4f);
        for (float t = 0f; t < duracion; t += Time.deltaTime)
        {
            float k = t / duracion;
            ConstructorEscena.Pintar(panal.tapa, Color.Lerp(destello, panal.color, k));
            panal.icono.localScale = Vector3.one * (1f + 0.35f * Mathf.Sin(k * Mathf.PI));
            yield return null;
        }
        ConstructorEscena.Pintar(panal.tapa, panal.color);
        panal.icono.localScale = Vector3.one;
    }

    void ConstruirHud()
    {
        if (FindAnyObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

        var hud = ConstructorEscena.CanvasPantalla("Hud").transform;

        var titulo = ConstructorEscena.Texto(hud, "Titulo", Estilo.Negrita, 68, TextAnchor.UpperCenter, Estilo.Miel, new Vector2(0.3f, 0.86f), new Vector2(0.7f, 0.985f));
        titulo.text = "BUMBLE CRUSH";
        ConstructorEscena.Contorno(titulo, Estilo.Cafe, 5f);
        ConstructorEscena.Sombra(titulo, new Color(1f, 1f, 1f, 0.9f), new Vector2(0f, -8f));

        insigniaScore = ConstructorEscena.Rect(hud, "Score", Vector2.zero, Vector2.zero);
        ConstructorEscena.Fijar(insigniaScore, new Vector2(0f, 1f), new Vector2(0.5f, 0.5f), new Vector2(125f, -120f), new Vector2(190f, 190f));
        var sombraHex = ConstructorEscena.Imagen(insigniaScore, "Sombra", SpritesUI.Hexagono, Estilo.MielOscura, Vector2.zero, Vector2.one);
        sombraHex.rectTransform.anchoredPosition = new Vector2(0f, -8f);
        ConstructorEscena.Imagen(insigniaScore, "Hexagono", SpritesUI.Hexagono, Estilo.Miel, Vector2.zero, Vector2.one);
        ConstructorEscena.Imagen(insigniaScore, "Brillo", SpritesUI.Hexagono, Estilo.MielClara, new Vector2(0.14f, 0.14f), new Vector2(0.86f, 0.86f));
        textoScore = ConstructorEscena.Texto(insigniaScore, "Numero", Estilo.Negrita, 78, TextAnchor.MiddleCenter, Color.white, new Vector2(0f, 0.22f), new Vector2(1f, 0.92f));
        ConstructorEscena.Contorno(textoScore, Estilo.Cafe, 4f);
        var etiquetaScore = ConstructorEscena.Texto(insigniaScore, "Etiqueta", Estilo.Negrita, 24, TextAnchor.MiddleCenter, Estilo.Cafe, new Vector2(0f, 0.12f), new Vector2(1f, 0.32f));
        etiquetaScore.text = "SCORE";

        var pastilla = ConstructorEscena.Imagen(hud, "Lista", SpritesUI.Redondeado, new Color(1f, 1f, 1f, 0.94f), Vector2.zero, Vector2.zero, 1.2f);
        ConstructorEscena.Fijar(pastilla.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-36f, -36f), new Vector2(430f, 84f));
        ConstructorEscena.Sombra(pastilla, new Color(0.15f, 0.3f, 0.55f, 0.35f), new Vector2(0f, -7f));
        var hexLista = ConstructorEscena.Imagen(pastilla.transform, "Icono", SpritesUI.Hexagono, Estilo.Miel, Vector2.zero, Vector2.zero);
        ConstructorEscena.Fijar(hexLista.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(18f, 0f), new Vector2(56f, 56f));
        textoRestantes = ConstructorEscena.Texto(pastilla.transform, "Texto", Estilo.SemiNegrita, 32, TextAnchor.MiddleLeft, Estilo.TextoOscuro, new Vector2(0.2f, 0f), new Vector2(0.97f, 1f));

        textoAviso = ConstructorEscena.Texto(hud, "Aviso", Estilo.Negrita, 88, TextAnchor.MiddleCenter, Color.white, new Vector2(0.15f, 0.56f), new Vector2(0.85f, 0.72f));
        ConstructorEscena.Contorno(textoAviso, new Color(0.15f, 0.15f, 0.25f, 0.9f), 5f);
        ConstructorEscena.Sombra(textoAviso, new Color(0f, 0f, 0f, 0.3f), new Vector2(0f, -8f));

        ConstruirConsejo(hud);
        ConstruirPanelFin(hud);
    }

    void ConstruirConsejo(Transform hud)
    {
        var panel = ConstructorEscena.Imagen(hud, "Consejo", SpritesUI.Redondeado, Estilo.Crema, Vector2.zero, Vector2.zero, 0.9f);
        ConstructorEscena.Fijar(panel.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 34f), new Vector2(1240f, 130f));
        ConstructorEscena.Contorno(panel, Estilo.Miel, 5f);
        ConstructorEscena.Sombra(panel, new Color(0f, 0f, 0f, 0.25f), new Vector2(0f, -8f));
        panelConsejo = panel.gameObject;

        var hex = ConstructorEscena.Imagen(panel.transform, "Icono", SpritesUI.Hexagono, Estilo.Miel, Vector2.zero, Vector2.zero);
        ConstructorEscena.Fijar(hex.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(24f, 0f), new Vector2(90f, 90f));
        var exclamacion = ConstructorEscena.Texto(hex.transform, "Signo", Estilo.Negrita, 54, TextAnchor.MiddleCenter, Color.white, Vector2.zero, Vector2.one);
        exclamacion.text = "!";
        ConstructorEscena.Contorno(exclamacion, Estilo.Cafe, 3f);

        textoConsejo = ConstructorEscena.Texto(panel.transform, "Texto", Estilo.SemiNegrita, 32, TextAnchor.MiddleLeft, Estilo.TextoOscuro, new Vector2(0.11f, 0.08f), new Vector2(0.97f, 0.92f));
    }

    void ConstruirPanelFin(Transform hud)
    {
        panelFin = ConstructorEscena.Imagen(hud, "Fin", null, new Color(0.08f, 0.14f, 0.3f, 0.55f), Vector2.zero, Vector2.one).gameObject;
        panelFin.GetComponent<Image>().raycastTarget = true;

        var tarjeta = ConstructorEscena.Imagen(panelFin.transform, "Tarjeta", SpritesUI.Redondeado, Estilo.Crema, Vector2.zero, Vector2.zero, 0.6f);
        ConstructorEscena.Fijar(tarjeta.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1180f, 860f));
        ConstructorEscena.Contorno(tarjeta, Estilo.Miel, 8f);
        ConstructorEscena.Sombra(tarjeta, new Color(0f, 0f, 0f, 0.3f), new Vector2(0f, -14f));
        var t = tarjeta.transform;

        tituloFin = ConstructorEscena.Texto(t, "Titulo", Estilo.Negrita, 110, TextAnchor.MiddleCenter, Estilo.Verde, new Vector2(0f, 0.84f), new Vector2(1f, 0.99f));
        ConstructorEscena.Contorno(tituloFin, Color.white, 6f);
        ConstructorEscena.Sombra(tituloFin, new Color(0f, 0f, 0f, 0.25f), new Vector2(0f, -8f));
        subtituloFin = ConstructorEscena.Texto(t, "Subtitulo", Estilo.SemiNegrita, 36, TextAnchor.MiddleCenter, Estilo.TextoOscuro, new Vector2(0.05f, 0.77f), new Vector2(0.95f, 0.85f));

        numeroAciertos = Insignia(t, 0.27f, Estilo.Verde, Estilo.VerdeOscuro, "ACIERTOS");
        numeroErrores = Insignia(t, 0.5f, Estilo.Rojo, Estilo.RojoOscuro, "ERRORES");
        numeroFinal = Insignia(t, 0.73f, Estilo.Miel, Estilo.MielOscura, "SCORE");

        var caja = ConstructorEscena.Imagen(t, "Repaso", SpritesUI.Redondeado, Color.white, new Vector2(0.06f, 0.2f), new Vector2(0.94f, 0.47f), 1.2f);
        textoRepaso = ConstructorEscena.Texto(caja.transform, "Texto", Estilo.SemiNegrita, 28, TextAnchor.MiddleLeft, Estilo.TextoOscuro, new Vector2(0.04f, 0.06f), new Vector2(0.96f, 0.94f));

        Boton(t, "JUGAR DE NUEVO  (R)", Estilo.Verde, Estilo.VerdeOscuro, new Vector2(-230f, 50f), Iniciar);
        Boton(t, "MENÚ  (M)", Estilo.Azul, Estilo.AzulOscuro, new Vector2(230f, 50f), IrAlMenu);

        panelFin.SetActive(false);
    }

    static Text Insignia(Transform padre, float x, Color color, Color oscuro, string etiqueta)
    {
        var raiz = ConstructorEscena.Rect(padre, etiqueta, Vector2.zero, Vector2.zero);
        ConstructorEscena.Fijar(raiz, new Vector2(x, 0.62f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(150f, 150f));
        var sombra = ConstructorEscena.Imagen(raiz, "Sombra", SpritesUI.Hexagono, oscuro, Vector2.zero, Vector2.one);
        sombra.rectTransform.anchoredPosition = new Vector2(0f, -7f);
        ConstructorEscena.Imagen(raiz, "Hexagono", SpritesUI.Hexagono, color, Vector2.zero, Vector2.one);
        var numero = ConstructorEscena.Texto(raiz, "Numero", Estilo.Negrita, 64, TextAnchor.MiddleCenter, Color.white, Vector2.zero, Vector2.one);
        ConstructorEscena.Contorno(numero, oscuro, 4f);
        var texto = ConstructorEscena.Texto(raiz, "Etiqueta", Estilo.Negrita, 26, TextAnchor.UpperCenter, oscuro, new Vector2(-0.5f, -0.32f), new Vector2(1.5f, -0.04f));
        texto.text = etiqueta;
        return numero;
    }

    static void Boton(Transform padre, string texto, Color color, Color oscuro, Vector2 posicion, UnityAction accion)
    {
        var fondo = ConstructorEscena.Imagen(padre, texto, SpritesUI.Redondeado, color, Vector2.zero, Vector2.zero, 0.8f);
        ConstructorEscena.Fijar(fondo.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), posicion, new Vector2(420f, 110f));
        ConstructorEscena.Sombra(fondo, oscuro, new Vector2(0f, -9f));
        fondo.raycastTarget = true;
        var boton = fondo.gameObject.AddComponent<Button>();
        boton.targetGraphic = fondo;
        boton.onClick.AddListener(accion);
        var etiqueta = ConstructorEscena.Texto(fondo.transform, "Texto", Estilo.Negrita, 38, TextAnchor.MiddleCenter, Color.white, Vector2.zero, Vector2.one);
        etiqueta.text = texto;
        ConstructorEscena.Contorno(etiqueta, oscuro, 3f);
    }

    void ActualizarHud()
    {
        textoScore.text = Mathf.Max(score, 0).ToString();
        textoRestantes.text = $"Mensajes en la lista: {pendientes.Cantidad}";
    }

    void Avisar(string texto, Color color)
    {
        textoAviso.text = texto;
        textoAviso.color = color;
        tiempoAviso = DuracionAviso;
        escalaAviso = 1.4f;
    }

    void MostrarConsejo(string texto)
    {
        textoConsejo.text = texto;
        panelConsejo.SetActive(!string.IsNullOrEmpty(texto));
    }

    void AnimarHud()
    {
        float suavizado = 1f - Mathf.Exp(-12f * Time.deltaTime);
        escalaAviso = Mathf.Lerp(escalaAviso, 1f, suavizado);
        escalaScore = Mathf.Lerp(escalaScore, 1f, suavizado);
        textoAviso.transform.localScale = Vector3.one * escalaAviso;
        insigniaScore.localScale = Vector3.one * escalaScore;

        if (tiempoAviso > 0f)
        {
            tiempoAviso -= Time.deltaTime;
            var color = textoAviso.color;
            color.a = Mathf.Clamp01(tiempoAviso / 0.3f);
            textoAviso.color = color;
        }
    }
}
