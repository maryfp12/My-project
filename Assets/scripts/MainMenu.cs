using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    public GameObject Main_Menu;

    [SerializeField] string nombreSiguienteEscene =  "Pasadizo";

    public void play (){
        SceneManager.LoadScene(nombreSiguienteEscene);
    }

    public void settings () {
        Debug.Log("abrir panel de opciones");
    }

    public void help () {
        Debug.Log("abrir panel de ayuda");
    }
}
