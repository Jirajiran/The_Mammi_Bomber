using UnityEngine;

// Put on parent empty. Child holds Trigger Collider.
// Parent needs a Kinematic Rigidbody so trigger messages reach this script.
public class SavePoint : MonoBehaviour
{
    [SerializeField] bool saveOnce = true;
    bool used;

    void OnTriggerEnter(Collider other)
    {
        if (used && saveOnce)
            return;
        if (!other.CompareTag("Player"))
            return;

        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        if (health == null)
            return;

        Setting.SaveGame(
            health.HP,
            health.Points,
            health.transform.position,
            CollectPointStates());

        used = true;
        gameObject.SetActive(false);
    }

    static bool[] CollectPointStates()
    {
        PointItem[] items = FindObjectsByType<PointItem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        System.Array.Sort(items, (a, b) => string.CompareOrdinal(a.name, b.name));

        bool[] states = new bool[items.Length];
        for (int i = 0; i < items.Length; i++)
            states[i] = items[i].gameObject.activeSelf;
        return states;
    }
}
