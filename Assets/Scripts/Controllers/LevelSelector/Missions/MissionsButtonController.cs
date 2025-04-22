using UnityEngine;
using TMPro;

public class MissionsButtonController : MonoBehaviour 
{
    [Header("UI References")]
    public GameObject missionsPopUp;
    public TMP_Text notificationBadge;

    public void OpenMissionsPopup()
    {
        if (MissionManager.Instance == null)
        {
            Debug.LogError("MissionManager no encontrado!");
            return;
        }

        missionsPopUp.SetActive(true);
        MissionManager.Instance.ShowMissions();
        UpdateNotificationBadge();
    }

    private void UpdateNotificationBadge()
    {
        if (notificationBadge != null)
        {
            int claimableMissions = CountClaimableMissions();
            notificationBadge.text = claimableMissions > 0 ? claimableMissions.ToString() : "";
            notificationBadge.gameObject.SetActive(claimableMissions > 0);
        }
    }

    private int CountClaimableMissions()
    {
        int count = 0;
        foreach (MissionData mission in MissionManager.Instance.allMissions)
        {
            int progress = PlayerPrefs.GetInt($"{mission.missionID}_Progress", 0);
            bool isClaimed = PlayerPrefs.GetInt($"{mission.missionID}_Claimed", 0) == 1;
            
            if (progress >= mission.targetProgress && !isClaimed)
            {
                count++;
            }
        }
        return count;
    }
}