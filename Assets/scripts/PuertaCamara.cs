using UnityEngine;

public class PuertaCamara : MonoBehaviour
{
    public GameObject camaraInterior;

    TriggerPuerta.Lado? ultimoTocado = null;
    bool dentro = false;

    void Start()
    {
        if (camaraInterior) camaraInterior.SetActive(false);
    }

    public void Tocado(TriggerPuerta.Lado lado)
    {
        if (ultimoTocado == lado) return;

        if (ultimoTocado == TriggerPuerta.Lado.Afuera && lado == TriggerPuerta.Lado.Adentro)
            SetInterior(true);
        else if (ultimoTocado == TriggerPuerta.Lado.Adentro && lado == TriggerPuerta.Lado.Afuera)
            SetInterior(false);

        ultimoTocado = lado;
    }

    void SetInterior(bool valor)
    {
        if (dentro == valor) return;
        dentro = valor;
        if (camaraInterior) camaraInterior.SetActive(valor);
    }
}
