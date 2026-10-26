using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using Photography;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DataRigController : MonoBehaviour
{
    [Header("Data")]
    public DailyNewsFeedDefinition dailyFeed;
    public int startingAnalysisTokens = 3;

    [Header("UI Roots")]
    public GameObject dataRigRoot;
    public NewsFeedController newsFeedController;
    public EvidencePanelController evidencePanelController;
    public FactAnalyserController factAnalyserController;

    [Header("Input")]
    public Key openKey = Key.Tab;
    public bool startHidden = true;

    [Header("Tutorial Overlay")]
    public bool showTutorialOnFirstOpen = true;
    public Sprite tutorialOverlaySprite;
    public string tutorialOverlaySpritePath = "Assets/Pictures/dataRigTutorialV3.png";
    [TextArea(1, 2)]
    public string tutorialCloseButtonText = "Close";
    public Vector2 tutorialOverlayPosition = new Vector2(35f, 0f);
    public Vector2 tutorialOverlaySize = new Vector2(1300f, 980f);
    [Range(0f, 1f)]
    public float tutorialBackgroundAlpha = 150f / 255f;
    public Vector2 tutorialCloseButtonPosition = new Vector2(-60f, -30f);
    public Sprite tutorialCloseButtonSprite;
    public string tutorialCloseButtonSpritePath = "Assets/UI/Button.png";
    public string tutorialInfoButtonName = "InfoButton";

    private DataRigSession session;
    private bool isOpen;
    private bool uiInitialized;
    private bool tutorialShownThisRun;
    private GameObject tutorialOverlayRoot;
    private Button tutorialInfoButton;
    private DataRigSession questTrackerSubscribedSession;
    private bool hudQuestTrackerReady;

    public DataRigSession Session
    {
        get
        {
            EnsureSession();
            return session;
        }
    }

    private void Start()
    {
        if (dataRigRoot != null && startHidden)
        {
            dataRigRoot.SetActive(false);
        }

        EnsureSession();
        UpdateHudQuestTracker();
    }

    private void Update()
    {
        if (!hudQuestTrackerReady)
        {
            UpdateHudQuestTracker();
        }

        if (Keyboard.current != null && Keyboard.current[openKey].wasPressedThisFrame)
        {
            Toggle();
        }
    }

    public void Toggle()
    {
        if (isOpen)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    public void Open()
    {
        EnsureSession();
        UpdateHudQuestTracker();

        if (dataRigRoot != null)
        {
            dataRigRoot.SetActive(true);
        }

        newsFeedController?.Populate(dailyFeed, session, this);

        if (!uiInitialized)
        {
            evidencePanelController?.Initialize(session, this);
            factAnalyserController?.Initialize(session, this);
            uiInitialized = true;
        }
        else
        {
            evidencePanelController?.RefreshActivePanel();
        }

        WireTutorialInfoButton();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SetCameraSuppressed(true);

        isOpen = true;
        TryShowTutorialOverlay();
    }

    public void Close()
    {
        if (dataRigRoot != null)
        {
            dataRigRoot.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SetCameraSuppressed(false);

        isOpen = false;
    }

    private void SetCameraSuppressed(bool suppressed)
    {
        var photo = FindObjectOfType<Photography.PhotoCaptureController>();
        if (photo != null) photo.SuppressEquip = suppressed;
    }

    public bool TryCollectEvidenceById(int storyIndex, string markerId, EvidenceSource source = EvidenceSource.DataRig)
    {
        EnsureSession();

        NewsStorySession storySession = session.GetStorySession(storyIndex);
        if (storySession?.Definition == null || string.IsNullOrEmpty(markerId))
        {
            return false;
        }

        EvidenceMarkerDefinition marker = storySession.Definition.GetMarkerById(markerId);
        if (marker != null && session.TryCollectMarker(storyIndex, marker, source))
        {
            RefreshAfterEvidenceCollected();
            return true;
        }

        return false;
    }

    public bool TryCollectEvidenceByStoryId(string storyId, string markerId, EvidenceSource source = EvidenceSource.Dialogue)
    {
        EnsureSession();

        if (string.IsNullOrEmpty(storyId) || string.IsNullOrEmpty(markerId))
        {
            return false;
        }

        for (int i = 0; i < session.StorySessions.Count; i++)
        {
            NewsStoryDefinition definition = session.StorySessions[i].Definition;
            if (definition == null || definition.storyId != storyId)
            {
                continue;
            }

            return TryCollectEvidenceById(i, markerId, source);
        }

        return false;
    }

    public int FindStoryIndexById(string storyId)
    {
        EnsureSession();

        if (string.IsNullOrEmpty(storyId))
        {
            return -1;
        }

        for (int i = 0; i < session.StorySessions.Count; i++)
        {
            NewsStoryDefinition definition = session.StorySessions[i].Definition;
            if (definition != null && definition.storyId == storyId)
            {
                return i;
            }
        }

        return -1;
    }

    public bool TryCollectEvidenceByMarkerId(string markerId, EvidenceSource source = EvidenceSource.Dialogue)
    {
        EnsureSession();
        if (string.IsNullOrEmpty(markerId))
        {
            return false;
        }

        for (int i = 0; i < session.StorySessions.Count; i++)
        {
            NewsStoryDefinition definition = session.StorySessions[i].Definition;
            if (definition == null)
            {
                continue;
            }

            EvidenceMarkerDefinition marker = definition.GetMarkerById(markerId);
            if (marker == null)
            {
                continue;
            }

            return TryCollectEvidenceById(i, markerId, source);
        }

        return false;
    }

    public void SetVerdict(int storyIndex, PlayerVerdict verdict)
    {
        EnsureSession();
        session.SetVerdict(storyIndex, verdict);
    }

    public DayScoreResult SubmitDay()
    {
        EnsureSession();
        DayScoreResult result = session.SubmitDay();
        evidencePanelController?.RefreshActivePanel();
        return result;
    }

    public void AddPhotographEvidence(int storyIndex, EvidenceMarkerDefinition marker, Sprite photograph)
    {
        EnsureSession();
        if (session.TryCollectMarker(storyIndex, marker, EvidenceSource.Photograph, photograph))
        {
            RefreshAfterEvidenceCollected();
        }
    }

    private void RefreshAfterEvidenceCollected()
    {
        newsFeedController?.RefreshObjectives();
        evidencePanelController?.RefreshActivePanel();
        UpdateHudQuestTracker();
    }

    private void EnsureSession()
    {
        if (session != null)
        {
            return;
        }

        session = new DataRigSession(dailyFeed, startingAnalysisTokens);
        SubscribeQuestTracker(session);
    }

    private void SubscribeQuestTracker(DataRigSession targetSession)
    {
        if (questTrackerSubscribedSession == targetSession)
        {
            return;
        }

        if (questTrackerSubscribedSession != null)
        {
            questTrackerSubscribedSession.SessionChanged -= UpdateHudQuestTracker;
        }

        questTrackerSubscribedSession = targetSession;
        if (questTrackerSubscribedSession != null)
        {
            questTrackerSubscribedSession.SessionChanged += UpdateHudQuestTracker;
        }
    }

    private void OnDestroy()
    {
        if (questTrackerSubscribedSession != null)
        {
            questTrackerSubscribedSession.SessionChanged -= UpdateHudQuestTracker;
        }
    }

    private void UpdateHudQuestTracker()
    {
        hudQuestTrackerReady = TryUpdateHudQuestTracker() || hudQuestTrackerReady;
    }

    private bool TryUpdateHudQuestTracker()
    {
        if (session == null)
        {
            return false;
        }

        PhotoHud hud = FindObjectOfType<PhotoHud>();
        if (hud == null)
        {
            return false;
        }

        var quests = new System.Collections.Generic.List<(string title, string objective)>();
        for (int i = 0; i < session.StorySessions.Count; i++)
        {
            NewsStorySession storySession = session.StorySessions[i];
            NewsStoryDefinition definition = storySession?.Definition;
            if (definition == null)
            {
                continue;
            }

            string title = !string.IsNullOrWhiteSpace(definition.shortTitle)
                ? definition.shortTitle
                : (!string.IsNullOrWhiteSpace(definition.title) ? definition.title : definition.storyId);

            quests.Add((title, storySession.GetCurrentObjective()));
        }

        hud.SetQuestTracker(quests);
        return true;
    }

    private void TryShowTutorialOverlay()
    {
        if (!showTutorialOnFirstOpen || tutorialShownThisRun || dataRigRoot == null)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();
        ShowTutorialOverlay();
        tutorialShownThisRun = true;
    }

    public void ShowTutorialOverlay()
    {
        if (dataRigRoot == null)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();
        EnsureTutorialOverlayCreated();

        if (tutorialOverlayRoot == null)
        {
            return;
        }

        tutorialOverlayRoot.transform.SetAsLastSibling();
        tutorialOverlayRoot.SetActive(true);
    }

    private void EnsureTutorialOverlayCreated()
    {
        if (tutorialOverlayRoot != null)
        {
            MatchTutorialOverlayRect(tutorialOverlayRoot.GetComponent<RectTransform>());
            return;
        }

        if (!TryGetDataRigMenuRect(out RectTransform overlayParent))
        {
            Debug.LogWarning("DataRigController: Could not find a valid Data-Rig menu RectTransform for tutorial overlay.");
            return;
        }

        var overlayRoot = new GameObject("DataRigTutorialOverlay", typeof(RectTransform), typeof(Image));
        overlayRoot.transform.SetParent(overlayParent, false);
        tutorialOverlayRoot = overlayRoot;

        var overlayRect = overlayRoot.GetComponent<RectTransform>();
        MatchTutorialOverlayRect(overlayRect);

        var overlayImage = overlayRoot.GetComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, tutorialBackgroundAlpha);
        overlayImage.raycastTarget = true;

        var tutorialImageGo = new GameObject("TutorialImage", typeof(RectTransform), typeof(Image));
        tutorialImageGo.transform.SetParent(overlayRoot.transform, false);
        var tutorialImageRect = tutorialImageGo.GetComponent<RectTransform>();
        StretchToParent(tutorialImageRect);

        var tutorialImage = tutorialImageGo.GetComponent<Image>();
        tutorialImage.preserveAspect = false;
        tutorialImage.raycastTarget = true;

        Sprite sprite = ResolveTutorialSprite();
        if (sprite != null)
        {
            tutorialImage.sprite = sprite;
            tutorialImage.color = Color.white;
        }
        else
        {
            tutorialImage.color = new Color(0f, 0f, 0f, 0f);
            Debug.LogWarning($"DataRigController: Tutorial overlay sprite is not assigned and could not be loaded from '{tutorialOverlaySpritePath}'.");
        }

        var closeButtonGo = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
        closeButtonGo.transform.SetParent(overlayRoot.transform, false);
        var closeButtonRect = closeButtonGo.GetComponent<RectTransform>();
        closeButtonRect.anchorMin = new Vector2(1f, 1f);
        closeButtonRect.anchorMax = new Vector2(1f, 1f);
        closeButtonRect.pivot = new Vector2(1f, 1f);
        closeButtonRect.anchoredPosition = tutorialCloseButtonPosition;
        closeButtonRect.sizeDelta = new Vector2(140f, 48f);

        var closeButtonImage = closeButtonGo.GetComponent<Image>();
        Sprite closeButtonSprite = ResolveCloseButtonSprite();
        if (closeButtonSprite != null)
        {
            closeButtonImage.sprite = closeButtonSprite;
            closeButtonImage.type = Image.Type.Sliced;
            closeButtonImage.color = Color.white;
        }
        else
        {
            closeButtonImage.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);
        }
        var closeButton = closeButtonGo.GetComponent<Button>();
        closeButton.onClick.AddListener(HideTutorialOverlay);

        var closeTextGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        closeTextGo.transform.SetParent(closeButtonGo.transform, false);
        var closeTextRect = closeTextGo.GetComponent<RectTransform>();
        StretchToParent(closeTextRect);

        var closeText = closeTextGo.GetComponent<TextMeshProUGUI>();
        closeText.text = string.IsNullOrWhiteSpace(tutorialCloseButtonText) ? "Close" : tutorialCloseButtonText;
        closeText.alignment = TextAlignmentOptions.Center;
        closeText.fontSize = 24f;
        closeText.color = Color.white;
        closeText.raycastTarget = false;

        tutorialOverlayRoot.SetActive(false);
    }

    private void HideTutorialOverlay()
    {
        if (tutorialOverlayRoot != null)
        {
            tutorialOverlayRoot.SetActive(false);
        }
    }

    private void WireTutorialInfoButton()
    {
        if (tutorialInfoButton == null)
        {
            tutorialInfoButton = FindButtonByName(tutorialInfoButtonName);
        }

        if (tutorialInfoButton == null)
        {
            return;
        }

        tutorialInfoButton.onClick.RemoveListener(ShowTutorialOverlay);
        tutorialInfoButton.onClick.AddListener(ShowTutorialOverlay);
    }

    private Button FindButtonByName(string buttonName)
    {
        if (string.IsNullOrWhiteSpace(buttonName) || dataRigRoot == null)
        {
            return null;
        }

        Transform searchRoot = dataRigRoot.transform;
        Canvas canvas = dataRigRoot.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            searchRoot = canvas.transform;
        }

        foreach (Button button in searchRoot.GetComponentsInChildren<Button>(true))
        {
            if (button != null && button.gameObject.name == buttonName)
            {
                return button;
            }
        }

        return null;
    }

    private static void StretchToParent(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
    }

    private bool TryGetDataRigMenuRect(out RectTransform overlayParent)
    {
        overlayParent = null;

        if (dataRigRoot == null)
        {
            return false;
        }

        Canvas.ForceUpdateCanvases();

        var menuRect = dataRigRoot.GetComponent<RectTransform>();
        if (menuRect == null || menuRect.parent is not RectTransform parentRect)
        {
            return false;
        }

        overlayParent = parentRect;
        return true;
    }

    private void MatchTutorialOverlayRect(RectTransform rect)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = tutorialOverlayPosition;
        rect.sizeDelta = tutorialOverlaySize;
        rect.localScale = Vector3.one;
    }

    private Sprite ResolveTutorialSprite()
    {
        if (tutorialOverlaySprite != null)
        {
            return tutorialOverlaySprite;
        }

#if UNITY_EDITOR
        if (!string.IsNullOrWhiteSpace(tutorialOverlaySpritePath))
        {
            tutorialOverlaySprite = AssetDatabase.LoadAssetAtPath<Sprite>(tutorialOverlaySpritePath);
            return tutorialOverlaySprite;
        }
#endif

        return null;
    }

    private Sprite ResolveCloseButtonSprite()
    {
        if (tutorialCloseButtonSprite != null)
        {
            return tutorialCloseButtonSprite;
        }

#if UNITY_EDITOR
        if (!string.IsNullOrWhiteSpace(tutorialCloseButtonSpritePath))
        {
            tutorialCloseButtonSprite = AssetDatabase.LoadAssetAtPath<Sprite>(tutorialCloseButtonSpritePath);
            return tutorialCloseButtonSprite;
        }
#endif

        return null;
    }
}
