using System.Collections.Generic;
using UnityEngine;

public class PoseNino : MonoBehaviour
{
    class Brazo
    {
        public Transform brazo, antebrazo, mano;
        public Transform[] indice;
    }

    class Pierna
    {
        public Transform muslo, pierna, pie;
    }

    const float DuracionSalto = 0.45f;
    const float DuracionNegar = 0.7f;
    const float VelocidadAnimacion = 1.5f;

    readonly Dictionary<string, Transform> huesos = new Dictionary<string, Transform>();

    Transform[] articulaciones;
    Quaternion[] rotacionesReposo;
    Vector3[] posicionesReposo;
    Transform caderas, columna, cuello, cabeza;
    Brazo[] brazos;
    Pierna[] piernas;
    AnimationClip caminata;
    Vector3 posicionBase, caderasLocal;
    float pesoCaminar, pesoCargar, tiempoClip, tiempoSalto, tiempoNegar, fase;
    bool listo;

    public float Caminar { get; set; }
    public bool Cargando { get; set; }

    Vector3 Frente => transform.parent != null ? transform.parent.forward : transform.forward;

    public void Preparar(float altura)
    {
        foreach (var t in GetComponentsInChildren<Transform>(true))
        {
            string nombre = t.name;
            int separador = nombre.LastIndexOf(':');
            if (separador >= 0) nombre = nombre.Substring(separador + 1);
            if (!huesos.ContainsKey(nombre)) huesos[nombre] = t;
        }

        Orientar();
        Escalar(altura);

        caderas = Hueso("Hips");
        columna = Hueso("Spine");
        cuello = Hueso("Neck");
        cabeza = Hueso("Head");
        brazos = new[] { CrearBrazo("Left"), CrearBrazo("Right") };
        piernas = new[] { CrearPierna("Left"), CrearPierna("Right") };

        var lista = new List<Transform>();
        foreach (var t in GetComponentsInChildren<Transform>(true))
            if (t != transform) lista.Add(t);
        articulaciones = lista.ToArray();
        rotacionesReposo = new Quaternion[articulaciones.Length];
        posicionesReposo = new Vector3[articulaciones.Length];
        for (int i = 0; i < articulaciones.Length; i++)
        {
            rotacionesReposo[i] = articulaciones[i].localRotation;
            posicionesReposo[i] = articulaciones[i].localPosition;
        }

        posicionBase = transform.localPosition;
        if (caderas != null) caderasLocal = transform.InverseTransformPoint(caderas.position);
        listo = caderas != null;
    }

    public void ConfigurarCaminata(AnimationClip clip) => caminata = clip;

    public void Celebrar() => tiempoSalto = DuracionSalto;

    public void Negar() => tiempoNegar = DuracionNegar;

    void Orientar()
    {
        var deseado = transform.parent != null ? transform.parent.forward : Vector3.forward;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        var adelante = transform.forward;
        var izquierdo = Hueso("LeftArm");
        var derecho = Hueso("RightArm");
        if (izquierdo != null && derecho != null)
        {
            var derecha = derecho.position - izquierdo.position;
            derecha.y = 0f;
            if (derecha.sqrMagnitude > 1e-6f) adelante = Vector3.Cross(derecha.normalized, Vector3.up);
        }
        adelante.y = 0f;
        transform.rotation = Quaternion.AngleAxis(Vector3.SignedAngle(adelante, deseado, Vector3.up), Vector3.up) * transform.rotation;
    }

    void Escalar(float altura)
    {
        var limites = Limites();
        if (limites.size.y > 1e-4f) transform.localScale *= altura / limites.size.y;
        limites = Limites();
        var origen = transform.parent != null ? transform.parent.position : Vector3.zero;
        transform.position += new Vector3(origen.x - limites.center.x, origen.y - limites.min.y, origen.z - limites.center.z);
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
        indice = Cadena(lado + "HandIndex")
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

    void LateUpdate()
    {
        if (!listo) return;

        float dt = Time.deltaTime;
        fase += dt;
        pesoCaminar = Mathf.MoveTowards(pesoCaminar, Mathf.Clamp01(Caminar), dt * 6f);
        pesoCargar = Mathf.MoveTowards(pesoCargar, Cargando ? 1f : 0f, dt * 5f);

        for (int i = 0; i < articulaciones.Length; i++)
        {
            articulaciones[i].localRotation = rotacionesReposo[i];
            articulaciones[i].localPosition = posicionesReposo[i];
        }

        if (pesoCaminar > 0.001f)
        {
            if (caminata != null) AplicarCaminata(dt);
            else CaminataProcedural();
        }

        float quieto = 1f - pesoCaminar;
        PosarPiernas(quieto);
        PosarTronco(quieto);
        foreach (var brazo in brazos) PosarBrazo(brazo, quieto);
        PosarCabeza();

        float salto = 0f;
        if (tiempoSalto > 0f)
        {
            tiempoSalto -= dt;
            salto = Mathf.Sin(Mathf.Clamp01(1f - tiempoSalto / DuracionSalto) * Mathf.PI) * 0.25f;
        }
        transform.localPosition = posicionBase + Vector3.up * salto;
        if (tiempoNegar > 0f) tiempoNegar -= dt;
    }

    void AplicarCaminata(float dt)
    {
        tiempoClip = Mathf.Repeat(tiempoClip + dt * VelocidadAnimacion * Mathf.Max(0.6f, Caminar), caminata.length);

        var posicion = transform.localPosition;
        var rotacion = transform.localRotation;
        var escala = transform.localScale;
        caminata.SampleAnimation(gameObject, tiempoClip);
        transform.localPosition = posicion;
        transform.localRotation = rotacion;
        transform.localScale = escala;

        for (int i = 0; i < articulaciones.Length; i++)
        {
            articulaciones[i].localRotation = Quaternion.Slerp(rotacionesReposo[i], articulaciones[i].localRotation, pesoCaminar);
            articulaciones[i].localPosition = Vector3.Lerp(posicionesReposo[i], articulaciones[i].localPosition, pesoCaminar);
        }

        var fijo = transform.TransformPoint(caderasLocal);
        caderas.position = new Vector3(fijo.x, caderas.position.y, fijo.z);
    }

    void CaminataProcedural()
    {
        float paso = Mathf.Sin(fase * 7f * Mathf.Max(0.6f, Caminar));
        for (int i = 0; i < piernas.Length; i++)
        {
            var p = piernas[i];
            if (p.muslo == null || p.pierna == null || p.pie == null) continue;
            float lado = i == 0 ? paso : -paso;
            Apuntar(p.muslo, p.pierna, (Vector3.down + Frente * lado * 0.45f).normalized, pesoCaminar);
            Apuntar(p.pierna, p.pie, (Vector3.down - Frente * Mathf.Max(0f, -lado) * 0.5f).normalized, pesoCaminar);
        }
        for (int i = 0; i < brazos.Length; i++)
        {
            var b = brazos[i];
            if (b.brazo == null || b.antebrazo == null) continue;
            float lado = i == 0 ? -paso : paso;
            Apuntar(b.brazo, b.antebrazo, (Vector3.down + Frente * lado * 0.35f + Fuera(b.brazo) * 0.15f).normalized, pesoCaminar);
        }
    }

    Vector3 Fuera(Transform hueso)
    {
        var lado = hueso.position - caderas.position;
        lado.y = 0f;
        return lado.sqrMagnitude > 1e-6f ? lado.normalized : Vector3.right;
    }

    void PosarPiernas(float peso)
    {
        if (peso <= 0.001f) return;
        foreach (var p in piernas)
        {
            if (p.muslo == null || p.pierna == null || p.pie == null) continue;
            Apuntar(p.muslo, p.pierna, (Vector3.down + Fuera(p.muslo) * 0.05f).normalized, peso);
            Apuntar(p.pierna, p.pie, Vector3.down, peso);
        }
    }

    void PosarTronco(float peso)
    {
        if (columna == null || cuello == null) return;
        if (peso > 0.001f) Apuntar(columna, cuello, Vector3.up, peso);
        var lateral = Vector3.Cross(Vector3.up, Frente);
        columna.rotation = Quaternion.AngleAxis(Mathf.Sin(fase * 2.2f) * 1.5f, lateral) * columna.rotation;
    }

    void PosarBrazo(Brazo b, float quieto)
    {
        if (b.brazo == null || b.antebrazo == null || b.mano == null) return;
        var fuera = Fuera(b.brazo);

        float reposo = quieto * (1f - pesoCargar);
        if (reposo > 0.001f)
        {
            Apuntar(b.brazo, b.antebrazo, (Vector3.down + fuera * 0.18f + Frente * 0.05f).normalized, reposo);
            Apuntar(b.antebrazo, b.mano, (Vector3.down + Frente * 0.25f).normalized, reposo);
        }

        if (pesoCargar <= 0.001f) return;
        var arriba = (Vector3.up + Frente * 0.35f + fuera * 0.3f).normalized;
        var antebrazo = (Vector3.up + Frente * 0.15f - fuera * 0.2f).normalized;
        Apuntar(b.brazo, b.antebrazo, arriba, pesoCargar);
        Apuntar(b.antebrazo, b.mano, antebrazo, pesoCargar);
        if (b.indice.Length > 0) Apuntar(b.mano, b.indice[0], antebrazo, pesoCargar);
    }

    void PosarCabeza()
    {
        if (cabeza == null || tiempoNegar <= 0f) return;
        float intensidad = tiempoNegar / DuracionNegar;
        cabeza.rotation = Quaternion.AngleAxis(Mathf.Sin(fase * 26f) * 20f * intensidad, Vector3.up) * cabeza.rotation;
    }

    static void Apuntar(Transform hueso, Transform hijo, Vector3 direccion, float peso)
    {
        var actual = hijo.position - hueso.position;
        if (actual.sqrMagnitude < 1e-8f || direccion.sqrMagnitude < 1e-8f) return;
        var giro = Quaternion.FromToRotation(actual, direccion);
        hueso.rotation = Quaternion.Slerp(Quaternion.identity, giro, peso) * hueso.rotation;
    }
}
