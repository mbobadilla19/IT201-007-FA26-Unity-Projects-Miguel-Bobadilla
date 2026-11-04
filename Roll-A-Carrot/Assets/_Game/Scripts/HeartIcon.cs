using UnityEngine;
using UnityEngine.UI;

// Draws a heart directly as a UI shape; no downloaded artwork is required.
public class HeartIcon : MaskableGraphic
{
    protected override void OnPopulateMesh(VertexHelper vertices)
    {
        vertices.Clear();
        Rect rect = GetPixelAdjustedRect();
        DrawHeart(vertices, rect, 1f, new Color(0.18f, 0.08f, 0.08f, color.a));
        DrawHeart(vertices, rect, 0.83f, color);
    }

    private static void DrawHeart(VertexHelper vertices, Rect rect, float scale, Color tint)
    {
        const int segments = 64;
        int firstVertex = vertices.currentVertCount;
        vertices.AddVert((Vector3)rect.center, tint, Vector2.zero);

        for (int i = 0; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            float sin = Mathf.Sin(angle);
            float x = 16f * sin * sin * sin;
            float y = 13f * Mathf.Cos(angle) - 5f * Mathf.Cos(2f * angle)
                - 2f * Mathf.Cos(3f * angle) - Mathf.Cos(4f * angle);
            Vector2 position = rect.center + new Vector2(
                x / 32f * rect.width, (y + 2.5f) / 29f * rect.height) * scale;
            vertices.AddVert((Vector3)position, tint, Vector2.zero);
        }

        for (int i = 0; i < segments; i++)
        {
            vertices.AddTriangle(firstVertex, firstVertex + i + 1, firstVertex + i + 2);
        }
    }
}
