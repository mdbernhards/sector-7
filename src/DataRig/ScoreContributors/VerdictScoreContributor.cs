using System.Collections.Generic;

public class VerdictScoreContributor : IInfluenceScoreContributor
{
    public const int CorrectTrust = 150;
    public const int WrongTrust = -250;
    public const int MissingVerdictTrust = -300;

    public void Contribute(NewsStorySession session, StoryScoreResult result, List<ScoreBreakdownEntry> breakdown)
    {
        if (session.Verdict == PlayerVerdict.Undecided)
        {
            breakdown.Add(new ScoreBreakdownEntry
            {
                Category = "Verdict",
                Label = "No verdict submitted",
                Description = "You did not mark this story REAL or FAKE.",
                TrustDelta = MissingVerdictTrust
            });
            result.TrustDelta += MissingVerdictTrust;
            return;
        }

        if (result.VerdictCorrect)
        {
            breakdown.Add(new ScoreBreakdownEntry
            {
                Category = "Verdict",
                Label = "Correct verdict",
                Description = "You correctly identified whether this story was real or fake.",
                TrustDelta = CorrectTrust
            });
            result.TrustDelta += CorrectTrust;
            return;
        }

        breakdown.Add(new ScoreBreakdownEntry
        {
            Category = "Verdict",
            Label = "Wrong verdict",
            Description = "Your REAL/FAKE call did not match the truth.",
            TrustDelta = WrongTrust
        });
        result.TrustDelta += WrongTrust;
    }
}
