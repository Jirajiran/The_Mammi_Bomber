using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 6f;
    [SerializeField] float jumpForce = 7f;
    [SerializeField] int maxJumps = 2;
    [SerializeField] Transform cameraPivot;
    
    float mouseSensitivity = 2f;
    float minPitch = -80f;
    float maxPitch = 80f;
    bool mouseLookEnabled = true;

    Rigidbody rb;
    float cameraPitch;
    int jumpCount;
    bool inputEnabled = true;

    public bool InputEnabled
    {
        get => inputEnabled;
        set => inputEnabled = value;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        jumpCount = 1;

        SetMouseLook(mouseLookEnabled);
    }

    void Update()
    {
        if (!inputEnabled)
            return;

        if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt))
            SetMouseLook(!mouseLookEnabled);

        if (mouseLookEnabled)
            HandleMouseLook();

        if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space))
            TryJump();
    }

    void FixedUpdate()
    {
        if (!inputEnabled)
            return;

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 move = new Vector3(x, 0f, z).normalized * moveSpeed;
        Vector3 velocity = rb.linearVelocity;
        rb.linearVelocity = new Vector3(move.x, velocity.y, move.z);
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Player: ซ้าย / ขวา เฉพาะแกน Y
        transform.Rotate(Vector3.up * mouseX);

        // Camera Pivot: ขึ้น / ลง เฉพาะแกน X
        if (cameraPivot != null)
        {
            cameraPitch -= mouseY;
            cameraPitch = Mathf.Clamp(cameraPitch, minPitch, maxPitch);

            cameraPivot.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        }
    }

    void SetMouseLook(bool enabled)
    {
        mouseLookEnabled = enabled;

        Cursor.lockState = enabled ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !enabled;
    }

    void TryJump()
    {
        if (jumpCount >= maxJumps)
            return;

        Vector3 velocity = rb.linearVelocity;
        velocity.y = 0f;
        rb.linearVelocity = velocity;
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        jumpCount++;
    }

    void OnCollisionEnter(Collision collision)
    {
        CheckGround(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        CheckGround(collision);
    }

    void CheckGround(Collision collision)
    {
        if (!IsGroundLayer(collision.gameObject.layer))
            return;

        jumpCount = 1;
    }

    static bool IsGroundLayer(int layer)
    {
        return layer == LayerMask.NameToLayer("Ground");
    }

    public void StopMovement()
    {
        inputEnabled = false;
        SetMouseLook(false);
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public void ResumeInput()
    {
        inputEnabled = true;
        SetMouseLook(true);
    }
}
