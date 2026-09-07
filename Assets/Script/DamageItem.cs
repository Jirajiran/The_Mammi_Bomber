using UnityEngine;

// Put on parent empty. Child holds Trigger Collider.
// Parent needs a Kinematic Rigidbody so trigger messages reach this script.
public class DamageItem : MonoBehaviour
{
    [SerializeField] int damageAmount = 1;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        EventManager.OnTakeDamage?.Invoke(damageAmount);
        gameObject.SetActive(false);
    }
}
