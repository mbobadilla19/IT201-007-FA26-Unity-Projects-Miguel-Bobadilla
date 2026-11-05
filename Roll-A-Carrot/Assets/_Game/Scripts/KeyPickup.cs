using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class KeyPickup : MonoBehaviour
{
    [Tooltip("The gate that disappears when the player collects this key.")]
    public GameObject gate;
    private bool collected;

    void Reset()
    {
        SphereCollider trigger = GetComponent<SphereCollider>();
        trigger.isTrigger = true;
    }

    void Awake()
    {
        GetComponent<SphereCollider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (collected || other.attachedRigidbody == null)
            return;

        PlayerController player = other.attachedRigidbody.GetComponent<PlayerController>();
        if (player == null || player.LivesRemaining <= 0)
            return;

        // Keep the key available if its Inspector reference has not been assigned.
        if (gate == null || gate == gameObject || transform.IsChildOf(gate.transform))
        {
            Debug.LogWarning("Assign a separate KeyGate object to this key's Gate field.", this);
            return;
        }

        collected = true;
        gate.SetActive(false);
        gameObject.SetActive(false);
        Debug.Log("Key collected. Gate unlocked!", this);
    }
}