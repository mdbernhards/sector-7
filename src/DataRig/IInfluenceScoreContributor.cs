using System.Collections.Generic;

public interface IInfluenceScoreContributor
{
    void Contribute(NewsStorySession session, StoryScoreResult result, List<ScoreBreakdownEntry> breakdown);
}
