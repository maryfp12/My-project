using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float speed = 4f;
    public float rotationSpeed = 12f;
    public float gravity = -20f;
    public float jumpHeight = 1.5f;

    CharacterController cc;
    Animator anim;
    Transform cam;
    float vY;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();
        cam = Camera.main.transform;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

        Vector3 fwd = cam.forward; fwd.y = 0; fwd.Normalize();
        Vector3 right = cam.right; right.y = 0; right.Normalize();
        Vector3 dir = fwd * input.z + right * input.x;

        bool moving = dir.sqrMagnitude > 0.01f;
        if (moving)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), rotationSpeed * Time.deltaTime);

        if (cc.isGrounded && vY < 0) vY = -2f;
        if (cc.isGrounded && Input.GetButtonDown("Jump")) vY = Mathf.Sqrt(jumpHeight * -2f * gravity);
        vY += gravity * Time.deltaTime;

        Vector3 move = dir * speed;
        move.y = vY;
        cc.Move(move * Time.deltaTime);

        if (anim) anim.SetFloat("Speed", moving ? 1f : 0f, 0.1f, Time.deltaTime);
    }
}
