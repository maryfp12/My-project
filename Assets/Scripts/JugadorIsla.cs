using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class JugadorIsla : MonoBehaviour
{
    public float velocidad = 6f;
    public float fuerzaSalto = 9f;
    public float gravedad = -25f;
    public float velocidadGiro = 150f;
    public Transform puntoInicio;

    CharacterController controlador;
    Animator animador;
    float velocidadY;

    void Start()
    {
        controlador = GetComponent<CharacterController>();
        animador = GetComponentInChildren<Animator>();

        if (puntoInicio != null)
        {
            controlador.enabled = false;
            transform.position = puntoInicio.position;
            transform.rotation = puntoInicio.rotation;
            controlador.enabled = true;
        }
    }

    void Update()
    {
        var teclado = Keyboard.current;
        if (teclado == null) return;

        float giro = 0;
        float avance = 0;
        if (teclado.aKey.isPressed) giro = -1;
        if (teclado.dKey.isPressed) giro = 1;
        if (teclado.wKey.isPressed) avance = 1;
        if (teclado.sKey.isPressed) avance = -1;

        transform.Rotate(0, giro * velocidadGiro * Time.deltaTime, 0);

        if (controlador.isGrounded)
        {
            velocidadY = -2f;
            if (teclado.spaceKey.wasPressedThisFrame)
            {
                velocidadY = fuerzaSalto;
                if (animador != null) animador.SetTrigger("Salto");
            }
        }
        velocidadY += gravedad * Time.deltaTime;

        Vector3 movimiento = transform.forward * avance * velocidad;
        movimiento.y = velocidadY;
        controlador.Move(movimiento * Time.deltaTime);

        if (animador != null)
        {
            animador.SetBool("EnSuelo", controlador.isGrounded);
            animador.SetFloat("Velocidad", avance != 0 ? 1f : 0f);
        }
    }
   void OnTriggerEnter(Collider other)
   {
        if (other.CompareTag("Martillo") || other.CompareTag("Clavos"))
        {
           GameOver gameOver = FindObjectOfType<GameOver>();
           if (gameOver != null)
             gameOver.Terminar();
        }
    }
}



    
