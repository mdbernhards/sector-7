using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewsPostView : MonoBehaviour
{
    private const string EvidenceMarkColor = "#4DA6FF40";
    private const float MaxPostImageWidth = 550f;

    [Header("Header")]
    public TMP_Text displayNameText;
    public TMP_Text userHandleText;
    public TMP_Text datePostedText;
    public Image profilePictureImage;
    public EvidenceClickTarget profilePictureTarget;

    [Header("Content")]
    public TMP_Text titleText;
    public EvidenceBodyText bodyText;
    public Image storyImage;
    public EvidenceClickTarget storyImageTarget;

    [Header("Optional")]
    public TMP_Text displayNameClickTarget;
    public TMP_Text userHandleClickTarget;
    public TMP_Text datePostedClickTarget;
    public TMP_Text titleClickTarget;
    public TMP_Text questObjectiveText;

    private int storyIndex = -1;
    private NewsStorySession session;
    private DataRigController controller;

    public void Bind(int index, NewsStorySession storySession, DataRigController dataRigController)
    {
        storyIndex = index;
        session = storySession;
        controller = dataRigController;

        NewsStoryDefinition story = storySession.Definition;
        if (story == null)
        {
            return;
        }

        if (displayNameText != null)
        {
            displayNameText.text = BuildFieldHighlightText(story.displayName, story, EvidenceTarget.DisplayName);
        }

        if (userHandleText != null)
        {
            userHandleText.text = BuildFieldHighlightText(story.userHandle, story, EvidenceTarget.UserHandle);
        }

        if (datePostedText != null)
        {
            datePostedText.text = BuildFieldHighlightText(story.datePosted, story, EvidenceTarget.DatePosted);
        }

        if (titleText != null)
        {
            titleText.text = BuildFieldHighlightText(story.title, story, EvidenceTarget.Title);
        }

        if (profilePictureImage != null)
        {
            profilePictureImage.sprite = story.profilePicture;
            profilePictureImage.enabled = story.profilePicture != null;
        }

        if (storyImage != null)
        {
            storyImage.sprite = story.storyImage;
            storyImage.enabled = story.storyImage != null;
            ApplyPostImageSizing(story.storyImage);
        }

        BindBodyText(story);
        BindClickTargets(story);
        UpdateObjectiveText();
        RebuildLayout();
    }

    private void BindBodyText(NewsStoryDefinition story)
    {
        if (bodyText == null)
        {
            return;
        }

        string formattedBody = BuildBodyWithEvidenceLinks(story.bodyText, story);
        bodyText.Initialize(storyIndex, controller, formattedBody);
    }

    private void BindClickTargets(NewsStoryDefinition story)
    {
        BindTarget(profilePictureTarget, EvidenceTarget.ProfilePicture, story);
        BindTarget(storyImageTarget, EvidenceTarget.StoryImage, story);
        BindTextTarget(displayNameClickTarget ?? displayNameText, EvidenceTarget.DisplayName, story);
        BindTextTarget(userHandleClickTarget ?? userHandleText, EvidenceTarget.UserHandle, story);
        BindTextTarget(datePostedClickTarget ?? datePostedText, EvidenceTarget.DatePosted, story);
        BindTextTarget(titleClickTarget ?? titleText, EvidenceTarget.Title, story);
    }

    private static void BindTarget(EvidenceClickTarget target, EvidenceTarget evidenceTarget, NewsStoryDefinition story)
    {
        if (target == null)
        {
            return;
        }

        target.Initialize(story, evidenceTarget);
    }

    private static void BindTextTarget(TMP_Text text, EvidenceTarget evidenceTarget, NewsStoryDefinition story)
    {
        if (text == null)
        {
            return;
        }

        EvidenceClickTarget target = text.GetComponent<EvidenceClickTarget>();
        if (target == null)
        {
            target = text.gameObject.AddComponent<EvidenceClickTarget>();
        }

        target.Initialize(story, evidenceTarget);
    }

    private static string BuildBodyWithEvidenceLinks(string plainBody, NewsStoryDefinition story)
    {
        if (string.IsNullOrEmpty(plainBody))
        {
            return string.Empty;
        }

        string result = plainBody;
        List<EvidenceMarkerDefinition> bodyMarkers = new List<EvidenceMarkerDefinition>(story.GetMarkersForTarget(EvidenceTarget.BodyText));

        bodyMarkers.Sort((left, right) => string.IsNullOrEmpty(right.highlightText)
            ? -1
            : string.IsNullOrEmpty(left.highlightText)
                ? 1
                : right.highlightText.Length.CompareTo(left.highlightText.Length));

        foreach (EvidenceMarkerDefinition marker in bodyMarkers)
        {
            if (!marker.isEnabled || string.IsNullOrEmpty(marker.highlightText))
            {
                continue;
            }

            string wrapped = $"<link=\"{marker.id}\"><mark={EvidenceMarkColor}>{marker.highlightText}</mark></link>";
            result = result.Replace(marker.highlightText, wrapped);
        }

        return result;
    }

    private static string BuildFieldHighlightText(string plainText, NewsStoryDefinition story, EvidenceTarget target)
    {
        if (string.IsNullOrEmpty(plainText) || story == null)
        {
            return plainText;
        }

        foreach (EvidenceMarkerDefinition marker in story.GetMarkersForTarget(target))
        {
            if (marker != null && marker.isEnabled)
            {
                return $"<mark={EvidenceMarkColor}>{plainText}</mark>";
            }
        }

        return plainText;
    }

    private void RebuildLayout()
    {
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
    }

    private void ApplyPostImageSizing(Sprite sprite)
    {
        if (storyImage == null)
        {
            return;
        }

        LayoutElement layout = storyImage.GetComponent<LayoutElement>();
        if (layout == null)
        {
            layout = storyImage.gameObject.AddComponent<LayoutElement>();
        }

        if (sprite == null)
        {
            layout.preferredWidth = -1f;
            layout.preferredHeight = -1f;
            return;
        }

        float sourceWidth = sprite.rect.width;
        float sourceHeight = sprite.rect.height;
        if (sourceWidth <= 0f || sourceHeight <= 0f)
        {
            return;
        }

        float targetWidth = sourceWidth <= MaxPostImageWidth ? sourceWidth : MaxPostImageWidth;
        float targetHeight = targetWidth * (sourceHeight / sourceWidth);

        storyImage.preserveAspect = true;
        layout.preferredWidth = targetWidth;
        layout.preferredHeight = targetHeight;
        layout.flexibleWidth = 0f;
        layout.flexibleHeight = 0f;

        RectTransform rect = storyImage.rectTransform;
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);
    }

    public void RefreshObjective()
    {
        UpdateObjectiveText();
    }

    private void UpdateObjectiveText()
    {
        if (questObjectiveText == null)
        {
            questObjectiveText = FindQuestObjectiveText();
        }

        if (questObjectiveText == null || session == null)
        {
            return;
        }

        questObjectiveText.text = $"Current Task: {session.GetCurrentObjective()}";
    }

    private TMP_Text FindQuestObjectiveText()
    {
        TMP_Text[] textObjects = GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < textObjects.Length; i++)
        {
            string name = textObjects[i].name;
            if (name == "QuestObjective" || name == "QuestObjectiveLabel" || name == "QuestObjectiveText" || name == "CurrentTaskText")
            {
                return textObjects[i];
            }
        }

        return null;
    }
}
