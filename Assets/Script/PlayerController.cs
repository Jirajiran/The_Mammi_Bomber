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
    bool onLadder;
    float climbSpeed = 4f;

    // LookCancelled free-look (Alt): cache + RMB orbit without rotating player
    Transform freeLookCamera;
    bool hasCachedCameraPose;
    Vector3 cachedPivotLocalPos;
    Quaternion cachedPivotLocalRot;
    Vector3 cachedCameraLocalPos;
    Quaternion cachedCameraLocalRot;
    float freeLookYaw;
    float freeLookPitch;

    public bool InputEnabled
    {
        get => inputEnabled;
        set => inputEnabled = value;
    }

    public bool OnLadder => onLadder;

    public void SetOnLadder(bool value, float speed = 0f)
    {
        onLadder = value;
        if (value && speed > 0f)
            climbSpeed = speed;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        jumpCount = 1;

        if (cameraPivot != null && cameraPivot.childCount > 0)
            freeLookCamera = cameraPivot.GetChild(0);
        else if (Camera.main != null)
            freeLookCamera = Camera.main.transform;

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
        else
            HandleCancelledFreeLook();

        if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space))
            TryJump();
    }

    void FixedUpdate()
    {
        if (!inputEnabled)
            return;

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 velocity = rb.linearVelocity;

        if (onLadder)
        {
            // Walk force → Y climb; strafe only on XZ
            Vector3 move = transform.right * x;
            if (move.sqrMagnitude > 0.0001f)
                move = move.normalized * moveSpeed;

            float y = velocity.y;
            if (Mathf.Abs(z) > 0.01f)
                y = climbSpeed * z;

            rb.linearVelocity = new Vector3(move.x, y, move.z);
        }
        else
        {
            Vector3 move = (transform.right * x + transform.forward * z);
            if (move.sqrMagnitude > 0.0001f)
                move = move.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(move.x, velocity.y, move.z);
        }
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

    void HandleCancelledFreeLook()
    {
        bool holdingRmb = Input.GetMouseButton(1);

        // RMB held: lock cursor and orbit camera only (no player yaw)
        if (holdingRmb)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (cameraPivot == null)
                return;

            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            freeLookYaw += mouseX;
            freeLookPitch -= mouseY;
            freeLookPitch = Mathf.Clamp(freeLookPitch, minPitch, maxPitch);

            cameraPivot.localRotation = Quaternion.Euler(freeLookPitch, freeLookYaw, 0f);
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void CacheCameraPoseForFreeLook()
    {
        if (cameraPivot == null)
        {
            hasCachedCameraPose = false;
            return;
        }

        cachedPivotLocalPos = cameraPivot.localPosition;
        cachedPivotLocalRot = cameraPivot.localRotation;

        if (freeLookCamera != null)
        {
            cachedCameraLocalPos = freeLookCamera.localPosition;
            cachedCameraLocalRot = freeLookCamera.localRotation;
        }

        freeLookYaw = 0f;
        freeLookPitch = cameraPitch;
        hasCachedCameraPose = true;
    }

    void RestoreCameraPoseFromFreeLook()
    {
        if (!hasCachedCameraPose || cameraPivot == null)
            return;

        cameraPivot.localPosition = cachedPivotLocalPos;
        cameraPivot.localRotation = cachedPivotLocalRot;

        if (freeLookCamera != null)
        {
            freeLookCamera.localPosition = cachedCameraLocalPos;
            freeLookCamera.localRotation = cachedCameraLocalRot;
        }

        // Keep normal look pitch in sync with restored pivot
        float restoredPitch = cachedPivotLocalRot.eulerAngles.x;
        if (restoredPitch > 180f)
            restoredPitch -= 360f;
        cameraPitch = Mathf.Clamp(restoredPitch, minPitch, maxPitch);

        freeLookYaw = 0f;
        freeLookPitch = cameraPitch;
        hasCachedCameraPose = false;
    }

    void SetMouseLook(bool enabled)
    {
        if (enabled == mouseLookEnabled)
        {
            Cursor.lockState = enabled ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !enabled;
            return;
        }

        if (!enabled)
            CacheCameraPoseForFreeLook();
        else
            RestoreCameraPoseFromFreeLook();

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
