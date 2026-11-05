using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class SeedPickup : MonoBehaviour
{
    private bool collected;
    void Reset() { GetComponent<SphereCollider>().isTrigger = true; }
    void Awake() { GetComponent<SphereCollider>().isTrigger = true; }

    void OnTriggerEnter(Collider other)
    {
        if (collected || other.attachedRigidbody == null) return;
        SeedWallet wallet = other.attachedRigidbody.GetComponent<SeedWallet>();
        if (wallet == null) return;
        PlayerController player = wallet.GetComponent<PlayerController>();
        if (player.LivesRemaining <= 0) return;
        collected = true;
        wallet.AddSeed();
        gameObject.SetActive(false);
    }
}
