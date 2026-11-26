using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class JuegoMensajes : MonoBehaviour
{
    [Header("Reglas")]
    [SerializeField] int scoreInicial = 3;
    [SerializeField] float segundosPorCartel = 40f;
    [SerializeField] float segundosMinimos = 22f;
    [SerializeField] float reduccionPorCartel = 0.6f;
    [SerializeField] float intervaloInicial = 7f;
    [SerializeField] float intervaloMinimo = 3.5f;
    [SerializeField] float reduccionIntervalo = 0.15f;

    [Header("Escenas")]
    [SerializeField] string escenaMenu = "SalaMinijuegos";

    [Header("Personaje")]
    [SerializeField] GameObject personaje;
    [SerializeField] Texture2D texturaPersonaje;
    [SerializeField] float alturaPersonaje = 1.7f;

    const int CantidadPrevias = 4;
    const float EscalaPrevia = 0.35f;
    const float SeparacionPrevias = 0.78f;
    const float RadioRecoger = 2.1f;
    const float DuracionAviso = 1.3f;
    const float CintaDesde = -8f;
    const float CintaHasta = 5f;
    const float CintaZ = 7.5f;

    static readonly Vector3 InicioJugador = new Vector3(0f, 0.1f, -3f);
    static readonly Vector3 CentroPlaza = new Vector3(0f, 0f, 1f);
    static readonly Vector3 PosicionZonaNegativa = new Vector3(-9.6f, 0f, 1f);
    static readonly Vector3 PosicionZonaPositiva = new Vector3(9.6f, 0f, 1f);

    ListaMensajes pendientes;
    ListaMensajes enJuego;
    ListaMensajes historial;
    readonly List<Cartel> activos = new List<Cartel>();
    readonly TarjetaMensaje[] previas = new TarjetaMensaje[CantidadPrevias];
    readonly GameObject[] eslabones = new GameObject[CantidadPrevias - 1];
    Cartel cargado, resaltado;

    int score, aciertos, errores, vencidos, mejorAciertos;
    float segundosActuales, intervaloActual, tiempoSiguiente, tiempoPartida, tiempoAviso, escalaAviso = 1f, escalaScore = 1f;
    bool jugando;

    Camera camara;
    CamaraOrbita orbita;
    JugadorMovimiento jugador;
    PoseNino nino;
    CintaTransportadora cinta;
    ZonaClasificacion zonaPositiva, zonaNegativa;

    GameObject panelFin, panelConsejo, panelLectura;
    RectTransform insigniaScore, barraLectura;
    Image rellenoLectura;
    Text textoScore, textoRecord, textoRestantes, textoAviso, textoConsejo;
    Text lecturaUsuario, lecturaTexto, lecturaAccion;
    Text tituloFin, subtituloFin, textoRepaso, textoArchivo, numeroAciertos, numeroErrores, numeroFinal;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        ConfigurarCamaraYLuz();
        Parque.Construir();
        cinta = CintaTransportadora.Crear(CintaDesde, CintaHasta, CintaZ);
        zonaNegativa = ZonaClasificacion.Crear(false, PosicionZonaNegativa, CentroPlaza, camara.transform);
        zonaPositiva = ZonaClasificacion.Crear(true, PosicionZonaPositiva, CentroPlaza, camara.transform);
        CrearEslabones();
        CrearJugador();
        orbita.Seguir(jugador.transform, 0f);
        ConstruirHud();
        mejorAciertos = RegistroPartidas.MejorAciertos();
        Iniciar();
        Transicion.Entrar();
    }

    void Update()
    {
        AnimarHud();

        var teclado = Keyboard.current;
        if (!jugando)
        {
            if (teclado == null) return;
            if (teclado.rKey.wasPressedThisFrame || teclado.enterKey.wasPressedThisFrame) Iniciar();
            else if (teclado.mKey.wasPressedThisFrame) IrAlMenu();
            return;
        }

        tiempoPartida += Time.deltaTime;
        GenerarCarteles(Time.deltaTime);

        var cercano = cargado == null ? CartelCercano() : null;
        Resaltar(cercano);

        bool llevando = cargado != null;
        zonaPositiva.Invitar(llevando);
        zonaNegativa.Invitar(llevando);

        if (llevando)
        {
            cargado.Sostener(PosicionCarga());
            var zona = ZonaDelJugador();
            if (zona != null)
            {
                Entregar(zona);
                return;
            }
        }

        if (PresionoInteractuar())
        {
            if (cargado != null) SoltarEnSuelo();
            else if (cercano != null) Recoger(cercano);
        }

        ActualizarLectura(cargado != null ? cargado : cercano);
    }

    void Iniciar()
    {
        LimpiarCarteles();
        pendientes = ArchivoMensajes.Cargar();
        pendientes.Mezclar();
        enJuego = new ListaMensajes();
        historial = new ListaMensajes();

        score = scoreInicial;
        aciertos = 0;
        errores = 0;
        vencidos = 0;
        segundosActuales = segundosPorCartel;
        intervaloActual = intervaloInicial;
        tiempoSiguiente = 1.5f;
        tiempoPartida = 0f;
        jugando = true;

        jugador.Teletransportar(InicioJugador, Quaternion.identity);
        jugador.PuedeMoverse = true;
        panelFin.SetActive(false);
        MostrarConsejo("");
        Avisar("¡Lleva cada cartel al panal correcto!", Color.white);
        ActualizarPrevias(false);
        ActualizarHud();
    }

    void GenerarCarteles(float dt)
    {
        tiempoSiguiente -= dt;
        if (tiempoSiguiente > 0f || pendientes.EstaVacia || cinta.Llena) return;

        var nodo = pendientes.SacarPrimero();
        enJuego.AgregarAlFinal(nodo);

        var cartel = Cartel.Crear(nodo, cinta.Entrada, segundosActuales, camara.transform, AlVencer);
        cinta.Agregar(cartel);
        activos.Add(cartel);

        segundosActuales = Mathf.Max(segundosMinimos, segundosActuales - reduccionPorCartel);
        intervaloActual = Mathf.Max(intervaloMinimo, intervaloActual - reduccionIntervalo);
        tiempoSiguiente = intervaloActual;

        ActualizarPrevias(true);
        ActualizarHud();
    }

    Cartel CartelCercano()
    {
        Cartel mejor = null;
        float mejorDistancia = RadioRecoger;
        var posicion = jugador.transform.position;
        foreach (var cartel in activos)
        {
            if (cartel == null || !cartel.Recogible) continue;
            var diferencia = cartel.transform.position - posicion;
            diferencia.y = 0f;
            float distancia = diferencia.magnitude;
            if (distancia >= mejorDistancia) continue;
            mejorDistancia = distancia;
            mejor = cartel;
        }
        return mejor;
    }

    void Resaltar(Cartel cartel)
    {
        if (resaltado == cartel) return;
        if (resaltado != null) resaltado.Resaltar(false);
        if (cartel != null) cartel.Resaltar(true);
        resaltado = cartel;
    }

    void Recoger(Cartel cartel)
    {
        cinta.Quitar(cartel);
        cartel.Resaltar(false);
        resaltado = null;
        cartel.Cargar();
        cargado = cartel;
        nino.Cargando = true;
    }

    void SoltarEnSuelo()
    {
        var suelo = jugador.transform.position + jugador.transform.forward * 1.3f;
        suelo.y = 0f;
        cargado.Soltar(Parque.DentroDeLaPlaza(suelo, 0.8f));
        cargado = null;
        nino.Cargando = false;
    }

    void Entregar(ZonaClasificacion zona)
    {
        var cartel = cargado;
        cargado = null;
        nino.Cargando = false;
        activos.Remove(cartel);

        bool acierto = cartel.Nodo.EsPositivo == zona.EsPositiva;
        zona.Destellar(acierto);
        if (acierto) nino.Celebrar();
        else nino.Negar();
        Avisar(acierto ? "¡Correcto!  +1" : "¡Ups!  −1", acierto ? Estilo.Verde : Estilo.Rojo);

        cartel.Lanzar(zona.PuntoEntrega, 1.4f, 0.1f, true, null);
        ActualizarLectura(null);
        Registrar(cartel.Nodo, acierto, false);
    }

    void AlVencer(Cartel cartel)
    {
        if (!jugando) return;
        if (cartel == cargado)
        {
            cargado = null;
            nino.Cargando = false;
        }
        cinta.Quitar(cartel);
        activos.Remove(cartel);

        Avisar("¡Se venció un cartel!  −1", Estilo.Rojo);
        nino.Negar();
        cartel.Explotar();
        Registrar(cartel.Nodo, false, true);
    }

    void Registrar(NodoMensaje nodo, bool acierto, bool vencido)
    {
        nodo.Acertado = acierto;
        enJuego.Quitar(nodo);
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
            if (vencido) vencidos++;
            MostrarConsejo((nodo.EsPositivo ? "Ese mensaje era POSITIVO. " : "Ese mensaje era NEGATIVO. ") + nodo.Consejo);
        }

        escalaScore = 1.35f;
        ActualizarHud();
        if (score <= 0) Terminar(false);
        else if (pendientes.EstaVacia && enJuego.EstaVacia) Terminar(true);
    }

    void Terminar(bool gano)
    {
        jugando = false;
        jugador.PuedeMoverse = false;
        LimpiarCarteles();
        MostrarConsejo("");
        ActualizarLectura(null);

        bool guardado = RegistroPartidas.Guardar(gano, aciertos, errores, vencidos, Mathf.Max(score, 0), tiempoPartida);
        mejorAciertos = Mathf.Max(mejorAciertos, aciertos);

        tituloFin.text = gano ? "¡Ganaste!" : "¡Perdiste!";
        tituloFin.color = gano ? Estilo.Verde : Estilo.Rojo;
        subtituloFin.text = gano
            ? "Llevaste todos los carteles sin quedarte sin puntos."
            : "Tu score llegó a 0. ¡Inténtalo otra vez!";
        if (vencidos > 0) subtituloFin.text += $"  ({vencidos} se vencieron)";
        numeroAciertos.text = aciertos.ToString();
        numeroErrores.text = errores.ToString();
        numeroFinal.text = Mathf.Max(score, 0).ToString();
        textoArchivo.text = guardado
            ? $"Partida guardada en {RegistroPartidas.NombreArchivo}  ·  Partidas jugadas: {RegistroPartidas.CantidadPartidas()}  ·  Récord de aciertos: {mejorAciertos}"
            : $"No se pudo guardar {RegistroPartidas.NombreArchivo}  ·  Récord de aciertos: {mejorAciertos}";

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
        textoRepaso.text = mostrados == 0 ? "¡No fallaste ningún cartel!" : "<b>Para repasar:</b>\n" + repaso;
        panelFin.SetActive(true);
        ActualizarHud();
    }

    void IrAlMenu()
    {
        if (Transicion.Ir(escenaMenu)) return;
        string aviso = $"Agrega \"{escenaMenu}\" a las escenas del build";
        if (panelFin.activeSelf) subtituloFin.text = aviso;
        else Avisar(aviso, Color.white);
    }

    void LimpiarCarteles()
    {
        foreach (var cartel in activos)
            if (cartel != null) Destroy(cartel.gameObject);
        activos.Clear();
        cinta.Vaciar();
        cargado = null;
        resaltado = null;
        if (nino != null) nino.Cargando = false;
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

    Vector3 PosicionPrevia(int indice) => cinta.Tolva + new Vector3(0f, 0.75f + indice * SeparacionPrevias, 0f);

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

    void CrearEslabones()
    {
        for (int i = 0; i < eslabones.Length; i++)
        {
            var centro = (PosicionPrevia(i) + PosicionPrevia(i + 1)) * 0.5f + Vector3.forward * 0.08f;
            eslabones[i] = ConstructorEscena.Bloque(PrimitiveType.Capsule, "Siguiente", null, centro, new Vector3(0.08f, 0.12f, 0.08f), Estilo.MielOscura);
        }
    }

    Vector3 PosicionCarga() => jugador.transform.position + Vector3.up * (alturaPersonaje * 1.12f) + jugador.transform.forward * 0.15f;

    ZonaClasificacion ZonaDelJugador()
    {
        var posicion = jugador.transform.position;
        if (zonaPositiva.Contiene(posicion)) return zonaPositiva;
        if (zonaNegativa.Contiene(posicion)) return zonaNegativa;
        return null;
    }

    static bool PresionoInteractuar()
    {
        var teclado = Keyboard.current;
        if (teclado != null && teclado.eKey.wasPressedThisFrame) return true;
        var mando = Gamepad.current;
        return mando != null && mando.buttonSouth.wasPressedThisFrame;
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
        camara.fieldOfView = 55f;
        camara.nearClipPlane = 0.1f;
        camara.farClipPlane = 220f;
        camara.clearFlags = CameraClearFlags.SolidColor;
        camara.backgroundColor = Estilo.Cielo;
        var datos = camara.GetUniversalAdditionalCameraData();
        datos.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
        datos.renderShadows = true;
        orbita = camara.gameObject.AddComponent<CamaraOrbita>();

        var luz = new GameObject("Sol", typeof(Light)).GetComponent<Light>();
        luz.type = LightType.Directional;
        luz.color = new Color(1f, 0.96f, 0.88f);
        luz.intensity = 1.25f;
        luz.shadows = LightShadows.Soft;
        luz.shadowStrength = 0.55f;
        luz.transform.rotation = Quaternion.Euler(48f, -35f, 0f);

        RenderSettings.skybox = null;
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.68f, 0.79f, 0.96f);
        RenderSettings.ambientEquatorColor = new Color(0.70f, 0.76f, 0.64f);
        RenderSettings.ambientGroundColor = new Color(0.42f, 0.46f, 0.34f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = Estilo.Neblina;
        RenderSettings.fogStartDistance = 28f;
        RenderSettings.fogEndDistance = 95f;
        DynamicGI.UpdateEnvironment();
    }

    void CrearJugador()
    {
        jugador = FabricaJugador.CrearNino(InicioJugador, Quaternion.identity, alturaPersonaje, personaje, texturaPersonaje, camara.transform, out nino);
    }

    void ConstruirHud()
    {
        if (FindAnyObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

        var hud = ConstructorEscena.CanvasPantalla("Hud").transform;

        var titulo = ConstructorEscena.Texto(hud, "Titulo", Estilo.Negrita, 64, TextAnchor.UpperCenter, Estilo.Miel, new Vector2(0.3f, 0.87f), new Vector2(0.7f, 0.985f));
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
        textoRecord = ConstructorEscena.Texto(hud, "Record", Estilo.Negrita, 26, TextAnchor.UpperCenter, Color.white, Vector2.zero, Vector2.zero);
        ConstructorEscena.Fijar(textoRecord.rectTransform, new Vector2(0f, 1f), new Vector2(0.5f, 1f), new Vector2(125f, -224f), new Vector2(260f, 40f));
        ConstructorEscena.Contorno(textoRecord, Estilo.Cafe, 2.5f);

        var pastilla = ConstructorEscena.Imagen(hud, "Lista", SpritesUI.Redondeado, new Color(1f, 1f, 1f, 0.94f), Vector2.zero, Vector2.zero, 1.2f);
        ConstructorEscena.Fijar(pastilla.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-36f, -36f), new Vector2(470f, 84f));
        ConstructorEscena.Sombra(pastilla, new Color(0.15f, 0.3f, 0.55f, 0.35f), new Vector2(0f, -7f));
        var hexLista = ConstructorEscena.Imagen(pastilla.transform, "Icono", SpritesUI.Hexagono, Estilo.Miel, Vector2.zero, Vector2.zero);
        ConstructorEscena.Fijar(hexLista.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(18f, 0f), new Vector2(56f, 56f));
        textoRestantes = ConstructorEscena.Texto(pastilla.transform, "Texto", Estilo.SemiNegrita, 30, TextAnchor.MiddleLeft, Estilo.TextoOscuro, new Vector2(0.18f, 0f), new Vector2(0.97f, 1f));

        textoAviso = ConstructorEscena.Texto(hud, "Aviso", Estilo.Negrita, 84, TextAnchor.MiddleCenter, Color.white, new Vector2(0.1f, 0.56f), new Vector2(0.9f, 0.72f));
        ConstructorEscena.Contorno(textoAviso, new Color(0.15f, 0.15f, 0.25f, 0.9f), 5f);
        ConstructorEscena.Sombra(textoAviso, new Color(0f, 0f, 0f, 0.3f), new Vector2(0f, -8f));

        ConstruirControles(hud);
        ConstruirConsejo(hud);
        ConstruirLectura(hud);
        ConstruirPanelFin(hud);
    }

    void ConstruirControles(Transform hud)
    {
        var panel = ConstructorEscena.Imagen(hud, "Controles", SpritesUI.Redondeado, new Color(0.12f, 0.16f, 0.28f, 0.72f), Vector2.zero, Vector2.zero, 1.4f);
        ConstructorEscena.Fijar(panel.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(28f, 28f), new Vector2(330f, 210f));
        var texto = ConstructorEscena.Texto(panel.transform, "Texto", Estilo.SemiNegrita, 22, TextAnchor.MiddleLeft, Color.white, new Vector2(0.07f, 0.05f), new Vector2(0.95f, 0.95f));
        texto.lineSpacing = 1.15f;
        texto.text = "<b>WASD</b>  moverse\n<b>Shift</b>  correr\n<b>E</b>  recoger / soltar\n<b>Clic derecho</b>  girar cámara\n<b>Rueda</b>  acercar / alejar";
    }

    void ConstruirConsejo(Transform hud)
    {
        var panel = ConstructorEscena.Imagen(hud, "Consejo", SpritesUI.Redondeado, Estilo.Crema, Vector2.zero, Vector2.zero, 0.9f);
        ConstructorEscena.Fijar(panel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -150f), new Vector2(1100f, 116f));
        ConstructorEscena.Contorno(panel, Estilo.Miel, 5f);
        ConstructorEscena.Sombra(panel, new Color(0f, 0f, 0f, 0.25f), new Vector2(0f, -8f));
        panelConsejo = panel.gameObject;

        var hex = ConstructorEscena.Imagen(panel.transform, "Icono", SpritesUI.Hexagono, Estilo.Miel, Vector2.zero, Vector2.zero);
        ConstructorEscena.Fijar(hex.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(22f, 0f), new Vector2(80f, 80f));
        var exclamacion = ConstructorEscena.Texto(hex.transform, "Signo", Estilo.Negrita, 48, TextAnchor.MiddleCenter, Color.white, Vector2.zero, Vector2.one);
        exclamacion.text = "!";
        ConstructorEscena.Contorno(exclamacion, Estilo.Cafe, 3f);

        textoConsejo = ConstructorEscena.Texto(panel.transform, "Texto", Estilo.SemiNegrita, 28, TextAnchor.MiddleLeft, Estilo.TextoOscuro, new Vector2(0.11f, 0.08f), new Vector2(0.97f, 0.92f));
    }

    void ConstruirLectura(Transform hud)
    {
        var panel = ConstructorEscena.Imagen(hud, "Lectura", SpritesUI.Redondeado, Color.white, Vector2.zero, Vector2.zero, 0.8f);
        ConstructorEscena.Fijar(panel.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(60f, 32f), new Vector2(1180f, 200f));
        ConstructorEscena.Contorno(panel, Estilo.BordeBurbuja, 5f);
        ConstructorEscena.Sombra(panel, new Color(0.1f, 0.2f, 0.4f, 0.3f), new Vector2(0f, -9f));
        panelLectura = panel.gameObject;

        lecturaUsuario = ConstructorEscena.Texto(panel.transform, "Usuario", Estilo.Negrita, 28, TextAnchor.MiddleLeft, new Color(0.22f, 0.38f, 0.75f), new Vector2(0.04f, 0.74f), new Vector2(0.6f, 0.95f));
        lecturaAccion = ConstructorEscena.Texto(panel.transform, "Accion", Estilo.Negrita, 26, TextAnchor.MiddleRight, Estilo.MielOscura, new Vector2(0.4f, 0.74f), new Vector2(0.96f, 0.95f));
        lecturaTexto = ConstructorEscena.Texto(panel.transform, "Texto", Estilo.SemiNegrita, 38, TextAnchor.MiddleCenter, Estilo.TextoOscuro, new Vector2(0.04f, 0.2f), new Vector2(0.96f, 0.76f));
        lecturaTexto.verticalOverflow = VerticalWrapMode.Truncate;
        lecturaTexto.resizeTextForBestFit = true;
        lecturaTexto.resizeTextMinSize = 24;
        lecturaTexto.resizeTextMaxSize = 40;

        var barra = ConstructorEscena.Imagen(panel.transform, "Tiempo", SpritesUI.Redondeado, new Color(0.9f, 0.92f, 0.96f), new Vector2(0.04f, 0.08f), new Vector2(0.96f, 0.15f), 3.5f);
        rellenoLectura = ConstructorEscena.Imagen(barra.transform, "Restante", SpritesUI.Redondeado, Estilo.Miel, Vector2.zero, Vector2.one, 3.5f);
        barraLectura = rellenoLectura.rectTransform;
        panelLectura.SetActive(false);
    }

    void ConstruirPanelFin(Transform hud)
    {
        panelFin = ConstructorEscena.Imagen(hud, "Fin", null, new Color(0.08f, 0.14f, 0.3f, 0.55f), Vector2.zero, Vector2.one).gameObject;
        panelFin.GetComponent<Image>().raycastTarget = true;

        var tarjeta = ConstructorEscena.Imagen(panelFin.transform, "Tarjeta", SpritesUI.Redondeado, Estilo.Crema, Vector2.zero, Vector2.zero, 0.6f);
        ConstructorEscena.Fijar(tarjeta.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1260f, 900f));
        ConstructorEscena.Contorno(tarjeta, Estilo.Miel, 8f);
        ConstructorEscena.Sombra(tarjeta, new Color(0f, 0f, 0f, 0.3f), new Vector2(0f, -14f));
        var t = tarjeta.transform;

        tituloFin = ConstructorEscena.Texto(t, "Titulo", Estilo.Negrita, 110, TextAnchor.MiddleCenter, Estilo.Verde, new Vector2(0f, 0.85f), new Vector2(1f, 0.99f));
        ConstructorEscena.Contorno(tituloFin, Color.white, 6f);
        ConstructorEscena.Sombra(tituloFin, new Color(0f, 0f, 0f, 0.25f), new Vector2(0f, -8f));
        subtituloFin = ConstructorEscena.Texto(t, "Subtitulo", Estilo.SemiNegrita, 34, TextAnchor.MiddleCenter, Estilo.TextoOscuro, new Vector2(0.05f, 0.78f), new Vector2(0.95f, 0.86f));

        numeroAciertos = Insignia(t, 0.27f, Estilo.Verde, Estilo.VerdeOscuro, "ACIERTOS");
        numeroErrores = Insignia(t, 0.5f, Estilo.Rojo, Estilo.RojoOscuro, "ERRORES");
        numeroFinal = Insignia(t, 0.73f, Estilo.Miel, Estilo.MielOscura, "SCORE");

        textoArchivo = ConstructorEscena.Texto(t, "Archivo", Estilo.SemiNegrita, 26, TextAnchor.MiddleCenter, new Color(0.35f, 0.38f, 0.5f), new Vector2(0.04f, 0.455f), new Vector2(0.96f, 0.5f));

        var caja = ConstructorEscena.Imagen(t, "Repaso", SpritesUI.Redondeado, Color.white, new Vector2(0.06f, 0.18f), new Vector2(0.94f, 0.44f), 1.2f);
        textoRepaso = ConstructorEscena.Texto(caja.transform, "Texto", Estilo.SemiNegrita, 28, TextAnchor.MiddleLeft, Estilo.TextoOscuro, new Vector2(0.04f, 0.06f), new Vector2(0.96f, 0.94f));

        Boton(t, "JUGAR DE NUEVO (R)", Estilo.Verde, Estilo.VerdeOscuro, new Vector2(-400f, 40f), Iniciar);
        Boton(t, "VER ARCHIVOS CSV", Estilo.Miel, Estilo.MielOscura, new Vector2(0f, 40f), RegistroPartidas.AbrirCarpeta);
        Boton(t, "SALA (M)", Estilo.Azul, Estilo.AzulOscuro, new Vector2(400f, 40f), IrAlMenu);

        panelFin.SetActive(false);
    }

    static Text Insignia(Transform padre, float x, Color color, Color oscuro, string etiqueta)
    {
        var raiz = ConstructorEscena.Rect(padre, etiqueta, Vector2.zero, Vector2.zero);
        ConstructorEscena.Fijar(raiz, new Vector2(x, 0.65f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(140f, 140f));
        var sombra = ConstructorEscena.Imagen(raiz, "Sombra", SpritesUI.Hexagono, oscuro, Vector2.zero, Vector2.one);
        sombra.rectTransform.anchoredPosition = new Vector2(0f, -7f);
        ConstructorEscena.Imagen(raiz, "Hexagono", SpritesUI.Hexagono, color, Vector2.zero, Vector2.one);
        var numero = ConstructorEscena.Texto(raiz, "Numero", Estilo.Negrita, 60, TextAnchor.MiddleCenter, Color.white, Vector2.zero, Vector2.one);
        ConstructorEscena.Contorno(numero, oscuro, 4f);
        var texto = ConstructorEscena.Texto(raiz, "Etiqueta", Estilo.Negrita, 24, TextAnchor.UpperCenter, oscuro, new Vector2(-0.5f, -0.3f), new Vector2(1.5f, -0.04f));
        texto.text = etiqueta;
        return numero;
    }

    static void Boton(Transform padre, string texto, Color color, Color oscuro, Vector2 posicion, UnityAction accion)
    {
        var fondo = ConstructorEscena.Imagen(padre, texto, SpritesUI.Redondeado, color, Vector2.zero, Vector2.zero, 0.8f);
        ConstructorEscena.Fijar(fondo.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), posicion, new Vector2(380f, 100f));
        ConstructorEscena.Sombra(fondo, oscuro, new Vector2(0f, -9f));
        fondo.raycastTarget = true;
        var boton = fondo.gameObject.AddComponent<Button>();
        boton.targetGraphic = fondo;
        boton.onClick.AddListener(accion);
        var etiqueta = ConstructorEscena.Texto(fondo.transform, "Texto", Estilo.Negrita, 32, TextAnchor.MiddleCenter, Color.white, Vector2.zero, Vector2.one);
        etiqueta.text = texto;
        ConstructorEscena.Contorno(etiqueta, oscuro, 3f);
    }

    void ActualizarHud()
    {
        textoScore.text = Mathf.Max(score, 0).ToString();
        textoRecord.text = $"Récord: {mejorAciertos}";
        textoRestantes.text = $"En la lista: {pendientes.Cantidad}  ·  En juego: {enJuego.Cantidad}";
    }

    void ActualizarLectura(Cartel cartel)
    {
        if (cartel == null)
        {
            panelLectura.SetActive(false);
            return;
        }
        panelLectura.SetActive(true);
        lecturaUsuario.text = cartel.Nodo.Usuario;
        lecturaTexto.text = cartel.Nodo.Texto;
        lecturaAccion.text = cartel == cargado ? "Llévalo al panal correcto  ·  E: soltar" : "E: recoger";

        float restante = cartel.Restante01;
        barraLectura.anchorMax = new Vector2(restante, 1f);
        rellenoLectura.color = restante > 0.3f ? Estilo.Miel : Color.Lerp(Estilo.Rojo, Estilo.Miel, restante / 0.3f);
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

        if (tiempoAviso <= 0f) return;
        tiempoAviso -= Time.deltaTime;
        var color = textoAviso.color;
        color.a = Mathf.Clamp01(tiempoAviso / 0.3f);
        textoAviso.color = color;
    }
}
