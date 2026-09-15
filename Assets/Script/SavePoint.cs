using UnityEngine;

// Put on parent empty. Child holds Trigger Collider.
// Parent needs a Kinematic Rigidbody so trigger messages reach this script.
public class SavePoint : MonoBehaviour
{
    public static Vector3 LastCheckpoint { get; private set; }
    public static bool HasCheckpoint { get; private set; }

    [SerializeField] bool saveOnce = true;
    bool used;

    public static void SetCheckpoint(Vector3 position)
    {
        LastCheckpoint = position;
        HasCheckpoint = true;
    }

    public static void ClearCheckpoint()
    {
        HasCheckpoint = false;
        LastCheckpoint = Vector3.zero;
    }

    void OnTriggerEnter(Collider other)
    {
        if (used && saveOnce)
            return;
        if (!other.CompareTag("Player"))
            return;

        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        if (health == null)
            return;

        health.HealFull();
        SetCheckpoint(health.transform.position);

        Setting.SaveGame(
            health.HP,
            health.Points,
            health.transform.position,
            CollectPointStates());

        used = true;
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
