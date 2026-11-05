using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DragonDialogue : MonoBehaviour
{
    public Transform player;
    public string speakerName = "Ember";
    [Min(0.5f)] public float talkDistance = 3.5f;
    [TextArea(2, 4)] public string[] lines =
    {
        "Whoa, easy! I'm Ember. I'm not going to chase you.",
        "Sorry about my brothers back there. They see one running carrot and forget all their manners.",
        "Let me make it up to you. Push that loose rock aside. There's a key behind it that opens the gate.",
        "Watch the spinning barrier. Wait for an opening, then dash through with Shift.",
        "Three gems can buy you one life. Press B when you're running low. They won't be spent if your lives are full.",
        "Good luck, little carrot. If you see my brothers again, tell them Ember said to behave."
    };

    private GameObject canvasObject;
    private GameObject prompt;
    private GameObject dialogue;
    private TMP_Text message;
    private TMP_Text nextLabel;
    private int lineIndex;
    private bool talking;

    void Start()
    {
        if (player == null)
        {
            PlayerController controller = FindFirstObjectByType<PlayerController>();
            if (controller != null) player = controller.transform;
        }
        BuildUI();
    }

    void Update()
    {
        // Pause menus take priority over conversations.
        PlayerController controller = player != null ? player.GetComponent<PlayerController>() : null;
        bool canTalk = player != null && Time.timeScale > 0f &&
            (controller == null || controller.LivesRemaining > 0) &&
            (player.position - transform.position).sqrMagnitude <= talkDistance * talkDistance;
        if (!canTalk)
        {
            Close();
            return;
        }
        prompt.SetActive(!talking);
        Keyboard keys = Keyboard.current;
        if (keys == null) return;
        if (keys.escapeKey.wasPressedThisFrame)
        {
            Close();
            return;
        }
        if (!keys.eKey.wasPressedThisFrame) return;
        if (!talking)
        {
            if (lines == null || lines.Length == 0) return;
            talking = true;
            lineIndex = 0;
        }
        else if (++lineIndex >= lines.Length)
        {
            Close();
            return;
        }
        prompt.SetActive(false);
        dialogue.SetActive(true);
        message.text = lines[lineIndex];
        nextLabel.text = lineIndex == lines.Length - 1 ? "E  Close" : "E  Next    |    Esc  Close";
    }

    private void Close()
    {
        talking = false;
        if (prompt != null) prompt.SetActive(false);
        if (dialogue != null) dialogue.SetActive(false);
    }

    private void BuildUI()
    {
        canvasObject = new GameObject("Dragon Conversation UI", typeof(RectTransform),
            typeof(Canvas), typeof(CanvasScaler));
        canvasObject.transform.SetParent(transform, false);
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 90;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform hint = Box("Talk Prompt", canvasObject.transform,
            new Vector2(0f, 98f), new Vector2(380f, 44f));
        hint.gameObject.AddComponent<Image>().color = new Color32(28, 33, 28, 235);
        Text(hint, "E  Talk to " + speakerName, Vector2.zero, new Vector2(360f, 40f),
            22f, Color.white, TextAlignmentOptions.Center);
        prompt = hint.gameObject;

        RectTransform card = Box("Dialogue", canvasObject.transform,
            new Vector2(0f, 185f), new Vector2(780f, 210f));
        card.gameObject.AddComponent<Image>().color = new Color32(250, 248, 240, 255);
        Text(card, speakerName, new Vector2(0f, 67f), new Vector2(720f, 32f),
            25f, new Color32(66, 100, 53, 255), TextAlignmentOptions.Left);
        message = Text(card, "", new Vector2(0f, 0f), new Vector2(720f, 94f),
            26f, new Color32(65, 44, 32, 255), TextAlignmentOptions.Left);
        nextLabel = Text(card, "", new Vector2(0f, -76f), new Vector2(720f, 28f),
            19f, new Color32(95, 103, 90, 255), TextAlignmentOptions.Right);
        dialogue = card.gameObject;
        Close();
    }

    private static RectTransform Box(string name, Transform parent, Vector2 position, Vector2 size)
    {
        RectTransform rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    private static TMP_Text Text(RectTransform parent, string value, Vector2 position,
        Vector2 size, float fontSize, Color color, TextAlignmentOptions alignment)
    {
        RectTransform rect = Box("Text", parent, position, size);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        TMP_Text text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.raycastTarget = false;
        return text;
    }

    void OnDisable()
    {
        Close();
        if (canvasObject != null) canvasObject.SetActive(false);
    }

    void OnEnable()
    {
        if (canvasObject != null) canvasObject.SetActive(true);
    }
}