using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class JuegoCartas : MonoBehaviour
{
    [Header("Texturas (arrástralas desde Assets)")]
    public Texture2D dorso;
    public Texture2D[] frentes;
    public Texture2D fondoMesa;

    [Header("Ajustes")]
    public int columnas = 4;
    public float separacionX = 1.35f;
    public float separacionZ = 1.85f;
    public float segundosVistaPrevia = 2.5f;
    public string escenaMenu = "MainMenu";

    readonly string[] mensajes =
    {
        "Privacidad: protege tus datos",
        "Respeto: trata bien a los demás",
        "Piensa antes de publicar",
        "Equipo: juntos llegamos más lejos",
        "Comunidad: todos cuentan",
        "Amabilidad: un mensaje amable alegra el día"
    };

    class DatosCarta
    {
        public int id;
        public Transform raiz;
        public Renderer cuerpo;
        public Vector3 posBase;
        public bool volteada;
        public bool emparejada;
    }

    readonly List<DatosCarta> cartas = new List<DatosCarta>();
    readonly Dictionary<Collider, DatosCarta> porCollider = new Dictionary<Collider, DatosCarta>();

    DatosCarta primera;
    bool bloqueado = true;
    bool terminado;
    bool contando;
    int intentos;
    int parejas;
    int totalParejas;
    int mejor;
    float tiempo;
    string mensaje = "";

    Camera cam;
    Transform contenedor;
    Material matCuerpo;
    Material matAcierto;

    AudioSource fuente;
    AudioClip sVoltear, sAcierto, sFallo, sGanar;

    GUIStyle estTitulo, estOscuro, estIzq, estDer, estClaro, estGrande, estBoton;

    void Start()
    {
        cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("Falta una cámara con el tag MainCamera en la escena.");
            return;
        }
        if (dorso == null || frentes == null || frentes.Length == 0)
        {
            Debug.LogError("Asigna el dorso y los frentes en el Inspector de JuegoCartas.");
            return;
        }

        mejor = PlayerPrefs.GetInt("mejorIntentos", 0);

        fuente = gameObject.AddComponent<AudioSource>();
        fuente.playOnAwake = false;
        sVoltear = Secuencia(new[] { 520f }, 0.07f);
        sAcierto = Secuencia(new[] { 523f, 659f, 784f }, 0.11f);
        sFallo = Secuencia(new[] { 220f, 175f }, 0.16f);
        sGanar = Secuencia(new[] { 523f, 659f, 784f, 1047f }, 0.16f);

        ConfigurarCamara();
        StartCoroutine(Iniciar());
    }

    void ConfigurarCamara()
    {
        cam.orthographic = false;
        cam.transform.position = new Vector3(0f, 8f, 0f);
        cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.85f, 0.92f, 0.77f);
    }

    IEnumerator Iniciar()
    {
        contenedor = new GameObject("Mesa y cartas").transform;
        CrearMesa();
        CrearCartas();

        bloqueado = true;
        contando = false;
        mensaje = "Memoriza las cartas...";

        yield return new WaitForSeconds(0.6f);
        foreach (var c in cartas) StartCoroutine(Voltear(c, true));
        yield return new WaitForSeconds(0.4f + segundosVistaPrevia);
        foreach (var c in cartas) StartCoroutine(Voltear(c, false));
        yield return new WaitForSeconds(0.5f);

        mensaje = "Encuentra las parejas";
        bloqueado = false;
        contando = true;
    }

    void Reiniciar()
    {
        StopAllCoroutines();
        if (contenedor != null) Destroy(contenedor.gameObject);
        cartas.Clear();
        porCollider.Clear();
        primera = null;
        terminado = false;
        intentos = 0;
        parejas = 0;
        tiempo = 0f;
        StartCoroutine(Iniciar());
    }

    Material CrearMaterial(Texture2D tex, Color color)
    {
        Shader sh = Shader.Find("Universal Render Pipeline/Unlit");
        if (sh == null) sh = Shader.Find("Unlit/Texture");
        if (sh == null) sh = Shader.Find("Sprites/Default");

        var m = new Material(sh);
        if (m.HasProperty("_BaseColor") || m.HasProperty("_Color")) m.color = color;
        if (tex != null) m.mainTexture = tex;
        return m;
    }

    void CrearMesa()
    {
        var mesa = GameObject.CreatePrimitive(PrimitiveType.Plane);
        mesa.name = "Mesa";
        mesa.transform.SetParent(contenedor, false);
        mesa.transform.localScale = new Vector3(1.9f, 1f, 1.07f);
        Destroy(mesa.GetComponent<Collider>());
        mesa.GetComponent<Renderer>().sharedMaterial = CrearMaterial(fondoMesa, Color.white);
    }

    void CrearCartas()
    {
        totalParejas = frentes.Length;

        var ids = new List<int>();
        for (int i = 0; i < totalParejas; i++)
        {
            ids.Add(i);
            ids.Add(i);
        }
        for (int i = ids.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int tmp = ids[i];
            ids[i] = ids[j];
            ids[j] = tmp;
        }

        matCuerpo = CrearMaterial(null, new Color(0.91f, 0.66f, 0.23f));
        matAcierto = CrearMaterial(null, new Color(0.30f, 0.75f, 0.30f));
        Material matDorso = CrearMaterial(dorso, Color.white);

        int filas = Mathf.CeilToInt(ids.Count / (float)columnas);
        for (int n = 0; n < ids.Count; n++)
        {
            int c = n % columnas;
            int f = n / columnas;
            Vector3 pos = new Vector3(
                (c - (columnas - 1) / 2f) * separacionX,
                0.1f,
                -(f - (filas - 1) / 2f) * separacionZ);
            cartas.Add(CrearCarta(ids[n], pos, matDorso));
        }
    }

    DatosCarta CrearCarta(int id, Vector3 pos, Material matDorso)
    {
        Transform raiz = new GameObject("Carta " + (id + 1)).transform;
        raiz.SetParent(contenedor, false);
        raiz.position = pos;

        var cuerpo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cuerpo.name = "Cuerpo";
        cuerpo.transform.SetParent(raiz, false);
        cuerpo.transform.localScale = new Vector3(1f, 0.06f, 1.4f);
        Renderer rend = cuerpo.GetComponent<Renderer>();
        rend.sharedMaterial = matCuerpo;

        CrearCara(raiz, "Dorso", matDorso, 90f, 0.031f);
        CrearCara(raiz, "Frente", CrearMaterial(frentes[id], Color.white), -90f, -0.031f);

        var datos = new DatosCarta { id = id, raiz = raiz, cuerpo = rend, posBase = pos };
        porCollider[cuerpo.GetComponent<Collider>()] = datos;
        return datos;
    }

    void CrearCara(Transform padre, string nombre, Material mat, float rotX, float y)
    {
        var q = GameObject.CreatePrimitive(PrimitiveType.Quad);
        q.name = nombre;
        Destroy(q.GetComponent<Collider>());
        q.transform.SetParent(padre, false);
        q.transform.localPosition = new Vector3(0f, y, 0f);
        q.transform.localRotation = Quaternion.Euler(rotX, 0f, 0f);
        q.transform.localScale = new Vector3(0.92f, 1.32f, 1f);
        q.GetComponent<Renderer>().sharedMaterial = mat;
    }

    IEnumerator Voltear(DatosCarta c, bool mostrar)
    {
        c.volteada = mostrar;
        float desde = mostrar ? 0f : 180f;
        float hasta = mostrar ? 180f : 0f;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / 0.35f;
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
            c.raiz.rotation = Quaternion.Euler(Mathf.Lerp(desde, hasta, k), 0f, 0f);
            c.raiz.position = c.posBase + Vector3.up * (Mathf.Sin(k * Mathf.PI) * 0.9f);
            yield return null;
        }

        c.raiz.rotation = Quaternion.Euler(hasta, 0f, 0f);
        c.raiz.position = c.posBase;
    }

    void Update()
    {
        if (contando && !terminado) tiempo += Time.deltaTime;
        if (bloqueado || terminado) return;

        var mouse = Mouse.current;
        if (mouse == null || !mouse.leftButton.wasPressedThisFrame) return;

        Ray rayo = cam.ScreenPointToRay(mouse.position.ReadValue());
        if (Physics.Raycast(rayo, out RaycastHit hit) && porCollider.TryGetValue(hit.collider, out DatosCarta c))
        {
            if (!c.volteada && !c.emparejada) StartCoroutine(Elegir(c));
        }
    }

    IEnumerator Elegir(DatosCarta c)
    {
        bloqueado = true;
        Sonar(sVoltear);
        yield return StartCoroutine(Voltear(c, true));

        if (primera == null)
        {
            primera = c;
            bloqueado = false;
            yield break;
        }

        intentos++;
        DatosCarta a = primera;
        primera = null;

        if (a.id == c.id)
        {
            a.emparejada = true;
            c.emparejada = true;
            a.cuerpo.sharedMaterial = matAcierto;
            c.cuerpo.sharedMaterial = matAcierto;
            parejas++;
            mensaje = ObtenerMensaje(c.id);

            if (parejas >= totalParejas)
            {
                yield return StartCoroutine(Ganar());
                yield break;
            }
            Sonar(sAcierto);
        }
        else
        {
            mensaje = "Casi... ¡inténtalo de nuevo!";
            Sonar(sFallo);
            yield return new WaitForSeconds(0.8f);
            StartCoroutine(Voltear(a, false));
            yield return StartCoroutine(Voltear(c, false));
        }

        bloqueado = false;
    }

    IEnumerator Ganar()
    {
        contando = false;
        Sonar(sGanar);

        if (mejor == 0 || intentos < mejor)
        {
            mejor = intentos;
            PlayerPrefs.SetInt("mejorIntentos", mejor);
            PlayerPrefs.Save();
        }

        yield return new WaitForSeconds(1f);
        terminado = true;
    }

    string ObtenerMensaje(int id)
    {
        if (id >= 0 && id < mensajes.Length) return mensajes[id];
        return "¡Pareja encontrada!";
    }

    string FormatoTiempo()
    {
        int m = (int)(tiempo / 60f);
        int s = (int)(tiempo % 60f);
        return m.ToString("00") + ":" + s.ToString("00");
    }

    void Sonar(AudioClip clip)
    {
        if (fuente != null && clip != null) fuente.PlayOneShot(clip);
    }

    AudioClip Secuencia(float[] frecuencias, float duracionCada)
    {
        const int sr = 44100;
        int porNota = (int)(sr * duracionCada);
        var datos = new float[porNota * frecuencias.Length];

        for (int n = 0; n < frecuencias.Length; n++)
        {
            for (int i = 0; i < porNota; i++)
            {
                float env = 1f - (float)i / porNota;
                datos[n * porNota + i] = Mathf.Sin(2f * Mathf.PI * frecuencias[n] * i / sr) * 0.25f * env;
            }
        }

        var clip = AudioClip.Create("tono", datos.Length, 1, sr, false);
        clip.SetData(datos, 0);
        return clip;
    }

    GUIStyle Estilo(TextAnchor ancla, Color color)
    {
        var e = new GUIStyle(GUI.skin.label) { alignment = ancla, fontStyle = FontStyle.Bold };
        e.normal.textColor = color;
        return e;
    }

    void CrearEstilos()
    {
        Color marron = new Color(0.23f, 0.16f, 0.08f);
        estTitulo = Estilo(TextAnchor.MiddleCenter, marron);
        estOscuro = Estilo(TextAnchor.MiddleCenter, marron);
        estIzq = Estilo(TextAnchor.MiddleLeft, marron);
        estDer = Estilo(TextAnchor.MiddleRight, marron);
        estClaro = Estilo(TextAnchor.MiddleCenter, Color.white);
        estGrande = Estilo(TextAnchor.MiddleCenter, new Color(1f, 0.78f, 0.17f));
        estBoton = new GUIStyle(GUI.skin.button) { fontStyle = FontStyle.Bold };
    }

    void OnGUI()
    {
        if (cam == null) return;
        if (estOscuro == null) CrearEstilos();

        float s = Screen.height / 720f;
        float w = Screen.width;
        float h = Screen.height;

        estTitulo.fontSize = (int)(46 * s);
        estOscuro.fontSize = (int)(30 * s);
        estIzq.fontSize = (int)(26 * s);
        estDer.fontSize = (int)(26 * s);
        estClaro.fontSize = (int)(30 * s);
        estGrande.fontSize = (int)(60 * s);
        estBoton.fontSize = (int)(26 * s);

        GUI.Label(new Rect(0, 8 * s, w, 56 * s), "BUMBLE MEMORIA", estTitulo);
        GUI.Label(new Rect(24 * s, 70 * s, 460 * s, 36 * s),
            "Intentos: " + intentos + "     Parejas: " + parejas + "/" + totalParejas, estIzq);
        if (mejor > 0)
            GUI.Label(new Rect(24 * s, 104 * s, 460 * s, 36 * s), "Mejor: " + mejor + " intentos", estIzq);
        GUI.Label(new Rect(w - 324 * s, 70 * s, 300 * s, 36 * s), "Tiempo: " + FormatoTiempo(), estDer);
        GUI.Label(new Rect(0, h - 70 * s, w, 50 * s), mensaje, estOscuro);

        if (!terminado) return;

        GUI.color = new Color(0f, 0f, 0f, 0.55f);
        GUI.DrawTexture(new Rect(0, 0, w, h), Texture2D.whiteTexture);
        GUI.color = Color.white;

        Rect panel = new Rect(w / 2 - 340 * s, h / 2 - 200 * s, 680 * s, 400 * s);
        GUI.Box(panel, GUIContent.none);
        GUI.Label(new Rect(panel.x, panel.y + 20 * s, panel.width, 80 * s), "¡Ganaste!", estGrande);
        GUI.Label(new Rect(panel.x, panel.y + 120 * s, panel.width, 40 * s),
            "Intentos: " + intentos + "     Tiempo: " + FormatoTiempo(), estClaro);
        GUI.Label(new Rect(panel.x, panel.y + 165 * s, panel.width, 40 * s),
            "Mejor puntaje: " + mejor + " intentos", estClaro);

        if (GUI.Button(new Rect(panel.x + 40 * s, panel.y + 270 * s, 280 * s, 70 * s), "Jugar de nuevo", estBoton))
            Reiniciar();
        if (GUI.Button(new Rect(panel.x + 360 * s, panel.y + 270 * s, 280 * s, 70 * s), "Volver al menú", estBoton))
            SceneManager.LoadScene(escenaMenu);
    }
}
