using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EvidencePanelController : MonoBehaviour
{
    [Header("Tabs")]
    public Button[] postTabButtons;

    [Header("Evidence List")]
    public Transform evidenceListRoot;
    public EvidenceListItemView evidenceItemPrefab;
    public TMP_Text evidenceTitleText;

    [Header("Verdict")]
    public Button realButton;
    public Button fakeButton;
    public TMP_Text verdictStatusText;

    private DataRigSession session;
    private DataRigController controller;
    private int activeStoryIndex;
    private bool isInitialized;
    private readonly List<EvidenceListItemView> activeItems = new List<EvidenceListItemView>();

    public void Initialize(DataRigSession dataRigSession, DataRigController dataRigController)
    {
        if (isInitialized)
        {
            return;
        }

        session = dataRigSession;
        controller = dataRigController;
        session.SessionChanged += Refresh;
        isInitialized = true;
        TryAutoBindEvidenceTitleText();
        TryAutoBindEvidenceListRoot();
        EnsureScrollableEvidenceLayout();

        for (int i = 0; i < postTabButtons.Length; i++)
        {
            int capturedIndex = i;
            postTabButtons[i].onClick.AddListener(() => SelectStory(capturedIndex));
        }

        UpdateTabButtonLabels();

        if (realButton != null)
        {
            realButton.onClick.AddListener(() => SetVerdict(PlayerVerdict.Real));
        }

        if (fakeButton != null)
        {
            fakeButton.onClick.AddListener(() => SetVerdict(PlayerVerdict.Fake));
        }

        SelectStory(0);
    }

    public void RefreshActivePanel()
    {
        Refresh();
    }

    private void OnDestroy()
    {
        if (session != null)
        {
            session.SessionChanged -= Refresh;
        }
    }

    public void SelectStory(int storyIndex)
    {
        activeStoryIndex = storyIndex;
        Refresh();
    }

    private void Refresh()
    {
        ClearItems();

        NewsStorySession storySession = session?.GetStorySession(activeStoryIndex);
        if (storySession == null || evidenceListRoot == null || evidenceItemPrefab == null)
        {
            UpdateEvidenceTitle(null);
            UpdateVerdictLabel(null);
            return;
        }

        foreach (CollectedEvidence evidence in storySession.CollectedEvidence)
        {
            EvidenceListItemView item = Instantiate(evidenceItemPrefab, evidenceListRoot);
            item.gameObject.SetActive(true);
            item.Bind(evidence);
            activeItems.Add(item);
        }

        UpdateEvidenceTitle(storySession);
        UpdateVerdictLabel(storySession);
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(evidenceListRoot as RectTransform);
        if (evidenceListRoot.parent is RectTransform parentRect)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
        }
    }

    private void UpdateVerdictLabel(NewsStorySession storySession)
    {
        if (verdictStatusText == null)
        {
            return;
        }

        if (storySession == null)
        {
            verdictStatusText.text = string.Empty;
            return;
        }

        if (session.IsDaySubmitted && session.LastDayScoreResult != null
            && activeStoryIndex < session.LastDayScoreResult.Stories.Count)
        {
            StoryScoreResult score = session.LastDayScoreResult.Stories[activeStoryIndex];
            string playerLabel = storySession.Verdict switch
            {
                PlayerVerdict.Real => "REAL",
                PlayerVerdict.Fake => "FAKE",
                _ => "None (failed)"
            };
            string truthLabel = score.ActuallyFake ? "FAKE" : "REAL";
            string verdictOutcome = storySession.Verdict == PlayerVerdict.Undecided
                ? "No verdict"
                : score.VerdictCorrect ? "Correct" : "Wrong";

            verdictStatusText.text =
                $"You: {playerLabel} | Truth: {truthLabel} ({verdictOutcome})\n" +
                $"Key evidence: {score.RelevantEvidenceCollected}/{score.RelevantEvidenceTotal}";
            return;
        }

        verdictStatusText.text = storySession.Verdict switch
        {
            PlayerVerdict.Real => "Marked REAL",
            PlayerVerdict.Fake => "Marked FAKE",
            _ => "No verdict yet"
        };
    }

    private void SetVerdict(PlayerVerdict verdict)
    {
        controller?.SetVerdict(activeStoryIndex, verdict);
    }

    private void ClearItems()
    {
        foreach (EvidenceListItemView item in activeItems)
        {
            if (item != null)
            {
                Destroy(item.gameObject);
            }
        }

        activeItems.Clear();
    }

    private void UpdateTabButtonLabels()
    {
        if (session == null || postTabButtons == null)
        {
            return;
        }

        for (int i = 0; i < postTabButtons.Length; i++)
        {
            TMP_Text label = postTabButtons[i] != null
                ? postTabButtons[i].GetComponentInChildren<TMP_Text>(true)
                : null;

            if (label == null)
            {
                continue;
            }

            NewsStorySession storySession = session.GetStorySession(i);
            string shortTitle = storySession?.Definition != null
                ? storySession.Definition.shortTitle
                : null;

            label.text = string.IsNullOrWhiteSpace(shortTitle)
                ? $"Post {i + 1}"
                : shortTitle.Trim();
        }
    }

    private void UpdateEvidenceTitle(NewsStorySession storySession)
    {
        if (evidenceTitleText == null)
        {
            return;
        }

        if (storySession?.Definition == null)
        {
            evidenceTitleText.text = "Evidence";
            return;
        }

        string storyName = !string.IsNullOrWhiteSpace(storySession.Definition.shortTitle)
            ? storySession.Definition.shortTitle.Trim()
            : !string.IsNullOrWhiteSpace(storySession.Definition.title)
                ? storySession.Definition.title.Trim()
                : $"Story {activeStoryIndex + 1}";

        evidenceTitleText.text = $"{storyName} Evidence";
    }

    private void TryAutoBindEvidenceTitleText()
    {
        if (evidenceTitleText != null)
        {
            return;
        }

        TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i].name == "EvidenceTitleText" || texts[i].name == "EvidenceTitleLabel")
            {
                evidenceTitleText = texts[i];
                return;
            }
        }
    }

    private void TryAutoBindEvidenceListRoot()
    {
        if (evidenceListRoot != null)
        {
            return;
        }

        Transform[] children = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].name == "EvidenceList")
            {
                evidenceListRoot = children[i];
                return;
            }
        }
    }

    private void EnsureScrollableEvidenceLayout()
    {
        RectTransform panelRect = transform as RectTransform;
        if (panelRect != null)
        {
            // If this panel was left at zero size in the scene, force it to fill its parent
            // so the nested ScrollRect has a valid viewport size.
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = Vector2.zero;
        }

        RectTransform listRect = evidenceListRoot as RectTransform;
        if (listRect == null)
        {
            return;
        }

        ScrollRect scrollRect = GetComponentInChildren<ScrollRect>(true);
        if (scrollRect != null)
        {
            RectTransform scrollRectTransform = scrollRect.transform as RectTransform;
            if (scrollRectTransform != null)
            {
                scrollRectTransform.anchorMin = Vector2.zero;
                scrollRectTransform.anchorMax = Vector2.one;
                scrollRectTransform.anchoredPosition = Vector2.zero;
                scrollRectTransform.sizeDelta = Vector2.zero;
                scrollRectTransform.pivot = new Vector2(0.5f, 0.5f);
            }

            RectTransform viewportRect = scrollRect.viewport;
            if (viewportRect != null)
            {
                viewportRect.anchorMin = Vector2.zero;
                viewportRect.anchorMax = Vector2.one;
                viewportRect.anchoredPosition = Vector2.zero;
                viewportRect.sizeDelta = Vector2.zero;
                viewportRect.pivot = new Vector2(0.5f, 0.5f);
            }

            RectTransform contentRect = scrollRect.content;
            if (contentRect != null)
            {
                contentRect.anchorMin = new Vector2(0f, 1f);
                contentRect.anchorMax = new Vector2(1f, 1f);
                contentRect.pivot = new Vector2(0.5f, 1f);
                contentRect.sizeDelta = new Vector2(0f, contentRect.sizeDelta.y);
            }
        }

        // Keep the list stretched to the content width while allowing its height
        // to grow based on the number of evidence items.
        listRect.anchorMin = new Vector2(0f, 1f);
        listRect.anchorMax = new Vector2(1f, 1f);
        listRect.pivot = new Vector2(0.5f, 1f);
        listRect.sizeDelta = new Vector2(0f, listRect.sizeDelta.y);

        VerticalLayoutGroup verticalLayout = listRect.GetComponent<VerticalLayoutGroup>();
        if (verticalLayout == null)
        {
            verticalLayout = listRect.gameObject.AddComponent<VerticalLayoutGroup>();
        }

        verticalLayout.childControlWidth = true;
        verticalLayout.childControlHeight = true;
        verticalLayout.childForceExpandWidth = true;
        verticalLayout.childForceExpandHeight = false;

        ContentSizeFitter fitter = listRect.GetComponent<ContentSizeFitter>();
        if (fitter == null)
        {
            fitter = listRect.gameObject.AddComponent<ContentSizeFitter>();
        }

        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }
}
