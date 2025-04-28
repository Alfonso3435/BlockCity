using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance { get; private set; }

    [Header("Configuration")]
    public GameObject missionPrefab;
    public Transform missionsContainer;
    public TMP_Text missionsTitleText;
    
    private Dictionary<int, Mission> activeMissions = new Dictionary<int, Mission>();
    private float lastUpdateTime;
    private bool missionsLoaded = false;
    private List<UserQuest> loadedMissions = new List<UserQuest>();

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

    void Start()
    {
        StartCoroutine(InitializeMissions());
    }

    // En MissionManager
    public IEnumerator InitializeMissions()
    {   
        int userId = DBQuizReqHolder.Instance.GetUserID();
        yield return StartCoroutine(DBQuizReqHolder.Instance.GetUserQuestsData(userId));
        loadedMissions = new List<UserQuest>(DBQuizReqHolder.Instance.GetUserQuests());
        missionsLoaded = true;
    }

    public void ShowMissions(string panelTitle = "BlockCity Missions")
    {
        if (!missionsLoaded)
        {
            Debug.LogWarning("Las misiones no han terminado de cargarse");
            return;
        }

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
        if (loadedMissions == null || loadedMissions.Count == 0)
        {
            Debug.LogWarning("No hay misiones cargadas para mostrar");
            return;
        }

        foreach (UserQuest quest in loadedMissions)
        {
            GameObject missionObj = Instantiate(missionPrefab, missionsContainer);
            Mission missionComponent = missionObj.GetComponent<Mission>();
            
            if (missionComponent != null)
            {
                missionComponent.Setup(quest);
                activeMissions[quest.id_quest] = missionComponent;
            }
        }
    }

    public void UpdateQuestProgress(int questId, int progress)
    {
        int userId = DBQuizReqHolder.Instance.GetUserID();
        StartCoroutine(DBQuizReqHolder.Instance.UpdateQuestProgress(userId, questId, progress));
    }

    public IEnumerator ClaimQuestReward(int questId)
    {
        int userId = DBQuizReqHolder.Instance.GetUserID();
        yield return StartCoroutine(DBQuizReqHolder.Instance.ClaimQuestReward(userId, questId));
        
        // Actualizar la lista de misiones después de reclamar
        yield return StartCoroutine(InitializeMissions());
        
        if (missionsContainer.gameObject.activeInHierarchy)
        {
            ShowMissions();
        }
    }

    public bool AreMissionsLoaded()
    {
        return missionsLoaded;
    }

    public List<UserQuest> GetAllMissions()
    {
        return loadedMissions;
    }

    public UserQuest GetMissionById(int questId)
    {
        return loadedMissions.Find(q => q.id_quest == questId);
    }
}