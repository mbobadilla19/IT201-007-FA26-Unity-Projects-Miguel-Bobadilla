using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class RepairCoinLayout
{
    [MenuItem("Tools/Roll-A-Carrot/Apply 12-Coin Layout")]
    public static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("Stop Play before repairing the coin layout.");
            return;
        }
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name != "RollACarrot")
        {
            Debug.LogWarning("Open RollACarrot before applying its coin layout.");
            return;
        }
        Transform[] objects = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Transform>(true)).ToArray();
        PlayerController player = objects.Select(t => t.GetComponent<PlayerController>()).FirstOrDefault(p => p != null);
        GameObject coins = objects.FirstOrDefault(t => t.name == "Coins")?.gameObject;
        GameObject exit = objects.FirstOrDefault(t => t.name == "LevelExit")?.gameObject;
        if (player == null || coins == null || exit == null || exit.GetComponent<LevelExit>() == null)
        {
            Debug.LogError("Repair stopped: Player, Coins, or LevelExit component is missing.");
            return;
        }
        Transform[] pickups = coins.GetComponentsInChildren<Transform>(true)
            .Where(t => t.GetComponent<Collider>() != null && t.CompareTag("PickUp")).ToArray();
        string[] extraNames = { "PickUp (14)", "PickUp (15)" };
        Transform[] keep = pickups.Where(t => !extraNames.Contains(t.name) && t.gameObject.activeInHierarchy).ToArray();
        if (keep.Length != 12)
        {
            Debug.LogError("Repair stopped: expected 12 remaining active coins, found " + keep.Length + ". No scene changes made.");
            return;
        }

        Undo.SetCurrentGroupName("Repair 12-Coin Progression");
        int group = Undo.GetCurrentGroup();
        foreach (Transform pickup in pickups.Where(t => extraNames.Contains(t.name)))
        {
            Undo.RecordObject(pickup.gameObject, "Disable extra coin");
            pickup.gameObject.SetActive(false);
            PrefabUtility.RecordPrefabInstancePropertyModifications(pickup.gameObject);
        }
        Undo.RecordObject(player, "Reconnect level exit");
        player.requiredPickups = 12;
        player.winAfterPickups = false;
        player.levelExit = exit;
        EditorUtility.SetDirty(player);
        PrefabUtility.RecordPrefabInstancePropertyModifications(player);
        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(scene);
        Selection.activeGameObject = player.gameObject;
        Debug.Log("Layout repaired: Grass 2, Desert 3, Ice 3, Castle 3, Sky 1. Doors unlock at 2 / 5 / 8 / 11; exit at 12. Save the scene.", player);
    }
}