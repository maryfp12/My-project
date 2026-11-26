using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class JugadorMovimiento : MonoBehaviour
{
    const float VelocidadCaminar = 3.8f;
    const float VelocidadCorrer = 6.5f;
    const float Gravedad = -25f;
    const float VelocidadGiro = 720f;
    const float AlturaCaida = -5f;

    CharacterController control;
    Transform camara;
    PoseNino pose;
    Vector3 puntoInicio;
    float velocidadVertical, movimiento;

    public bool PuedeMoverse { get; set; } = true;

    public void Configurar(Transform camaraJuego, PoseNino poseNino)
    {
        camara = camaraJuego;
        pose = poseNino;
        puntoInicio = transform.position;
    }

    public void Teletransportar(Vector3 posicion, Quaternion rotacion)
    {
        control.enabled = false;
        transform.SetPositionAndRotation(posicion, rotacion);
        control.enabled = true;
        velocidadVertical = 0f;
    }

    void Awake() => control = GetComponent<CharacterController>();

    void Update()
    {
        float dt = Time.deltaTime;
        var entrada = PuedeMoverse ? LeerEntrada() : Vector2.zero;
        var adelante = Plano(camara != null ? camara.forward : Vector3.forward);
        var derecha = Plano(camara != null ? camara.right : Vector3.right);

        var direccion = adelante * entrada.y + derecha * entrada.x;
        if (direccion.sqrMagnitude > 1f) direccion.Normalize();
        bool corre = PuedeMoverse && Corriendo();

        if (direccion.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direccion), VelocidadGiro * dt);

        velocidadVertical = control.isGrounded ? -2f : velocidadVertical + Gravedad * dt;
        var desplazamiento = direccion * (corre ? VelocidadCorrer : VelocidadCaminar) + Vector3.up * velocidadVertical;
        control.Move(desplazamiento * dt);

        movimiento = Mathf.MoveTowards(movimiento, direccion.magnitude * (corre ? 1.6f : 1f), dt * 6f);
        if (pose != null) pose.Caminar = movimiento;

        if (transform.position.y < AlturaCaida) Teletransportar(puntoInicio, Quaternion.identity);
    }

    static Vector3 Plano(Vector3 vector)
    {
        vector.y = 0f;
        return vector.sqrMagnitude > 1e-6f ? vector.normalized : Vector3.forward;
    }

    static Vector2 LeerEntrada()
    {
        var entrada = Vector2.zero;
        var teclado = Keyboard.current;
        if (teclado != null)
        {
            if (teclado.wKey.isPressed) entrada.y += 1f;
            if (teclado.sKey.isPressed) entrada.y -= 1f;
            if (teclado.dKey.isPressed) entrada.x += 1f;
            if (teclado.aKey.isPressed) entrada.x -= 1f;
        }
        var mando = Gamepad.current;
        if (mando != null) entrada += mando.leftStick.ReadValue();
        return Vector2.ClampMagnitude(entrada, 1f);
    }

    static bool Corriendo()
    {
        var teclado = Keyboard.current;
        if (teclado != null && (teclado.leftShiftKey.isPressed || teclado.rightShiftKey.isPressed)) return true;
        var mando = Gamepad.current;
        return mando != null && (mando.leftStickButton.isPressed || mando.rightTrigger.isPressed);
    }
}
