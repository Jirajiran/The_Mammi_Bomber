using UnityEngine;

public class AreaHole : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        var pottedBall = other.GetComponent<ball>();
        if (pottedBall == null)
            pottedBall = other.GetComponentInParent<ball>();
        if (pottedBall == null)
            return;

        AudioManager.instance.PlaySfx(2);
        GameManager.instance.BallPotted(pottedBall);
    }
}
