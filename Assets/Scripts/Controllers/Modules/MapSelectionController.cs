using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Networking;

// Descripción: Este archivo controla la selección de mapas dentro del juego, gestionando el estado de desbloqueo, la visualización de estrellas obtenidas y la interacción con el servidor para desbloquear módulos y actualizar monedas del jugador.
// Autor: Alfonso Vega
public class MapSelectionController : MonoBehaviour
{
    [Header("Module Settings")]
    public int mapIndex;
    public int coinsRequired;
    public string levelSelectionSceneName;

    [Header("UI References")]
    public GameObject lockPanel;
    public GameObject unlockPanel;
    public TextMeshProUGUI starsText;
    public TextMeshProUGUI amountText;

    private bool isUnlocked = false;
    private int userId;

    private void Start()
    {
        userId = DBQuizReqHolder.Instance.GetUserID();
        StartCoroutine(CheckModuleStatus());
    }

    private IEnumerator CheckModuleStatus()
    {

        yield return StartCoroutine(GetModuleStatus(userId, mapIndex, (status) => {
            isUnlocked = status;
            UpdateUI();
        }));
    }

    private void UpdateUI()
    {
        lockPanel.SetActive(!isUnlocked);
        unlockPanel.SetActive(isUnlocked);
        /*
        if (isUnlocked)
        {
            //int totalStars = LevelSelectionController.GetTotalStars(levelSelectionSceneName);
            int totalStars = 
            StartCoroutine(
                DBQuizReqHolder.Instance.GetModuleStars(
                    mapIndex,
                    DBQuizReqHolder.Instance.GetUserID() 
            ));
            starsText.text = totalStars.ToString() + "/15";
        }
        */
        if (isUnlocked)
        {
            StartCoroutine(DBQuizReqHolder.Instance.GetModuleStars(
                mapIndex,
                DBQuizReqHolder.Instance.GetUserID(),
                onSuccess: (totalStars) =>
                {
                    starsText.text = $"{totalStars}/15";
                },
                onError: (error) =>
                {
                    Debug.LogError($"Failed to fetch module stars: {error}");
                    starsText.text = "0/15"; 
                }
            ));
        }
        
        amountText.text = "Unlock for " + coinsRequired.ToString();
    }

    public void OnModuleClicked()
    {
        if (isUnlocked)
        {
            PlayerPrefs.SetInt("CurrentBook", mapIndex);
            PlayerPrefs.SetString("CurrentModule", levelSelectionSceneName);
            DBQuizReqHolder.Instance.SetModuleID(mapIndex);
            SceneManager.LoadScene(levelSelectionSceneName);
        }
        else
        {
            StartCoroutine(TryUnlockModule());
        }
    }

    private IEnumerator TryUnlockModule()
    {

        int playerCoins = DBQuizReqHolder.Instance.GetCoins();
        
        if (playerCoins >= coinsRequired)
        {

            yield return StartCoroutine(UnlockModule(userId, mapIndex, coinsRequired, (success) => {
                if (success)
                {
                    isUnlocked = true;
                    UpdateUI();

                    DBQuizReqHolder.Instance.SetCoins(playerCoins - coinsRequired);
                    UpdateUI();
                }
            }));
        }
        else
        {
            Debug.Log("No tienes suficientes monedas");
           
        }
    }


    private IEnumerator GetModuleStatus(int userId, int moduleId, System.Action<bool> callback)
    {
        string url = DBQuizReqHolder.Instance.urlBD + "module/status?userId=" + userId + "&moduleId=" + moduleId;

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                ModuleStatusResponse response = JsonUtility.FromJson<ModuleStatusResponse>(request.downloadHandler.text);
                callback(response.desbloqueado);
            }
            else
            {
                Debug.LogError("Error al obtener estado del módulo: " + request.error);
                callback(false);
            }
        }
    }

    private IEnumerator UnlockModule(int userId, int moduleId, int price, System.Action<bool> callback)
    {
        string url = DBQuizReqHolder.Instance.urlBD + "module/unlock";

        ModuleUnlockRequest data = new ModuleUnlockRequest
        {
            id_usuario = userId,
            id_modulo = moduleId,
            precio = price
        };

        string json = JsonUtility.ToJson(data);
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            ModuleUnlockResponse response = JsonUtility.FromJson<ModuleUnlockResponse>(request.downloadHandler.text);
            callback(response.success);
        }
        else
        {
            Debug.LogError("Error al desbloquear módulo: " + request.error);
            callback(false);
        }
    }
}


[System.Serializable]
public class ModuleStatusResponse
{
    public bool desbloqueado;
}

[System.Serializable]
public class ModuleUnlockRequest
{
    public int id_usuario;
    public int id_modulo;
    public int precio;
}

[System.Serializable]
public class ModuleUnlockResponse
{
    public bool success;
    public int nuevas_monedas;
}