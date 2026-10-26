using UnityEngine;

public class CollectedEvidence
{
    public EvidenceMarkerDefinition Marker { get; }
    public EvidenceSource Source { get; }
    public Sprite Photograph { get; }
    public bool IsVerified { get; set; }

    public CollectedEvidence(EvidenceMarkerDefinition marker, EvidenceSource source, Sprite photograph = null)
    {
        Marker = marker;
        Source = source;
        Photograph = photograph;
        IsVerified = marker != null && !marker.requiresAnalysis;
    }

    public string GetDisplayLabel()
    {
        if (Source == EvidenceSource.Photograph && Photograph != null)
        {
            return "Photograph evidence";
        }

        if (Marker != null && !string.IsNullOrWhiteSpace(Marker.collectedLabel))
        {
            return Marker.collectedLabel;
        }

        return Marker != null ? Marker.id : "Unknown evidence";
    }

    public Sprite GetDisplaySprite()
    {
        if (Photograph != null)
        {
            return Photograph;
        }

        if (Marker != null && Marker.evidenceImage != null)
        {
            return Marker.evidenceImage;
        }

        return null;
    }
}
