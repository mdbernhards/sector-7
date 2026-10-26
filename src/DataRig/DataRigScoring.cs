using System.Collections.Generic;

public static class DataRigScoring
{
    private static readonly List<IInfluenceScoreContributor> Contributors = new List<IInfluenceScoreContributor>
    {
        new VerdictScoreContributor(),
        new EvidenceScoreContributor()
    };

    public static void RegisterContributor(IInfluenceScoreContributor contributor)
    {
        if (contributor != null && !Contributors.Contains(contributor))
        {
            Contributors.Add(contributor);
        }
    }

    public static StoryScoreResult EvaluateStory(int storyIndex, NewsStorySession session)
    {
        StoryScoreResult result = new StoryScoreResult
        {
            StoryIndex = storyIndex,
            StoryTitle = session.Definition != null ? session.Definition.title : $"Story {storyIndex + 1}",
            ActuallyFake = session.Definition != null && session.Definition.isFake,
            PlayerVerdict = session.Verdict,
            VerdictCorrect = IsVerdictCorrect(session)
        };

        CountEvidence(session, result);

        foreach (IInfluenceScoreContributor contributor in Contributors)
        {
            contributor.Contribute(session, result, result.Breakdown);
        }

        return result;
    }

    public static DayScoreResult EvaluateDay(DataRigSession session)
    {
        DayScoreResult dayResult = new DayScoreResult();

        for (int i = 0; i < session.StorySessions.Count; i++)
        {
            StoryScoreResult storyResult = EvaluateStory(i, session.StorySessions[i]);
            dayResult.Stories.Add(storyResult);
            dayResult.TotalTrustDelta += storyResult.TrustDelta;

            if (storyResult.VerdictCorrect)
            {
                dayResult.CorrectVerdictCount++;
            }
        }

        dayResult.TotalStories = dayResult.Stories.Count;
        return dayResult;
    }

    public static bool IsVerdictCorrect(NewsStorySession session)
    {
        if (session?.Definition == null || session.Verdict == PlayerVerdict.Undecided)
        {
            return false;
        }

        bool actuallyFake = session.Definition.isFake;
        return actuallyFake
            ? session.Verdict == PlayerVerdict.Fake
            : session.Verdict == PlayerVerdict.Real;
    }

    private static void CountEvidence(NewsStorySession session, StoryScoreResult result)
    {
        if (session.Definition?.evidenceMarkers == null)
        {
            return;
        }

        HashSet<string> collectedIds = new HashSet<string>();
        foreach (CollectedEvidence evidence in session.CollectedEvidence)
        {
            if (evidence.Marker != null && evidence.Marker.isEnabled)
            {
                collectedIds.Add(evidence.Marker.id);
            }
        }

        foreach (EvidenceMarkerDefinition marker in session.Definition.evidenceMarkers)
        {
            if (marker == null)
            {
                continue;
            }

            if (!marker.isEnabled)
            {
                continue;
            }

            if (marker.isRelevant)
            {
                result.RelevantEvidenceTotal++;
            }

            if (!collectedIds.Contains(marker.id))
            {
                continue;
            }

            if (marker.isRelevant)
            {
                result.RelevantEvidenceCollected++;
            }
            else
            {
                result.RedHerringsCollected++;
            }
        }
    }
}
