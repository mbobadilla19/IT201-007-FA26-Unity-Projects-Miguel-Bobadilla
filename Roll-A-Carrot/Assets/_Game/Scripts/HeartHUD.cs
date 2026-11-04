using UnityEngine;

public class HeartHUD : MonoBehaviour
{
    public PlayerController player;
    public Vector2 topLeftPadding = new Vector2(24f, 24f);
    public float heartSize = 36f;
    public float spacing = 8f;
    public Color fullHeartColor = new Color(0.95f, 0.15f, 0.22f, 1f);
    public Color emptyHeartColor = new Color(0.32f, 0.32f, 0.35f, 1f);

    private HeartIcon[] hearts;
    private int displayedLives = -1;
    private GameObject heartRow;

    void Start()
    {
        if (player == null)
        {
            Debug.LogWarning("HeartHUD: assign the Player in the Inspector.", this);
            return;
        }

        heartRow = new GameObject("LifeHearts", typeof(RectTransform));
        RectTransform row = heartRow.GetComponent<RectTransform>();
        row.SetParent(transform, false);
        row.anchorMin = row.anchorMax = new Vector2(0f, 1f);
        row.pivot = new Vector2(0f, 1f);
        row.anchoredPosition = new Vector2(topLeftPadding.x, -topLeftPadding.y);

        hearts = new HeartIcon[Mathf.Max(1, player.startingLives)];
        row.sizeDelta = new Vector2(hearts.Length * (heartSize + spacing) - spacing, heartSize);

        for (int i = 0; i < hearts.Length; i++)
        {
            GameObject icon = new GameObject("Heart " + (i + 1),
                typeof(RectTransform), typeof(CanvasRenderer), typeof(HeartIcon));
            RectTransform rect = icon.GetComponent<RectTransform>();
            rect.SetParent(row, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(i * (heartSize + spacing), 0f);
            rect.sizeDelta = new Vector2(heartSize, heartSize);
            hearts[i] = icon.GetComponent<HeartIcon>();
            hearts[i].raycastTarget = false;
        }
    }

    void LateUpdate()
    {
        if (player == null || hearts == null || displayedLives == player.LivesRemaining)
        {
            return;
        }

        displayedLives = player.LivesRemaining;
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].color = i < displayedLives ? fullHeartColor : emptyHeartColor;
        }
    }

    void OnDestroy()
    {
        if (heartRow != null)
        {
            Destroy(heartRow);
        }
    }
}
