using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;
using System.Linq;
using UnityEngine.Networking;

// Descripción: Este archivo gestiona las misiones del juego, incluyendo la inicialización, actualización del progreso, reclamación de recompensas y la interacción con el servidor para sincronizar los datos de las misiones.
// Autor: Alfonso Vega

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

    [SerializeField] private TMP_Text coinsText;

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

    private void Update()
    {
        coinsText.text = DBQuizReqHolder.Instance.GetCoins().ToString();
    }


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
    string url = $"{DBQuizReqHolder.Instance.urlBD}claim-quest-reward";
    
    Debug.Log($"Intentando reclamar recompensa para misión {questId}, usuario {userId}");
    
    WWWForm form = new WWWForm();
    form.AddField("userId", userId);
    form.AddField("questId", questId);

    using (UnityWebRequest request = UnityWebRequest.Post(url, form))
    {
        request.timeout = 10;
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Error al reclamar: {request.error}");
            Debug.LogError($"Respuesta: {request.downloadHandler.text}");
            yield break;
        }

        Debug.Log($"Recompensa reclamada con éxito: {request.downloadHandler.text}");

        var quest = loadedMissions.FirstOrDefault(q => q.id_quest == questId);
        if (quest != null)
        {
            quest.completado = 2;
            if (questId ==1){
                DBQuizReqHolder.Instance.SetCoins(DBQuizReqHolder.Instance.GetCoins() + 300);
            }
            else if (questId ==2){
                DBQuizReqHolder.Instance.SetCoins(DBQuizReqHolder.Instance.GetCoins() + 250);
            }
            else if (questId ==3){
                DBQuizReqHolder.Instance.SetCoins(DBQuizReqHolder.Instance.GetCoins() + 400);
            }
        }

        yield return StartCoroutine(InitializeMissions());
        
        if (activeMissions.TryGetValue(questId, out Mission mission))
        {
            var updatedQuest = loadedMissions.FirstOrDefault(q => q.id_quest == questId);
            if (updatedQuest != null)
            {
                mission.UpdateFromServerData(updatedQuest);
            }
        }
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