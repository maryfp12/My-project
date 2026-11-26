using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestorCartas : MonoBehaviour
{
    public GameObject cartaPrefab;
    public Material[] materialesFrente;
    public Transform punto;
    public int columnas = 4;
    public float separacionX = 1.3f;
    public float separacionY = 1.5f;
    public float tiempoMostrar = 5f;
    public GameObject camaraLibre;
    public GameObject camaraCartas;
    public TMPro.TMP_Text textoResultado;
    public PanelFinal panelFinal;

    List<Carta> cartas = new List<Carta>();
    bool iniciado, puedeElegir, comparando;
    Carta primera;
    int aciertos, intentos;
    PlayerMovement jugador;

    public void Iniciar()
    {
        if (iniciado) return;
        iniciado = true;
        StartCoroutine(Rutina());
    }

    IEnumerator Rutina()
    {
        Congelar(true);
        yield return new WaitForSeconds(0.8f);

        List<int> ids = new List<int>();
        for (int i = 0; i < materialesFrente.Length; i++) { ids.Add(i); ids.Add(i); }

        for (int i = ids.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int tmp = ids[i]; ids[i] = ids[j]; ids[j] = tmp;
        }

        int filas = Mathf.CeilToInt(ids.Count / (float)columnas);
        for (int n = 0; n < ids.Count; n++)
        {
            int col = n % columnas;
            int fila = n / columnas;
            Vector3 pos = punto.position
                + punto.right * (col - (columnas - 1) / 2f) * separacionX
                + punto.up * ((filas - 1) / 2f - fila) * separacionY;

            GameObject obj = Instantiate(cartaPrefab, pos, punto.rotation, punto);
            Carta c = obj.GetComponent<Carta>();
            c.FijarBase();
            c.Configurar(ids[n], materialesFrente[ids[n]]);
            cartas.Add(c);
        }

        yield return new WaitForSeconds(1f);
        foreach (Carta c in cartas) c.Voltear(true);

        yield return new WaitForSeconds(tiempoMostrar);
        foreach (Carta c in cartas) c.Voltear(false);

        yield return new WaitForSeconds(0.6f);
        ActualizarTexto();
        puedeElegir = true;
    }

    void Update()
    {
        if (!puedeElegir || comparando) return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray rayo = Camera.main.ScreenPointToRay(Input.mousePosition);
            int mascara = ~(1 << LayerMask.NameToLayer("Player"));
            if (Physics.Raycast(rayo, out RaycastHit hit, 50f, mascara, QueryTriggerInteraction.Ignore))
            {
                Carta c = hit.collider.GetComponentInParent<Carta>();
                if (c != null && !c.Volteada) Elegir(c);
            }
        }
    }

    void Elegir(Carta c)
    {
        if (primera == null)
        {
            primera = c;
            c.Voltear(true);
            return;
        }

        comparando = true;
        c.Voltear(true);
        StartCoroutine(Comparar(primera, c));
        primera = null;
    }

    IEnumerator Comparar(Carta a, Carta b)
    {
        yield return new WaitForSeconds(1f);
        intentos++;

        if (a.id == b.id)
        {
            aciertos++;
            ActualizarTexto();
        }
        else
        {
            a.Voltear(false);
            b.Voltear(false);
            yield return new WaitForSeconds(0.5f);
        }

        comparando = false;
        if (aciertos >= materialesFrente.Length) StartCoroutine(Ganar());
    }

        IEnumerator Ganar()
    {
        puedeElegir = false;
        if (textoResultado) textoResultado.text = "¡Ganaste! Intentos: " + intentos;
        yield return new WaitForSeconds(3f);
        if (textoResultado) textoResultado.text = "";
        Congelar(false);
        yield return new WaitForSeconds(1.2f);
        BloquearFinal();
        if (panelFinal) panelFinal.Mostrar(intentos);
    }

    void BloquearFinal()
    {
        if (jugador)
        {
            jugador.enabled = false;
            Animator anim = jugador.GetComponentInChildren<Animator>();
            if (anim) anim.SetFloat("Speed", 0f);
        }

        if (camaraLibre)
        {
            foreach (Behaviour b in camaraLibre.GetComponents<Behaviour>())
                if (b.GetType().Name == "CinemachineInputAxisController") b.enabled = false;
        }
    }

    void ActualizarTexto()
    {
        if (textoResultado) textoResultado.text = "Pares: " + aciertos + " / " + materialesFrente.Length;
    }

    void Congelar(bool congelar)
    {
        if (jugador == null) jugador = FindAnyObjectByType<PlayerMovement>();
        if (jugador)
        {
            jugador.enabled = !congelar;
            Animator anim = jugador.GetComponentInChildren<Animator>();
            if (congelar && anim) anim.SetFloat("Speed", 0f);
        }

        if (camaraLibre)
        {
            foreach (Behaviour b in camaraLibre.GetComponents<Behaviour>())
                if (b.GetType().Name == "CinemachineInputAxisController") b.enabled = !congelar;
        }

        Cursor.lockState = congelar ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = congelar;
                if (camaraCartas) camaraCartas.SetActive(congelar);
    }
}