using UnityEngine;
using UnityEngine.InputSystem;

public class CamaraOrbita : MonoBehaviour
{
    const float SensibilidadRaton = 0.2f;
    const float GiroTeclas = 100f;
    const float InclinacionTeclas = 60f;
    const float GiroMando = 140f;
    const float DistanciaMinima = 3.5f;
    const float DistanciaMaxima = 14f;
    const float InclinacionMinima = 5f;
    const float InclinacionMaxima = 70f;
    const float AlturaFoco = 1.5f;
    const float RadioChoque = 0.3f;

    Transform objetivo;
    Vector3 foco;
    float giro, inclinacion = 24f, distancia = 8f, distanciaDeseada = 8f;

    public void Seguir(Transform nuevoObjetivo, float giroInicial)
    {
        objetivo = nuevoObjetivo;
        giro = giroInicial;
        foco = objetivo.position + Vector3.up * AlturaFoco;
        Colocar();
    }

    void LateUpdate()
    {
        if (objetivo == null) return;
        float dt = Time.deltaTime;
        LeerControles(dt);

        inclinacion = Mathf.Clamp(inclinacion, InclinacionMinima, InclinacionMaxima);
        distanciaDeseada = Mathf.Clamp(distanciaDeseada, DistanciaMinima, DistanciaMaxima);
        distancia = Mathf.Lerp(distancia, distanciaDeseada, 1f - Mathf.Exp(-10f * dt));
        foco = Vector3.Lerp(foco, objetivo.position + Vector3.up * AlturaFoco, 1f - Mathf.Exp(-12f * dt));
        Colocar();
    }

    void LeerControles(float dt)
    {
        var raton = Mouse.current;
        if (raton != null)
        {
            if (raton.rightButton.isPressed)
            {
                var delta = raton.delta.ReadValue();
                giro += delta.x * SensibilidadRaton;
                inclinacion -= delta.y * SensibilidadRaton;
            }
            float rueda = raton.scroll.ReadValue().y;
            if (Mathf.Abs(rueda) > 0.01f) distanciaDeseada -= Mathf.Sign(rueda) * 0.9f;
        }

        var teclado = Keyboard.current;
        if (teclado != null)
        {
            if (teclado.leftArrowKey.isPressed) giro -= GiroTeclas * dt;
            if (teclado.rightArrowKey.isPressed) giro += GiroTeclas * dt;
            if (teclado.upArrowKey.isPressed) inclinacion += InclinacionTeclas * dt;
            if (teclado.downArrowKey.isPressed) inclinacion -= InclinacionTeclas * dt;
        }

        var mando = Gamepad.current;
        if (mando != null)
        {
            var palanca = mando.rightStick.ReadValue();
            giro += palanca.x * GiroMando * dt;
            inclinacion -= palanca.y * InclinacionTeclas * dt;
        }
    }

    void Colocar()
    {
        var rotacion = Quaternion.Euler(inclinacion, giro, 0f);
        var atras = rotacion * Vector3.back;
        float alcance = distancia;
        if (Physics.SphereCast(foco, RadioChoque, atras, out var choque, distancia, ~0, QueryTriggerInteraction.Ignore))
            alcance = Mathf.Max(1f, choque.distance);
        transform.SetPositionAndRotation(foco + atras * alcance, rotacion);
    }
}
