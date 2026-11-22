using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class JugadorIsla : MonoBehaviour
{
    public float velocidad = 6f;
    public float fuerzaSalto = 9f;
    public float gravedad = -25f;
    public float velocidadGiro = 120f;   // grados por segundo

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
            transform.SetPositionAndRotation(puntoInicio.position, puntoInicio.rotation);
            controlador.enabled = true;
        }
        velocidadY = 0;
    }

    void Update()
    {
        var teclado = Keyboard.current;
        if (teclado == null) return;

        float x = 0;
        float z = 0;
        if (teclado.aKey.isPressed || teclado.leftArrowKey.isPressed) x = -1;
        if (teclado.dKey.isPressed || teclado.rightArrowKey.isPressed) x = 1;
        if (teclado.wKey.isPressed || teclado.upArrowKey.isPressed) z = 1;
        if (teclado.sKey.isPressed || teclado.downArrowKey.isPressed) z = -1;

        // A/D giran al personaje
        transform.Rotate(0f, x * velocidadGiro * Time.deltaTime, 0f);

        // W/S avanzan hacia donde mira
        Vector3 direccion = transform.forward * z;
        bool moviendo = Mathf.Abs(z) > 0.01f;

        bool saltoEsteFrame = false;

        if (controlador.isGrounded)
        {
            if (velocidadY < 0) velocidadY = -2f;
            if (teclado.spaceKey.wasPressedThisFrame)
            {
                velocidadY = fuerzaSalto;
                saltoEsteFrame = true;
                if (animador != null)
                {
                    animador.SetBool("EnSuelo", false);
                    animador.SetTrigger("Salto");
                }
            }
        }
        velocidadY += gravedad * Time.deltaTime;

        Vector3 movimiento = direccion * velocidad;
        movimiento.y = velocidadY;
        controlador.Move(movimiento * Time.deltaTime);

        if (animador != null)
        {
            if (!saltoEsteFrame)
                animador.SetBool("EnSuelo", controlador.isGrounded);

            animador.SetFloat("Velocidad", moviendo ? 1f : 0f);
            animador.speed = 1f;
        }
    }
}