using UnityEngine;

public class MetaFinal : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameOver gameOver = FindObjectOfType<GameOver>();
            if (gameOver != null)
                gameOver.Ganar();
        }
    }
}

