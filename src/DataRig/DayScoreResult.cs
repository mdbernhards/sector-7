using System.Collections.Generic;

public class DayScoreResult
{
    public List<StoryScoreResult> Stories = new List<StoryScoreResult>();
    public int CorrectVerdictCount;
    public int TotalStories;
    public int TotalTrustDelta;
}
