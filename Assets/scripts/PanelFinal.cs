using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PanelFinal : MonoBehaviour
{
    public TMP_Text textoDetalle;
    public string escenaMenu = "MainMenu";

    public void Mostrar(int intentos)
    {
        if (textoDetalle) textoDetalle.text = "Lo lograste en " + intentos + " intentos";
        gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Reintentar()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void IrAlMenu()
    {
        SceneManager.LoadScene(escenaMenu);
    }
}
