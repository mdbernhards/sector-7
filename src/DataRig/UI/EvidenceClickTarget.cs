using UnityEngine;
using UnityEngine.EventSystems;

public class EvidenceClickTarget : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private string markerIdOverride;

    private NewsStoryDefinition story;
    private EvidenceTarget target;
    private DataRigController controller;
    private int storyIndex = -1;

    public void Initialize(NewsStoryDefinition storyDefinition, EvidenceTarget evidenceTarget, DataRigController dataRigController = null, int index = -1)
    {
        story = storyDefinition;
        target = evidenceTarget;
        controller = dataRigController;
        storyIndex = index;
    }

    public void ConfigureRuntime(DataRigController dataRigController, int index)
    {
        controller = dataRigController;
        storyIndex = index;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (controller == null || story == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(markerIdOverride))
        {
            controller.TryCollectEvidenceById(storyIndex, markerIdOverride);
            return;
        }

        foreach (EvidenceMarkerDefinition marker in story.GetMarkersForTarget(target))
        {
            controller.TryCollectEvidenceById(storyIndex, marker.id);
            break;
        }
    }
}
