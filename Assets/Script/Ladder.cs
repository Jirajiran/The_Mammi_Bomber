using UnityEngine;

public class Ladder : MonoBehaviour
{
    float climbSpeed = 4f;

    void OnTriggerStay(Collider other)
    {
        TryEnter(other);
    }

    void OnTriggerExit(Collider other)
    {
        TryExit(other);
    }

    void OnCollisionStay(Collision collision)
    {
        TryEnter(collision.collider);
    }

    void OnCollisionExit(Collision collision)
    {
        TryExit(collision.collider);
    }

    void TryEnter(Collider other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player == null)
            return;

        player.SetOnLadder(true, climbSpeed);
    }

    void TryExit(Collider other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player == null)
            return;

        player.SetOnLadder(false);
    }
}
