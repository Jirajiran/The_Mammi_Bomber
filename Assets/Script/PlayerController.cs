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
    [SerializeField] float idleVocalInterval = 3f;

    static readonly string[] IdleVocals = { "idleTime_1", "IdleTime_2" };

    Rigidbody rb;
    Animator animator;
    PlayerHealth health;
    float cameraPitch;
    float pendingYaw;
    float idleVocalTimer;
    int jumpCount;
    bool inputEnabled = true;
    bool onLadder;
    float climbSpeed = 4f;
    bool isGrounded;

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

        health = GetComponent<PlayerHealth>();
        if (health != null && health.Animator != null)
            animator = health.Animator;
        if (animator == null)
            animator = GetComponentInChildren<Animator>(true);

        if (cameraPivot != null && cameraPivot.childCount > 0)
            freeLookCamera = cameraPivot.GetChild(0);
        else if (Camera.main != null)
            freeLookCamera = Camera.main.transform;
    }

    void Start()
    {
        SnapGameplayCursor();
    }

    void Update()
    {
        TickIdleVocal();

        if (!inputEnabled)
            return;

        if (GameManager.instance != null && GameManager.instance.IsLocked)
        {
            pendingYaw = 0f;
            return;
        }

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
        {
            pendingYaw = 0f;
            return;
        }

        if (GameManager.instance == null || !GameManager.instance.IsLocked)
            ApplyPendingYaw();

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 velocity = rb.linearVelocity;
        bool walking = Mathf.Abs(x) > 0.01f || Mathf.Abs(z) > 0.01f;

        if (animator != null)
        {
            animator.SetBool("IsWalk", walking);
            animator.SetBool("OnGround", isGrounded);
        }

        if (onLadder)
        {
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

        isGrounded = false;
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        pendingYaw += mouseX;

        if (cameraPivot != null)
        {
            cameraPitch -= mouseY;
            cameraPitch = Mathf.Clamp(cameraPitch, minPitch, maxPitch);
            cameraPivot.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        }
    }

    void ApplyPendingYaw()
    {
        if (!mouseLookEnabled || Mathf.Abs(pendingYaw) < 0.0001f)
            return;

        Quaternion yaw = Quaternion.Euler(0f, pendingYaw, 0f);
        rb.MoveRotation(rb.rotation * yaw);
        pendingYaw = 0f;
    }

    void HandleCancelledFreeLook()
    {
        bool holdingRmb = Input.GetMouseButton(1);

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
            RefreshCursor();
            return;
        }

        if (!enabled)
            CacheCameraPoseForFreeLook();
        else
            RestoreCameraPoseFromFreeLook();

        mouseLookEnabled = enabled;
        pendingYaw = 0f;
        RefreshCursor();
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
        isGrounded = false;

        if (animator != null)
            animator.SetTrigger("GetJump");

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySfx("JumpSFX");
            AudioManager.instance.PlaySfx("JumpVocal");
        }
    }

    void TickIdleVocal()
    {
        if (!inputEnabled)
            return;
        if (GameManager.instance != null && GameManager.instance.IsLocked)
            return;
        if (health != null && health.IsDead)
            return;
        if (AudioManager.instance == null)
            return;

        idleVocalTimer += Time.deltaTime;
        if (idleVocalTimer < idleVocalInterval)
            return;

        idleVocalTimer = 0f;
        int points = health != null ? health.Points : 0;
        int i = AudioManager.PickIndex(IdleVocals.Length, 1, transform.position, points);
        AudioManager.instance.PlayIdle(IdleVocals[i]);
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
        isGrounded = true;
    }

    static bool IsGroundLayer(int layer)
    {
        return layer == LayerMask.NameToLayer("Ground");
    }

    public void StopMovement()
    {
        inputEnabled = false;
        pendingYaw = 0f;
        idleVocalTimer = 0f;
        SetMouseLook(false);
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public void ResumeInput()
    {
        inputEnabled = true;
        SnapGameplayCursor();
    }

    public void SnapGameplayCursor()
    {
        mouseLookEnabled = true;
        pendingYaw = 0f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void RefreshCursor()
    {
        Cursor.lockState = mouseLookEnabled ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !mouseLookEnabled;
    }
}
