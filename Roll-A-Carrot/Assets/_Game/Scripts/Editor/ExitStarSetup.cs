using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ExitStarSetup
{
    [MenuItem("Tools/Roll-A-Carrot/Make LevelExit a Star")]
    public static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("Stop Play mode before applying the star.");
            return;
        }
        GameObject exit = null;
        foreach (GameObject root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                if (child.name == "LevelExit") exit = child.gameObject;
        }
        if (exit == null)
        {
            Debug.LogWarning("Open RollACarrot, which contains your LevelExit object.");
            return;
        }
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) { Debug.LogError("URP Lit shader could not be found."); return; }
        const string folder = "Assets/_Game/Models";
        if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/_Game", "Models");
        const string meshPath = folder + "/ExitStar.asset";
        Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
        if (mesh == null)
        {
            mesh = BuildStar();
            AssetDatabase.CreateAsset(mesh, meshPath);
        }
        const string materialPath = folder + "/ExitStarGold.mat";
        Material gold = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (gold == null)
        {
            gold = new Material(shader);
            gold.SetColor("_BaseColor", new Color32(255, 199, 51, 255));
            gold.SetFloat("_Metallic", 0.25f);
            gold.SetFloat("_Smoothness", 0.45f);
            gold.EnableKeyword("_EMISSION");
            gold.SetColor("_EmissionColor", new Color(0.25f, 0.12f, 0.01f));
            AssetDatabase.CreateAsset(gold, materialPath);
        }
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Give LevelExit a star visual");
        Transform previous = exit.transform.Find("Exit Star Visual");
        if (previous != null) Undo.DestroyObjectImmediate(previous.gameObject);
        // Hide the old cube/model while leaving all gameplay components intact.
        foreach (Renderer renderer in exit.GetComponentsInChildren<Renderer>(true))
        {
            Undo.RecordObject(renderer, "Hide old exit visual");
            renderer.enabled = false;
            PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
        }
        GameObject visual = new GameObject("Exit Star Visual", typeof(MeshFilter), typeof(MeshRenderer));
        Undo.RegisterCreatedObjectUndo(visual, "Create exit star");
        visual.transform.SetParent(exit.transform, false);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.Euler(0f, 25f, 0f);
        visual.GetComponent<MeshFilter>().sharedMesh = mesh;
        visual.GetComponent<MeshRenderer>().sharedMaterial = gold;
        EditorSceneManager.MarkSceneDirty(exit.scene);
        AssetDatabase.SaveAssets();
        Undo.CollapseUndoOperations(group);
        Selection.activeGameObject = exit;
        Debug.Log("Gold exit star applied. Save the scene with Ctrl+S. The existing exit trigger and script were preserved.");
    }

    private static Mesh BuildStar()
    {
        Vector3[] vertices = new Vector3[22];
        for (int i = 0; i < 10; i++)
        {
            float angle = (90f + i * 36f) * Mathf.Deg2Rad;
            float radius = i % 2 == 0 ? 0.7f : 0.32f;
            vertices[i] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, -0.12f);
            vertices[i + 10] = new Vector3(vertices[i].x, vertices[i].y, 0.12f);
        }
        vertices[20] = new Vector3(0f, 0f, -0.12f);
        vertices[21] = new Vector3(0f, 0f, 0.12f);
        List<int> triangles = new List<int>();
        for (int i = 0; i < 10; i++)
        {
            int next = (i + 1) % 10;
            triangles.AddRange(new[] {20, next, i, 21, i + 10, next + 10,
                i, next, next + 10, i, next + 10, i + 10});
        }
        Mesh mesh = new Mesh { name = "Five Point Exit Star" };
        mesh.vertices = vertices;
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}