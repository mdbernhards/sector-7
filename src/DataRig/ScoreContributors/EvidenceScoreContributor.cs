using System.Collections.Generic;

public class EvidenceScoreContributor : IInfluenceScoreContributor
{
    public const int PerRelevantEvidenceTrust = 25;
    public const int PerIrrelevantEvidenceTrust = -150;

    public void Contribute(NewsStorySession session, StoryScoreResult result, List<ScoreBreakdownEntry> breakdown)
    {
        if (session?.CollectedEvidence == null)
        {
            return;
        }

        for (int i = 0; i < session.CollectedEvidence.Count; i++)
        {
            CollectedEvidence collected = session.CollectedEvidence[i];
            EvidenceMarkerDefinition marker = collected.Marker;
            if (marker == null || !marker.isEnabled)
            {
                continue;
            }

            bool isRelevant = marker.isRelevant;
            int trustDelta = isRelevant ? PerRelevantEvidenceTrust : PerIrrelevantEvidenceTrust;

            string label = isRelevant ? "Relevant evidence collected" : "Irrelevant evidence collected";
            string markerLabel = string.IsNullOrWhiteSpace(marker.collectedLabel) ? marker.id : marker.collectedLabel;
            string defaultExplanation = isRelevant
                ? "This clue supports the investigation."
                : "This clue was misleading and hurt your credibility.";

            breakdown.Add(new ScoreBreakdownEntry
            {
                Category = "Evidence",
                Label = $"{label}: {markerLabel}",
                Description = string.IsNullOrWhiteSpace(marker.resultExplanation)
                    ? defaultExplanation
                    : marker.resultExplanation.Trim(),
                TrustDelta = trustDelta
            });

            result.TrustDelta += trustDelta;
        }
    }
}
