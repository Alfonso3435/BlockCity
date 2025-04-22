using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance { get; private set; }

    [Header("Configuration")]
    public List<MissionData> allMissions;
    public GameObject missionPrefab;
    public Transform missionsContainer;
    public TMP_Text missionsTitleText;
    
    private Dictionary<string, Mission> activeMissions = new Dictionary<string, Mission>();
    private float lastUpdateTime;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // Verificar cambios en PlayerPrefs cada segundo (ajustable)
        if (Time.time - lastUpdateTime > 1f)
        {
            CheckForUpdates();
            lastUpdateTime = Time.time;
        }
    }

    public void ShowMissions(string panelTitle = "BlockCity Missions")
    {
        if (missionsTitleText != null)
            missionsTitleText.text = panelTitle;

        ClearMissions();
        CreateMissions();
    }

    private void ClearMissions()
    {
        foreach (Transform child in missionsContainer)
        {
            Destroy(child.gameObject);
        }
        activeMissions.Clear();
    }

    private void CreateMissions()
    {
        foreach (MissionData missionData in allMissions)
        {
            GameObject missionObj = Instantiate(missionPrefab, missionsContainer);
            Mission missionComponent = missionObj.GetComponent<Mission>();
            
            if (missionComponent != null)
            {
                missionComponent.Setup(missionData);
                activeMissions[missionData.missionID] = missionComponent;
            }
        }
    }

    private void CheckForUpdates()
    {
        if (!missionsContainer.gameObject.activeInHierarchy)
            return;

        foreach (var missionPair in activeMissions)
        {
            string missionID = missionPair.Key;
            Mission mission = missionPair.Value;
            
            // Solo actualizar si la misión está visible
            if (mission.gameObject.activeInHierarchy)
            {
                mission.UpdateFromPlayerPrefs();
            }
        }
    }

    [ContextMenu("Reset All Missions")]
    public void ResetAllMissions()
    {
        foreach (MissionData mission in allMissions)
        {
            PlayerPrefs.DeleteKey($"Mission_{mission.missionID}_Progress");
            PlayerPrefs.DeleteKey($"Mission_{mission.missionID}_Claimed");
        }
        PlayerPrefs.Save();
        
        if (missionsContainer.gameObject.activeInHierarchy)
        {
            ShowMissions();
        }
    }
}