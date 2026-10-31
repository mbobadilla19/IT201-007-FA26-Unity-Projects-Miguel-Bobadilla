using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class SkyboxZone : MonoBehaviour
{
    public Material skyboxMaterial;

    void Reset()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        // Only the player changes the sky, not the pet or enemies.
        Rigidbody body = other.attachedRigidbody;
        if (body == null || body.GetComponent<PlayerController>() == null)
        {
            return;
        }

        if (skyboxMaterial != null && RenderSettings.skybox != skyboxMaterial)
        {
            RenderSettings.skybox = skyboxMaterial;
        }
    }
}