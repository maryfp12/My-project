using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class SalaMinijuegos : MonoBehaviour
{
    [Serializable]
    public class Minijuego
    {
        public string titulo;
        public string descripcion;
        public string escena;
        public Color color;
    }

    class Puerta
    {
        public string titulo, escena;
        public Vector3 frente;
        public bool disponible;
    }

    [Header("Minijuegos, de izquierda a derecha")]
    [SerializeField] Minijuego[] minijuegos =
    {
        new Minijuego { titulo = "Memoria", descripcion = "Encuentra las parejas de cartas", escena = "Juego", color = new Color(0.30f, 0.62f, 0.95f) },
        new Minijuego { titulo = "Clasifica", descripcion = "Lleva cada mensaje al panal correcto", escena = "JuegoMensajes", color = new Color(1f, 0.74f, 0.12f) },
        new Minijuego { titulo = "Atrapa", descripcion = "El tercer minijuego del equipo", escena = "JuegoAtrapa", color = new Color(0.38f, 0.80f, 0.38f) }
    };

    [Header("Salida")]
    [SerializeField] string escenaSalida = "inicio 2.0";

    [Header("Recursos del equipo (opcionales)")]
    [SerializeField] GameObject jugadorPrimeraPersona;
    [SerializeField] GameObject modeloPuerta;

    const float Ancho = 20f;
    const float Fondo = 16f;
    const float Alto = 6f;
    const float SeparacionPuertas = 6.5f;
    const float RadioPuerta = 2f;
    const float AltoPuerta = 2.6f;

    static readonly Vector3 InicioJugador = new Vector3(0f, 0f, -3.5f);
    static readonly Vector3 CentroAlfombra = new Vector3(0f, 0f, -0.5f);
    static readonly Color ColorPared = new Color(0.99f, 0.95f, 0.86f);
    static readonly Color ColorMadera = new Color(0.80f, 0.60f, 0.38f);
    static readonly Color ColorPuerta = new Color(0.55f, 0.36f, 0.22f);
    static readonly Color ColorApagado = new Color(0.62f, 0.62f, 0.66f);

    readonly List<Puerta> puertas = new List<Puerta>();
    Transform jugador;
    GameObject panelAviso;
    Text textoAviso;

    void Start()
    {
        ConfigurarLuz();
        ConstruirSala();

        float inicioX = -(minijuegos.Length - 1) * SeparacionPuertas * 0.5f;
        for (int i = 0; i < minijuegos.Length; i++)
        {
            var juego = minijuegos[i];
            var posicion = new Vector3(inicioX + i * SeparacionPuertas, 0f, Fondo * 0.5f);
            CrearPuerta(posicion, 0f, juego.titulo, juego.descripcion, juego.escena, juego.color, "¡Listo para jugar!");
        }
        CrearPuerta(new Vector3(0f, 0f, -Fondo * 0.5f), 180f, "Salir", "Volver al inicio", escenaSalida, Estilo.MielOscura, "Volver");

        bool primeraPersona = CrearJugador();
        ConstruirHud(primeraPersona);
        Transicion.Entrar();
    }

    void Update()
    {
        if (jugador == null) return;

        Puerta cercana = null;
        float mejor = RadioPuerta;
        foreach (var puerta in puertas)
        {
            var diferencia = puerta.frente - jugador.position;
            diferencia.y = 0f;
            float distancia = diferencia.magnitude;
            if (distancia >= mejor) continue;
            mejor = distancia;
            cercana = puerta;
        }

        panelAviso.SetActive(cercana != null);
        if (cercana == null) return;

        textoAviso.text = cercana.disponible
            ? $"<b>E</b>  ·  Entrar a «{cercana.titulo}»"
            : $"«{cercana.titulo}» todavía no está listo";
        if (cercana.disponible && PresionoEntrar()) Transicion.Ir(cercana.escena);
    }

    static bool PresionoEntrar()
    {
        var teclado = Keyboard.current;
        if (teclado != null && teclado.eKey.wasPressedThisFrame) return true;
        var mando = Gamepad.current;
        return mando != null && mando.buttonSouth.wasPressedThisFrame;
    }

    bool CrearJugador()
    {
        if (jugadorPrimeraPersona != null)
        {
            var instancia = Instantiate(jugadorPrimeraPersona, InicioJugador + Vector3.up * 1.05f, Quaternion.identity);
            jugador = instancia.transform;
            return true;
        }

        var camara = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
        camara.tag = "MainCamera";
        camara.fieldOfView = 55f;
        camara.nearClipPlane = 0.1f;
        camara.GetUniversalAdditionalCameraData().antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
        var orbita = camara.gameObject.AddComponent<CamaraOrbita>();
        jugador = FabricaJugador.CrearNino(InicioJugador, Quaternion.identity, 1.7f, null, null, camara.transform, out _).transform;
        orbita.Seguir(jugador, 0f);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        return false;
    }

    void ConfigurarLuz()
    {
        var luz = new GameObject("Luz", typeof(Light)).GetComponent<Light>();
        luz.type = LightType.Directional;
        luz.color = new Color(1f, 0.95f, 0.86f);
        luz.intensity = 0.9f;
        luz.shadows = LightShadows.Soft;
        luz.shadowStrength = 0.4f;
        luz.transform.rotation = Quaternion.Euler(65f, -30f, 0f);

        RenderSettings.skybox = null;
        RenderSettings.fog = false;
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.86f, 0.86f, 0.92f);
        RenderSettings.ambientEquatorColor = new Color(0.82f, 0.77f, 0.70f);
        RenderSettings.ambientGroundColor = new Color(0.56f, 0.50f, 0.44f);
        DynamicGI.UpdateEnvironment();
    }

    void ConstruirSala()
    {
        var sala = new GameObject("Sala").transform;
        float mx = Ancho * 0.5f;
        float mz = Fondo * 0.5f;

        ConstructorEscena.Bloque(PrimitiveType.Cube, "Piso", sala, new Vector3(0f, -0.1f, 0f), new Vector3(Ancho, 0.2f, Fondo), ColorMadera, true);
        var junta = Estilo.Oscurecer(ColorMadera, 0.85f);
        for (float x = -mx + 0.8f; x < mx; x += 0.8f)
            ConstructorEscena.Bloque(PrimitiveType.Cube, "Tablon", sala, new Vector3(x, 0.002f, 0f), new Vector3(0.03f, 0.004f, Fondo), junta);

        var acostado = Quaternion.Euler(90f, 0f, 0f);
        ConstructorEscena.Figura("Alfombra", Mallas.Hexagono, sala, CentroAlfombra + Vector3.up * 0.02f, acostado, new Vector3(3.2f, 3.2f, 0.03f), Estilo.Crema);
        ConstructorEscena.Figura("Centro", Mallas.Hexagono, sala, CentroAlfombra + Vector3.up * 0.035f, acostado, new Vector3(2.5f, 2.5f, 0.03f), Estilo.MielClara);

        SinSombra(ConstructorEscena.Bloque(PrimitiveType.Cube, "ParedNorte", sala, new Vector3(0f, Alto * 0.5f, mz + 0.2f), new Vector3(Ancho + 0.8f, Alto, 0.4f), ColorPared, true));
        SinSombra(ConstructorEscena.Bloque(PrimitiveType.Cube, "ParedSur", sala, new Vector3(0f, Alto * 0.5f, -mz - 0.2f), new Vector3(Ancho + 0.8f, Alto, 0.4f), ColorPared, true));
        SinSombra(ConstructorEscena.Bloque(PrimitiveType.Cube, "ParedEste", sala, new Vector3(mx + 0.2f, Alto * 0.5f, 0f), new Vector3(0.4f, Alto, Fondo), ColorPared, true));
        SinSombra(ConstructorEscena.Bloque(PrimitiveType.Cube, "ParedOeste", sala, new Vector3(-mx - 0.2f, Alto * 0.5f, 0f), new Vector3(0.4f, Alto, Fondo), ColorPared, true));
        SinSombra(ConstructorEscena.Bloque(PrimitiveType.Cube, "Techo", sala, new Vector3(0f, Alto + 0.1f, 0f), new Vector3(Ancho + 0.8f, 0.2f, Fondo + 0.8f), new Color(0.98f, 0.96f, 0.9f)));

        var zocalo = new Color(0.86f, 0.62f, 0.30f);
        Banda(sala, 0.55f, 1.1f, 0.06f, zocalo);
        Banda(sala, Alto - 0.35f, 0.3f, 0.08f, Estilo.Miel);

        foreach (float z in new[] { -4f, 0f, 4f })
            SinSombra(ConstructorEscena.Bloque(PrimitiveType.Cube, "Viga", sala, new Vector3(0f, Alto - 0.15f, z), new Vector3(Ancho, 0.3f, 0.35f), Estilo.Miel));

        foreach (float x in new[] { -5f, 5f })
            foreach (float z in new[] { -3f, 3f })
                Lampara(sala, new Vector3(x, Alto, z));

        foreach (float lado in new[] { -1f, 1f })
            Ventana(sala, new Vector3(lado * (mx - 0.03f), 3.3f, 5.6f), lado);

        Burbuja(sala, new Vector3(-mx + 0.12f, 3.1f, -1.9f), Quaternion.LookRotation(Vector3.left), "Privacidad");
        Burbuja(sala, new Vector3(-mx + 0.12f, 2.3f, 1.6f), Quaternion.LookRotation(Vector3.left), "Respeto");
        Burbuja(sala, new Vector3(-mx + 0.12f, 2f, -5.4f), Quaternion.LookRotation(Vector3.left), "Piensa antes de publicar");
        Burbuja(sala, new Vector3(mx - 0.12f, 3.1f, -1.9f), Quaternion.LookRotation(Vector3.right), "Verifica antes de compartir");
        Burbuja(sala, new Vector3(mx - 0.12f, 2.3f, 1.6f), Quaternion.LookRotation(Vector3.right), "Cuida tus contraseñas");
        Burbuja(sala, new Vector3(mx - 0.12f, 2f, -5.4f), Quaternion.LookRotation(Vector3.right), "Un mensaje amable alegra el día");

        foreach (float x in new[] { -SeparacionPuertas * 0.5f, SeparacionPuertas * 0.5f })
            Panal(sala, new Vector3(x, 1.75f, mz - 0.09f));

        foreach (float x in new[] { -mx + 0.9f, mx - 0.9f })
            foreach (float z in new[] { -mz + 0.9f, mz - 0.9f })
                Planta(sala, new Vector3(x, 0f, z));

        Abeja.Crear(sala, new Vector3(-3f, 4.6f, 0f), new Vector2(2.2f, 1.2f), 0.7f, 0f);
        Abeja.Crear(sala, new Vector3(4f, 4.3f, -2f), new Vector2(1.6f, 1.4f), -0.8f, 2f);
    }

    static void SinSombra(GameObject objeto) => objeto.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;

    static void Banda(Transform padre, float y, float alto, float grosor, Color color)
    {
        float mx = Ancho * 0.5f;
        float mz = Fondo * 0.5f;
        ConstructorEscena.Bloque(PrimitiveType.Cube, "Banda", padre, new Vector3(0f, y, mz - grosor * 0.5f), new Vector3(Ancho, alto, grosor), color);
        ConstructorEscena.Bloque(PrimitiveType.Cube, "Banda", padre, new Vector3(0f, y, -mz + grosor * 0.5f), new Vector3(Ancho, alto, grosor), color);
        ConstructorEscena.Bloque(PrimitiveType.Cube, "Banda", padre, new Vector3(mx - grosor * 0.5f, y, 0f), new Vector3(grosor, alto, Fondo), color);
        ConstructorEscena.Bloque(PrimitiveType.Cube, "Banda", padre, new Vector3(-mx + grosor * 0.5f, y, 0f), new Vector3(grosor, alto, Fondo), color);
    }

    static void Lampara(Transform padre, Vector3 techo)
    {
        ConstructorEscena.Bloque(PrimitiveType.Cylinder, "Cable", padre, techo + Vector3.down * 0.6f, new Vector3(0.03f, 0.6f, 0.03f), Estilo.TextoOscuro);
        ConstructorEscena.Bloque(PrimitiveType.Cylinder, "Pantalla", padre, techo + Vector3.down * 1.25f, new Vector3(0.7f, 0.08f, 0.7f), Estilo.Miel);
        SinSombra(ConstructorEscena.Bloque(PrimitiveType.Sphere, "Foco", padre, techo + Vector3.down * 1.4f, Vector3.one * 0.35f, Estilo.MielClara));

        var luz = new GameObject("LuzLampara", typeof(Light)).GetComponent<Light>();
        luz.transform.SetParent(padre, false);
        luz.transform.position = techo + Vector3.down * 1.6f;
        luz.type = LightType.Point;
        luz.color = new Color(1f, 0.86f, 0.62f);
        luz.range = 9f;
        luz.intensity = 1.3f;
    }

    static void Ventana(Transform padre, Vector3 centro, float lado)
    {
        ConstructorEscena.Bloque(PrimitiveType.Cube, "Marco", padre, centro, new Vector3(0.08f, 1.8f, 2.4f), Color.white);
        SinSombra(ConstructorEscena.Bloque(PrimitiveType.Cube, "Vidrio", padre, centro - Vector3.right * lado * 0.03f, new Vector3(0.06f, 1.6f, 2.2f), new Color(0.62f, 0.84f, 0.98f)));
        ConstructorEscena.Bloque(PrimitiveType.Cube, "Division", padre, centro - Vector3.right * lado * 0.05f, new Vector3(0.05f, 1.6f, 0.08f), Color.white);
    }

    static void Burbuja(Transform padre, Vector3 posicion, Quaternion rotacion, string texto)
    {
        var burbuja = new GameObject("Burbuja").transform;
        burbuja.SetParent(padre, false);
        burbuja.SetPositionAndRotation(posicion, rotacion);
        burbuja.localScale = Vector3.one * 0.62f;
        ConstructorEscena.Figura("Borde", Mallas.Burbuja, burbuja, new Vector3(0f, -0.03f, 0.05f), Quaternion.identity, new Vector3(1.045f, 1.08f, 0.8f), Estilo.BordeBurbuja);
        ConstructorEscena.Figura("Cuerpo", Mallas.Burbuja, burbuja, Vector3.zero, Quaternion.identity, Vector3.one, Color.white);
        var lienzo = ConstructorEscena.CanvasMundo("Texto", burbuja, new Vector3(0f, 0f, -0.08f), new Vector2(720f, 360f), 0.005f).transform;
        var etiqueta = ConstructorEscena.Texto(lienzo, "Texto", Estilo.Negrita, 76, TextAnchor.MiddleCenter, Estilo.TextoOscuro, new Vector2(0.06f, 0.08f), new Vector2(0.94f, 0.92f));
        etiqueta.resizeTextForBestFit = true;
        etiqueta.resizeTextMinSize = 36;
        etiqueta.resizeTextMaxSize = 76;
        etiqueta.text = texto;
    }

    static void Panal(Transform padre, Vector3 centro)
    {
        const float distancia = 0.62f;
        var desplazamientos = new[]
        {
            Vector2.zero, new Vector2(0f, distancia), new Vector2(0f, -distancia),
            new Vector2(distancia * 0.866f, distancia * 0.5f), new Vector2(distancia * 0.866f, -distancia * 0.5f),
            new Vector2(-distancia * 0.866f, distancia * 0.5f), new Vector2(-distancia * 0.866f, -distancia * 0.5f)
        };
        Color[] tonos = { Estilo.Miel, Estilo.MielClara, Estilo.MielOscura };
        for (int i = 0; i < desplazamientos.Length; i++)
        {
            var posicion = centro + new Vector3(desplazamientos[i].x, desplazamientos[i].y, 0f);
            ConstructorEscena.Figura("Celda", Mallas.Hexagono, padre, posicion, Quaternion.identity, new Vector3(0.33f, 0.33f, 0.05f), tonos[i % tonos.Length]);
        }
    }

    static void Planta(Transform padre, Vector3 posicion)
    {
        ConstructorEscena.Bloque(PrimitiveType.Cylinder, "Maceta", padre, posicion + Vector3.up * 0.35f, new Vector3(0.7f, 0.35f, 0.7f), new Color(0.78f, 0.45f, 0.30f), true);
        ConstructorEscena.Bloque(PrimitiveType.Sphere, "Hojas", padre, posicion + Vector3.up * 1.05f, Vector3.one * 0.9f, Estilo.Hojas[0]);
        ConstructorEscena.Bloque(PrimitiveType.Sphere, "Hojas", padre, posicion + new Vector3(0.22f, 1.35f, 0.1f), Vector3.one * 0.6f, Estilo.Hojas[1]);
        ConstructorEscena.Bloque(PrimitiveType.Sphere, "Hojas", padre, posicion + new Vector3(-0.2f, 1.3f, -0.12f), Vector3.one * 0.55f, Estilo.Hojas[2]);
    }

    void CrearPuerta(Vector3 posicion, float giro, string titulo, string descripcion, string escena, Color color, string textoListo)
    {
        bool disponible = Transicion.Disponible(escena);
        var tono = disponible ? color : ColorApagado;

        var raiz = new GameObject("Puerta " + titulo).transform;
        raiz.SetPositionAndRotation(posicion, Quaternion.Euler(0f, giro, 0f));
        var colisionador = raiz.gameObject.AddComponent<BoxCollider>();
        colisionador.center = new Vector3(0f, 1.7f, -0.15f);
        colisionador.size = new Vector3(2.6f, 3.4f, 0.6f);

        ConstructorEscena.Bloque(PrimitiveType.Cube, "Pilar", raiz, new Vector3(-1.05f, 1.6f, -0.15f), new Vector3(0.35f, 3.2f, 0.45f), tono);
        ConstructorEscena.Bloque(PrimitiveType.Cube, "Pilar", raiz, new Vector3(1.05f, 1.6f, -0.15f), new Vector3(0.35f, 3.2f, 0.45f), tono);
        ConstructorEscena.Bloque(PrimitiveType.Cube, "Dintel", raiz, new Vector3(0f, 3.35f, -0.15f), new Vector3(2.45f, 0.4f, 0.5f), Estilo.Oscurecer(tono, 0.85f));
        ConstructorEscena.Figura("Emblema", Mallas.Hexagono, raiz, new Vector3(0f, 3.35f, -0.42f), Quaternion.identity, new Vector3(0.3f, 0.3f, 0.06f), Estilo.Crema);
        CrearHoja(raiz);

        var acostado = Quaternion.Euler(90f, 0f, 0f);
        var frente = raiz.TransformPoint(new Vector3(0f, 0f, -1.4f));
        ConstructorEscena.Figura("Tapete", Mallas.Hexagono, raiz, new Vector3(0f, 0.02f, -1.4f), acostado, new Vector3(1.2f, 1.2f, 0.04f), Color.Lerp(tono, Color.white, 0.35f));
        Camino(frente, Color.Lerp(tono, Color.white, 0.5f));
        CrearLetrero(raiz, titulo, descripcion, tono, disponible ? textoListo : "Muy pronto");

        var luz = new GameObject("LuzPuerta", typeof(Light)).GetComponent<Light>();
        luz.transform.SetParent(raiz, false);
        luz.transform.localPosition = new Vector3(0f, 3f, -1.6f);
        luz.type = LightType.Point;
        luz.color = Color.Lerp(tono, Color.white, 0.5f);
        luz.range = 6f;
        luz.intensity = disponible ? 1.4f : 0.5f;

        puertas.Add(new Puerta { titulo = titulo, escena = escena, frente = frente, disponible = disponible });
    }

    void CrearHoja(Transform raiz)
    {
        if (modeloPuerta != null)
        {
            var modelo = Instantiate(modeloPuerta, raiz);
            modelo.transform.localPosition = Vector3.zero;
            modelo.transform.localRotation = Quaternion.identity;
            foreach (var colisionador in modelo.GetComponentsInChildren<Collider>()) colisionador.enabled = false;
            foreach (var animador in modelo.GetComponentsInChildren<Animator>()) animador.enabled = false;
            foreach (var componente in modelo.GetComponentsInChildren<MonoBehaviour>()) componente.enabled = false;
            if (AjustarModelo(modelo.transform, raiz)) return;
            Destroy(modelo);
        }

        ConstructorEscena.Bloque(PrimitiveType.Cube, "Hoja", raiz, new Vector3(0f, AltoPuerta * 0.5f, -0.08f), new Vector3(1.75f, AltoPuerta, 0.12f), ColorPuerta);
        var panel = Estilo.Oscurecer(ColorPuerta, 1.15f);
        ConstructorEscena.Bloque(PrimitiveType.Cube, "Panel", raiz, new Vector3(0f, 1.85f, -0.15f), new Vector3(1.3f, 0.85f, 0.04f), panel);
        ConstructorEscena.Bloque(PrimitiveType.Cube, "Panel", raiz, new Vector3(0f, 0.75f, -0.15f), new Vector3(1.3f, 0.95f, 0.04f), panel);
        ConstructorEscena.Bloque(PrimitiveType.Sphere, "Perilla", raiz, new Vector3(0.6f, 1.25f, -0.2f), Vector3.one * 0.13f, Estilo.Miel);
    }

    static bool AjustarModelo(Transform modelo, Transform raiz)
    {
        var limites = Limites(modelo);
        if (limites.size.y < 1e-4f) return false;
        if (limites.size.x < limites.size.z)
        {
            modelo.localRotation = Quaternion.Euler(0f, 90f, 0f);
            limites = Limites(modelo);
        }

        modelo.localScale *= AltoPuerta / limites.size.y;
        limites = Limites(modelo);
        var destino = raiz.TransformPoint(new Vector3(0f, 0f, -0.08f));
        modelo.position += new Vector3(destino.x - limites.center.x, destino.y - limites.min.y, destino.z - limites.center.z);
        return true;
    }

    static Bounds Limites(Transform objeto)
    {
        var renderers = objeto.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(objeto.position, Vector3.zero);
        var limites = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) limites.Encapsulate(renderers[i].bounds);
        return limites;
    }

    static void Camino(Vector3 destino, Color color)
    {
        var acostado = Quaternion.Euler(90f, 0f, 0f);
        for (float t = 0.38f; t < 0.86f; t += 0.16f)
        {
            var punto = Vector3.Lerp(CentroAlfombra, destino, t);
            punto.y = 0.015f;
            ConstructorEscena.Figura("Paso", Mallas.Hexagono, null, punto, acostado, new Vector3(0.38f, 0.38f, 0.03f), color);
        }
    }

    static void CrearLetrero(Transform raiz, string titulo, string descripcion, Color color, string estado)
    {
        var lienzo = ConstructorEscena.CanvasMundo("Letrero", raiz, new Vector3(0f, 4.55f, -0.3f), new Vector2(700f, 300f), 0.0045f).transform;
        var fondo = ConstructorEscena.Imagen(lienzo, "Fondo", SpritesUI.Redondeado, Color.white, Vector2.zero, Vector2.one, 0.7f);
        ConstructorEscena.Contorno(fondo, color, 8f);

        var textoTitulo = ConstructorEscena.Texto(lienzo, "Titulo", Estilo.Negrita, 92, TextAnchor.MiddleCenter, color, new Vector2(0f, 0.5f), new Vector2(1f, 0.98f));
        textoTitulo.text = titulo;
        ConstructorEscena.Contorno(textoTitulo, Estilo.Oscurecer(color, 0.55f), 3f);
        var textoDescripcion = ConstructorEscena.Texto(lienzo, "Descripcion", Estilo.SemiNegrita, 32, TextAnchor.MiddleCenter, Estilo.TextoOscuro, new Vector2(0.05f, 0.3f), new Vector2(0.95f, 0.52f));
        textoDescripcion.text = descripcion;

        var pastilla = ConstructorEscena.Imagen(lienzo, "Estado", SpritesUI.Redondeado, color, new Vector2(0.27f, 0.06f), new Vector2(0.73f, 0.26f), 1.2f);
        var textoEstado = ConstructorEscena.Texto(pastilla.transform, "Texto", Estilo.Negrita, 30, TextAnchor.MiddleCenter, Color.white, Vector2.zero, Vector2.one);
        textoEstado.text = estado;
        ConstructorEscena.Contorno(textoEstado, Estilo.Oscurecer(color, 0.55f), 2f);
    }

    void ConstruirHud(bool primeraPersona)
    {
        var hud = ConstructorEscena.CanvasPantalla("Hud").transform;

        var titulo = ConstructorEscena.Texto(hud, "Titulo", Estilo.Negrita, 60, TextAnchor.UpperCenter, Estilo.Miel, new Vector2(0.2f, 0.89f), new Vector2(0.8f, 0.985f));
        titulo.text = "SALA DE MINIJUEGOS";
        ConstructorEscena.Contorno(titulo, Estilo.Cafe, 5f);
        ConstructorEscena.Sombra(titulo, new Color(1f, 1f, 1f, 0.9f), new Vector2(0f, -7f));
        var subtitulo = ConstructorEscena.Texto(hud, "Subtitulo", Estilo.Negrita, 30, TextAnchor.UpperCenter, Color.white, new Vector2(0.2f, 0.84f), new Vector2(0.8f, 0.9f));
        subtitulo.text = "Acércate a una puerta y presiona E";
        ConstructorEscena.Contorno(subtitulo, Estilo.Cafe, 3f);

        var controles = ConstructorEscena.Imagen(hud, "Controles", SpritesUI.Redondeado, new Color(0.12f, 0.16f, 0.28f, 0.72f), Vector2.zero, Vector2.zero, 1.4f);
        ConstructorEscena.Fijar(controles.rectTransform, Vector2.zero, Vector2.zero, new Vector2(28f, 28f), new Vector2(330f, 150f));
        var textoControles = ConstructorEscena.Texto(controles.transform, "Texto", Estilo.SemiNegrita, 22, TextAnchor.MiddleLeft, Color.white, new Vector2(0.07f, 0.05f), new Vector2(0.95f, 0.95f));
        textoControles.lineSpacing = 1.15f;
        textoControles.text = primeraPersona
            ? "<b>WASD</b>  moverse\n<b>Mouse</b>  mirar\n<b>Shift</b>  correr\n<b>E</b>  entrar"
            : "<b>WASD</b>  moverse\n<b>Clic derecho</b>  girar cámara\n<b>Shift</b>  correr\n<b>E</b>  entrar";

        var aviso = ConstructorEscena.Imagen(hud, "Aviso", SpritesUI.Redondeado, Estilo.Crema, Vector2.zero, Vector2.zero, 0.9f);
        ConstructorEscena.Fijar(aviso.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 60f), new Vector2(820f, 110f));
        ConstructorEscena.Contorno(aviso, Estilo.Miel, 5f);
        ConstructorEscena.Sombra(aviso, new Color(0f, 0f, 0f, 0.25f), new Vector2(0f, -8f));
        textoAviso = ConstructorEscena.Texto(aviso.transform, "Texto", Estilo.SemiNegrita, 36, TextAnchor.MiddleCenter, Estilo.TextoOscuro, new Vector2(0.04f, 0.1f), new Vector2(0.96f, 0.9f));
        panelAviso = aviso.gameObject;
        panelAviso.SetActive(false);
    }
}
