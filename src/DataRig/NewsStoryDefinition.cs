using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewsStory", menuName = "Data Rig/News Story", order = 1)]
public class NewsStoryDefinition : ScriptableObject
{
    public string storyId;
    [Tooltip("Short title used on story tab buttons.")]
    public string shortTitle;
    public string displayName;
    public string userHandle;
    public string datePosted;
    public Sprite profilePicture;
    public string title;

    [TextArea(4, 12)]
    public string bodyText;

    public Sprite storyImage;

    [Tooltip("Ground truth used for scoring. Hidden from the player.")]
    public bool isFake;

    [Tooltip("Default objective shown before objective markers advance the task.")]
    public string initialObjectiveText = "Collect Evidence";

    public List<EvidenceMarkerDefinition> evidenceMarkers = new List<EvidenceMarkerDefinition>();

    public IEnumerable<EvidenceMarkerDefinition> GetMarkersForTarget(EvidenceTarget target)
    {
        foreach (EvidenceMarkerDefinition marker in evidenceMarkers)
        {
            if (marker != null && marker.isEnabled && marker.target == target)
            {
                yield return marker;
            }
        }
    }

    public EvidenceMarkerDefinition GetMarkerById(string markerId)
    {
        if (string.IsNullOrEmpty(markerId))
        {
            return null;
        }

        for (int i = 0; i < evidenceMarkers.Count; i++)
        {
            EvidenceMarkerDefinition marker = evidenceMarkers[i];
            if (marker != null && marker.isEnabled && marker.id == markerId)
            {
                return marker;
            }
        }

        return null;
    }
}
