using UnityEngine;

public enum BallColor
{
    White,
    Red,
    Yellow,
    Green,
    Brown,
    Blue,
    Pink,
    Black
}

public class ball : MonoBehaviour
{
    [SerializeField] int point;

    MeshRenderer meshRenderer;
    Collider ballCollider;

    public int Point => point;

    public bool IsHidden
    {
        get
        {
            CacheComponents();
            return meshRenderer != null && !meshRenderer.enabled;
        }
    }

    void Awake()
    {
        CacheComponents();
    }

    void CacheComponents()
    {
        if (meshRenderer == null)
            meshRenderer = GetComponentInChildren<MeshRenderer>(true);
        if (ballCollider == null)
            ballCollider = GetComponent<Collider>();
    }

    public void Apply(int points, Material ballMaterial)
    {
        point = points;
        CacheComponents();
        if (ballMaterial != null && meshRenderer != null)
            meshRenderer.material = ballMaterial;
    }

    public void HideBall()
    {
        CacheComponents();
        if (meshRenderer != null)
            meshRenderer.enabled = false;
        if (ballCollider != null)
            ballCollider.enabled = false;
    }

    public void ShowBall()
    {
        CacheComponents();
        if (meshRenderer != null)
            meshRenderer.enabled = true;
        if (ballCollider != null)
            ballCollider.enabled = true;
    }

    void OnCollisionEnter(Collision collision)
    {
        float vol = Mathf.Clamp01(collision.relativeVelocity.magnitude / 8f);
        if (vol < 0.05f)
            return;

        ball otherBall = collision.collider.GetComponentInParent<ball>();
        if (otherBall != null)
        {
            if (otherBall == this)
                return;
            // Only one of the two balls plays the hit.
            if (GetInstanceID() > otherBall.GetInstanceID())
                return;
            AudioManager.instance.PlaySfx(3, vol);
            return;
        }

        string hitName = collision.collider.gameObject.name;
        if (hitName.StartsWith("Edge") || hitName.Contains("Edge"))
            AudioManager.instance.PlaySfx(3, vol);
    }
}
