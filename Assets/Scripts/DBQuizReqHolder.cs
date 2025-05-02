using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Data.Common;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

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

[System.Serializable]
public class HangmanItem
{
    public int id;
    public int id_ahorcado;
    public string word;
    public string definition;
}

[System.Serializable]
public class HangmanResponse
{
    public bool success;
    public HangmanItem[] items;
}

[System.Serializable]
public class HangmanDataWrapper
{
    public bool success;
    public HangmanResponse[] data;
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
    public string urlBD = "";
    private int userID = 13; // Cambia esto por el ID de usuario real
    private int coins = 0; // Store the coins value
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

        string localIP = GetLocalIPAddress();
        urlBD = $"http://{localIP}:3000/";
        Debug.Log($"Server URL set to: {urlBD}");
        }
        else
        {
            Destroy(gameObject); // Esto asegura que no haya duplicados
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public string GetLocalIPAddress()
    {
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString(); // Return the first IPv4 address found
                }
            }
            Debug.LogError("No IPv4 address found.");
            return "127.0.0.1"; // Default to localhost if no IP is found
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error obtaining local IP address: {ex.Message}");
            return "127.0.0.1"; // Default to localhost in case of an error
        }
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
        string url = $"{urlBD}items/{userID}";
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
        
        //Debug.Log($"Enviando a {url} con body: {jsonBody}");
        yield return request.SendWebRequest();
        
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Error: {request.error}");
            Debug.LogError($"Respuesta: {request.downloadHandler.text}");
        }
        else
        {
            //Debug.Log($"Respuesta exitosa: {request.downloadHandler.text}");
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

                // Parse the JSON response to extract the coins value
                CoinsResponse coinsResponse = JsonUtility.FromJson<CoinsResponse>(jsonResponse);
                //Debug.Log($"Coins for user {userID}: {coinsResponse.coins}");
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

public IEnumerator GetHangmanData(int idQuiz, System.Action<bool, HangmanItem[]> callback)
{
    string url = $"{urlBD}hangman/{idQuiz}";
    Debug.Log($"Requesting hangman data from: {url}");

    using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
    {
        yield return webRequest.SendWebRequest();

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Network error: {webRequest.error}");
            callback?.Invoke(false, null);
            yield break;
        }

        string jsonResponse = webRequest.downloadHandler.text;
        Debug.Log($"Raw JSON response: {jsonResponse}");

        try
        {
            // Parsear la respuesta
            HangmanResponse response = JsonUtility.FromJson<HangmanResponse>(jsonResponse);
            
            if (response != null && response.success && response.items != null)
            {
                Debug.Log($"Successfully loaded {response.items.Length} hangman items");
                callback?.Invoke(true, response.items);
            }
            else
            {
                Debug.LogError("Invalid response format");
                callback?.Invoke(false, null);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"JSON parse error: {e.Message}");
            Debug.LogError($"Problematic JSON: {jsonResponse}");
            callback?.Invoke(false, null);
        }
    }
}

// Clase helper para deserializar arrays JSON
public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        string newJson = "{ \"array\": " + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.array;
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] array;
    }
}
private HangmanResponse[] hangmanData;

public void SetHangmanData(HangmanResponse[] data)
{
    hangmanData = data;
    Debug.Log($"Set {data.Length} hangman items");
}

public HangmanResponse[] GetHangmanDataArray()
{
    return hangmanData ?? new HangmanResponse[0];
}
public IEnumerator UpdateCoins(int userID, int amount)
{
    string url = $"{urlBD}coins/update";
    Debug.Log($"Updating coins for user {userID} by {amount}");

    // Create the request body as JSON
    string jsonBody = $"{{\"id_usuario\":{userID},\"coins\":{amount}}}";
    byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

    using (UnityWebRequest request = new UnityWebRequest(url, "PUT"))
    {
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"Error updating coins: {request.error}");
            Debug.LogError($"Response: {request.downloadHandler.text}");
        }
        else
        {
            Debug.Log($"Coins updated successfully: {request.downloadHandler.text}");
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

