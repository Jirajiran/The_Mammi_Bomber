using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 6f;
    [SerializeField] float jumpForce = 7f;
    [SerializeField] int maxJumps = 2;

    Rigidbody rb;
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
    }

    void Update()
    {
        if (!inputEnabled)
            return;

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

        jumpCount = 0;
    }

    static bool IsGroundLayer(int layer)
    {
        return layer == LayerMask.NameToLayer("Ground");
    }

    public void StopMovement()
    {
        inputEnabled = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}
