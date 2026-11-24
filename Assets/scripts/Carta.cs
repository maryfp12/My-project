using System.Collections;
using UnityEngine;

public class Carta : MonoBehaviour
{
    public int id;
    public Renderer rendererFrente;
    public bool probarConT;

    public bool Volteada { get; private set; }

    Quaternion rotBase;
    Coroutine giro;

    void Start()
    {
        FijarBase();
    }

    public void FijarBase()
    {
        rotBase = transform.rotation;
    }

    public void Configurar(int nuevoId, Material material)
    {
        id = nuevoId;
        rendererFrente.material = material;
    }

    public void Voltear(bool mostrarFrente, float duracion = 0.4f)
    {
        Volteada = mostrarFrente;
        if (giro != null) StopCoroutine(giro);
        giro = StartCoroutine(Girar(mostrarFrente ? 180f : 0f, duracion));
    }

    IEnumerator Girar(float angulo, float duracion)
    {
        Quaternion inicio = transform.rotation;
        Quaternion fin = rotBase * Quaternion.Euler(0f, angulo, 0f);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duracion;
            transform.rotation = Quaternion.Slerp(inicio, fin, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        transform.rotation = fin;
    }

    void Update()
    {
        if (probarConT && Input.GetKeyDown(KeyCode.T))
            Voltear(!Volteada);
    }
}