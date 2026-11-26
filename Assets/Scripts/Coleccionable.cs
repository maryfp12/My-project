using UnityEngine;

public class Coleccionable : MonoBehaviour
{
    [Header("Datos del objeto")]
    public string nombre = "Objeto";
    public bool esBueno = true;
    public int puntos = 1;               // +1 buenos, -2 malos

    [Header("Efecto al recogerlo (opcional)")]
    public GameObject efecto;

    [Header("Movimiento (0 = quieto)")]
    public float velocidadGiro = 0f;
    public float amplitudFlote = 0f;
    public float velocidadFlote = 2f;

    Vector3 posicionInicial;

    void Start()
    {
        posicionInicial = transform.position;
    }

    void Update()
    {
        if (velocidadGiro != 0)
            transform.Rotate(0, velocidadGiro * Time.deltaTime, 0, Space.World);

        if (amplitudFlote != 0)
        {
            float y = posicionInicial.y + Mathf.Sin(Time.time * velocidadFlote) * amplitudFlote;
            transform.position = new Vector3(transform.position.x, y, transform.position.z);
        }
    }

    void OnTriggerEnter(Collider otro)
    {
        if (!otro.CompareTag("Player")) return;

        Hobby h = new Hobby(nombre, null, esBueno, puntos);
        ControlHobbies control = FindFirstObjectByType<ControlHobbies>();
        if (control != null) control.Recoger(h);

        if (Marcador.instancia != null) Marcador.instancia.Sumar(puntos);
        if (efecto != null) Instantiate(efecto, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
