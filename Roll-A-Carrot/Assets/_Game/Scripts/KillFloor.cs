using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class KillFloor : MonoBehaviour
{
    void Reset()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        Rigidbody body = other.attachedRigidbody;
        if (body == null)
        {
            return;
        }

        PlayerController player = body.GetComponent<PlayerController>();
        if (player != null)
        {
            player.Respawn();
        }
    }
}