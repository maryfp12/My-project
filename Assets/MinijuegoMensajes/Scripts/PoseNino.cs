using System.Collections.Generic;
using UnityEngine;

public class PoseNino : MonoBehaviour
{
    class Brazo
    {
        public Transform brazo, antebrazo, mano;
        public Transform[] indice;
        public Transform[][] dedos;
        public bool conTelefono;
    }

    class Pierna
    {
        public Transform muslo, pierna, pie;
    }

    const float DuracionSalto = 0.45f;
    const float DuracionNegar = 0.7f;
    const float DuracionAviso = 0.5f;

    readonly Dictionary<string, Transform> huesos = new Dictionary<string, Transform>();
    readonly Dictionary<Transform, Quaternion> reposo = new Dictionary<Transform, Quaternion>();

    Transform caderas, columna, cuello, cabeza, telefono;
    Brazo brazoPantallaIzquierda, brazoPantallaDerecha, brazoSenal;
    Pierna[] piernas;
    Vector3 posicionBase, frenteLocalCabeza, objetivoSenal;
    float tiempoSenal, duracionSenal, tiempoSalto, tiempoNegar, tiempoAviso, fase;
    bool listo;

    static readonly Vector3 Frente = Vector3.back;

    public void Preparar(Vector3 posicion, float altura)
    {
        foreach (var t in GetComponentsInChildren<Transform>(true))
        {
            string nombre = t.name;
            int separador = nombre.LastIndexOf(':');
            if (separador >= 0) nombre = nombre.Substring(separador + 1);
            if (!huesos.ContainsKey(nombre)) huesos[nombre] = t;
        }

        Orientar();
        Escalar(posicion, altura);

        caderas = Hueso("Hips");
        columna = Hueso("Spine");
        cuello = Hueso("Neck");
        cabeza = Hueso("Head");

        var izquierdo = CrearBrazo("Left");
        var derecho = CrearBrazo("Right");
        if (izquierdo.brazo != null && derecho.brazo != null && izquierdo.brazo.position.x > derecho.brazo.position.x)
        {
            (izquierdo, derecho) = (derecho, izquierdo);
        }
        brazoPantallaIzquierda = izquierdo;
        brazoPantallaDerecha = derecho;
        brazoPantallaDerecha.conTelefono = true;

        piernas = new[] { CrearPierna("Left"), CrearPierna("Right") };

        foreach (var t in GetComponentsInChildren<Transform>(true))
            if (t != transform) reposo[t] = t.localRotation;

        if (cabeza != null) frenteLocalCabeza = Quaternion.Inverse(cabeza.rotation) * Frente;
        posicionBase = transform.position;
        listo = caderas != null;

        if (listo && brazoPantallaDerecha.mano != null) CrearTelefono();
    }

    public void Senalar(Vector3 objetivo, float segundos)
    {
        if (!listo) return;
        objetivoSenal = objetivo;
        brazoSenal = objetivo.x < transform.position.x ? brazoPantallaIzquierda : brazoPantallaDerecha;
        duracionSenal = segundos;
        tiempoSenal = segundos;
    }

    public void Celebrar() => tiempoSalto = DuracionSalto;

    public void Negar() => tiempoNegar = DuracionNegar;

    public void AvisarMensaje() => tiempoAviso = DuracionAviso;

    void Orientar()
    {
        transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        var izquierdo = Hueso("LeftArm");
        var derecho = Hueso("RightArm");
        var adelante = Vector3.forward;
        if (izquierdo != null && derecho != null)
        {
            var derecha = derecho.position - izquierdo.position;
            derecha.y = 0f;
            if (derecha.sqrMagnitude > 1e-6f) adelante = Vector3.Cross(derecha.normalized, Vector3.up);
        }
        transform.rotation = Quaternion.AngleAxis(Vector3.SignedAngle(adelante, Frente, Vector3.up), Vector3.up);
    }

    void Escalar(Vector3 posicion, float altura)
    {
        var limites = Limites();
        if (limites.size.y > 1e-4f) transform.localScale *= altura / limites.size.y;
        limites = Limites();
        transform.position += new Vector3(posicion.x - limites.center.x, posicion.y - limites.min.y, posicion.z - limites.center.z);
    }

    Bounds Limites()
    {
        var renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(transform.position, Vector3.zero);
        var limites = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) limites.Encapsulate(renderers[i].bounds);
        return limites;
    }

    Transform Hueso(string nombre) => huesos.TryGetValue(nombre, out var hueso) ? hueso : null;

    Brazo CrearBrazo(string lado) => new Brazo
    {
        brazo = Hueso(lado + "Arm"),
        antebrazo = Hueso(lado + "ForeArm"),
        mano = Hueso(lado + "Hand"),
        indice = Cadena(lado + "HandIndex"),
        dedos = new[] { Cadena(lado + "HandMiddle"), Cadena(lado + "HandRing"), Cadena(lado + "HandPinky") }
    };

    Pierna CrearPierna(string lado) => new Pierna
    {
        muslo = Hueso(lado + "UpLeg"),
        pierna = Hueso(lado + "Leg"),
        pie = Hueso(lado + "Foot")
    };

    Transform[] Cadena(string prefijo)
    {
        var cadena = new List<Transform>();
        for (int i = 1; i <= 4; i++)
        {
            var hueso = Hueso(prefijo + i);
            if (hueso == null) break;
            cadena.Add(hueso);
        }
        return cadena.ToArray();
    }

    void CrearTelefono()
    {
        telefono = new GameObject("Telefono").transform;
        ConstructorEscena.Bloque(PrimitiveType.Cube, "Carcasa", telefono, Vector3.zero, new Vector3(0.085f, 0.165f, 0.014f), new Color(0.2f, 0.22f, 0.32f));
        ConstructorEscena.Bloque(PrimitiveType.Cube, "Pantalla", telefono, new Vector3(0f, 0f, -0.0075f), new Vector3(0.072f, 0.145f, 0.002f), new Color(0.5f, 0.8f, 1f));
    }

    void LateUpdate()
    {
        if (!listo) return;

        float dt = Time.deltaTime;
        fase += dt;
        foreach (var par in reposo) par.Key.localRotation = par.Value;

        float salto = 0f;
        if (tiempoSalto > 0f)
        {
            tiempoSalto -= dt;
            salto = Mathf.Sin(Mathf.Clamp01(1f - tiempoSalto / DuracionSalto) * Mathf.PI) * 0.22f;
        }
        transform.position = posicionBase + Vector3.up * salto;

        float peso = PesoSenal();
        PosarPiernas();
        PosarTronco(peso);
        PosarBrazo(brazoPantallaIzquierda, brazoSenal == brazoPantallaIzquierda ? peso : 0f);
        PosarBrazo(brazoPantallaDerecha, brazoSenal == brazoPantallaDerecha ? peso : 0f);
        ColocarTelefono();
        PosarCabeza(peso);

        if (tiempoSenal > 0f) tiempoSenal -= dt;
        if (tiempoNegar > 0f) tiempoNegar -= dt;
        if (tiempoAviso > 0f) tiempoAviso -= dt;
    }

    float PesoSenal()
    {
        if (tiempoSenal <= 0f || brazoSenal == null) return 0f;
        float transcurrido = duracionSenal - tiempoSenal;
        float entrada = Mathf.Clamp01(transcurrido / 0.15f);
        float salida = Mathf.Clamp01(tiempoSenal / 0.3f);
        return Mathf.SmoothStep(0f, 1f, Mathf.Min(entrada, salida));
    }

    Vector3 Fuera(Transform hueso)
    {
        var lado = hueso.position - caderas.position;
        lado.y = 0f;
        return lado.sqrMagnitude > 1e-6f ? lado.normalized : Vector3.right;
    }

    void PosarPiernas()
    {
        foreach (var p in piernas)
        {
            if (p.muslo == null || p.pierna == null || p.pie == null) continue;
            Apuntar(p.muslo, p.pierna, (Vector3.down + Fuera(p.muslo) * 0.05f).normalized, 1f);
            Apuntar(p.pierna, p.pie, Vector3.down, 1f);
        }
    }

    void PosarTronco(float peso)
    {
        if (columna == null || cuello == null) return;
        float lado = Mathf.Sign(objetivoSenal.x - transform.position.x);
        var direccion = (Vector3.up + Vector3.right * lado * 0.1f * peso).normalized;
        Apuntar(columna, cuello, direccion, 1f);
        columna.rotation = Quaternion.AngleAxis(-lado * 14f * peso, Vector3.up) * columna.rotation;
        columna.rotation = Quaternion.AngleAxis(Mathf.Sin(fase * 2.2f) * 1.5f, Vector3.right) * columna.rotation;
    }

    void PosarBrazo(Brazo b, float peso)
    {
        if (b == null || b.brazo == null || b.antebrazo == null || b.mano == null) return;

        var fuera = Fuera(b.brazo);
        Vector3 reposoBrazo, reposoAntebrazo;
        if (b.conTelefono)
        {
            reposoBrazo = (Vector3.down + Frente * 0.5f + fuera * 0.12f).normalized;
            reposoAntebrazo = (Frente + Vector3.up * 0.4f - fuera * 0.6f).normalized;
        }
        else
        {
            float balanceo = Mathf.Sin(fase * 1.6f) * 0.04f;
            reposoBrazo = (Vector3.down + fuera * 0.18f + Frente * (0.05f + balanceo)).normalized;
            reposoAntebrazo = (Vector3.down + Frente * 0.25f + fuera * 0.03f).normalized;
        }

        var hacia = (objetivoSenal - b.brazo.position).normalized;
        Apuntar(b.brazo, b.antebrazo, Vector3.Slerp(reposoBrazo, hacia, peso), 1f);
        Apuntar(b.antebrazo, b.mano, Vector3.Slerp(reposoAntebrazo, hacia, peso), 1f);

        var puntaMano = b.indice.Length > 0 ? b.indice[0] : null;
        if (puntaMano != null) Apuntar(b.mano, puntaMano, Vector3.Slerp(reposoAntebrazo, hacia, peso), 1f);
        if (peso <= 0.01f) return;

        for (int i = 0; i < b.indice.Length - 1; i++) Apuntar(b.indice[i], b.indice[i + 1], hacia, peso);

        var abajo = Vector3.down - Vector3.Dot(Vector3.down, hacia) * hacia;
        abajo = abajo.sqrMagnitude > 1e-6f ? abajo.normalized : Frente;
        var curvas = new[]
        {
            Vector3.Slerp(hacia, abajo, 0.9f),
            Vector3.Slerp(abajo, -hacia, 0.65f),
            -hacia
        };
        foreach (var dedo in b.dedos)
            for (int i = 0; i < dedo.Length - 1 && i < curvas.Length; i++)
                Apuntar(dedo[i], dedo[i + 1], curvas[i], peso);
    }

    void ColocarTelefono()
    {
        if (telefono == null) return;
        var b = brazoPantallaDerecha;
        var medio = b.dedos[0].Length > 0 ? b.dedos[0][0].position : b.mano.position;
        var posicion = Vector3.Lerp(b.mano.position, medio, 0.7f);
        var haciaCabeza = cabeza != null ? cabeza.position - posicion : Vector3.up;
        posicion += haciaCabeza.normalized * 0.025f;
        telefono.position = posicion;
        telefono.rotation = Quaternion.LookRotation(-haciaCabeza.normalized, Vector3.up);
        float pulso = tiempoAviso > 0f ? Mathf.Sin((1f - tiempoAviso / DuracionAviso) * Mathf.PI) * 0.35f : 0f;
        telefono.localScale = Vector3.one * (1f + pulso);
    }

    void PosarCabeza(float peso)
    {
        if (cabeza == null) return;

        Vector3? objetivo = null;
        if (peso > 0.2f) objetivo = objetivoSenal;
        else if (telefono != null) objetivo = telefono.position;

        if (objetivo.HasValue)
        {
            var actual = cabeza.rotation * frenteLocalCabeza;
            var deseada = objetivo.Value - cabeza.position;
            cabeza.rotation = Quaternion.Slerp(Quaternion.identity, Quaternion.FromToRotation(actual, deseada), 0.45f) * cabeza.rotation;
        }

        if (tiempoNegar > 0f)
        {
            float intensidad = tiempoNegar / DuracionNegar;
            cabeza.rotation = Quaternion.AngleAxis(Mathf.Sin(fase * 26f) * 20f * intensidad, Vector3.up) * cabeza.rotation;
        }
    }

    static void Apuntar(Transform hueso, Transform hijo, Vector3 direccion, float peso)
    {
        var actual = hijo.position - hueso.position;
        if (actual.sqrMagnitude < 1e-8f || direccion.sqrMagnitude < 1e-8f) return;
        var giro = Quaternion.FromToRotation(actual, direccion);
        hueso.rotation = Quaternion.Slerp(Quaternion.identity, giro, peso) * hueso.rotation;
    }
}
