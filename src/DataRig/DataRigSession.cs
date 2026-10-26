using System;
using System.Collections.Generic;
using UnityEngine;

public class DataRigSession
{
    public DailyNewsFeedDefinition Feed { get; }
    public IReadOnlyList<NewsStorySession> StorySessions => storySessions;

    public int AnalysisTokensRemaining { get; private set; }
    public bool IsDaySubmitted { get; private set; }
    public DayScoreResult LastDayScoreResult { get; private set; }

    private readonly List<NewsStorySession> storySessions = new List<NewsStorySession>();

    public event Action SessionChanged;

    public DataRigSession(DailyNewsFeedDefinition feed, int startingAnalysisTokens = 3)
    {
        Feed = feed;
        AnalysisTokensRemaining = startingAnalysisTokens;

        if (feed?.stories == null)
        {
            return;
        }

        foreach (NewsStoryDefinition story in feed.stories)
        {
            if (story != null)
            {
                storySessions.Add(new NewsStorySession(story));
            }
        }
    }

    public NewsStorySession GetStorySession(int index)
    {
        if (index < 0 || index >= storySessions.Count)
        {
            return null;
        }

        return storySessions[index];
    }

    public bool TryCollectMarker(int storyIndex, EvidenceMarkerDefinition marker, EvidenceSource source, Sprite photograph = null)
    {
        NewsStorySession session = GetStorySession(storyIndex);
        if (session == null || !session.TryCollectMarker(marker, source, photograph))
        {
            return false;
        }

        SessionChanged?.Invoke();
        return true;
    }

    public bool TryAnalyzeEvidence(CollectedEvidence evidence)
    {
        if (evidence == null || evidence.IsVerified || AnalysisTokensRemaining <= 0)
        {
            return false;
        }

        evidence.IsVerified = true;
        AnalysisTokensRemaining--;
        SessionChanged?.Invoke();
        return true;
    }

    public void SetVerdict(int storyIndex, PlayerVerdict verdict)
    {
        NewsStorySession session = GetStorySession(storyIndex);
        if (session == null)
        {
            return;
        }

        session.SetVerdict(verdict);
        SessionChanged?.Invoke();
    }

    public bool AllVerdictsSet()
    {
        if (storySessions.Count == 0)
        {
            return false;
        }

        foreach (NewsStorySession storySession in storySessions)
        {
            if (storySession.Verdict == PlayerVerdict.Undecided)
            {
                return false;
            }
        }

        return true;
    }

    public DayScoreResult SubmitDay()
    {
        LastDayScoreResult = DataRigScoring.EvaluateDay(this);
        IsDaySubmitted = true;
        SessionChanged?.Invoke();
        return LastDayScoreResult;
    }
}
