using System.Collections.Generic;
using UnityEngine;

public class NewsStorySession
{
    public NewsStoryDefinition Definition { get; }
    public PlayerVerdict Verdict { get; private set; } = PlayerVerdict.Undecided;
    public IReadOnlyList<CollectedEvidence> CollectedEvidence => collectedEvidence;

    private readonly List<CollectedEvidence> collectedEvidence = new List<CollectedEvidence>();
    private readonly HashSet<string> collectedMarkerIds = new HashSet<string>();

    public NewsStorySession(NewsStoryDefinition definition)
    {
        Definition = definition;
    }

    public bool TryCollectMarker(EvidenceMarkerDefinition marker, EvidenceSource source, Sprite photograph = null)
    {
        if (marker == null || !marker.isEnabled || collectedMarkerIds.Contains(marker.id))
        {
            return false;
        }

        collectedMarkerIds.Add(marker.id);
        collectedEvidence.Add(new CollectedEvidence(marker, source, photograph));
        return true;
    }

    public bool TryAddExternalEvidence(CollectedEvidence evidence)
    {
        if (evidence == null)
        {
            return false;
        }

        if (evidence.Marker != null && collectedMarkerIds.Contains(evidence.Marker.id))
        {
            return false;
        }

        if (evidence.Marker != null)
        {
            collectedMarkerIds.Add(evidence.Marker.id);
        }

        collectedEvidence.Add(evidence);
        return true;
    }

    public void SetVerdict(PlayerVerdict verdict)
    {
        Verdict = verdict;
    }

    public string GetCurrentObjective()
    {
        if (Definition == null)
        {
            return "Collect Evidence";
        }

        string objective = string.IsNullOrWhiteSpace(Definition.initialObjectiveText)
            ? "Collect Evidence"
            : Definition.initialObjectiveText.Trim();

        int highestPriority = int.MinValue;
        int totalObjectiveMarkers = 0;
        int collectedObjectiveMarkers = 0;

        if (Definition.evidenceMarkers != null)
        {
            for (int i = 0; i < Definition.evidenceMarkers.Count; i++)
            {
                EvidenceMarkerDefinition definitionMarker = Definition.evidenceMarkers[i];
                if (definitionMarker == null || !definitionMarker.isEnabled || !definitionMarker.advancesObjective)
                {
                    continue;
                }

                totalObjectiveMarkers++;
                if (!string.IsNullOrEmpty(definitionMarker.id) && collectedMarkerIds.Contains(definitionMarker.id))
                {
                    collectedObjectiveMarkers++;
                }
            }
        }

        for (int i = 0; i < collectedEvidence.Count; i++)
        {
            EvidenceMarkerDefinition marker = collectedEvidence[i].Marker;
            if (marker == null || !marker.isEnabled || !marker.advancesObjective)
            {
                continue;
            }

            if (marker.objectivePriority < highestPriority)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(marker.objectiveText))
            {
                continue;
            }

            highestPriority = marker.objectivePriority;
            objective = marker.objectiveText.Trim();
        }

        bool allObjectiveMarkersCollected =
            totalObjectiveMarkers > 0 && collectedObjectiveMarkers >= totalObjectiveMarkers;
        if (allObjectiveMarkersCollected)
        {
            return Verdict == PlayerVerdict.Undecided
                ? "Make a Verdict"
                : "Verdict has been made";
        }

        return objective;
    }
}
