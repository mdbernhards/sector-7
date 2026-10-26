using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EvidenceListItemView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI References")]
    public TMP_Text labelText;
    public Image backgroundImage;
    public Image iconImage;

    [Header("Layout")]
    [Tooltip("Icon height when evidence has an image.")]
    public float iconExpandedHeight = 90f;

    public CollectedEvidence Evidence { get; private set; }

    private Canvas rootCanvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private LayoutElement iconLayout;
    private Transform originalParent;
    private int originalSiblingIndex;
    private Vector2 pointerOffset;

    private void Awake()
    {
        CacheReferences();
    }

    private void Reset()
    {
        CacheReferences();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        CacheReferences();
    }
#endif

    public void Bind(CollectedEvidence evidence)
    {
        Evidence = evidence;
        CacheReferences();

        if (labelText != null)
        {
            labelText.text = evidence.GetDisplayLabel();
        }

        if (iconImage != null)
        {
            Sprite icon = evidence.GetDisplaySprite();
            if (icon != null)
            {
                iconImage.gameObject.SetActive(true);
                iconImage.enabled = true;
                iconImage.sprite = icon;
                ExpandIconLayoutHeight();
            }
            else
            {
                iconImage.gameObject.SetActive(false);
                iconImage.enabled = false;
                iconImage.sprite = null;
                CollapseIconLayoutHeight();
            }
        }

        if (rectTransform == null)
        {
            rectTransform = transform as RectTransform;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (Evidence == null)
        {
            return;
        }

        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();
        rootCanvas = GetComponentInParent<Canvas>();
        rectTransform = transform as RectTransform;
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.blocksRaycasts = false;
        transform.SetParent(rootCanvas.transform, true);
        transform.SetAsLastSibling();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out pointerOffset);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (rectTransform == null)
        {
            return;
        }

        RectTransform parentRect = rectTransform.parent as RectTransform;
        if (parentRect == null)
        {
            return;
        }

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint))
        {
            rectTransform.localPosition = localPoint - pointerOffset;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
        }

        TryDropOnAnalyser(eventData);

        if (originalParent != null)
        {
            transform.SetParent(originalParent, false);
            transform.SetSiblingIndex(originalSiblingIndex);
        }
    }

    private void CacheReferences()
    {
        if (labelText == null)
        {
            labelText = GetComponentInChildren<TMP_Text>(true);
        }

        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
        }

        if (iconImage != null && iconLayout == null)
        {
            iconLayout = iconImage.GetComponent<LayoutElement>();
        }
    }

    private void CollapseIconLayoutHeight()
    {
        if (iconLayout == null)
        {
            return;
        }

        iconLayout.minHeight = 0f;
        iconLayout.preferredHeight = 0f;
        iconLayout.flexibleHeight = 0f;
    }

    private void ExpandIconLayoutHeight()
    {
        if (iconLayout == null)
        {
            return;
        }

        iconLayout.minHeight = iconExpandedHeight;
        iconLayout.preferredHeight = iconExpandedHeight;
        iconLayout.flexibleHeight = 0f;
    }

    private static void TryDropOnAnalyser(PointerEventData eventData)
    {
        if (EventSystem.current == null || eventData.pointerDrag == null)
        {
            return;
        }

        var hits = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, hits);

        foreach (RaycastResult hit in hits)
        {
            FactAnalyserDropZone dropZone = hit.gameObject.GetComponent<FactAnalyserDropZone>();
            if (dropZone == null)
            {
                continue;
            }

            dropZone.HandleDropFromItem(eventData.pointerDrag.GetComponent<EvidenceListItemView>());
            return;
        }
    }
}
