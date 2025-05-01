using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Networking;

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
        // Consultar estado del módulo en la base de datos
        yield return StartCoroutine(GetModuleStatus(userId, mapIndex, (status) => {
            isUnlocked = status;
            UpdateUI();
        }));
    }

    private void UpdateUI()
    {
        lockPanel.SetActive(!isUnlocked);
        unlockPanel.SetActive(isUnlocked);

        if (isUnlocked)
        {
            int totalStars = LevelSelectionController.GetTotalStars(levelSelectionSceneName);
            starsText.text = totalStars.ToString() + "/15";
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
        // Verificar monedas del usuario
        int playerCoins = DBQuizReqHolder.Instance.GetCoins();
        
        if (playerCoins >= coinsRequired)
        {
            // Intentar desbloquear el módulo
            yield return StartCoroutine(UnlockModule(userId, mapIndex, coinsRequired, (success) => {
                if (success)
                {
                    isUnlocked = true;
                    UpdateUI();
                    // Actualizar monedas localmente
                    DBQuizReqHolder.Instance.SetCoins(playerCoins - coinsRequired);
                    UpdateUI();
                }
            }));
        }
        else
        {
            Debug.Log("No tienes suficientes monedas");
            // Mostrar mensaje de error al usuario
        }
    }

    // Métodos para comunicación con el servidor
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

// Clases para serialización JSON
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