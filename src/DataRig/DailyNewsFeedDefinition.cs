using UnityEngine;

[CreateAssetMenu(fileName = "DailyNewsFeed", menuName = "Data Rig/Daily News Feed", order = 2)]
public class DailyNewsFeedDefinition : ScriptableObject
{
    public string feedDateLabel = "Day 1";
    public int daysRemaining = 3;
    public NewsStoryDefinition[] stories = new NewsStoryDefinition[3];
}
