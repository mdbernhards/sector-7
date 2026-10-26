using UnityEngine;
using UnityEngine.EventSystems;

public class FactAnalyserDropZone : MonoBehaviour, IDropHandler
{
    public FactAnalyserController analyser;

    public void OnDrop(PointerEventData eventData)
    {
        analyser?.HandleDrop(eventData);
    }

    public void HandleDropFromItem(EvidenceListItemView item)
    {
        analyser?.HandleDropFromItem(item);
    }
}
