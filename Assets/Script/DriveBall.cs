using UnityEngine;
using UnityEngine.UI;

public class DriveBall : MonoBehaviour
{
    public static DriveBall Instance { get; private set; }

    [SerializeField] bool canForce = true;
    [SerializeField] GameObject showArrow;
    [SerializeField] Button shotButton;
    [SerializeField] int forcePower = 10;
    [SerializeField] float rotateSpeed = 100f;
    [SerializeField] float rotateSpeedBoost = 2.5f;
    [SerializeField] float rotateAccel = 8f;
    [SerializeField] float stopSpeed = 0.05f;
    [SerializeField] float stillDuration = 0.20f;
    [SerializeField] Transform cueRespawn;

    Rigidbody rb;
    float stillTime;
    int shotCount;
    float currentTurn;
    Vector3 arrowLocalOffset;
    float arrowYawOffset;
    public bool CanForce => canForce;
    public int ShotCount => shotCount;

    void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        arrowLocalOffset = showArrow.transform.localPosition;
        arrowYawOffset = showArrow.transform.localEulerAngles.y;
        
        if (canForce)
            SetAimMode(true);
    }

    void Start()
    {
        if (shotButton != null)
            shotButton.onClick.AddListener(Shoot);
        ShowArrow(canForce);
        SetShotButton(canForce);
    }

    void Update()
    {
        if (!canForce || GameManager.instance == null || GameManager.instance.IsLocked)
            return;

        RotationBall();
        if (Input.GetKeyDown(KeyCode.Space))
            Shoot();
    }

    void LateUpdate()
    {
        AlignArrow();
    }

    void FixedUpdate()
    {
        if (canForce || rb == null)
            return;

        if (IsAlmostStopped())
        {
            stillTime += Time.fixedDeltaTime;
            if (stillTime >= stillDuration)
                ResetPos();
        }
        else
        {
            stillTime = 0f;
        }
    }

    public void Shoot()
    {
        if (!canForce || GameManager.instance == null || GameManager.instance.IsLocked)
            return;

        Vector3 shotDir = transform.forward;
        shotDir.y = 0f;
        if (shotDir.sqrMagnitude < 0.0001f)
            return;
        shotDir.Normalize();

        shotCount++;
        stillTime = 0f;
        ShowArrow(false);
        SetShotButton(false);
        SetAimMode(false);
        currentTurn = 0f;
        if (GameManager.instance != null)
            GameManager.instance.SetTurn(shotCount);
        AudioManager.instance.PlaySfx(1);
        rb.linearDamping = 0.8f;
        rb.angularDamping = 0.8f;
        rb.AddForce(shotDir * forcePower, ForceMode.Impulse);
    }

    void RotationBall()
    {
        float input = 0f;
        if (Input.GetKey(KeyCode.A))
            input -= 0.3f;
        if (Input.GetKey(KeyCode.D))
            input += 0.3f;

        float speed = rotateSpeed;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            speed *= rotateSpeedBoost;

        float target = input * speed;
        currentTurn = Mathf.Lerp(
            currentTurn,
            target,
            1f - Mathf.Exp(-rotateAccel * Time.deltaTime));
        transform.Rotate(0f, currentTurn * Time.deltaTime, 0f);
    }

    void ResetPos()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        var euler = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(0f, euler.y, 0f);

        SetAimMode(true);
        ShowArrow(true);
        SetShotButton(true);
        currentTurn = 0f;
        GameManager.instance.OnCueStopped(shotCount);
    }

    public void SetShotCount(int shots)
    {
        shotCount = shots;
    }

    public void ApplySaveState(Vector3 pos, float rotY, int shots, bool aiming)
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = pos;
        transform.rotation = Quaternion.Euler(0f, rotY, 0f);
        shotCount = shots;
        stillTime = 0f;
        currentTurn = 0f;
        SetAimMode(aiming);
        ShowArrow(aiming);
        SetShotButton(aiming);
    }

    public void RespawnAtCuePoint()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = cueRespawn.position;

        var euler = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(0f, euler.y, 0f);

        GetComponent<ball>().ShowBall();

        stillTime = 0f;
        currentTurn = 0f;
        SetAimMode(true);
        ShowArrow(true);
        SetShotButton(true);
    }

    void SetAimMode(bool aiming)
    {
        canForce = aiming;
        rb.isKinematic = aiming;
        if (aiming)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    bool IsAlmostStopped()
    {
        float stopSpeedSquared = stopSpeed * stopSpeed;
        return rb.linearVelocity.sqrMagnitude < stopSpeedSquared;
    }

    void ShowArrow(bool visible)
    {
        if (showArrow != null)
            showArrow.SetActive(visible);
    }

    void SetShotButton(bool visible)
    {
        if (shotButton != null)
            shotButton.gameObject.SetActive(visible);
    }

    void AlignArrow()
    {
        if (showArrow == null || !showArrow.activeSelf)
            return;

        float yaw = transform.eulerAngles.y;
        Quaternion yawOnly = Quaternion.Euler(0f, yaw, 0f);
        showArrow.transform.SetPositionAndRotation(
            transform.position + yawOnly * arrowLocalOffset,
            Quaternion.Euler(0f, yaw + arrowYawOffset, 0f));
    }
}
