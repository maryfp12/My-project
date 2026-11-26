using UnityEngine;

public class SeguirPersonaje:
 MonoBehaviour
{
     public Transform objetivo;                       
    public Vector3 desfase = new Vector3(0, 4, -12); 
    public float suavizado = 5f;                  
    [Range(0f, 1f)]
    public float seguirEnX = 0.6f;                   

    void LateUpdate()
    {
        Vector3 destino = new Vector3(
            objetivo.position.x * seguirEnX + desfase.x,
            desfase.y,
            desfase.z);

        transform.position = Vector3.Lerp(transform.position, destino, suavizado * Time.deltaTime);
    }
}
    

