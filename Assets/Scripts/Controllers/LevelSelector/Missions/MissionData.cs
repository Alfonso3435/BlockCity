using UnityEngine;

[CreateAssetMenu(fileName = "NewMission", menuName = "Missions/Mission Data")]
public class MissionData : ScriptableObject
{
    public string missionID;
    public string missionTitle;
    [TextArea] public string missionDescription;
    public int targetProgress;
    public int rewardCoins;
    public MissionType missionType;
}

public enum MissionType
{
    Trivia,
    Memory,
    Hangman,
    Streak,
    Points,
    Daily
}