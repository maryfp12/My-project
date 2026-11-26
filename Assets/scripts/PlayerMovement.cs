using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float speed = 4f;
    public float runSpeed = 7f;          // NUEVO: velocidad al correr con Shift
    public float acceleration = 15f;     // NUEVO: que tan rapido cambia entre caminar y correr
    public float rotationSpeed = 12f;
    public float gravity = -20f;
    public float jumpHeight = 1.5f;

    public bool canMove = true;          // NUEVO: ponlo en false para congelar al personaje

    CharacterController cc;
    Animator anim;
    Transform cam;
    float vY;
    float currentSpeed;                  // NUEVO
    bool hasRunningParam;                // NUEVO

    void Start()
    {
        cc = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();
        cam = Camera.main.transform;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Solo usa el bool "Running" si existe en el Animator
        if (anim != null)
            foreach (var p in anim.parameters)
                if (p.name == "Running" && p.type == AnimatorControllerParameterType.Bool) hasRunningParam = true;
    }

    void Update()
    {
        Vector3 input = canMove
            ? new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized
            : Vector3.zero;

        Vector3 fwd = cam.forward; fwd.y = 0; fwd.Normalize();
        Vector3 right = cam.right; right.y = 0; right.Normalize();
        Vector3 dir = fwd * input.z + right * input.x;

        bool moving = dir.sqrMagnitude > 0.01f;
        bool running = moving && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));

        if (moving)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), rotationSpeed * Time.deltaTime);

        // La velocidad sube o baja poco a poco hacia la meta
        float target = !moving ? 0f : (running ? runSpeed : speed);
        currentSpeed = Mathf.MoveTowards(currentSpeed, target, acceleration * Time.deltaTime);

        if (cc.isGrounded && vY < 0) vY = -2f;
        if (canMove && cc.isGrounded && Input.GetButtonDown("Jump")) vY = Mathf.Sqrt(jumpHeight * -2f * gravity);
        vY += gravity * Time.deltaTime;

        Vector3 move = dir * currentSpeed;
        move.y = vY;
        cc.Move(move * Time.deltaTime);

        if (anim)
        {
            anim.SetFloat("Speed", moving ? 1f : 0f, 0.1f, Time.deltaTime);
            if (hasRunningParam) anim.SetBool("Running", running);
        }
    }
}