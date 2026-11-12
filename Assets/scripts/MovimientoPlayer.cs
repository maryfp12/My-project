using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
public class MovimientoPlayer : MonoBehaviour
{
    public float velocidad = 3;
    public Transform puerta;
    public float distanciaPuerta = 1.5f;
    public string siguienteEscena = "";

    CharacterController control;
    Animator anim;
    Transform camara;
    bool cambiando;

    void Start()
    {
        control = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        camara = Camera.main.transform;
    }

    void Update()
    {
        float x = 0; //izq y derecha
        float z = 0; // adelante y atras

        if (Keyboard.current.wKey.isPressed) z = 1;
        if (Keyboard.current.sKey.isPressed) z = -1;
        if (Keyboard.current.dKey.isPressed) x = 1;
        if (Keyboard.current.aKey.isPressed) x = -1;

        Vector3 adelante = camara.forward;
        adelante.y = 0;
        adelante.Normalize();
        Vector3 derecha = camara.right;
        derecha.y = 0;
        derecha.Normalize();

        Vector3 direccion = adelante * z + derecha * x;
        if (direccion.magnitude > 1f) direccion.Normalize();

        bool mueve = direccion != Vector3.zero;
        anim.speed = mueve ? 1f : 0f;

        if (mueve)
        {
            transform.rotation = Quaternion.LookRotation(direccion);
        }

        control.SimpleMove(direccion * velocidad);

        if (!cambiando && puerta != null)
        {
            Vector3 a = transform.position;
            Vector3 b = puerta.position;
            a.y = 0;
            b.y = 0;

            if (Vector3.Distance(a, b) < distanciaPuerta)
            {
                cambiando = true;
                if (siguienteEscena != "")
                    SceneManager.LoadScene(siguienteEscena);
                else
                    Debug.Log("Llegó a la puerta");
            }
        }
    }
}
