using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class JugadorIsla : MonoBehaviour
{
    public float velocidad = 5f;
    public float fuerzaSalto = 9f;
    public float gravedad = -25f;
    public float giro = 12f;

    CharacterController controlador;
    Animator animador;
    float velocidadY;
    Transform camara;

    void Start()
    {
        controlador = GetComponent<CharacterController>();
        animador = GetComponentInChildren<Animator>();
        camara = Camera.main.transform;
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

        Vector3 adelante = camara.forward;
        adelante.y = 0;
        adelante.Normalize();
        Vector3 derecha = camara.right;
        derecha.y = 0;
        derecha.Normalize();

        Vector3 direccion = adelante * z + derecha * x;
        bool moviendo = direccion.sqrMagnitude > 0.01f;
        if (moviendo) direccion.Normalize();

        if (controlador.isGrounded)
        {
            if (velocidadY < 0) velocidadY = -2f;
            if (teclado.spaceKey.wasPressedThisFrame) velocidadY = fuerzaSalto;
        }
        velocidadY += gravedad * Time.deltaTime;

        Vector3 movimiento = direccion * velocidad;
        movimiento.y = velocidadY;
        controlador.Move(movimiento * Time.deltaTime);

        if (moviendo)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(direccion), giro * Time.deltaTime);
        }

        if (animador != null) animador.speed = moviendo ? 1f : 0f;
    }
}
