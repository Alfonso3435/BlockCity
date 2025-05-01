using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;
using System.Data.Common;
using System.Linq;
using System.Collections.Generic;

[System.Serializable]
public class Pregunta
{
    public int id_pregunta;
    public string pregunta;
    public int indice_correcto;
    public string explicacion;
    public string tip;
    public Respuesta[] respuestas;
}

[System.Serializable]
public class Respuesta
{
    public int indice;
    public string respuesta;
}

[System.Serializable]
public class PreguntasWrapper
{
    public Pregunta[] preguntas;
}

[System.Serializable]
public class Item
{
    public int id_item;
    public string nombre_item;
    public int cantidad;
}

[System.Serializable]
public class ItemsWrapper
{
    public Item[] items;
}

[System.Serializable]
public class Quest
{
    public int id_quest;
    public string nombre;
    public string descrip_quest;
    public int TargetProgress;
    public int RewardCoins;
}

[System.Serializable]
public class QuestsWrapper
{
    public Quest[] quests;
}

[System.Serializable]
public class UserQuest
{
    public int id_quest;
    public string nombre;
    public string descrip_quest;
    public int TargetProgress;
    public int RewardCoins;
    public int userProgress;
    public int completado; // 0 = no completada, 1 = completada, 2 = reclamada
}

[System.Serializable]
public class UserQuestsWrapper
{
    public UserQuest[] quests;
}
public class CoinsResponse
{
    public int id_usuario;
    public int coins;
}

[System.Serializable]
public class MemoryResponse
{
    public string concepto;
    public string definicion;
}


public class DBQuizReqHolder : MonoBehaviour
{
    public static DBQuizReqHolder Instance;

    private int ModuleID;
    private int LevelNumber;
    private string GameType; // Quiz, Memory, Hangman
    private Pregunta[] preguntas;
    private Quest[] quests;
    private UserQuest[] userQuests;
    private bool isLoggedIn = false;
    private int potionCount = 0; // Store the potion count
    private int shieldCount = 0; // Store the shield count
    private MemoryResponse[] memoryData; // Array to store memory data
    //public string urlBD = "http://bd-cryptochicks.cmirgwrejba3.us-east-1.rds.amazonaws.com:3000/";
    public string urlBD = "http://10.48.66.147:3000/";
    private int userID = 13; // Cambia esto por el ID de usuario real
    private int coins = 0; // Store the coins value
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // Esto asegura que no haya duplicados
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // Desuscribirse del evento de cambio de escena
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        /*
        Debug.Log("Escena cargada: " + scene.name);
        Debug.Log($"ModuleID: {ModuleID}");
        Debug.Log($"LevelNumber: {LevelNumber}");
        Debug.Log($"GameType: {GameType}");¨
        */

        // This is to avoid having loading screens
        StartCoroutine(GetQuizData(LevelNumber));
        if (scene.name == "ModuleSelection")
        {
            StartCoroutine(GetItemsData(userID)); // Cambia esto por el ID de usuario real
        }
        //Debug.Log($"UserID: {userID}");
        
    }

    public IEnumerator GetQuizData(int NivelID)
    {
        //string url = $"http://localhost:3000/quiz/{NivelID}";
        string url = $"{urlBD}quiz/{NivelID}";
        //Debug.Log("Realizando solicitud a: " + url);

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error en la solicitud: " + request.error);
            }
            else
            {
                string jsonResponse = request.downloadHandler.text;
                //Debug.Log("Respuesta recibida: " + jsonResponse);

                // Deserializar el JSON utilizando el wrapper
                PreguntasWrapper preguntasWrapper = JsonUtility.FromJson<PreguntasWrapper>($"{{\"preguntas\":{jsonResponse}}}");
                preguntas = preguntasWrapper.preguntas;

                /*
                // Acceder a los valores individuales
                foreach (Pregunta pregunta in preguntas)
                {
                    
                    Debug.Log($"Pregunta ID: {pregunta.id_pregunta}");
                    Debug.Log($"Texto de la pregunta: {pregunta.pregunta}");
                    Debug.Log($"Índice correcto: {pregunta.indice_correcto}");
                    Debug.Log($"Explicación: {pregunta.explicacion}"); // Mostrar la explicación
                    Debug.Log($"Tip: {pregunta.tip}");               // Mostrar el tip
                    

                    foreach (Respuesta respuesta in pregunta.respuestas)
                    {
                        
                        Debug.Log($"Índice de respuesta: {respuesta.indice}");
                        Debug.Log($"Texto de respuesta: {respuesta.respuesta}");
                        
                    }
                }
                */
            }
        }
    }

    public IEnumerator GetItemsData(int userID)
    {
    //string url = $"http://localhost:3000/items/{userID}";
        string url = $"{urlBD}items/{userID}";
        Debug.Log("Realizando solicitud a: " + url);

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error en la solicitud: " + request.error);
            }
            else
            {
                string jsonResponse = request.downloadHandler.text;
                //Debug.Log("Respuesta recibida: " + jsonResponse);

                // Deserialize the JSON into a list of items
                Item[] items = JsonUtility.FromJson<ItemsWrapper>($"{{\"items\":{jsonResponse}}}").items;

                // Log all items to the console
                /*
                foreach (Item item in items)
                {
                    Debug.Log($"Item ID: {item.id_item}, Name: {item.nombre_item}, Quantity: {item.cantidad}");
                }
                */

                // Update potion and shield counts
                UpdateItemCounts(items);
            }
        }
    }
    private void UpdateItemCounts(Item[] items)
    {
        foreach (Item item in items)
        {
            if (item.id_item == 1) // Potion ItemId
            {
                shieldCount = item.cantidad;
            }
            else if (item.id_item == 2) // Shield ItemId
            {
                potionCount = item.cantidad;
            }
        }
    }

    public IEnumerator GetQuestsData()
{
    // Construimos la URL
    string url = $"{urlBD}quests";  // Asegúrate que urlBD termina en "/"
    Debug.Log("Realizando solicitud a: " + url);

    using (UnityWebRequest request = UnityWebRequest.Get(url))
    {
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error en la solicitud: " + request.error);
        }
        else
        {
            string jsonResponse = request.downloadHandler.text;
            Debug.Log("Respuesta recibida: " + jsonResponse);

            // Deserialize the JSON into a list of quests
            Quest[] questsWrapper = JsonUtility.FromJson<QuestsWrapper>($"{{\"quests\":{jsonResponse}}}").quests;
            quests = questsWrapper;
        }
    }
}

public IEnumerator GetUserQuestsData(int userId)
{
    string url = $"{urlBD}user-quests/{userId}";
    //Debug.Log("Obteniendo misiones del usuario: " + url);

    using (UnityWebRequest request = UnityWebRequest.Get(url))
    {
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || 
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error en la solicitud: " + request.error);
        }
        else
        {
            string jsonResponse = request.downloadHandler.text;
            //Debug.Log("Respuesta recibida: " + jsonResponse);

            userQuests = JsonUtility.FromJson<UserQuestsWrapper>($"{{\"quests\":{jsonResponse}}}").quests;
        }
    }
}

public IEnumerator UpdateQuestProgress(int userId, int questId, int progress)
{
    string url = $"{urlBD}update-quest-progress";
    Debug.Log("Actualizando progreso de misión: " + url);

    WWWForm form = new WWWForm();
    form.AddField("userId", userId);
    form.AddField("questId", questId);
    form.AddField("progress", progress);

    using (UnityWebRequest request = UnityWebRequest.Post(url, form))
    {
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || 
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error en la solicitud: " + request.error);
        }
        else
        {
            Debug.Log("Progreso actualizado correctamente");
        }
    }
}

// Método para incrementar progreso
public IEnumerator IncrementQuestProgress(int questId, int increment = 1)
{
    int userId = GetUserID();
    string url = $"{urlBD}increment-quest-progress";
    
    // Crea el cuerpo de la petición como JSON
    string jsonBody = $"{{\"userId\":{userId},\"questId\":{questId},\"increment\":{increment}}}";
    byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
    
    using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
    {
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.timeout = 10;
        
        Debug.Log($"Enviando a {url} con body: {jsonBody}");
        yield return request.SendWebRequest();
        
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Error: {request.error}");
            Debug.LogError($"Respuesta: {request.downloadHandler.text}");
        }
        else
        {
            Debug.Log($"Respuesta exitosa: {request.downloadHandler.text}");
            yield return GetUserQuestsData(userId);
        }
    }
}
[System.Serializable]
private class QuestProgressResponse
{
    public bool success;
    public int newProgress;
    public bool isCompleted;
    public int targetProgress;
}

// Método para actualizar múltiples misiones
public IEnumerator UpdateMultipleQuests(Dictionary<int, int> questUpdates)
{
    foreach (var update in questUpdates)
    {
        yield return IncrementQuestProgress(update.Key, update.Value);
    }
}

// Método para verificar y completar misiones
public IEnumerator CheckAndCompleteQuest(int questId, int requiredProgress = 1)
{
    var quest = GetUserQuests()?.FirstOrDefault(q => q.id_quest == questId);
    if (quest != null && quest.userProgress < requiredProgress)
    {
        yield return IncrementQuestProgress(questId, requiredProgress - quest.userProgress);
    }
}

public IEnumerator ClaimQuestReward(int userId, int questId)
{
    string url = $"{urlBD}claim-quest-reward";
    Debug.Log("Reclamando recompensa: " + url);

    WWWForm form = new WWWForm();
    form.AddField("userId", userId);
    form.AddField("questId", questId);

    using (UnityWebRequest request = UnityWebRequest.Post(url, form))
    {
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || 
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error en la solicitud: " + request.error);
            yield return request.error;
        }
        else
        {
            string jsonResponse = request.downloadHandler.text;
            Debug.Log("Recompensa reclamada: " + jsonResponse);
            yield return jsonResponse;
        }
    }
}

public IEnumerator GetCoinsData(int userID)
    {
        string url = $"{urlBD}coins/{userID}";
        Debug.Log("Realizando solicitud a: " + url);

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error en la solicitud: " + request.error);
            }
            else
            {
                string jsonResponse = request.downloadHandler.text;
                Debug.Log("Respuesta recibida: " + jsonResponse);

                // Parse the JSON response to extract the coins value
                CoinsResponse coinsResponse = JsonUtility.FromJson<CoinsResponse>(jsonResponse);
                Debug.Log($"Coins for user {userID}: {coinsResponse.coins}");
                coins = coinsResponse.coins; // Store the coins value
            }
        }
    }

public IEnumerator GetMemoryData(int idMemorama)
{
    string url = $"{urlBD}memory/{idMemorama}";
    Debug.Log("Realizando solicitud a: " + url);

    using (UnityWebRequest request = UnityWebRequest.Get(url))
    {
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error en la solicitud: " + request.error);
        }
        else
        {
            string jsonResponse = request.downloadHandler.text;
            Debug.Log("Respuesta recibida: " + jsonResponse);

            // Deserialize the JSON response into an array of MemoryResponse
            memoryData = JsonUtility.FromJson<MemoryWrapper>($"{{\"memory\":{jsonResponse}}}").memory;

            // Log the memory data for debugging
            foreach (var memory in memoryData)
            {
                Debug.Log($"Concepto: {memory.concepto}\nDefinición: {memory.definicion}");
            }
        }
    }
}



    public int GetPotionCount()
    {
        return potionCount;
    }

    public void SetPotionCount(int count) // MANDAR DATOS A LA DB
    {
        potionCount = count;
    }


    public int GetShieldCount()
    {
        return shieldCount;
    }

    public void SetShieldCount(int count) // MANDAR DATOS A LA DB
    {
        shieldCount = count;
    }

    // Métodos para ModuleID
    public int GetModuleID()
    {
        return ModuleID;
    }

    public void SetModuleID(int moduleID)
    {
        ModuleID = moduleID;
    }

    // Métodos para LevelNumber
    public int GetLevelNumber()
    {
        return LevelNumber;
    }

    public void SetLevelNumber(int levelNumber)
    {
        LevelNumber = levelNumber;
    }

    // Métodos para GameType
    public string GetGameType()
    {
        return GameType;
    }

    public void SetGameType(string gameType)
    {
        GameType = gameType;
    }

    // Métodos para Preguntas
    public Pregunta[] GetPreguntas()
    {
        return preguntas;
    }

    public void SetPreguntas(Pregunta[] preguntasArray)
    {
        preguntas = preguntasArray;
    }

    // Getter and Setter for IsLoggedIn
    public bool GetIsLoggedIn()
    {
        return isLoggedIn;
    }

    public void SetIsLoggedIn(bool loggedIn)
    {
        isLoggedIn = loggedIn;
    }

    public int GetUserID()
    {
        return userID;
    }

    public void SetUserID(int id)
    {
        userID = id;
    }
    public Quest[] GetQuests()
    {
        return quests;
    }

    public UserQuest[] GetUserQuests()
{
    return userQuests;
}

    public MemoryResponse[] GetMemoryDataArray()
    {
        return memoryData;
    }

    [System.Serializable]
    private class MemoryWrapper
    {
        public MemoryResponse[] memory;
    }


    public int GetCoins()
    {
        return coins;
    }

    public void SetCoins(int value)
    {
        coins = value;
    }


}

