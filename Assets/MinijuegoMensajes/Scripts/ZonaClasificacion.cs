using System.Collections;
using UnityEngine;

public class ZonaClasificacion : MonoBehaviour
{
    public const float Radio = 2.4f;

    const float AlturaIcono = 2.75f;
    const float AlturaEtiqueta = 3.9f;

    public bool EsPositiva { get; private set; }
    public Vector3 PuntoEntrega => colmena.position + Vector3.up * 1.7f;

    Transform colmena, icono, etiqueta, camara;
    Renderer piso;
    Color colorBase, colorPiso;
    bool invitando, destellando;
    float fase;

    public static ZonaClasificacion Crear(bool positiva, Vector3 posicion, Vector3 centroPlaza, Transform camara)
    {
        var raiz = new GameObject(positiva ? "ZonaPositiva" : "ZonaNegativa").transform;
        var hacia = centroPlaza - posicion;
        hacia.y = 0f;
        raiz.SetPositionAndRotation(posicion, Quaternion.LookRotation(hacia));
        var zona = raiz.gameObject.AddComponent<ZonaClasificacion>();
        zona.Construir(positiva, camara);
        return zona;
    }

    public bool Contiene(Vector3 punto)
    {
        var diferencia = punto - transform.position;
        diferencia.y = 0f;
        return diferencia.magnitude < Radio;
    }

    public void Invitar(bool activo) => invitando = activo;

    public void Destellar(bool acierto) => StartCoroutine(Destello(acierto));

    void Construir(bool positiva, Transform camaraJuego)
    {
        EsPositiva = positiva;
        camara = camaraJuego;
        colorBase = positiva ? Estilo.Verde : Estilo.Rojo;
        var oscuro = positiva ? Estilo.VerdeOscuro : Estilo.RojoOscuro;
        colorPiso = Color.Lerp(colorBase, Color.white, 0.3f);

        var acostado = Quaternion.Euler(90f, 0f, 0f);
        ConstructorEscena.Figura("Borde", Mallas.Hexagono, transform, new Vector3(0f, 0.03f, 0f), acostado, new Vector3(Radio + 0.3f, Radio + 0.3f, 0.06f), oscuro);
        piso = ConstructorEscena.Figura("Piso", Mallas.Hexagono, transform, new Vector3(0f, 0.05f, 0f), acostado, new Vector3(Radio, Radio, 0.08f), colorPiso).GetComponent<Renderer>();

        colmena = new GameObject("Colmena").transform;
        colmena.SetParent(transform, false);
        colmena.localPosition = new Vector3(0f, 0f, -Radio * 0.55f);
        var colisionador = colmena.gameObject.AddComponent<CapsuleCollider>();
        colisionador.center = new Vector3(0f, 1.1f, 0f);
        colisionador.height = 2.2f;
        colisionador.radius = 0.9f;

        float[] radios = { 0.92f, 1f, 0.9f, 0.72f, 0.46f };
        for (int i = 0; i < radios.Length; i++)
        {
            float diametro = radios[i] * 2f;
            var color = i % 2 == 0 ? Estilo.Miel : Estilo.MielClara;
            ConstructorEscena.Bloque(PrimitiveType.Cylinder, "Anillo", colmena, new Vector3(0f, 0.2f + i * 0.4f, 0f), new Vector3(diametro, 0.2f, diametro), color);
        }
        ConstructorEscena.Bloque(PrimitiveType.Sphere, "Entrada", colmena, new Vector3(0f, 0.55f, 0.92f), new Vector3(0.5f, 0.32f, 0.16f), new Color(0.22f, 0.12f, 0.04f));

        icono = new GameObject("Icono").transform;
        icono.SetParent(colmena, false);
        icono.localPosition = new Vector3(0f, AlturaIcono, 0f);
        var deFrente = Quaternion.Euler(90f, 0f, 0f);
        ConstructorEscena.Bloque(PrimitiveType.Cylinder, "Aro", icono, new Vector3(0f, 0f, 0.03f), deFrente, new Vector3(1.08f, 0.04f, 1.08f), oscuro);
        ConstructorEscena.Bloque(PrimitiveType.Cylinder, "Disco", icono, Vector3.zero, deFrente, new Vector3(0.95f, 0.05f, 0.95f), Color.white);
        if (positiva)
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

        etiqueta = ConstructorEscena.CanvasMundo("Etiqueta", colmena, new Vector3(0f, AlturaEtiqueta, 0f), new Vector2(560f, 210f), 0.0075f).transform;
        var titulo = ConstructorEscena.Texto(etiqueta, "Titulo", Estilo.Negrita, 110, TextAnchor.MiddleCenter, Color.white, new Vector2(0f, 0.38f), Vector2.one);
        titulo.text = positiva ? "POSITIVO" : "NEGATIVO";
        ConstructorEscena.Contorno(titulo, oscuro, 5f);
        ConstructorEscena.Sombra(titulo, new Color(0f, 0f, 0f, 0.25f), new Vector2(0f, -7f));
        var subtitulo = ConstructorEscena.Texto(etiqueta, "Subtitulo", Estilo.Negrita, 44, TextAnchor.MiddleCenter, oscuro, Vector2.zero, new Vector2(1f, 0.4f));
        subtitulo.text = positiva ? "Trae aquí los mensajes buenos" : "Trae aquí los mensajes dañinos";
        ConstructorEscena.Contorno(subtitulo, Color.white, 3f);
    }

    void Update()
    {
        fase += Time.deltaTime;
        Mirar(icono);
        Mirar(etiqueta);
        icono.localPosition = new Vector3(0f, AlturaIcono + Mathf.Sin(fase * 1.8f) * 0.08f, 0f);

        if (destellando) return;
        float brillo = invitando ? (Mathf.Sin(fase * 5f) + 1f) * 0.2f : 0f;
        ConstructorEscena.Pintar(piso, Color.Lerp(colorPiso, Color.white, brillo));
    }

    void Mirar(Transform objeto)
    {
        if (camara == null) return;
        var direccion = objeto.position - camara.position;
        direccion.y = 0f;
        if (direccion.sqrMagnitude > 0.01f) objeto.rotation = Quaternion.LookRotation(direccion);
    }

    IEnumerator Destello(bool acierto)
    {
        const float duracion = 0.5f;
        destellando = true;
        var destello = acierto ? Color.white : Estilo.Oscurecer(colorBase, 0.35f);
        for (float t = 0f; t < duracion; t += Time.deltaTime)
        {
            float k = t / duracion;
            ConstructorEscena.Pintar(piso, Color.Lerp(destello, colorPiso, k));
            icono.localScale = Vector3.one * (1f + 0.4f * Mathf.Sin(k * Mathf.PI));
            yield return null;
        }
        icono.localScale = Vector3.one;
        destellando = false;
    }
}
