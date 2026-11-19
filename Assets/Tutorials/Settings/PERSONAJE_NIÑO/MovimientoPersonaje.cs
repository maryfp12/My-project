using UnityEngine;
using UnityEngine.InputSystem;

public class MovimientoPersonaje : MonoBehaviour
{
    public float velocidad = 5f;
    public float limiteX = 40f;     // hasta dónde llega a izquierda y derecha
    public float limiteZ = 40f;     // hasta dónde llega adelante y atrás
    CharacterController control;
    Animator animador;

    void Start()
    {
        control = GetComponent<CharacterController>();
        animador = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        float x = 0;   // izquierda y derecha
        float z = 0;   // adelante y atrás

        if (Keyboard.current.wKey.isPressed) z = 1;
        if (Keyboard.current.sKey.isPressed) z = -1;
        if (Keyboard.current.dKey.isPressed) x = 1;
        if (Keyboard.current.aKey.isPressed) x = -1;

        Vector3 pos = transform.position;
        if ((pos.x <= -limiteX && x < 0) || (pos.x >= limiteX && x > 0)) x = 0;
        if ((pos.z <= -limiteZ && z < 0) || (pos.z >= limiteZ && z > 0)) z = 0;

        Vector3 direccion = new Vector3(x, 0, z).normalized;
        bool seMueve = direccion != Vector3.zero;

        if (seMueve)
        {
            transform.rotation = Quaternion.LookRotation(direccion);
        }

        control.SimpleMove(direccion * velocidad);

        if (animador != null)
        {
            animador.speed = seMueve ? 1f : 0f;   
        }
    }
}