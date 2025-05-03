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

// Descripción: Este archivo gestiona las solicitudes y respuestas al servidor relacionadas con los datos del juego, incluyendo cuestionarios, misiones, monedas, niveles y módulos. También almacena y proporciona acceso a datos globales del jugador y del juego.
// Autor: Israel González y Alfonso Vega

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

public class QuizUsuarioUpdateRequest
{
    public int id_quiz;
    public int id_usuario;
    public int desbloqueado;
    public int estrellas;
    public int puntos;
    public int completado;
}

[System.Serializable]
public class ModuleStarsResponse
{
    public bool success;
    public int total_stars;
    public string error;
}


[System.Serializable]
public class ModuleLevelsResponse
{
    public bool success;
    public List<LevelData> levels;
    public string error;
}


[System.Serializable]
public class LevelData
{
    public int id_quiz;
    public string nombre_quiz;
    public bool desbloqueado;
}

[System.Serializable]
public class UnlockQuizRequest
{
    public int id_quiz;
    public int id_usuario;
    public int desbloqueado;
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
    private int potionCount = 0;
    private int shieldCount = 0;
    private MemoryResponse[] memoryData;
    //public string urlBD = "http://bd-cryptochicks.cmirgwrejba3.us-east-1.rds.amazonaws.com:3000/";
    public string urlBD = "";
    private int userID;
    private int coins = 0;
    private List<QuizStarsData> moduleQuizStars;

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
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public IEnumerator GetModuleQuizStars(int idModulo, int idUsuario)
    {
        string url = $"{urlBD}module/quiz-stars?id_modulo={idModulo}&id_usuario={idUsuario}";
        Debug.Log($"Requesting quiz stars for module {idModulo} and user {idUsuario} from: {url}");

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error fetching quiz stars: {request.error}");
                moduleQuizStars = null; 
            }
            else
            {
                string jsonResponse = request.downloadHandler.text;
                Debug.Log($"Response received: {jsonResponse}");

                try
                {
                    
                    QuizStarsResponse response = JsonUtility.FromJson<QuizStarsResponse>(jsonResponse);
                    if (response.success)
                    {
                        moduleQuizStars = response.quizzes; 
                    }
                    else
                    {
                        Debug.LogError($"Error in response: {response.error}");
                        moduleQuizStars = null; 
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error parsing response: {ex.Message}");
                    moduleQuizStars = null;
                }
            }
        }
    }


    public List<QuizStarsData> GetModuleQuizStarsData()
    {
        return moduleQuizStars;
    }


    [System.Serializable]
    private class QuizStarsResponse
    {
        public bool success;
        public List<QuizStarsData> quizzes;
        public string error;
    }


    [System.Serializable]
    public class QuizStarsData
    {
        public int id_quiz;
        public string nombre_quiz;
        public int estrellas;
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
                    return ip.ToString();
                }
            }
            Debug.LogError("No IPv4 address found.");
            return "127.0.0.1";
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error obtaining local IP address: {ex.Message}");
            return "127.0.0.1";
        }
    }

    private void OnDestroy()
    {

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


        StartCoroutine(GetQuizData(LevelNumber));
        if (scene.name == "ModuleSelection")
        {
            StartCoroutine(GetItemsData(userID));
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


                PreguntasWrapper preguntasWrapper = JsonUtility.FromJson<PreguntasWrapper>($"{{\"preguntas\":{jsonResponse}}}");
                preguntas = preguntasWrapper.preguntas;

                /*

                foreach (Pregunta pregunta in preguntas)
                {
                    
                    Debug.Log($"Pregunta ID: {pregunta.id_pregunta}");
                    Debug.Log($"Texto de la pregunta: {pregunta.pregunta}");
                    Debug.Log($"Índice correcto: {pregunta.indice_correcto}");
                    Debug.Log($"Explicación: {pregunta.explicacion}"); 
                    Debug.Log($"Tip: {pregunta.tip}");               
                    

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


                Item[] items = JsonUtility.FromJson<ItemsWrapper>($"{{\"items\":{jsonResponse}}}").items;


                /*
                foreach (Item item in items)
                {
                    Debug.Log($"Item ID: {item.id_item}, Name: {item.nombre_item}, Quantity: {item.cantidad}");
                }
                */


                UpdateItemCounts(items);
            }
        }
    }
    private void UpdateItemCounts(Item[] items)
    {
        foreach (Item item in items)
        {
            if (item.id_item == 1)
            {
                shieldCount = item.cantidad;
            }
            else if (item.id_item == 2)
            {
                potionCount = item.cantidad;
            }
        }
    }

    public IEnumerator GetQuestsData()
{

    string url = $"{urlBD}quests"; 
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


public IEnumerator IncrementQuestProgress(int questId, int increment = 1)
{
    int userId = GetUserID();
    string url = $"{urlBD}increment-quest-progress";
    

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

public IEnumerator UpdateMultipleQuests(Dictionary<int, int> questUpdates)
{
    foreach (var update in questUpdates)
    {
        yield return IncrementQuestProgress(update.Key, update.Value);
    }
}

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


                CoinsResponse coinsResponse = JsonUtility.FromJson<CoinsResponse>(jsonResponse);
                //Debug.Log($"Coins for user {userID}: {coinsResponse.coins}");
                coins = coinsResponse.coins;
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

 
            memoryData = JsonUtility.FromJson<MemoryWrapper>($"{{\"memory\":{jsonResponse}}}").memory;


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
            Debug.LogError($"Response: {webRequest.downloadHandler.text}");
            callback?.Invoke(false, null);
            yield break;
        }

        string jsonResponse = webRequest.downloadHandler.text;
        Debug.Log($"Raw JSON response: {jsonResponse}");

        try
        {

            HangmanResponse response = JsonUtility.FromJson<HangmanResponse>(jsonResponse);
            
            if (response == null)
            {
                Debug.LogError("Failed to parse response (null)");
                callback?.Invoke(false, null);
            }
            else if (!response.success)
            {
                Debug.LogError("Server returned success: false");
                callback?.Invoke(false, null);
            }
            else if (response.items == null)
            {
                Debug.LogError("Items array is null");
                callback?.Invoke(false, null);
            }
            else
            {
                Debug.Log($"Successfully loaded {response.items.Length} hangman items");
                callback?.Invoke(true, response.items);
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

public IEnumerator UpdateQuizUsuario(int idQuiz, int idUsuario, int desbloqueado, int estrellas, int puntos, int completado)
{
    string url = $"{urlBD}quiz-usuario/update";


    QuizUsuarioUpdateRequest data = new QuizUsuarioUpdateRequest
    {
        id_quiz = idQuiz,
        id_usuario = idUsuario,
        desbloqueado = desbloqueado,
        estrellas = estrellas,
        puntos = puntos,
        completado = completado
    };

    string json = JsonUtility.ToJson(data);
    byte[] body = System.Text.Encoding.UTF8.GetBytes(json);

    using (UnityWebRequest request = new UnityWebRequest(url, "PUT"))
    {
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Quiz_Usuario updated successfully: " + request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Error updating Quiz_Usuario: " + request.error);
        }
    }
}

public IEnumerator UnlockQuiz(int idQuiz, int idUsuario, int desbloqueado)
{
    string url = $"{urlBD}quiz-usuario/unlock";
    Debug.Log($"Unlocking quiz {idQuiz} for user {idUsuario} with desbloqueado: {desbloqueado}");


    string jsonBody = JsonUtility.ToJson(new UnlockQuizRequest
    {
        id_quiz = idQuiz,
        id_usuario = idUsuario,
        desbloqueado = desbloqueado
    });

    byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

    using (UnityWebRequest request = new UnityWebRequest(url, "PUT"))
    {
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"Quiz {idQuiz} unlocked successfully for user {idUsuario}: {request.downloadHandler.text}");
        }
        else
        {
            Debug.LogError($"Error unlocking quiz {idQuiz} for user {idUsuario}: {request.error}");
        }
    }
}


public IEnumerator GetModuleStars(int idModulo, int idUsuario, Action<int> onSuccess, Action<string> onError)
{
    string url = $"{urlBD}module/stars?id_modulo={idModulo}&id_usuario={idUsuario}";
    Debug.Log($"Requesting total stars for module {idModulo} and user {idUsuario} from: {url}");

    using (UnityWebRequest request = UnityWebRequest.Get(url))
    {
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"Error fetching module stars: {request.error}");
            onError?.Invoke(request.error);
        }
        else
        {
            string jsonResponse = request.downloadHandler.text;
            Debug.Log($"Response received: {jsonResponse}");

            try
            {

                ModuleStarsResponse response = JsonUtility.FromJson<ModuleStarsResponse>(jsonResponse);
                if (response.success)
                {
                    onSuccess?.Invoke(response.total_stars);
                }
                else
                {
                    onError?.Invoke(response.error);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error parsing response: {ex.Message}");
                onError?.Invoke("Failed to parse server response.");
            }
        }
    }
}

public IEnumerator GetModuleLevels(int idModulo, int idUsuario, Action<List<LevelData>> onSuccess, Action<string> onError)
{
    string url = $"{urlBD}module/levels?id_modulo={idModulo}&id_usuario={idUsuario}";
    Debug.Log($"Requesting levels for module {idModulo} and user {idUsuario} from: {url}");

    using (UnityWebRequest request = UnityWebRequest.Get(url))
    {
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"Error fetching module levels: {request.error}");
            onError?.Invoke(request.error);
        }
        else
        {
            string jsonResponse = request.downloadHandler.text;
            Debug.Log($"Response received: {jsonResponse}");

            try
            {

                ModuleLevelsResponse response = JsonUtility.FromJson<ModuleLevelsResponse>(jsonResponse);
                if (response.success)
                {
                    onSuccess?.Invoke(response.levels);
                }
                else
                {
                    onError?.Invoke(response.error);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error parsing response: {ex.Message}");
                onError?.Invoke("Failed to parse server response.");
            }
        }
    }
}
    public int GetPotionCount()
    {
        return potionCount;
    }

    public void SetPotionCount(int count)
    {
        potionCount = count;
    }

    public int GetShieldCount()
    {
        return shieldCount;
    }

    public void SetShieldCount(int count)
    {
        shieldCount = count;
    }

    public int GetModuleID()
    {
        return ModuleID;
    }

    public void SetModuleID(int moduleID)
    {
        ModuleID = moduleID;
    }

    public int GetLevelNumber()
    {
        return LevelNumber;
    }

    public void SetLevelNumber(int levelNumber)
    {
        LevelNumber = levelNumber;
    }

    public string GetGameType()
    {
        return GameType;
    }

    public void SetGameType(string gameType)
    {
        GameType = gameType;
    }

    public Pregunta[] GetPreguntas()
    {
        return preguntas;
    }

    public void SetPreguntas(Pregunta[] preguntasArray)
    {
        preguntas = preguntasArray;
    }

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

