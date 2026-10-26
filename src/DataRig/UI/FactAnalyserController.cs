using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FactAnalyserController : MonoBehaviour
{
    [Header("References")]
    public TMP_Text tokensRemainingText;
    public TMP_Text resultText;
    public TMP_Text slotPreviewText;
    public Image previewImage;
    public Image slotBackground;
    public FactAnalyserDropZone dropZone;

    [Header("Slot Colors")]
    public Color emptySlotColor = new Color(0.325f, 0.811f, 0.75f, 1f);
    public Color filledSlotColor = new Color(0.2f, 0.65f, 0.95f, 1f);

    private DataRigSession session;
    private CollectedEvidence slottedEvidence;
    private bool isInitialized;

    public void Initialize(DataRigSession dataRigSession, DataRigController dataRigController)
    {
        if (isInitialized)
        {
            RefreshTokens();
            return;
        }

        session = dataRigSession;
        session.SessionChanged += RefreshTokens;
        CacheDefaultReferences();
        EnsureSlotPreviewText();
        ClearSlotVisual();
        isInitialized = true;
        RefreshTokens();
    }

    private void OnDestroy()
    {
        if (session != null)
        {
            session.SessionChanged -= RefreshTokens;
        }
    }

    public void HandleDrop(PointerEventData eventData)
    {
        if (eventData?.pointerDrag == null)
        {
            return;
        }

        EvidenceListItemView draggedItem = eventData.pointerDrag.GetComponent<EvidenceListItemView>();
        if (draggedItem != null)
        {
            SlotEvidence(draggedItem.Evidence);
        }
    }

    public void HandleDropFromItem(EvidenceListItemView item)
    {
        if (item != null)
        {
            SlotEvidence(item.Evidence);
        }
    }

    public void AnalyzeSlottedEvidence()
    {
        if (session == null || slottedEvidence == null)
        {
            if (resultText != null)
            {
                resultText.text = "Drag evidence into the green slot first.";
            }

            return;
        }

        if (slottedEvidence.IsVerified)
        {
            if (resultText != null)
            {
                resultText.text = "This evidence is already verified.";
            }

            return;
        }

        if (slottedEvidence.Marker != null && !slottedEvidence.Marker.requiresAnalysis)
        {
            if (resultText != null)
            {
                resultText.text = "This clue does not need analyser verification.";
            }

            return;
        }

        if (!session.TryAnalyzeEvidence(slottedEvidence))
        {
            if (resultText != null)
            {
                resultText.text = "No analysis tokens left.";
            }

            return;
        }

        if (resultText != null && slottedEvidence.Marker != null)
        {
            string truthLabel = slottedEvidence.Marker.isFactuallyTrue ? "confirmed" : "debunked";
            resultText.text = $"Analysis complete: clue {truthLabel}.";
        }
    }

    private void SlotEvidence(CollectedEvidence evidence)
    {
        if (evidence == null)
        {
            return;
        }

        slottedEvidence = evidence;
        UpdateSlotVisual();

        if (resultText != null)
        {
            resultText.text = "Evidence loaded. Press ANALYSE.";
        }
    }

    private void EnsureSlotPreviewText()
    {
        if (slotPreviewText != null || dropZone == null)
        {
            return;
        }

        GameObject textObject = new GameObject("SlotPreviewText", typeof(RectTransform));
        textObject.transform.SetParent(dropZone.transform, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(6f, 6f);
        rect.offsetMax = new Vector2(-6f, -6f);

        slotPreviewText = textObject.AddComponent<TextMeshProUGUI>();
        slotPreviewText.fontSize = 11;
        slotPreviewText.alignment = TextAlignmentOptions.Center;
        slotPreviewText.textWrappingMode = TextWrappingModes.Normal;
        slotPreviewText.raycastTarget = false;
        slotPreviewText.color = Color.black;
    }

    private void CacheDefaultReferences()
    {
        if (dropZone == null)
        {
            dropZone = GetComponentInChildren<FactAnalyserDropZone>(true);
        }

        if (dropZone != null)
        {
            if (slotBackground == null)
            {
                slotBackground = dropZone.GetComponent<Image>();
            }

            if (slotPreviewText == null)
            {
                slotPreviewText = dropZone.GetComponentInChildren<TMP_Text>(true);
            }
        }
    }

    private void RefreshTokens()
    {
        if (tokensRemainingText != null && session != null)
        {
            tokensRemainingText.text = $"{session.AnalysisTokensRemaining} Tokens Left";
        }
    }

    private void UpdateSlotVisual()
    {
        if (slottedEvidence == null)
        {
            ClearSlotVisual();
            return;
        }

        if (slotBackground != null)
        {
            slotBackground.color = filledSlotColor;
        }

        if (slotPreviewText != null)
        {
            slotPreviewText.text = slottedEvidence.GetDisplayLabel();
        }

        if (previewImage == null)
        {
            return;
        }

        if (slottedEvidence.Photograph != null)
        {
            previewImage.enabled = true;
            previewImage.sprite = slottedEvidence.Photograph;
        }
        else
        {
            previewImage.enabled = false;
            previewImage.sprite = null;
        }
    }

    private void ClearSlotVisual()
    {
        if (slotBackground != null)
        {
            slotBackground.color = emptySlotColor;
        }

        if (slotPreviewText != null)
        {
            slotPreviewText.text = "Drop evidence here";
        }

        if (previewImage != null)
        {
            previewImage.enabled = false;
            previewImage.sprite = null;
        }
    }
}
