using UnityEngine;
using UnityEngine.SceneManagement;

public class Pasadizo : MonoBehaviour
{
    [SerializeField] float duration = 5f; 
    [SerializeField] float escalaFinal = 3f;
    [SerializeField] string siguienteEscena = "juego"; 

    RectTransform rt;
    float t;

    void Start()
    {
        rt = GetComponent<RectTransform>();
    }

   
    void Update()
    {
        if (t >= 1f) return;
        t += Time.deltaTime/duration;
        float k = Mathf.SmoothStep(0f, 1f, t);
        rt.localScale = Vector3.one * Mathf.Lerp(1f, escalaFinal, k);

        if ( t >= 1f && siguienteEscena != "")
            SceneManager.LoadScene(siguienteEscena);

    }
}
