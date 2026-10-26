using System.Collections.Generic;

public class StoryScoreResult
{
    public int StoryIndex;
    public string StoryTitle;
    public bool ActuallyFake;
    public PlayerVerdict PlayerVerdict;
    public bool VerdictCorrect;
    public int RelevantEvidenceCollected;
    public int RelevantEvidenceTotal;
    public int RedHerringsCollected;
    public int TrustDelta;
    public List<ScoreBreakdownEntry> Breakdown = new List<ScoreBreakdownEntry>();
}
