using System.Collections;
using UnityEngine;

// Scene setup:
//   Empty (reset anchor)
//    └─ Board (Mesh + Collider + Rigidbody isKinematic=true + BreakPlatform)
// Assign resetPoint to the Empty; if null, uses parent or Awake pose.
public class BreakPlatform : MonoBehaviour
{
    [SerializeField] float fallDelay = 3f;
    [SerializeField] float resetDelay = 3f;
    [SerializeField] Transform resetPoint;

    Rigidbody rb;
    Vector3 startPos;
    Quaternion startRot;
    bool busy;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (resetPoint == null)
            resetPoint = transform.parent;

        if (resetPoint != null)
        {
            startPos = resetPoint.position;
            startRot = resetPoint.rotation;
        }
        else
        {
            startPos = transform.position;
            startRot = transform.rotation;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        TryStart(collision.collider);
    }

    void OnTriggerEnter(Collider other)
    {
        TryStart(other);
    }

    void TryStart(Collider other)
    {
        if (busy)
            return;
        if (!IsPlayer(other))
            return;

        busy = true;
        StartCoroutine(FallAndReset());
    }

    IEnumerator FallAndReset()
    {
        yield return new WaitForSeconds(fallDelay);

        if (rb != null)
            rb.isKinematic = false;

        yield return new WaitForSeconds(resetDelay);

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        if (resetPoint != null)
        {
            transform.position = resetPoint.position;
            transform.rotation = resetPoint.rotation;
        }
        else
        {
            transform.position = startPos;
            transform.rotation = startRot;
        }

        busy = false;
    }

    static bool IsPlayer(Collider other)
    {
        if (other.CompareTag("Player"))
            return true;

        return other.GetComponentInParent<PlayerHealth>() != null;
    }
}
