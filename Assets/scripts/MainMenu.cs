using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    public GameObject Main_Menu;

    [SerializeField] string nombreSiguienteEscena =  "inicio 2.0";
    
    public void play (){

        SceneManager.LoadScene(nombreSiguienteEscena);
    }

    public void settings () {
        Debug.Log("abrir panel de opciones");
    }

    public void help () {
        Debug.Log("abrir panel de ayuda");
    }
}
