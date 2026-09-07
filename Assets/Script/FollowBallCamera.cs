using UnityEngine;

public class FollowBallCamera : MonoBehaviour
{
    [SerializeField] Transform ball;
    [SerializeField] float heightOffset = 1.31f;
    [SerializeField] float zOffset = -1.2f;
    [SerializeField] float yawSmooth = 4f;

    float xOffset;
    float lockedPitch;
    float followYaw;
    bool hasOffset;

    public void SetTarget(Transform target)
    {
        ball = target;
        hasOffset = false;
    }

    public void ApplySavedTransform(Vector3 pos, Vector3 euler)
    {
        transform.position = pos;
        transform.rotation = Quaternion.Euler(euler);
        lockedPitch = euler.x;
        followYaw = euler.y;
        RebuildOffsetFromTransform();
    }

    void RebuildOffsetFromTransform()
    {
        if (ball == null && DriveBall.Instance != null)
            ball = DriveBall.Instance.transform;
        if (ball == null)
        {
            hasOffset = false;
            return;
        }

        Quaternion yawOnly = Quaternion.Euler(0f, followYaw, 0f);
        Vector3 local = Quaternion.Inverse(yawOnly) * (transform.position - ball.position);
        xOffset = local.x;
        heightOffset = local.y;
        zOffset = local.z;
        hasOffset = true;
    }

    void Awake()
    {
        lockedPitch = transform.eulerAngles.x;
        followYaw = transform.eulerAngles.y;
    }

    void LateUpdate()
    {
        if (ball == null && DriveBall.Instance != null)
            ball = DriveBall.Instance.transform;
        if (ball == null)
            return;

        bool aiming = DriveBall.Instance != null && DriveBall.Instance.CanForce;
        if (aiming)
        {
            float targetYaw = ball.eulerAngles.y;
            followYaw = Mathf.LerpAngle(
                followYaw,
                targetYaw,
                1f - Mathf.Exp(-yawSmooth * Time.deltaTime));
        }

        Quaternion yawOnly = Quaternion.Euler(0f, followYaw, 0f);

        if (!hasOffset)
        {
            Vector3 local = Quaternion.Inverse(yawOnly) * (transform.position - ball.position);
            xOffset = local.x;
            heightOffset = local.y;
            zOffset = local.z;
            hasOffset = true;
        }

        transform.position = ball.position + yawOnly * new Vector3(xOffset, heightOffset, zOffset);
        transform.rotation = Quaternion.Euler(lockedPitch, followYaw, 0f);
    }
}
