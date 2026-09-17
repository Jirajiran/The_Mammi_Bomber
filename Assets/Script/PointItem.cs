using UnityEngine;

// Put on parent empty. Child holds Trigger Collider.
// Parent needs a Kinematic Rigidbody so trigger messages reach this script.
public class PointItem : MonoBehaviour
{
    [SerializeField] int pointValue = 1;

    int index = -1;

    public void Setup(int pointIndex)
    {
        index = pointIndex;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        if (health == null)
            return;

        health.AddPoints(pointValue);

        if (GameManager.instance != null)
            GameManager.instance.MarkPointCollected(index);
        AudioManager.instance?.PlaySfx(2);
        gameObject.SetActive(false);
    }
}
