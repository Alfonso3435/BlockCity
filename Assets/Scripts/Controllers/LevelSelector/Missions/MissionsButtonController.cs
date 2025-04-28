using UnityEngine;
using TMPro;
using System.Collections;

public class MissionsButtonController : MonoBehaviour 
{
    [Header("UI References")]
    public GameObject missionsPopUp;
    public TMP_Text notificationBadge;
    public float checkMissionsInterval = 5f;

    private void Start()
    {
        StartCoroutine(PeriodicMissionsCheck());
    }

    public void OpenMissionsPopup()
    {
        if (MissionManager.Instance == null)
        {
            Debug.LogError("MissionManager no encontrado!");
            return;
        }

        if (!MissionManager.Instance.AreMissionsLoaded())
        {
            Debug.LogWarning("Las misiones aún no han terminado de cargarse");
            StartCoroutine(WaitAndOpenMissions());
            return;
        }

        missionsPopUp.SetActive(true);
        MissionManager.Instance.ShowMissions();
        UpdateNotificationBadge();
    }

    private IEnumerator WaitAndOpenMissions()
    {
        yield return new WaitUntil(() => MissionManager.Instance.AreMissionsLoaded());
        missionsPopUp.SetActive(true);
        MissionManager.Instance.ShowMissions();
        UpdateNotificationBadge();
    }

    private IEnumerator PeriodicMissionsCheck()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkMissionsInterval);
            
            // Recargar misiones del servidor
            yield return MissionManager.Instance.InitializeMissions();
            UpdateNotificationBadge();
        }
    }

    public void UpdateNotificationBadge()
    {
        if (notificationBadge != null && MissionManager.Instance.AreMissionsLoaded())
        {
            int claimableMissions = CountClaimableMissions();
            notificationBadge.text = claimableMissions > 0 ? claimableMissions.ToString() : "";
            notificationBadge.gameObject.SetActive(claimableMissions > 0);
        }
    }

    private int CountClaimableMissions()
    {
        if (!MissionManager.Instance.AreMissionsLoaded())
            return 0;

        int count = 0;
        var missions = MissionManager.Instance.GetAllMissions();

        foreach (var mission in missions)
        {
            if (mission.completado == 1 && mission.userProgress >= mission.TargetProgress)
            {
                count++;
            }
        }
        return count;
    }

    public void CloseMissionsPopup()
    {
        missionsPopUp.SetActive(false);
    }
}