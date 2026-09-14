using UnityEngine;

// Put on the same GameObject as the MeshCollider (single object).
// Prefer isTrigger + OnTriggerEnter; non-trigger MeshCollider uses OnCollisionEnter.
public class DamageItem : MonoBehaviour
{
    [SerializeField] int damageAmount = 1;

    void OnTriggerEnter(Collider other)
    {
        TryHit(other);
    }

    void OnCollisionEnter(Collision collision)
    {
        TryHit(collision.collider);
    }

    void TryHit(Collider other)
    {
        if (!IsPlayer(other))
            return;

        EventManager.OnTakeDamage?.Invoke(damageAmount);
        gameObject.SetActive(false);
    }

    static bool IsPlayer(Collider other)
    {
        if (other.CompareTag("Player"))
            return true;

        // Child colliders on the player may not carry the tag
        return other.GetComponentInParent<PlayerHealth>() != null;
    }
}
