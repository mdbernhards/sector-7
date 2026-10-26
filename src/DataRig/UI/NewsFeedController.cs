using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewsFeedController : MonoBehaviour
{
    public Transform feedContentRoot;
    public NewsPostView postPrefab;
    public TMP_Text feedDateText;
    public TMP_Text daysRemainingText;

    private readonly List<NewsPostView> activePosts = new List<NewsPostView>();
    private DataRigSession activeSession;

    public void Populate(DailyNewsFeedDefinition feed, DataRigSession session, DataRigController controller)
    {
        if (activeSession != null)
        {
            activeSession.SessionChanged -= HandleSessionChanged;
        }

        ClearPosts();
        activeSession = session;
        if (activeSession != null)
        {
            activeSession.SessionChanged += HandleSessionChanged;
        }

        if (feedDateText != null)
        {
            feedDateText.text = feed != null ? feed.feedDateLabel : string.Empty;
        }

        if (daysRemainingText != null && feed != null)
        {
            daysRemainingText.text = $"{feed.daysRemaining} Days Left";
        }

        if (feedContentRoot == null || postPrefab == null || session == null)
        {
            return;
        }

        for (int i = 0; i < session.StorySessions.Count; i++)
        {
            NewsPostView postView = Instantiate(postPrefab, feedContentRoot);
            postView.gameObject.SetActive(true);
            postView.Bind(i, session.StorySessions[i], controller);
            ConfigureClickTargets(postView, controller, i);
            activePosts.Add(postView);
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(feedContentRoot as RectTransform);
    }

    public void RefreshObjectives()
    {
        if (activeSession == null)
        {
            return;
        }

        for (int i = 0; i < activePosts.Count; i++)
        {
            if (activePosts[i] != null)
            {
                activePosts[i].RefreshObjective();
            }
        }
    }

    private static void ConfigureClickTargets(NewsPostView postView, DataRigController controller, int storyIndex)
    {
        foreach (EvidenceClickTarget target in postView.GetComponentsInChildren<EvidenceClickTarget>(true))
        {
            target.ConfigureRuntime(controller, storyIndex);
        }
    }

    private void ClearPosts()
    {
        if (activeSession != null)
        {
            activeSession.SessionChanged -= HandleSessionChanged;
        }

        foreach (NewsPostView post in activePosts)
        {
            if (post != null)
            {
                Destroy(post.gameObject);
            }
        }

        activePosts.Clear();
        activeSession = null;
    }

    private void HandleSessionChanged()
    {
        RefreshObjectives();
    }
}
