using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class SkyboxZone : MonoBehaviour
{
    public Material skyboxMaterial;

    [Header("Area Checkpoint")]
    [Tooltip("A safe player spawn point inside this area. Leave empty to keep the current checkpoint.")]
    public Transform respawnPoint;

    [Header("Zone Music")]
    public AudioSource musicSource;
    public AudioClip zoneMusic;
    [Range(0f, 1f)] public float musicVolume = 0.3f;

    void Reset()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        // Only the player changes the sky and music, not pets or enemies.
        Rigidbody body = other.attachedRigidbody;
        PlayerController player = body != null
            ? body.GetComponent<PlayerController>() : null;
        if (player == null)
        {
            return;
        }

        // Entering an area changes where the next lost life respawns the player.
        if (respawnPoint != null)
        {
            player.respawnPoint = respawnPoint;
        }

        ApplySettings();
    }

    public void ApplySettings()
    {
        if (skyboxMaterial != null && RenderSettings.skybox != skyboxMaterial)
        {
            RenderSettings.skybox = skyboxMaterial;
        }

        // Every zone shares one source, so background tracks cannot overlap.
        if (musicSource != null && zoneMusic != null)
        {
            musicSource.volume = musicVolume;
            musicSource.loop = true;
            musicSource.spatialBlend = 0f;

            if (musicSource.clip != zoneMusic)
            {
                musicSource.Stop();
                musicSource.clip = zoneMusic;
                musicSource.Play();
            }
            else if (!musicSource.isPlaying)
            {
                // Re-entering a zone with the same track does not restart it.
                musicSource.Play();
            }
        }
    }
}