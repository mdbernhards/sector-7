using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TMP_Text))]
public class EvidenceBodyText : MonoBehaviour, IPointerClickHandler
{
    private TMP_Text text;
    private int storyIndex = -1;
    private DataRigController controller;

    public void Initialize(int index, DataRigController dataRigController, string richText)
    {
        storyIndex = index;
        controller = dataRigController;

        if (text == null)
        {
            text = GetComponent<TMP_Text>();
        }

        text.richText = true;
        text.text = richText;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (text == null || controller == null || storyIndex < 0)
        {
            return;
        }

        int linkIndex = TMP_TextUtilities.FindIntersectingLink(text, eventData.position, eventData.pressEventCamera);
        if (linkIndex < 0)
        {
            return;
        }

        TMP_LinkInfo linkInfo = text.textInfo.linkInfo[linkIndex];
        controller.TryCollectEvidenceById(storyIndex, linkInfo.GetLinkID());
    }

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
    }
}
