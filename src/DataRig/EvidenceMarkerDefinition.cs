using System;
using UnityEngine;

[Serializable]
public class EvidenceMarkerDefinition
{
    [Tooltip("Unique id used in TMP link tags and save data.")]
    public string id;

    public EvidenceTarget target = EvidenceTarget.BodyText;
    public EvidenceCategory category = EvidenceCategory.Other;

    [Tooltip("Plain text snippet to highlight inside body text. Leave empty for non-body targets.")]
    public string highlightText;

    [Tooltip("Shown in the evidence panel after the player collects this marker.")]
    [TextArea(2, 4)]
    public string collectedLabel;

    [Tooltip("Enable/disable this evidence marker for playtesting.")]
    public bool isEnabled = true;

    [Tooltip("If true, collecting this marker updates the current objective text.")]
    public bool advancesObjective;

    [Tooltip("Higher priority objectives replace lower ones. Older/lower steps are ignored.")]
    public int objectivePriority;

    [Tooltip("Objective text after collecting this marker. Example: Visit The Hospital")]
    public string objectiveText;

    [Tooltip("Optional image shown on the evidence item card.")]
    public Sprite evidenceImage;

    [Tooltip("Why this clue is true/false/relevant. Displayed on end-day results.")]
    [TextArea(2, 6)]
    public string resultExplanation;

    [Tooltip("True if this clue actually helps identify fake/real news.")]
    public bool isRelevant = true;

    [Tooltip("True if the fact analyser must confirm this before it counts.")]
    public bool requiresAnalysis;

    [Tooltip("Ground truth: is this specific clue factually correct in-world?")]
    public bool isFactuallyTrue = true;
}
