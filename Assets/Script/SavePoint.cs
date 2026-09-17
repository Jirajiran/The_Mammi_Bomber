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
        AudioManager.instance?.PlaySfx(4);
        health.HealFull();
        SetCheckpoint(health.transform.position);

        bool[] collected = GameManager.instance != null
            ? GameManager.instance.GetCollectedStates()
            : System.Array.Empty<bool>();

        Setting.SaveGame(
            health.HP,
            health.Points,
            health.transform.position,
            collected);

        used = true;
    }
}
