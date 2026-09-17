using System.Collections;
using UnityEngine;

public class BreakPlatform : MonoBehaviour
{
    [SerializeField] float fallDelay = 3f;
    [SerializeField] float resetDelay = 3f;

    Rigidbody rb;
    Vector3 startPos;
    Quaternion startRot;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        startPos = transform.position;
        startRot = transform.rotation;
        rb.isKinematic = true;
        gameObject.SetActive(true);
    }

    void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(FallAndReset());
        }
    }

    IEnumerator FallAndReset()
    {
        yield return new WaitForSeconds(fallDelay);
        rb.isKinematic = false;
        gameObject.SetActive(false);
        yield return new WaitForSeconds(resetDelay);
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        gameObject.SetActive(true);
        transform.position = startPos;
        transform.rotation = startRot;
    }
}