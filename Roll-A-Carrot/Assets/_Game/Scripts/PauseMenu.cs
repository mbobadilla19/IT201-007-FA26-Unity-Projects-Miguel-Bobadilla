using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public TMP_Text buttonLabel;
    [Header("On-screen Menu")]
    public bool createOnScreenMenu = true;
    public string controlsText = "Move: WASD / Arrow keys\nJump: Space    |    Dash: Left Shift\nPause / Resume: P";

    private bool isPaused;
    private float previousTimeScale = 1f;
    private bool previousAudioPause;
    private GameObject menuOverlay;
    private GameObject gameOverOverlay;
    private TMP_Text gameOverCoins;
    private TMP_Text resultHeading;
    private TMP_Text resultMessage;
    private Image resultAccent;
    private TMP_Text hudSeeds;
    private TMP_Text hudControls;
    private TMP_Text hudNotice;
    private SeedWallet seedWallet;
    private CanvasGroup legacyNoticeGroup;
    private float legacyNoticeAlpha;
    private GameObject generatedCanvas;
    private static PauseMenu activeMenu;

    void Awake()
    {
        // Only one pause menu may control the scene at a time.
        if (activeMenu != null && activeMenu != this)
        {
            enabled = false;
            return;
        }
        activeMenu = this;
    }

    void Start()
    {
        if (createOnScreenMenu)
            BuildMenu();
        UpdateLabel();
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.pKey.wasPressedThisFrame)
            TogglePause();
#if UNITY_EDITOR
        if (keyboard != null && keyboard.mKey.wasPressedThisFrame)
            NextSceneForTesting();
#endif
    }

    public void TogglePause()
    {
        if (hudPlayer != null && (hudPlayer.LivesRemaining <= 0 || hudPlayer.HasWon))
            return;

        if (isPaused)
        {
            Resume();
            return;
        }

        previousTimeScale = Time.timeScale;
        previousAudioPause = AudioListener.pause;
        isPaused = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        UpdateLabel();
    }

    public void Resume()
    {
        if (!isPaused)
            return;

        Time.timeScale = previousTimeScale;
        AudioListener.pause = previousAudioPause;
        isPaused = false;
        UpdateLabel();
    }

    public void RestartLevel()
    {
        Resume();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

#if UNITY_EDITOR
    public void NextSceneForTesting()
    {
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        if (SceneManager.GetActiveScene().buildIndex < 0 ||
            next >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log("No next scene in the Build Profiles scene list.");
            if (menuMessage != null)
                menuMessage.text = "You're already in the last scene.";
            return;
        }

        Resume();
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(next);
    }
#endif

    void UpdateLabel()
    {
        if (buttonLabel != null)
            buttonLabel.text = "Pause (P)";
        if (gameplayHud != null)
            gameplayHud.SetActive(!isPaused && (hudPlayer == null ||
                (hudPlayer.LivesRemaining > 0 && !hudPlayer.HasWon)));
        if (menuOverlay != null)
            menuOverlay.SetActive(isPaused);
    }

    private static readonly Color Cream = new Color32(250, 248, 240, 255);
    private static readonly Color Ink = new Color32(65, 44, 32, 255);
    private static readonly Color Leaf = new Color32(66, 100, 53, 255);
    private static readonly Color Carrot = new Color32(227, 116, 49, 255);

    private GameObject gameplayHud;
    private PlayerController hudPlayer;
    private TMP_Text hudCoins;
    private TMP_Text hudLives;
    private TMP_Text menuMessage;
    private bool oldCoinsActive;
    private bool oldLivesActive;

    private void BuildMenu()
    {
        generatedCanvas = new GameObject("Pause Menu UI", typeof(RectTransform),
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        generatedCanvas.transform.SetParent(transform, false);
        Canvas canvas = generatedCanvas.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = generatedCanvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform hud = MakeRect("Gameplay HUD", generatedCanvas.transform,
            Vector2.zero, Vector2.zero, Vector2.zero);
        Stretch(hud);
        gameplayHud = hud.gameObject;
        hudPlayer = FindFirstObjectByType<PlayerController>();
        if (hudPlayer != null)
        {
            hudLives = MakeCounter(hud, "Lives", 110f, 160f);
            hudCoins = MakeCounter(hud, "Coins", 307f, 210f);
            seedWallet = hudPlayer.GetComponent<SeedWallet>();
            if (seedWallet != null)
                hudSeeds = MakeCounter(hud, "Gems", 525f, 200f);
            if (hudPlayer.winTextObject != null)
            {
                legacyNoticeGroup = hudPlayer.winTextObject.GetComponent<CanvasGroup>();
                if (legacyNoticeGroup == null)
                    legacyNoticeGroup = hudPlayer.winTextObject.AddComponent<CanvasGroup>();
                legacyNoticeAlpha = legacyNoticeGroup.alpha;
                legacyNoticeGroup.alpha = 0f;
            }
            if (hudPlayer.countText != null)
            {
                oldCoinsActive = hudPlayer.countText.gameObject.activeSelf;
                hudPlayer.countText.gameObject.SetActive(false);
            }
            if (hudPlayer.livesText != null)
            {
                oldLivesActive = hudPlayer.livesText.gameObject.activeSelf;
                hudPlayer.livesText.gameObject.SetActive(false);
            }
        }

        Button pauseButton = MakeButton("Pause Button", hud,
            Vector2.one, new Vector2(-100f, -52f), "Pause (P)");
        pauseButton.GetComponent<RectTransform>().sizeDelta = new Vector2(140f, 48f);
        pauseButton.GetComponentInChildren<TMP_Text>().fontSize = 22f;
        pauseButton.onClick.AddListener(TogglePause);
        buttonLabel = pauseButton.GetComponentInChildren<TMP_Text>();

        RectTransform footer = MakeRect("Controls Strip", hud, new Vector2(0.5f, 0f),
            new Vector2(0f, 38f), new Vector2(1220f, 44f));
        Panel(footer, new Color32(28, 33, 28, 220));
        footer.GetComponent<Image>().raycastTarget = false;
        hudControls = MakeText("Controls", footer,
            "WASD / Arrows  Move     Space  Jump     Shift  Dash     P  Pause", 20f);

        RectTransform notice = MakeRect("Progress Notice", hud, new Vector2(0.5f, 0f),
            new Vector2(0f, 98f), new Vector2(620f, 44f));
        Panel(notice, new Color32(28, 33, 28, 230));
        notice.GetComponent<Image>().raycastTarget = false;
        hudNotice = MakeText("Notice", notice, "", 22f);
        notice.gameObject.SetActive(false);

        RectTransform overlay = MakeRect("Pause Overlay", generatedCanvas.transform,
            Vector2.zero, Vector2.zero, Vector2.zero);
        Stretch(overlay);
        menuOverlay = overlay.gameObject;
        Panel(overlay, new Color32(20, 26, 22, 190));
        float menuHeight = 470f;
#if UNITY_EDITOR
        menuHeight = 550f;
#endif
        RectTransform card = MakeRect("Pause Card", overlay, new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(520f, menuHeight));
        Panel(card, Cream);
        Accent(card, Carrot, 516f);
        Label(card, "Brand", "Roll-A-Carrot", new Vector2(0f, -42f),
            new Vector2(440f, 32f), 20f, Leaf);
        Label(card, "Heading", "Paused", new Vector2(0f, -101f),
            new Vector2(440f, 60f), 40f, Ink, FontStyles.Bold);
        MenuAction(card, "Resume", "Resume", -185f, Resume, true);
        MenuAction(card, "Restart", "Restart level", -257f, RestartLevel);
#if UNITY_EDITOR
        LevelTestTeleport tester = hudPlayer != null ? hudPlayer.GetComponent<LevelTestTeleport>() : null;
        if (tester != null && tester.enabled)
        {
            MenuAction(card, "Next Area", "Next area", -329f, () =>
            {
                tester.NextArea();
                if (tester.LastTeleportSucceeded)
                    Resume();
                else if (menuMessage != null)
                    menuMessage.text = tester.Status;
            });
        }
        MenuAction(card, "Next Scene", "Next scene (M)", -401f, NextSceneForTesting);
#endif
        float noticeY = -393f;
        float shortcutY = -437f;
#if UNITY_EDITOR
        noticeY = -473f;
        shortcutY = -517f;
#endif
        menuMessage = Label(card, "Notice", "Restart resets coins and lives in this scene.",
            new Vector2(0f, noticeY), new Vector2(460f, 34f), 18f, Ink);
        Label(card, "Shortcut", "Press P to resume", new Vector2(0f, shortcutY),
            new Vector2(440f, 28f), 18f, Leaf);
        menuOverlay.SetActive(false);
        BuildGameOverCard();
    }

    private static void Accent(RectTransform card, Color color, float width)
    {
        RectTransform stripe = MakeRect("Accent", card, new Vector2(0.5f, 1f),
            new Vector2(0f, -2f), new Vector2(width, 4f));
        Panel(stripe, color);
    }

    private void BuildGameOverCard()
    {
        Color red = new Color32(183, 43, 49, 255);
        RectTransform overlay = MakeRect("Game Over Overlay", generatedCanvas.transform,
            Vector2.zero, Vector2.zero, Vector2.zero);
        Stretch(overlay);
        gameOverOverlay = overlay.gameObject;
        Panel(overlay, new Color32(20, 26, 22, 200));
        RectTransform card = MakeRect("Game Over Card", overlay, new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(520f, 360f));
        Panel(card, Cream);
        Accent(card, red, 516f);
        resultAccent = card.Find("Accent").GetComponent<Image>();
        resultHeading = Label(card, "Heading", "Game Over", new Vector2(0f, -70f),
            new Vector2(460f, 70f), 48f, red, FontStyles.Bold);
        resultMessage = Label(card, "Message", "Out of lives. Give it another go.", new Vector2(0f, -133f),
            new Vector2(460f, 38f), 22f, Ink);
        gameOverCoins = Label(card, "Coins Collected", "", new Vector2(0f, -182f),
            new Vector2(460f, 34f), 22f, Leaf);
        MenuAction(card, "Try Again", "Try again", -260f, RestartLevel, true);
        Label(card, "Restart Note", "Restart this scene", new Vector2(0f, -323f),
            new Vector2(460f, 28f), 18f, Ink);
        gameOverOverlay.SetActive(false);
    }

    private TMP_Text MakeCounter(RectTransform parent, string caption, float x, float width)
    {
        RectTransform badge = MakeRect(caption + " Badge", parent, new Vector2(0f, 1f),
            new Vector2(x, -52f), new Vector2(width, 48f));
        Panel(badge, new Color32(28, 33, 28, 225));
        badge.GetComponent<Image>().raycastTarget = false;
        TMP_Text value = MakeText("Value", badge, "", 23f);
        value.color = Cream;
        return value;
    }

    private static void MenuAction(RectTransform parent, string name, string caption,
        float y, UnityEngine.Events.UnityAction action, bool primary = false)
    {
        Button button = MakeButton(name, parent, new Vector2(0.5f, 1f), new Vector2(0f, y), caption);
        button.GetComponent<RectTransform>().sizeDelta = new Vector2(440f, 56f);
        if (!primary)
        {
            button.GetComponent<Image>().color = new Color32(233, 234, 225, 255);
            button.GetComponentInChildren<TMP_Text>().color = Ink;
        }
        button.onClick.AddListener(action);
    }

    private void LateUpdate()
    {
        if (hudPlayer == null) return;
        bool won = hudPlayer.HasWon;
        bool ended = hudPlayer.LivesRemaining <= 0 || won;
        if (gameOverOverlay != null)
        {
            gameOverOverlay.SetActive(ended);
            if (ended)
            {
                gameplayHud.SetActive(false);
                menuOverlay.SetActive(false);
                resultHeading.text = won ? "You Win!" : "Game Over";
                resultHeading.color = won ? Leaf : new Color32(183, 43, 49, 255);
                resultAccent.color = resultHeading.color;
                resultMessage.text = won ? "You made it through CarrotValley." : "Out of lives. Give it another go.";
                if (gameOverCoins != null && hudPlayer.countText != null)
                    gameOverCoins.text = hudPlayer.countText.text;
            }
        }
        if (hudSeeds != null && seedWallet != null)
            hudSeeds.text = "Gems  " + seedWallet.Seeds;
        if (hudControls != null)
        {
            hudControls.text = "WASD / Arrows  Move     Space  Jump     Shift  Dash     E  Talk     P  Pause";
            if (seedWallet != null)
                hudControls.text += seedWallet.CanBuyLife ? "     B  +1 life (3 gems)" : "     B  Buy life (3 gems)";
        }
        if (hudNotice != null)
        {
            bool show = !ended && hudPlayer.winTextObject != null && hudPlayer.winTextObject.activeInHierarchy;
            TMP_Text source = show ? hudPlayer.winTextObject.GetComponent<TMP_Text>() : null;
            hudNotice.transform.parent.gameObject.SetActive(source != null && !string.IsNullOrWhiteSpace(source.text));
            if (source != null) hudNotice.text = source.text;
        }
        if (hudLives != null) hudLives.text = "Lives  " + hudPlayer.LivesRemaining;
        if (hudCoins != null && hudPlayer.countText != null)
            hudCoins.text = hudPlayer.countText.text.Replace("Coins:", "Coins ").Trim();
    }

    private static void Panel(RectTransform rect, Color color, Color? border = null)
    {
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        if (border.HasValue)
        {
            Outline outline = rect.gameObject.AddComponent<Outline>();
            outline.effectColor = border.Value;
            outline.effectDistance = new Vector2(2f, -2f);
        }
    }

    private static TMP_Text Label(RectTransform parent, string objectName, string message,
        Vector2 position, Vector2 size, float fontSize, Color color, FontStyles style = FontStyles.Normal)
    {
        RectTransform rect = MakeRect(objectName, parent, new Vector2(0.5f, 1f), position, size);
        TMP_Text text = MakeText("Text", rect, message, fontSize);
        text.color = color;
        text.fontStyle = style;
        return text;
    }

    private static RectTransform MakeRect(string objectName, Transform parent,
        Vector2 anchor, Vector2 position, Vector2 size)
    {
        RectTransform rect = new GameObject(objectName, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static TMP_Text MakeText(string objectName, RectTransform parent,
        string message, float size)
    {
        RectTransform rect = MakeRect(objectName, parent, Vector2.zero, Vector2.zero, Vector2.zero);
        Stretch(rect);
        TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.text = message;
        text.fontSize = size;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }

    private static Button MakeButton(string objectName, Transform parent,
        Vector2 anchor, Vector2 position, string label)
    {
        RectTransform rect = MakeRect(objectName, parent, anchor, position, new Vector2(320f, 72f));
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = Leaf;
        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(1f, 0.85f, 0.6f, 1f);
        colors.pressedColor = new Color(0.7f, 0.8f, 0.9f, 1f);
        button.colors = colors;
        TMP_Text caption = MakeText("Label", rect, label, 24f);
        caption.color = Cream;
        caption.fontStyle = FontStyles.Bold;
        return button;
    }

    void OnDisable()
    {
        Resume();
        if (generatedCanvas != null)
            generatedCanvas.SetActive(false);
        if (hudPlayer != null)
        {
            if (hudPlayer.countText != null) hudPlayer.countText.gameObject.SetActive(oldCoinsActive);
            if (hudPlayer.livesText != null) hudPlayer.livesText.gameObject.SetActive(oldLivesActive);
        }
        if (legacyNoticeGroup != null)
            legacyNoticeGroup.alpha = legacyNoticeAlpha;
        if (activeMenu == this)
            activeMenu = null;
    }

    void OnEnable()
    {
        if (generatedCanvas != null)
            generatedCanvas.SetActive(true);
    }
}