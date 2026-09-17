using UnityEngine;

// Put on parent empty. Child holds Trigger Collider.
// Parent needs a Kinematic Rigidbody so trigger messages reach this script.
public class WinFlagItem : MonoBehaviour
{
    static int collectedCount;

    public static int CollectedCount => collectedCount;

    public static void ResetCount()
    {
        collectedCount = 0;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;
        AudioManager.instance?.PlaySfx(6);
        collectedCount++;
        EventManager.OnWinFlagReached?.Invoke(collectedCount);
        gameObject.SetActive(false);
    }
}
