using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider))]
public class LevelExit : MonoBehaviour
{
    public string nextSceneName = "CarrotValley";
    private bool loading;

    private void Reset()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (loading || other.attachedRigidbody == null)
            return;

        PlayerController player = other.attachedRigidbody.GetComponent<PlayerController>();
        if (player == null || !player.CanUseExit)
            return;

        if (!Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            Debug.LogError("Add the destination scene to the Build Profiles Scene List: " + nextSceneName, this);
            return;
        }

        // Only the player with enough pickups can move to the next scene.
        loading = true;
        SceneManager.LoadScene(nextSceneName);
    }
}