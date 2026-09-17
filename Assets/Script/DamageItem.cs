using UnityEngine;

public class DamageItem : MonoBehaviour
{
    [SerializeField] int damageAmount = 1;

    void OnTriggerEnter(Collider other)
    {
        TryFallWorld(other);
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

    void TryFallWorld(Collider other)
    {
        if (!IsPlayer(other))
            return;
        damageAmount = 999;
        EventManager.OnTakeDamage?.Invoke(damageAmount);
    }

    static bool IsPlayer(Collider other)
    {
        if (other.CompareTag("Player"))
            return true;

        return other.GetComponentInParent<PlayerHealth>() != null;
    }
}
