using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneUiBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        EnsureMenu();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) { EnsureMenu(); }

    private static void EnsureMenu()
    {
        foreach (PauseMenu menu in Object.FindObjectsByType<PauseMenu>(FindObjectsSortMode.None))
            if (menu.isActiveAndEnabled) return;
        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null) player.gameObject.AddComponent<PauseMenu>();
    }
}
